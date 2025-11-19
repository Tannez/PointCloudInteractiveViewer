using System.Collections.Generic;
using System.Collections;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.CloudData;
using BAPointCloudRenderer.Edl;
using BAPointCloudRenderer.Loading;
using BAPointCloudRenderer.ObjectCreation;
using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NUnit.Framework;
using System.Linq;

public enum LLMTestModes
{
    None = 0,
    Chat = 1,
    Function = 2,
}
public class UIInstanceController : MonoBehaviour
{
    //Cam Access
    Camera cam;

    //Reference to pointcloud gameobject and its script
    [Header("Cloud Instantiation Components")]

    [Tooltip("Before running scene, a cloud instantiator is required in hierarchy. Create empty GameObject and add a Cloud Instantiator script to it. Then insert the cloud instantiator here. Remember to set asset path and prefabs in cloud instantiator script")]
    [SerializeField] CloudInstanceInstantiatior cloudInstantiator;
    [Tooltip("Before running scene, an initial cloud loader is required in hierarchy. Create empty GameObject beneath Cloud Instantiator and add a Dynamic Loader to it. Then insert this cloud loader here.")]
    [SerializeField] GameObject InitialCloudLoader;
    [SerializeField] private GameObject clippingPlane;
    DirectoryInstanceLoader directoryInstanceLoader;
    // // Mesh Configurations
    // [SerializeField] private PointMeshConfiguration pointMeshConfiguration;
    [Tooltip("Adds Mesh To points. Insert GameObject Beneath the cloud instantiator, and add the DefaultMeshConfiguration script. Then insert GameObject here.")]
    [SerializeField] private DefaultMeshConfiguration defaultMeshConfiguration;
    [SerializeField] public List<DirectoryInstanceLoader.PCInstances> PCClasses = new List<DirectoryInstanceLoader.PCInstances>();
    [SerializeField] public List<GameObject> PCInstances = new List<GameObject>();

    //UI Inspector Components
    [Header("Point Budget")]
    [SerializeField] public uint PCPointBudget;
    [SerializeField] public float EDLRadiusUI { get { return edlCamera._edlRadius; } set { edlCamera._edlRadius = EDLRadiusSlider.value; } }
    [SerializeField] public float EDLExpScaleUI { get { return edlCamera._edlExpScale; } set { edlCamera._edlExpScale = EDLExpScaleSlider.value; } }
    [SerializeField] public float EDLScaleUI { get { return edlCamera._edlScale; } set { edlCamera._edlScale = EDLScaleSlider.value; } }
    [SerializeField] private Slider pointBudgetSlider;
    [SerializeField] private TextMeshProUGUI pointBudgetSliderText;

    [Header("EDL Slider Parents")]
    [SerializeField] private GameObject EDLRadiusSliderParent;
    [SerializeField] private GameObject EDLScaleSliderParent;
    [SerializeField] private GameObject EDLExpScaleSliderParent;

    [Header("EDL Components")]
    [SerializeField] private EdlCamera edlCamera;
    [SerializeField] private Toggle EDLToggle;
    [SerializeField] private Slider EDLRadiusSlider;
    [SerializeField] private TextMeshProUGUI EDLRadiusSliderText;
    [SerializeField] private Slider EDLExpScaleSlider;
    [SerializeField] private TextMeshProUGUI EDLExpScaleSliderText;
    [SerializeField] private Slider EDLScaleSlider;
    [SerializeField] private TextMeshProUGUI EDLScaleSliderText;

    [Header("Toggles")]
    [Tooltip("These Toggles refer to the ones seen in the main UI, under Classification")]
    [SerializeField] public List<Toggle> classToggles = new List<Toggle>();
    private bool loadingClassToggles = true;
    private bool loadingAllInstanceToggles = true;
    [Tooltip("These Toggles are only shown if the Instance Menu Button in the main UI has been pressed. They will be displayed on an instance menu on the right.")]
    [SerializeField] private List<Toggle> InstanceTogglesAll = new List<Toggle>();
    private bool loadingClassInstanceToggles = true;
    [Tooltip("These Toggles are only shown if the Class Button in the Classification Section has been pressed. They will be displayed on an instance menu on the right.")]
    [SerializeField] private List<Toggle> InstanceTogglesClass = new List<Toggle>();

    [Header("Instance UI")]
    [Tooltip("This UI Image refers to the one used when using the instance menu button")]
    [SerializeField] private Image instanceUIImageAll;
    [Tooltip("This UI Image refers to the one used when using the class buttons")]
    [SerializeField] private Image instanceUIImageClass;
    [Tooltip("This is the instance menu button")]
    [SerializeField] private Button instanceUIButton;
    private bool instanceUIActive = false;
    public bool classInstanceUIActive = false;

    [Header("Color Mode Dropdown")]
    [SerializeField] public TMP_Dropdown colorModeDropDown;

    [Header("Clipper Components")]
    private GameObject[] Clipper = new GameObject[3];
    private bool activeBoxClipper = false;
    private bool activeSphereClipper = false;
    private bool activeCylinderClipper = false;

    [Header("Clipper Buttons")]
    [SerializeField] Button boxClipButton;
    [SerializeField] Button sphereClipButton;
    [SerializeField] Button cylinderClipButton;

    [Header("Exploded View Control")]
    [SerializeField] private Slider ExplodedViewSlider;

    // selection mode variables
    [Header("Class Buttons")]
    [Tooltip("Use this button to allow for class selection using the keyboard buttons (1,2,3,4,5)")]
    [SerializeField] bool keyboardClassSelection = true;
    [Tooltip("Class buttons within the Classification Section of the Main UI")]
    [SerializeField] List<Button> classButtons = new List<Button>();
    BAPointCloudRenderer.ObjectCreation.ColorMode previousClassColor;
    public bool[] classSelected = new bool[10];
    //private List<int> activeClasses = new List<int>();
    public bool[] classInstanceSelected = new bool[10];
    public bool[] classHidden = new bool[10]; // used to disable cloud selection when toggled off
    public int activeClassInstanceInMenu = 0;

    [Header("Instance Buttons")]
    [Tooltip("Buttons available within the Instance Menu. Only shown when Instance Menu is active")]
    [SerializeField] List<Button> AllInstancesButtons = new List<Button>();
    [Tooltip("Buttons available within the Instance Menu. Only shown when specific Class is selected")]
    [SerializeField] List<Button> ClassInstancesButtons = new List<Button>();
    private bool loadingInstanceButtons = true;
    // private bool loadingClassInstanceButtons = true;
    private bool[] instanceSelected = new bool[10];
    BAPointCloudRenderer.ObjectCreation.ColorMode previousInstanceColor;
    BAPointCloudRenderer.ObjectCreation.ColorMode previousClassInstanceColor;
    BAPointCloudRenderer.ObjectCreation.ColorMode previousClassPriorityColor;

    [HideInInspector] public int availableInstancesInClass = 0;

    [Header("LLM")]
    [Tooltip("Select Type of LLM Interaction to Test. 'Chat' uses ChatBot object, while 'funtion' uses FunctionCalling Object")]
    [SerializeField] LLMTestModes lLMTestModes;
    [SerializeField] GameObject LLMPanelsUI;
    [SerializeField] GameObject ChatBot;
    [SerializeField] GameObject FunctionCalling;
    [SerializeField] GameObject PointCloudAssistant;
    public bool LLMMenuActive = false;

    [HideInInspector] public bool clickedWithMouse = false;

    void Start()
    {
        // Access Main Camera
        cam = Camera.main;

        // Get Cloud objects
        directoryInstanceLoader = cloudInstantiator.DirectoryLoaderGO.GetComponent<DirectoryInstanceLoader>();
        PCClasses = directoryInstanceLoader.pointCloudClasses;
        PCInstances = directoryInstanceLoader.allInstances;

        // Set Initial Point Budget 
        PCPointBudget = InitialCloudLoader.GetComponent<DynamicPointCloudSet>().pointBudget;

        foreach (GameObject pci in PCInstances)
        {
            pci.GetComponentInChildren<DynamicPointCloudSet>().pointBudget = PCPointBudget;
        }

        //Debug.Log("Point Clouds available in UI: " + PCClasses.Count);

        // Get EDL values from EdlCamera Script
        EDLRadiusUI = edlCamera.EdlRadius;
        EDLExpScaleUI = edlCamera.EdlExpScale;
        EDLScaleUI = edlCamera.EdlScale;

        // Set Slider values
        EDLRadiusSlider.value = EDLRadiusUI;
        EDLRadiusSliderText.text = EDLRadiusSlider.value.ToString();
        EDLExpScaleSlider.value = EDLExpScaleUI;
        EDLExpScaleSliderText.text = EDLExpScaleSlider.value.ToString();
        EDLScaleSlider.value = EDLScaleUI;
        EDLScaleSliderText.text = EDLScaleSlider.value.ToString();

        // Create Slider Listeners 
        pointBudgetSlider.onValueChanged.AddListener(delegate { PCValueChangeCheck(); });
        EDLRadiusSlider.onValueChanged.AddListener(delegate { EDLRadiusChangeCheck(); });
        EDLExpScaleSlider.onValueChanged.AddListener(delegate { EDLExpScaleChangeCheck(); });
        EDLScaleSlider.onValueChanged.AddListener(delegate { EDLScaleChangeCheck(); });
        ExplodedViewSlider.onValueChanged.AddListener(delegate { ExplodedViewSpread(); });

        // Create Toggle Listener
        EDLToggle.onValueChanged.AddListener(delegate { EDLToggleChange(); });

        // Create Drop Down Listener
        colorModeDropDown.onValueChanged.AddListener(delegate { DropdownColorModeChange(); });

        // Set Available toggles and buttons based on class amount
        LoadClassToggles();
        LoadClassSelectionButtons();

        // ChatBot.SetActive(false);
        // FunctionCalling.SetActive(false);
    }

    void Update()
    {
        // Use keyboard numbers to select classes
        if (keyboardClassSelection && !LLMMenuActive)
        {
            KeyboardCloudSelection();
        }

        // Use keyboard to reload clouds in case they are not loading properly
        if (Input.GetKeyDown(KeyCode.R) && !LLMMenuActive)
        {
            StartCoroutine(ReloadClouds());
        }
    }

    // Method For Changing Point Budget Of Point Clouds
    public void PCValueChangeCheck()
    {
        int cloudClassIter = 0;
        foreach (DirectoryInstanceLoader.PCInstances cloud in PCClasses)
        {
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;

                if (instanceInClass.name.StartsWith("Cloud:") && classHidden[cloudClassIter] == false)
                {
                    // Remove Clouds
                    instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                    // ShutDown V2 Renderer
                    instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().PointRenderer.ShutDown();
                    // Disable DynamicPointCloudSet Component
                    instanceInClass.SetActive(false);
                }
            }
            cloudClassIter++;
        }

        // Change Value Of Point Budget
        PCPointBudget = (uint)pointBudgetSlider.value;

        cloudClassIter = 0;

        foreach (DirectoryInstanceLoader.PCInstances cloud in PCClasses)
        {
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;

                if (instanceInClass.name.StartsWith("Cloud:") && classHidden[cloudClassIter] == false)
                {
                    // Change Point Budget
                    instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().pointBudget = PCPointBudget;
                }
            }
            cloudClassIter++;
        }

        pointBudgetSliderText.text = PCPointBudget.ToString();

        cloudClassIter = 0;

        foreach (DirectoryInstanceLoader.PCInstances cloud in PCClasses)
        {
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;

                if (instanceInClass.name.StartsWith("Cloud:") && classHidden[cloudClassIter] == false)
                {
                    // Enable DynamicPointCloudSet Component
                    instanceInClass.SetActive(true);
                    // Enable Clouds
                    instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                    // Show V2 Renderer
                    instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
                }
            }
            cloudClassIter++;
        }
    }

    // Methods For EDL Parameters
    public void EDLRadiusChangeCheck()
    {
        EDLRadiusUI = EDLRadiusSlider.value;
        EDLRadiusSliderText.text = EDLRadiusUI.ToString();
    }
    public void EDLExpScaleChangeCheck()
    {
        EDLExpScaleUI = EDLExpScaleSlider.value;
        EDLExpScaleSliderText.text = EDLExpScaleUI.ToString();
    }
    public void EDLScaleChangeCheck()
    {
        EDLScaleUI = EDLScaleSlider.value;
        EDLScaleSliderText.text = EDLScaleUI.ToString();
    }

    // Method For Exploded View 
    public void ExplodedViewSpread()
    {
        int cloudRuns = 0;

        foreach (DirectoryInstanceLoader.PCInstances cloud in PCClasses)
        {
            cloud.cloudClassGO.transform.position = new Vector3(0.0f, 0.0f + ExplodedViewSlider.value * (PCClasses.Count - cloudRuns) / 10, 0.0f);
            cloudRuns++;
        }
    }

    // Methods For Changing Skybox/Background Color In Scene
    public void SkyBoxButton()
    {
        clippingPlane.SetActive(false);
        cam.clearFlags = CameraClearFlags.Skybox;
        //Debug.Log("SkyBox Button Pressed!");
    }
    public void BlackButton()
    {
        clippingPlane.SetActive(true);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        //Debug.Log("Black Button Pressed!");
    }
    public void WhiteButton()
    {
        clippingPlane.SetActive(true);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.white;
        //Debug.Log("White Button Pressed!");
    }

    // Methods For Loading And Removing Point Clouds
    public void HidePCClass(int CloudToHide)
    {
        GameObject PointCloudHidden = PCClasses[CloudToHide].cloudClassGO;
        classHidden[CloudToHide] = true;
        for (int i = 0; i < PointCloudHidden.transform.childCount; i++)
        {
            GameObject instanceInClass = PointCloudHidden.transform.GetChild(i).gameObject;
            if (instanceInClass.name.StartsWith("Cloud:"))
            {
                instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
            }
        }
        //Debug.Log($"Cloud: {CloudToHide} is hidden");
    }
    public void ShowPCClass(int CloudToShow)
    {
        GameObject PointCloudLoaded = PCClasses[CloudToShow].cloudClassGO;
        classHidden[CloudToShow] = false;
        for (int i = 0; i < PointCloudLoaded.transform.childCount; i++)
        {
            GameObject instanceInClass = PointCloudLoaded.transform.GetChild(i).gameObject;
            if (instanceInClass.name.StartsWith("Cloud:"))
            {
                instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
            }
        }
        //Debug.Log($"Cloud: {CloudToShow} is shown");
    }
    public void HidePCInstance(int CloudToHide)
    {
        GameObject PointCloudHidden = PCInstances[CloudToHide];
        PointCloudHidden.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
        //Debug.Log($"Cloud: {CloudToHide} is hidden");
    }
    public void ShowPCInstance(int CloudToShow)
    {
        GameObject PointCloudLoaded = PCInstances[CloudToShow];
        PointCloudLoaded.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
        //Debug.Log($"Cloud: {CloudToShow} is shown");
    }
    public void HidePCClassInstance(int CClassToHide, int CInstanceToHide)
    {
        GameObject PointCloudClass = PCClasses[CClassToHide - 1].cloudClassGO;
        for (int i = 0; i < PointCloudClass.transform.childCount; i++)
        {
            GameObject isInstancesInClass = PointCloudClass.transform.GetChild(i).gameObject;
            if (isInstancesInClass.name.StartsWith($"Cloud: {CInstanceToHide}"))
            {
                GameObject InstanceToHide = isInstancesInClass;
                //InstanceToHide.GetComponentInChildren<AbstractPointCloudSet>().showBoundingBox = false;
                InstanceToHide.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
            }
        }
        //Debug.Log($"Cloud: {CInstanceToHide}, in Class: {CClassToHide} is hidden");
    }
    public void ShowPCClassInstance(int CClassToShow, int CInstanceToShow)
    {
        GameObject PointCloudClass = PCClasses[CClassToShow - 1].cloudClassGO;
        for (int i = 0; i < PointCloudClass.transform.childCount; i++)
        {
            GameObject isInstancesInClass = PointCloudClass.transform.GetChild(i).gameObject;
            if (isInstancesInClass.name.StartsWith($"Cloud: {CInstanceToShow}"))
            {
                GameObject InstanceToShow = isInstancesInClass;
                //InstanceToShow.GetComponentInChildren<AbstractPointCloudSet>().showBoundingBox = true;
                InstanceToShow.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
            }
        }
        //Debug.Log($"Cloud: {CInstanceToShow}, in Class: {CClassToShow} is shown");
    }

    // Methods For Changing Color Mode Of Point Clouds.
    // NOTE: Ensure that Clouds are Reloaded after conversion
    public void ClassificationConversion()
    {
        // pointMeshConfiguration.material = new Material(Shader.Find("CustomRenderTexture/Classification"));
        // Debug.Log("Showing Classification");
        foreach (DirectoryInstanceLoader.PCInstances cloud in PCClasses)
        {
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;
                if (instanceInClass.name.StartsWith("Cloud"))
                {
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Classification;
                }
            }
        }
        // defaultMeshConfiguration.colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Classification;
        //Debug.Log("Showing Classification");
    }
    public void RGBConversion()
    {
        // pointMeshConfiguration.material = new Material(Shader.Find("Custom/PointShader"));
        // Debug.Log("Showing RGB");

        foreach (DirectoryInstanceLoader.PCInstances cloud in PCClasses)
        {
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;
                if (instanceInClass.name.StartsWith("Cloud"))
                {
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.RGBA;
                }

            }
        }

        // defaultMeshConfiguration.colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.RGBA;
        //Debug.Log("Showing RGBA");
    }
    public void IntensityConversion()
    {
        // pointMeshConfiguration.material = new Material(Shader.Find("Custom/PointShader"));
        // Debug.Log("Showing Intensity");
        foreach (DirectoryInstanceLoader.PCInstances cloud in PCClasses)
        {
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;
                if (instanceInClass.name.StartsWith("Cloud"))
                {
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Intensity;
                }

            }
        }
        // defaultMeshConfiguration.colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Intensity;
        //Debug.Log("Showing Intensity");
    }

    // Method For EDL Toggle
    public void EDLToggleChange()
    {
        if (EDLToggle.isOn)
        {
            edlCamera.EdlOn = true;
            EDLRadiusSliderParent.SetActive(true);
            EDLScaleSliderParent.SetActive(true);
            EDLExpScaleSliderParent.SetActive(true);
        }
        else if (!EDLToggle.isOn)
        {
            edlCamera.EdlOn = false;
            EDLRadiusSliderParent.SetActive(false);
            EDLScaleSliderParent.SetActive(false);
            EDLExpScaleSliderParent.SetActive(false);
        }
    }

    // Methods For Point Size Control
    public void PointSizeUp()
    {
        if (!instanceUIActive)
        {
            foreach (DirectoryInstanceLoader.PCInstances clouds in PCClasses)
            {
                for (int i = 0; i < clouds.cloudClassGO.transform.childCount; i++)
                {
                    GameObject instanceInClass = clouds.cloudClassGO.transform.GetChild(i).gameObject;
                    if (instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius < 5.0f)
                    {
                        instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius += 0.25f;
                        instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                    }
                }
            }
        }
        else if (instanceUIActive)
        {
            foreach (GameObject pci in PCInstances)
            {
                if (pci.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius < 5.0f)
                {
                    pci.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius += 0.25f;
                    pci.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                }
            }
        }

        // if (defaultMeshConfiguration.pointRadius < 5.0f)
        // {
        //     defaultMeshConfiguration.pointRadius += 0.25f;
        //     defaultMeshConfiguration.reload = true;
        //     // Debug.Log("Point Radius Increased");
        // }
    }
    public void PointSizeDown()
    {
        if (!instanceUIActive)
        {
            foreach (DirectoryInstanceLoader.PCInstances clouds in PCClasses)
            {
                for (int i = 0; i < clouds.cloudClassGO.transform.childCount; i++)
                {
                    GameObject instanceInClass = clouds.cloudClassGO.transform.GetChild(i).gameObject;
                    if (instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius > 0.25f)
                    {
                        instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius -= 0.25f;
                        instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                    }
                }
            }

        }
        else if (instanceUIActive)
        {
            foreach (GameObject pci in PCInstances)
            {
                if (pci.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius > 0.25f)
                {
                    pci.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius -= 0.25f;
                    pci.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                }
            }
        }
        // if (defaultMeshConfiguration.pointRadius > 0.25f)
        // {
        //     defaultMeshConfiguration.pointRadius -= 0.25f;
        //     defaultMeshConfiguration.reload = true;
        //     // Debug.Log("Point Radius Decreased");
        // }
    }

    // Methods For Insantiating Primitves With Hide-Object Shader
    public void BoxMeshClipper(GameObject clippingBox)
    {
        if (!activeBoxClipper)
        {
            Clipper[0] = Instantiate(clippingBox, new Vector3(0, 0, 0), Quaternion.identity);
            activeBoxClipper = true;
            boxClipButton.image.color = Color.red;

        }
        else if (activeBoxClipper)
        {
            GameObject.DestroyImmediate(Clipper[0], true);
            activeBoxClipper = false;
            boxClipButton.image.color = Color.white;
        }
    }
    public void SphereMeshClipper(GameObject clippingSphere)
    {
        if (!activeSphereClipper)
        {
            Clipper[1] = Instantiate(clippingSphere, new Vector3(0, 0, 0), Quaternion.identity);
            activeSphereClipper = true;
            sphereClipButton.image.color = Color.red;

        }
        else if (activeSphereClipper)
        {
            GameObject.DestroyImmediate(Clipper[1], true);
            activeSphereClipper = false;
            sphereClipButton.image.color = Color.white;
        }
    }
    public void CylinderMeshClipper(GameObject clippingCylinder)
    {
        if (!activeCylinderClipper)
        {
            Clipper[2] = Instantiate(clippingCylinder, new Vector3(0, 0, 0), Quaternion.identity);
            activeCylinderClipper = true;
            cylinderClipButton.image.color = Color.red;

        }
        else if (activeCylinderClipper)
        {
            GameObject.DestroyImmediate(Clipper[2], true);
            activeCylinderClipper = false;
            cylinderClipButton.image.color = Color.white;
        }
    }

    // Method For Changing Color Mode Via DropDown In UI
    public void DropdownColorModeChange()
    {
        if (colorModeDropDown.value == 0)
        {
            //  <- New !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            // Reset Selects
            UnSelectOnConversion();
                      
            // Remove Clouds
            //StartCoroutine(StopLoadingClouds());

            // Change ColorMode
            RGBConversion();

            //Enable Clouds
            //StartCoroutine(StartLoadingClouds());
        }

        else if (colorModeDropDown.value == 1)
        {
            //  <- New !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            // Reset Selects
            UnSelectOnConversion();

            // Remove Clouds
            //StartCoroutine(StopLoadingClouds());

            // Change ColorMode
            ClassificationConversion();

            //Enable Clouds
            //StartCoroutine(StartLoadingClouds());
        }

        else if (colorModeDropDown.value == 2)
        {
            //  <- New !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            // Reset Selects
            UnSelectOnConversion();

            // Remove Clouds
            //StartCoroutine(StopLoadingClouds());

            // Change ColorMode
            IntensityConversion();

            //Enable Clouds
            //StartCoroutine(StartLoadingClouds());
        }
    }

    // Method For Changing Color Mode when selecting cloud with keyboard input
    public void KeyboardCloudSelection()
    {
        // hard coded to keyboard buttons:
        if (Input.GetKeyDown(KeyCode.Alpha1) && classSelected[0] == false)
        {
            SelectCloudClass(1);
            classSelected[0] = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && classSelected[0] == true)
        {
            UnSelectCloudClass(1);
            classSelected[0] = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && classSelected[1] == false)
        {
            SelectCloudClass(2);
            classSelected[1] = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && classSelected[1] == true)
        {
            UnSelectCloudClass(2);
            classSelected[1] = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && classSelected[2] == false)
        {
            SelectCloudClass(3);
            classSelected[2] = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && classSelected[2] == true)
        {
            UnSelectCloudClass(3);
            classSelected[2] = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && classSelected[3] == false)
        {
            SelectCloudClass(4);
            classSelected[3] = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && classSelected[3] == true)
        {
            UnSelectCloudClass(4);
            classSelected[3] = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5) && classSelected[4] == false)
        {
            SelectCloudClass(5);
            classSelected[4] = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && classSelected[4] == true)
        {
            UnSelectCloudClass(5);
            classSelected[4] = false;
        }
    }

    // Method For Toggling The Various Classes
    public void classToggleChange()
    {
        if (!loadingClassToggles && !instanceUIActive)
        {
            int currentToggle = 0;

            foreach (Toggle toggle in classToggles)
            {
                if (toggle.isOn)
                {
                    ShowPCClass(currentToggle);
                    //Debug.Log("Cloud: " + currentToggle + " Toggle is on");
                }
                else if (!toggle.isOn)
                {
                    HidePCClass(currentToggle);
                    //Debug.Log("Cloud: " + currentToggle + " Toggle is off");
                }
                currentToggle++;

                if (currentToggle == classToggles.Count)
                {
                    break;
                }
            }
        }

        else if (!loadingClassToggles && instanceUIActive)
        {
            int currentToggle = 0;

            foreach (Toggle toggle in classToggles)
            {
                toggle.image.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
                currentToggle++;

                if (currentToggle == classToggles.Count)
                {
                    break;
                }
            }
        }
    }
    // Method To Set All Toggles Active Again
    public void ResetClassToggles()
    {
        int classTogglesIter = 0;
        foreach (Toggle ctoggle in classToggles)
        {
            ctoggle.isOn = true;
            classHidden[classTogglesIter] = false;
        }
        StartCoroutine(ReloadClouds());
    }
    // Method For Toggling The Various Classes
    public void InstanceToggleChange()
    {
        if (!loadingAllInstanceToggles)
        {
            int currentToggle = 0;

            foreach (Toggle toggle in InstanceTogglesAll)
            {
                if (toggle.isOn)
                {
                    ShowPCInstance(currentToggle);
                    //Debug.Log("Cloud: " + currentToggle + " Toggle is on");
                }
                else if (!toggle.isOn)
                {
                    HidePCInstance(currentToggle);
                    //Debug.Log("Cloud: " + currentToggle + " Toggle is off");
                }
                currentToggle++;

                if (currentToggle == InstanceTogglesAll.Count)
                {
                    break;
                }
            }
        }
    }

    // Method for toggling instance within class
    public void ClassInstanceToggleChange()
    {
        if (!loadingClassInstanceToggles)
        {
            int currentToggle = 0;
            int selectedClass = 1;
            int classSearch = 1;

            foreach (bool selected in classSelected)
            {
                if (selected == true)
                {
                    selectedClass = classSearch;
                    //Debug.Log("Selcted Class is: " + selectedClass);
                    break;
                }
                else if (selected == false)
                {
                    //Debug.Log("Selcted Class Is Not: " + selectedClass);
                    classSearch++;
                }
            }

            foreach (Toggle ciToggle in InstanceTogglesClass)
            {
                if (ciToggle.isOn)
                {
                    ShowPCClassInstance(selectedClass, currentToggle + 1);
                    //Debug.Log("Toggle for Cloud: " + (currentToggle+1) + " in Class: " + selectedClass + " is on");
                }
                else if (!ciToggle.isOn)
                {
                    HidePCClassInstance(selectedClass, currentToggle + 1);
                    //Debug.Log("Toggle for Cloud: " + (currentToggle+1) + " in Class: " + selectedClass + " is off");
                }
                currentToggle++;

                if (currentToggle == availableInstancesInClass)
                {
                    break;
                }
            }
        }
    }

    // Method For Showing Instance UI And Toggling The Various Instances
    public void ShowInstanceUI()
    {
        if (!instanceUIActive)
        {
            // show instance menu
            instanceUIActive = true;
            instanceUIImageAll.gameObject.SetActive(true);
            instanceUIButton.image.color = Color.gray;
            colorModeDropDown.value = 0;

            // Disable class toggles
            int currentToggle = 0;

            foreach (Toggle toggle in classToggles)
            {
                toggle.image.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
                toggle.graphic.enabled = false;
                currentToggle++;

                if (currentToggle == classToggles.Count)
                {
                    break;
                }
            }

            // Disable Buttons
            foreach (Button cb in classButtons)
            {
                cb.interactable = false;
            }

            // Unselect Classes 
            int unselectClass = 1;
            foreach (Button cb in classButtons)
            {
                UnSelectCloudClass(unselectClass);
                unselectClass++;
            }

            // Load Instance Toggles and Selction Buttons
            LoadAllInstanceToggles();
            LoadAllInstanceSelectionButtons();

            // Set toggle instances to true and active
            foreach (Toggle iToggle in InstanceTogglesAll)
            {
                iToggle.isOn = true;
                iToggle.gameObject.SetActive(true);
            }

            StartCoroutine(ReloadClouds());
        }
        else if (instanceUIActive)
        {
            instanceUIActive = false;
            instanceUIImageAll.gameObject.SetActive(false);
            instanceUIButton.image.color = Color.white;
            colorModeDropDown.value = 0;

            // reenable class toggles
            int currentToggle = 0;

            foreach (Toggle toggle in classToggles)
            {
                toggle.image.color = new Color(1, 1, 1, 1);
                toggle.graphic.enabled = true;
                currentToggle++;

                if (currentToggle == classToggles.Count)
                {
                    break;
                }
            }

            // reenable class buttons
            foreach (Button cb in classButtons)
            {
                cb.interactable = true;
            }

            // Unselect Instances 
            int unselectInstances = 1;
            foreach (Button ib in AllInstancesButtons)
            {
                UnSelectCloudInstance(unselectInstances);
                unselectInstances++;
            }

            // Hide Instance Toggles
            foreach (Toggle iToggle in InstanceTogglesAll)
            {
                iToggle.gameObject.SetActive(false);
            }

            StartCoroutine(ReloadClouds());
        }
    }

    // Method For Showing Instance UI (class specific) And Toggling The Various Instances
    public void ShowClassInstanceUI(int cloudClass)
    {
        // loadingClassInstanceToggles = true;
        // loadingClassInstanceButtons = true;

        if (!classInstanceUIActive)
        {
            // mark selected class
            classSelected[cloudClass - 1] = true;
            classButtons[cloudClass - 1].image.color = new Color(0, 0, 1, 0.4f);

            // show class instance menu
            classInstanceUIActive = true;
            instanceUIImageClass.gameObject.SetActive(true);

            // Load Instance Toggles and Selection Buttons Based on Instances in Class  
            LoadClassInstanceToggles(cloudClass);
            ClassPriority();

            // loadingClassInstanceToggles = false;
            // loadingClassInstanceButtons = false;

            activeClassInstanceInMenu = cloudClass;

            // // blink effect 
            // if (clickedWithMouse == false)
            // {
            //     StartBlinking(cloudClass - 1, 5f);
            // }
        }
        else if (classInstanceUIActive)
        {
            classInstanceUIActive = false;
            instanceUIImageClass.gameObject.SetActive(false);

            DePrioritise();

            activeClassInstanceInMenu = 0;
        }

        StartCoroutine(ReloadCloudClass(cloudClass));
    }

    // Methods and variables for blink effect
    private Coroutine blinkRoutine;
    private bool isBlinking;
    public void StartBlinking(int cloudClass, float blinkDuration = 2f)
    {
        if (isBlinking) return;

        isBlinking = true;
        blinkRoutine = StartCoroutine(ClassBlinkRoutine(cloudClass, blinkDuration));
    }

    public void StopBlinking()
    {
        if (!isBlinking) return;

        isBlinking = false;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
        }

        blinkRoutine = null;
    }

    // Coroutine to make blink effect
    IEnumerator ClassBlinkRoutine(int cloudClass, float blinkDuration)
    {
        float interval = 0.5f; // 150 ms – won't crash Unity
        float time = 0f;

        while (time < blinkDuration)
        {
            for (int i = 0; i < PCClasses[cloudClass].cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = PCClasses[cloudClass].cloudClassGO.transform.GetChild(i).gameObject;
                if (instanceInClass.name.StartsWith($"Cloud:"))
                {
                    instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().prioritiseCloud = !PCClasses[cloudClass].getMeshConfigGO.GetComponent<DefaultMeshConfiguration>().prioritiseCloud;
                    instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                }
            }

            yield return new WaitForSeconds(interval);
            time += interval;
        }

        for (int i = 0; i < PCClasses[cloudClass].cloudClassGO.transform.childCount; i++)
        {
            GameObject instanceInClass = PCClasses[cloudClass].cloudClassGO.transform.GetChild(i).gameObject;
            if (instanceInClass.name.StartsWith($"Cloud:"))
            {
                instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().prioritiseCloud = true;
                instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
            }
        }

        isBlinking = false;
        blinkRoutine = null;
    }

    public void ResetSelection()
    {
        if (classInstanceUIActive)
        {
            classInstanceUIActive = false;
            instanceUIImageClass.gameObject.SetActive(false);

            DePrioritise();
        }
    }

    // Methods for loading the necessery toggles based on Classes or instances in directory
    private void LoadClassInstanceToggles(int cloudClass) // also loads buttons 
    {
        loadingClassInstanceToggles = true;

        int instancesInClass = 0;
        for (int i = 0; i < PCClasses[cloudClass - 1].cloudClassGO.transform.childCount; i++)
        {
            GameObject instanceInClass = PCClasses[cloudClass - 1].cloudClassGO.transform.GetChild(i).gameObject;
            if (instanceInClass.name.StartsWith("Cloud"))
            {
                instancesInClass++;
                //Debug.Log("Instance added. Current Instances: " + instanceInClass);
            }
        }

        //Debug.Log("Instances in Class: " + instancesInClass);
        availableInstancesInClass = instancesInClass;

        for (int i = 29; i > (instancesInClass - 1); i--)
        {
            //Debug.Log("Removing toggle " + i);
            InstanceTogglesClass[i].isOn = false;
            InstanceTogglesClass[i].graphic.enabled = false;
            InstanceTogglesClass[i].gameObject.SetActive(false);
            //Debug.Log("Removing button " + i);
            ClassInstancesButtons[i].gameObject.SetActive(false);
            ClassInstancesButtons[i].interactable = false;
        }

        int pciT = 0;

        // Set toggle instances to true and active
        foreach (Toggle iToggle in InstanceTogglesClass)
        {
            if (pciT < instancesInClass)
            {
                iToggle.isOn = true;
                iToggle.gameObject.SetActive(true);
                iToggle.graphic.enabled = true;
                iToggle.GetComponentInChildren<Text>().text = $"Class {cloudClass}; Instance: " + (pciT + 1);
                pciT++;
            }
            else
            {
                //iToggle.isOn = false;
                iToggle.gameObject.SetActive(false);
                iToggle.graphic.enabled = false;
                iToggle.GetComponentInChildren<Text>().text = "";
                pciT++;
            }

            loadingClassInstanceToggles = false;
        }

        int pciB = 0;

        // Set button instances to true and active
        foreach (Button iButton in ClassInstancesButtons)
        {
            if (pciB < instancesInClass)
            {
                iButton.gameObject.SetActive(true);
                iButton.interactable = true;
                pciB++;
            }
            else
            {
                iButton.gameObject.SetActive(false);
                iButton.interactable = false;
                pciB++;
            }
        }

    }
    private void LoadClassToggles()
    {
        // Set Available toggles based on class amount
        while (classToggles.Count > PCClasses.Count)
        {
            int beforeToggles = classToggles.Count;
            // Debug.Log("Before Removal: " + beforeToggles);
            classToggles[classToggles.Count - 1].isOn = false;
            classToggles[classToggles.Count - 1].gameObject.SetActive(false);
            classToggles.Remove(classToggles[classToggles.Count - 1]);
            int afterToggles = classToggles.Count;
            // Debug.Log("After Removal: " + afterToggles);

            if (beforeToggles == afterToggles) // Avoid infinite Loop
            {
                break;
            }
        }

        int availableToggles = 0;
        foreach (Toggle cToggle in classToggles)
        {
            if (cToggle.isOn == true)
            {
                classHidden[availableToggles] = false;
                availableToggles++;
            }
        }
        loadingClassToggles = false;
    }
    private void LoadAllInstanceToggles()
    {
        loadingAllInstanceToggles = true;

        // Set Available toggles based on instance amount
        while (InstanceTogglesAll.Count > PCInstances.Count)
        {
            int beforeToggles = InstanceTogglesAll.Count;
            // Debug.Log("Before Removal: " + beforeToggles);
            InstanceTogglesAll[InstanceTogglesAll.Count - 1].isOn = false;
            InstanceTogglesAll[InstanceTogglesAll.Count - 1].gameObject.SetActive(false);
            InstanceTogglesAll.Remove(InstanceTogglesAll[InstanceTogglesAll.Count - 1]);
            int afterToggles = InstanceTogglesAll.Count;
            // Debug.Log("After Removal: " + afterToggles);

            if (beforeToggles == afterToggles) // Avoid infinite Loop
            {
                break;
            }
        }

        int pci = 0;

        foreach (Toggle iToggle in InstanceTogglesAll)
        {
            iToggle.GetComponentInChildren<Text>().text = "Cloud Instance: " + (pci + 1);
            pci++;
        }
        loadingAllInstanceToggles = false;
    }

    // Methods For Loading Selection Buttons Showing Either Selected Class or Selected Instance
    private void LoadAllInstanceSelectionButtons() // Selected Instances
    {
        loadingInstanceButtons = true;
        // Set Available buttons based on instance amount
        while (AllInstancesButtons.Count > PCInstances.Count)
        {
            int beforeButtons = AllInstancesButtons.Count;
            // Debug.Log("Before Removal: " + beforeToggles);
            AllInstancesButtons[AllInstancesButtons.Count - 1].gameObject.SetActive(false);
            AllInstancesButtons.Remove(AllInstancesButtons[AllInstancesButtons.Count - 1]);
            int afterButtons = AllInstancesButtons.Count;
            // Debug.Log("After Removal: " + afterToggles);

            if (beforeButtons == afterButtons) // Avoid infinite Loop
            {
                break;
            }
        }
        loadingInstanceButtons = false;
    }
    private void LoadClassSelectionButtons() // Selected Buttons
    {
        // Set Available toggles based on class amount
        while (classButtons.Count > PCClasses.Count)
        {
            int beforeButtons = classButtons.Count;
            // Debug.Log("Before Removal: " + beforeToggles);
            classButtons[classButtons.Count - 1].gameObject.SetActive(false);
            classButtons.Remove(classButtons[classButtons.Count - 1]);
            int afterButtons = classButtons.Count;
            // Debug.Log("After Removal: " + afterToggles);

            if (beforeButtons == afterButtons) // Avoid infinite Loop
            {
                break;
            }
        }
    }

    // Methods to condense code for the cloud class selection method
    private void SelectCloudClass(int cloudClass)
    {
        GameObject PointCloudSelected = PCClasses[cloudClass - 1].cloudClassGO;
        for (int i = 0; i < PointCloudSelected.transform.childCount; i++)
        {
            GameObject instanceInClass = PointCloudSelected.transform.GetChild(i).gameObject;
            previousClassColor = instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode;
            instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
            instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Selected;
            instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
            instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
            classButtons[cloudClass - 1].image.color = new Color(0, 0, 1, 0.4f);
        }
    }
    private IEnumerator UnSelectCloudClass(int cloudClass)
    {
        GameObject PointCloudUnSelected = PCClasses[cloudClass - 1].cloudClassGO;
        for (int i = 0; i < PointCloudUnSelected.transform.childCount; i++)
        {
            GameObject instanceInClass = PointCloudUnSelected.transform.GetChild(i).gameObject;
            instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
            instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = previousClassColor;
            if (classToggles[cloudClass - 1].isOn == false)
            {
                classToggles[cloudClass - 1].isOn = true;
            }
            instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
            instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
            classButtons[cloudClass - 1].image.color = new Color(1, 1, 1, 0.4f);
        }
        yield return null;
    }

    // Method for UI buttons to show selected cloud Classes // DEPRECATED METHOD
    // public void cloudClassSelection(int cloudClass) // DEPRECATED METHOD - AVAILABLE IF WANTING TO MAKE CLOUD SELECTION PER CLASS USE THE RED COLOR FOR CLASS SELECTION
    // {
    //     if (!loadingClassButtons)
    //     {
    //         // Class 1
    //         if (classSelected[0] == false && cloudClass == 1)
    //         {
    //             SelectCloudClass(1);
    //             classSelected[0] = true;
    //         }
    //         else if (classSelected[0] == true && cloudClass == 1)
    //         {
    //             UnSelectCloudClass(1);
    //             classSelected[0] = false;
    //         }

    //         // Class 2
    //         if (classSelected[1] == false && cloudClass == 2)
    //         {
    //             SelectCloudClass(2);
    //             classSelected[1] = true;
    //         }
    //         else if (classSelected[1] == true && cloudClass == 2)
    //         {
    //             UnSelectCloudClass(2);
    //             classSelected[1] = false;
    //         }

    //         // Class 3
    //         if (classSelected[2] == false && cloudClass == 3)
    //         {
    //             SelectCloudClass(3);
    //             classSelected[2] = true;
    //         }
    //         else if (classSelected[2] == true && cloudClass == 3)
    //         {
    //             UnSelectCloudClass(3);
    //             classSelected[2] = false;
    //         }

    //         // Class 4
    //         if (classSelected[3] == false && cloudClass == 4)
    //         {
    //             SelectCloudClass(4);
    //             classSelected[3] = true;
    //         }
    //         else if (classSelected[3] == true && cloudClass == 4)
    //         {
    //             UnSelectCloudClass(4);
    //             classSelected[3] = false;
    //         }

    //         // Class 5
    //         if (classSelected[4] == false && cloudClass == 5)
    //         {
    //             SelectCloudClass(5);
    //             classSelected[4] = true;
    //         }
    //         else if (classSelected[4] == true && cloudClass == 5)
    //         {
    //             UnSelectCloudClass(5);
    //             classSelected[4] = false;
    //         }
    //     }
    // }

    // Methods to condense code for the cloud instance selection method
    private void SelectCloudInstance(int cloudInstance)
    {
        GameObject PointInstanceSelected = PCInstances[cloudInstance - 1];
        previousInstanceColor = PointInstanceSelected.GetComponentInChildren<DefaultMeshConfiguration>().colorMode;
        PointInstanceSelected.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
        PointInstanceSelected.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Selected;
        PointInstanceSelected.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
        AllInstancesButtons[cloudInstance - 1].image.color = new Color(0, 0, 1, 0.4f);
    }
    private void UnSelectCloudInstance(int cloudInstance)
    {
        GameObject PointInstanceUnSelected = PCInstances[cloudInstance - 1];
        PointInstanceUnSelected.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
        PointInstanceUnSelected.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = previousInstanceColor;
        if (InstanceTogglesAll[cloudInstance - 1].isOn == false)
        {
            InstanceTogglesAll[cloudInstance - 1].isOn = true;
        }
        PointInstanceUnSelected.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
        AllInstancesButtons[cloudInstance - 1].image.color = new Color(1, 1, 1, 0.4f);
    }

    // Method for Instance Menu Buttons to show selected cloud instance
    public void cloudInstanceSelection(int cloudInstance)
    {
        if (!loadingInstanceButtons)
        {
            // Instance 1
            if (instanceSelected[0] == false && cloudInstance == 1)
            {
                SelectCloudInstance(1);
                instanceSelected[0] = true;
            }
            else if (instanceSelected[0] == true && cloudInstance == 1)
            {
                UnSelectCloudInstance(1);
                instanceSelected[0] = false;
            }

            // Instance 2
            if (instanceSelected[1] == false && cloudInstance == 2)
            {
                SelectCloudInstance(2);
                instanceSelected[1] = true;
            }
            else if (instanceSelected[1] == true && cloudInstance == 2)
            {
                UnSelectCloudInstance(2);
                instanceSelected[1] = false;
            }

            // Instance 3
            if (instanceSelected[2] == false && cloudInstance == 3)
            {
                SelectCloudInstance(3);
                instanceSelected[2] = true;
            }
            else if (instanceSelected[2] == true && cloudInstance == 3)
            {
                UnSelectCloudInstance(3);
                instanceSelected[2] = false;
            }

            // Instance 4
            if (instanceSelected[3] == false && cloudInstance == 4)
            {
                SelectCloudInstance(4);
                instanceSelected[3] = true;
            }
            else if (instanceSelected[3] == true && cloudInstance == 4)
            {
                UnSelectCloudInstance(4);
                instanceSelected[3] = false;
            }

            // Instance 5
            if (instanceSelected[4] == false && cloudInstance == 5)
            {
                SelectCloudInstance(5);
                instanceSelected[4] = true;
            }
            else if (instanceSelected[4] == true && cloudInstance == 5)
            {
                UnSelectCloudInstance(5);
                instanceSelected[4] = false;
            }

            // Instance 6
            if (instanceSelected[5] == false && cloudInstance == 6)
            {
                SelectCloudInstance(6);
                instanceSelected[5] = true;
            }
            else if (instanceSelected[5] == true && cloudInstance == 6)
            {
                UnSelectCloudInstance(6);
                instanceSelected[5] = false;
            }

            // Instance 7
            if (instanceSelected[6] == false && cloudInstance == 7)
            {
                SelectCloudInstance(7);
                instanceSelected[6] = true;
            }
            else if (instanceSelected[6] == true && cloudInstance == 7)
            {
                UnSelectCloudInstance(7);
                instanceSelected[6] = false;
            }

            // Instance 8
            if (instanceSelected[7] == false && cloudInstance == 8)
            {
                SelectCloudInstance(8);
                instanceSelected[7] = true;
            }
            else if (instanceSelected[7] == true && cloudInstance == 8)
            {
                UnSelectCloudInstance(8);
                instanceSelected[7] = false;
            }

            // Instance 9
            if (instanceSelected[8] == false && cloudInstance == 9)
            {
                SelectCloudInstance(9);
                instanceSelected[8] = true;
            }
            else if (instanceSelected[8] == true && cloudInstance == 9)
            {
                UnSelectCloudInstance(9);
                instanceSelected[8] = false;
            }
        }
    }

    // Methods to condense code for the cloud class instance selection method
    public void SelectCloudClassInstance(int cloudClass, int cloudInstance)
    {
        classInstanceSelected[cloudInstance - 1] = true;
        ClassInstancesButtons[cloudInstance - 1].image.color = new Color(0, 0, 1, 0.4f);

        for (int i = 0; i < PCClasses[cloudClass - 1].cloudClassGO.transform.childCount; i++)
        {
            GameObject instanceInClass = PCClasses[cloudClass - 1].cloudClassGO.transform.GetChild(i).gameObject;
            if (instanceInClass.name.StartsWith($"Cloud: {cloudInstance}"))
            {
                previousClassInstanceColor = instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode;
                instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Selected;
                instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                //Debug.Log($"Instance {cloudInstance} in Class {cloudClass} is selected");
                return;
            }
        }
    }
    public void UnSelectCloudClassInstance(int cloudClass, int cloudInstance)
    {
        classInstanceSelected[cloudInstance - 1] = false;
        ClassInstancesButtons[cloudInstance - 1].image.color = new Color(1, 1, 1, 0.4f);
        for (int i = 0; i < PCClasses[cloudClass - 1].cloudClassGO.transform.childCount; i++)
        {
            GameObject instanceInClass = PCClasses[cloudClass - 1].cloudClassGO.transform.GetChild(i).gameObject;
            if (instanceInClass.name.StartsWith($"Cloud: {cloudInstance}"))
            {
                instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = previousClassInstanceColor;
                if (InstanceTogglesClass[cloudInstance - 1].isOn == false)
                {
                    InstanceTogglesClass[cloudInstance - 1].isOn = true;
                }
                instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
            }
        }
    }

    // Method for Instance Menu Buttons to show selected cloud instance
    public void cloudClassInstanceSelection(int instanceInMenu)
    {
        int cloudClass = 0;

        // Check which class is selected and store as cloudClass
        foreach (bool cc in classSelected)
        {
            if (cc == true)
            {
                //activeClasses.Add(cloudClass); // if multiple can be active
                cloudClass++;
                break; // if only the first should be accounted for
            }
            cloudClass++;
        }

        // Get Instance Count For Class
        // int instancesInClass = 0;
        // for (int i = 0; i < PCClasses[cloudClass - 1].cloudClassGO.transform.childCount; i++)
        // {
        //     GameObject instanceInClass = PCClasses[cloudClass - 1].cloudClassGO.transform.GetChild(i).gameObject;
        //     if (instanceInClass.name.StartsWith("Cloud"))
        //     {
        //         instancesInClass++;
        //     }

        // }
        // Debug.Log("Instances i Class: " + instancesInClass);

        // // Select Cloud Class Instance
        // for (int i = 0; i < instancesInClass; i++)
        // {
        switch (instanceInMenu)
        {
            case 1:
                if (!classInstanceSelected[instanceInMenu - 1])
                {
                    SelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
                else
                {
                    UnSelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
            case 2:
                if (!classInstanceSelected[instanceInMenu - 1])
                {
                    SelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
                else
                {
                    UnSelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
            case 3:
                if (!classInstanceSelected[instanceInMenu - 1])
                {
                    SelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
                else
                {
                    UnSelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
            case 4:
                if (!classInstanceSelected[instanceInMenu - 1])
                {
                    SelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
                else
                {
                    UnSelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
            case 5:
                if (!classInstanceSelected[instanceInMenu - 1])
                {
                    SelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
                else
                {
                    UnSelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
            case 6:
                if (!classInstanceSelected[instanceInMenu - 1])
                {
                    SelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
                else
                {
                    UnSelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
            case 7:
                if (!classInstanceSelected[instanceInMenu - 1])
                {
                    SelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
                else
                {
                    UnSelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
            case 8:
                if (!classInstanceSelected[instanceInMenu - 1])
                {
                    SelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
                else
                {
                    UnSelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
            case 9:
                if (!classInstanceSelected[instanceInMenu - 1])
                {
                    SelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
                else
                {
                    UnSelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
            case 10:
                if (!classInstanceSelected[instanceInMenu - 1])
                {
                    SelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
                else
                {
                    UnSelectCloudClassInstance(cloudClass, instanceInMenu);
                    break;
                }
            default:
                Debug.Log("Above Switch Case Instance Limit. Add more to switch case since you have {instancesInClass} instances");
                break;
                //}
        }
    }

    // Methods to condense code for the cloud class instance Mouse selection method
    public IEnumerator MouseSelectCloudClassInstance(int cloudClass, int cloudInstance)
    {
        classInstanceSelected[cloudInstance - 1] = true;
        ClassInstancesButtons[cloudInstance - 1].image.color = new Color(0, 0, 1, 0.4f);

        for (int i = 0; i < PCClasses[cloudClass - 1].cloudClassGO.transform.childCount; i++)
        {
            GameObject instanceInClass = PCClasses[cloudClass - 1].cloudClassGO.transform.GetChild(i).gameObject;
            if (instanceInClass.name.StartsWith($"Cloud: {cloudInstance}"))
            {
                previousClassInstanceColor = instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode;
                instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().StopRendering();
                instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Selected;
                instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
                instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                //Debug.Log($"Instance {cloudInstance} in Class {cloudClass} is selected");
            }
        }
        yield return null;
    }
    public IEnumerator MouseUnSelectCloudClassInstance(int cloudClass, int cloudInstance)
    {
        classInstanceSelected[cloudInstance - 1] = false;
        ClassInstancesButtons[cloudInstance - 1].image.color = new Color(1, 1, 1, 0.4f);
        for (int i = 0; i < PCClasses[cloudClass - 1].cloudClassGO.transform.childCount; i++)
        {
            GameObject instanceInClass = PCClasses[cloudClass - 1].cloudClassGO.transform.GetChild(i).gameObject;
            if (instanceInClass.name.StartsWith($"Cloud: {cloudInstance}"))
            {
                instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().StopRendering();
                instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = previousClassInstanceColor;
                if (InstanceTogglesClass[cloudInstance - 1].isOn == false)
                {
                    InstanceTogglesClass[cloudInstance - 1].isOn = true;
                }
                instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
                instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
            }
        }
        yield return null;
    }

    // Method to show Selected cloud instance when cloud selected with mouse
    public void cloudClassInstanceMouseSelection(int cloudClass, int instanceInMenu)
    {
        switch (instanceInMenu)
        {
            case 1:
                if (!classInstanceSelected[instanceInMenu - 1] && instanceInMenu <= availableInstancesInClass)
                {
                    StartCoroutine(MouseSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
                else
                {
                    StartCoroutine(MouseUnSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
            case 2:
                if (!classInstanceSelected[instanceInMenu - 1] && instanceInMenu <= availableInstancesInClass)
                {
                    StartCoroutine(MouseSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
                else
                {
                    StartCoroutine(MouseUnSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
            case 3:
                if (!classInstanceSelected[instanceInMenu - 1] && instanceInMenu <= availableInstancesInClass)
                {
                    StartCoroutine(MouseSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
                else
                {
                    StartCoroutine(MouseUnSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
            case 4:
                if (!classInstanceSelected[instanceInMenu - 1] && instanceInMenu <= availableInstancesInClass)
                {
                    StartCoroutine(MouseSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
                else
                {
                    StartCoroutine(MouseUnSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
            case 5:
                if (!classInstanceSelected[instanceInMenu - 1] && instanceInMenu <= availableInstancesInClass)
                {
                    StartCoroutine(MouseSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
                else
                {
                    StartCoroutine(MouseUnSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
            case 6:
                if (!classInstanceSelected[instanceInMenu - 1] && instanceInMenu <= availableInstancesInClass)
                {
                    StartCoroutine(MouseSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
                else
                {
                    StartCoroutine(MouseUnSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
            case 7:
                if (!classInstanceSelected[instanceInMenu - 1] && instanceInMenu <= availableInstancesInClass)
                {
                    StartCoroutine(MouseSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
                else
                {
                    StartCoroutine(MouseUnSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
            case 8:
                if (!classInstanceSelected[instanceInMenu - 1] && instanceInMenu <= availableInstancesInClass)
                {
                    StartCoroutine(MouseSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
                else
                {
                    StartCoroutine(MouseUnSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
            case 9:
                if (!classInstanceSelected[instanceInMenu - 1] && instanceInMenu <= availableInstancesInClass)
                {
                    StartCoroutine(MouseSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
                else
                {
                    StartCoroutine(MouseUnSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
            case 10:
                if (!classInstanceSelected[instanceInMenu - 1] && instanceInMenu <= availableInstancesInClass)
                {
                    StartCoroutine(MouseSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
                else
                {
                    StartCoroutine(MouseUnSelectCloudClassInstance(cloudClass, instanceInMenu));
                    break;
                }
            default:
                Debug.Log("Above Switch Case Instance Limit. Add more to switch case since you have {instancesInClass} instances");
                break;
                //}
        }
    }

    // Method to deselect classes when Color Mode is changed
    public void UnSelectOnConversion()
    {
        if (!instanceUIActive)
        {
            for (int i = 1; i <= PCClasses.Count; i++)
            {
                StartCoroutine(UnSelectCloudClass(i));
            }
        }

        // else if (instanceUIActive)
        // {
        //     for (int i = 1; i < PCInstances.Count; i++)
        //     {
        //         UnSelectCloudInstance(i);
        //     }
        // }
    }

    // Method to set point size based on selected class 
    // (SHOULD BE UPDATED TO CHANGE OTHER ASPECTS INSTEAD OF POINT SIZE)
    private void ClassPriority()
    {
        int currentClassInHierarchy = 0;

        foreach (bool selected in classSelected)
        {
            for (int i = 0; i < PCClasses[currentClassInHierarchy].cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = PCClasses[currentClassInHierarchy].cloudClassGO.transform.GetChild(i).gameObject;
                if (!instanceInClass.name.StartsWith("Cloud"))
                {
                    continue; // Move to next iteration of children, if instance is not a cloud instance GO
                }
                if (selected == false) // if cloud class has not been selected
                {
                    // Make Button not interactable
                    classButtons[currentClassInHierarchy].interactable = false;
                    classButtons[currentClassInHierarchy].gameObject.SetActive(false);
                    // Reduce alpha channel
                    // instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().prioritiseCloud = false;
                    // Save color
                    previousClassPriorityColor = instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode;
                    // Do not Mark class 
                    //StartCoroutine(StopLoadingClouds());
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().prioritiseCloud = false;
                    //StartCoroutine(StartLoadingClouds());
                    // Reload Mesh
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                }
                else if (selected == true) // if cloud class has been selected
                {
                    // Make Button interactable
                    classButtons[currentClassInHierarchy].interactable = true;
                    classButtons[currentClassInHierarchy].gameObject.SetActive(true);
                    // // Keep point size
                    // instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius = 1f;
                    // Save color 
                    previousClassPriorityColor = instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode;
                    // Mark class 
                    //StartCoroutine(StopLoadingClouds());
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().prioritiseCloud = true;
                    //StartCoroutine(StartLoadingClouds());
                    // Reload Mesh
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;

                }
            }

            currentClassInHierarchy++;
            if (currentClassInHierarchy == PCClasses.Count)
            {
                return;
            }
        }
    }
    private void DePrioritise() // Reset priority of classes so all are visualised as normally done
    {
        // Reset Cloud Visuals
        int currentInstanceInClass = 0;

        for (int currentClassInHierarchy = 0; currentClassInHierarchy < PCClasses.Count;)
        {
            for (int i = 0; i < PCClasses[currentClassInHierarchy].cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = PCClasses[currentClassInHierarchy].cloudClassGO.transform.GetChild(i).gameObject;
                if (instanceInClass.name.StartsWith("Cloud"))
                {
                    // Make Class Instance Buttons not select current Instance
                    classInstanceSelected[currentInstanceInClass] = false;
                    ClassInstancesButtons[currentInstanceInClass].image.color = new Color(1, 1, 1, 0.4f);
                    // Increment for exploring next instance
                    currentInstanceInClass++;
                    // Set Class Buttons back to default
                    classSelected[currentClassInHierarchy] = false;
                    classButtons[currentClassInHierarchy].image.color = new Color(1, 1, 1, 0.4f);
                    // Make All Instances in Class Reset back
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().prioritiseCloud = false;
                    // instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius = 1f;
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = previousClassPriorityColor;
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                    // Make All Class Buttons interactable again
                    classButtons[currentClassInHierarchy].interactable = true;
                    classButtons[currentClassInHierarchy].gameObject.SetActive(true);
                }
            }

            currentClassInHierarchy++;

            if (currentClassInHierarchy == PCClasses.Count)
            {
                return;
            }
        }

        // Hide Instance Toggles
        foreach (Toggle iToggle in InstanceTogglesClass)
        {
            iToggle.gameObject.SetActive(false);
        }
    }

    // Show the LLM window 
    public void ShowLLMInteract()
    {
        if (LLMMenuActive == false)
        {
            LLMPanelsUI.SetActive(true);

            switch (lLMTestModes)
            {
                case LLMTestModes.Chat:
                    ChatBot.SetActive(true);
                    FunctionCalling.SetActive(false);
                    PointCloudAssistant.SetActive(false);
                    break;
                case LLMTestModes.Function:
                    FunctionCalling.SetActive(true);
                    ChatBot.SetActive(false);
                    PointCloudAssistant.SetActive(false);
                    break;
                case LLMTestModes.None:
                    PointCloudAssistant.SetActive(true);
                    ChatBot.SetActive(false);
                    FunctionCalling.SetActive(false);
                    break;
            }

            LLMMenuActive = true;
        }
        else if (LLMMenuActive == true)
        {
            ChatBot.SetActive(false);
            FunctionCalling.SetActive(false);
            LLMPanelsUI.SetActive(false);
            LLMMenuActive = false;
        }
    }

    // Method for Reloading Point Clouds, in case some have not loaded properly
    public IEnumerator ReloadClouds()
    {
        if (instanceUIActive)
        {
            foreach (GameObject pci in PCInstances)
            {
                // Remove Clouds
                pci.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                // ShutDown V2 Renderer
                pci.GetComponentInChildren<DynamicPointCloudSet>().StopRendering();
                // Disable DynamicPointCloudSet Component
                //pci.SetActive(false);

                // Enable DynamicPointCloudSet Component
                //pci.SetActive(true);
                // Enable Clouds
                pci.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                // Show V2 Renderer
                pci.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
            }
        }

        else if (!instanceUIActive)
        {
            for (int currentClass = 0; currentClass < PCClasses.Count; currentClass++)
            {
                DirectoryInstanceLoader.PCInstances cloud = PCClasses[currentClass];
                for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
                {
                    GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;

                    if (instanceInClass.name.StartsWith("Cloud:") && classHidden[currentClass] == false)
                    {
                        // Remove Clouds
                        instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                        // ShutDown V2 Renderer
                        instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().StopRendering();
                        // Disable DynamicPointCloudSet Component
                        //instanceInClass.SetActive(false);

                        // Enable DynamicPointCloudSet Component
                        //instanceInClass.SetActive(true);
                        // Enable Clouds
                        instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                        // Show V2 Renderer
                        instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
                    }
                }
            }
            yield return null;
        }
    }

    // Method for Reloading Point Clouds, in case some have not loaded properly
    public IEnumerator ReloadCloudClass(int cloudClass)
    {
        DirectoryInstanceLoader.PCInstances cloudClassLoaderGO = PCClasses[cloudClass-1];
        for (int i = 0; i < cloudClassLoaderGO.cloudClassGO.transform.childCount; i++)
        {
            GameObject instanceInClass = cloudClassLoaderGO.cloudClassGO.transform.GetChild(i).gameObject;

            if (instanceInClass.name.StartsWith("Cloud:") && classHidden[cloudClass-1] == false)
            {
                // Remove Clouds
                instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                // ShutDown V2 Renderer
                instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().StopRendering();
                // // Disable DynamicPointCloudSet Component
                // instanceInClass.SetActive(false);

                // // Enable DynamicPointCloudSet Component
                // instanceInClass.SetActive(true);
                // Enable Clouds
                instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                // Show V2 Renderer
                instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
            }
        }
        yield return null;
    }

    // Coroutine to stop loading point clouds 
    IEnumerator StopLoadingClouds()
    {
        for (int currentClass = 0; currentClass < PCClasses.Count; currentClass++)
        {
            DirectoryInstanceLoader.PCInstances cloud = PCClasses[currentClass];
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;

                if (instanceInClass.name.StartsWith("Cloud:") && classHidden[currentClass] == false)
                {
                    // Remove Clouds
                    instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                    // ShutDown V2 Renderer
                    instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().StopRendering();
                }
            }
        }
        yield return new WaitForSeconds(2);;
    }

    // Coroutine to start loading point clouds 
    IEnumerator StartLoadingClouds()
    {
        for (int currentClass = 0; currentClass < PCClasses.Count; currentClass++)
        {
            DirectoryInstanceLoader.PCInstances cloud = PCClasses[currentClass];
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;

                if (instanceInClass.name.StartsWith("Cloud:") && classHidden[currentClass] == false)
                {
                    // Enable DynamicPointCloudSet Component
                    // Enable Clouds
                    instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                    // Show V2 Renderer
                    instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
                }
            }
        }
        yield return new WaitForSeconds(2);
    }

}
