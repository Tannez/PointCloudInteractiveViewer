using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.Edl;
using BAPointCloudRenderer.Loading;
using BAPointCloudRenderer.ObjectCreation;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIInstanceController : MonoBehaviour
{
    //Cam Access
    Camera cam;

    //Reference to pointcloud gameobject and its script
    DirectoryInstanceLoader directoryInstanceLoader;
    [SerializeField] public List<DirectoryInstanceLoader.PCInstances> PCClasses = new List<DirectoryInstanceLoader.PCInstances>(); 
    [SerializeField] public List<GameObject> PCInstances = new List<GameObject>(); 

    [Tooltip("Before running scene, a cloud instantiator is required in hierarchy. Create empty GameObject and add a Cloud Instantiator script to it. Then insert the cloud instantiator here. Remember to set asset path and prefabs in cloud instantiator script")]
    [SerializeField] CloudInstanceInstantiatior cloudInstantiator;
    [Tooltip("Before running scene, an initial cloud loader is required in hierarchy. Create empty GameObject beneath Cloud Instantiator and add a Dynamic Loader to it. Then insert this cloud loader here.")]
    [SerializeField] GameObject InitialCloudLoader;
    [SerializeField] private GameObject clippingPlane;


    // // Mesh Configurations
    // [SerializeField] private PointMeshConfiguration pointMeshConfiguration;
    [Tooltip("Adds Mesh To points. Insert GameObject Beneath the cloud instantiator, and add the DefaultMeshConfiguration script. Then insert GameObject here.")]
    [SerializeField] private DefaultMeshConfiguration defaultMeshConfiguration;

    [Header("Point Budget")]
    [SerializeField] public uint PCPointBudget;
    [SerializeField] public float EDLRadiusUI { get { return edlCamera._edlRadius; } set { edlCamera._edlRadius = EDLRadiusSlider.value; } }
    [SerializeField] public float EDLExpScaleUI { get { return edlCamera._edlExpScale; } set { edlCamera._edlExpScale = EDLExpScaleSlider.value; } }
    [SerializeField] public float EDLScaleUI { get { return edlCamera._edlScale; } set { edlCamera._edlScale = EDLScaleSlider.value; } }


    //UI Inspector Components
    [Header("Point Budget Slider")]
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

    [Header("Classification Toggles")]
    private bool loadingClassToggles = true;
    [SerializeField] private List<Toggle> classToggles = new List<Toggle>();

    [Header("Instance Toggles")]
    private bool loadingInstanceToggles = true;
    [SerializeField] private List<Toggle> InstanceToggles = new List<Toggle>();

    [Header("Instance UI")]
    [SerializeField] private Image instanceUIImage;
    [SerializeField] private Button instanceUIButton;
    private bool instanceUIActive = false;

    [Header("Color Mode Dropdown")]
    [SerializeField] private TMP_Dropdown colorModeDropDown;

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
    public bool keyboardClassSelection = true;
    [SerializeField] List<Button> classButtons = new List<Button>();
    private bool loadingClassButtons = true;
    BAPointCloudRenderer.ObjectCreation.ColorMode previousColor;
    private bool class1Selected = false;
    private bool class2Selected = false;
    private bool class3Selected = false;
    private bool class4Selected = false;
    private bool class5Selected = false;

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

        Debug.Log("Point Clouds available in UI: " + PCClasses.Count);

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

        // Create Toggle Listeners
        EDLToggle.onValueChanged.AddListener(delegate { EDLToggleChange(); });

        // Create Drop Down Listener
        colorModeDropDown.onValueChanged.AddListener(delegate { DropdownColorModeChange(); });

        // Set Available toggles based on class amount
        LoadClassToggles();
        LoadClassSelectionButtons();
    }

    void Update()
    {
        // Use keyboard numbers to select classes
        if (keyboardClassSelection)
        {
            KeyboardCloudSelection();
        }

        // Use keyboard to reload clouds in case they are not loading properly
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadClouds();
        }
    }

    // Method For Changing Point Budget Of Point Clouds
    public void PCValueChangeCheck()
    {
        // Remove Clouds
        foreach (GameObject pci in PCInstances)
        {
            // Remove Clouds
            pci.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
            // ShutDown V2 Renderer
            pci.GetComponentInChildren<DynamicPointCloudSet>().PointRenderer.ShutDown();  
            // Disable DynamicPointCloudSet Component
            pci.SetActive(false);
        }

        // Change Value Of Point Budget
        PCPointBudget = (uint)pointBudgetSlider.value;


        foreach (GameObject pci in PCInstances)
        {
            pci.GetComponentInChildren<DynamicPointCloudSet>().pointBudget = PCPointBudget;
        }

        pointBudgetSliderText.text = PCPointBudget.ToString();

        //Enable Clouds
        foreach (GameObject pci in PCInstances)
        {
            // Enable DynamicPointCloudSet Component
            pci.SetActive(true);
            // Enable Clouds
            pci.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
            // Show V2 Renderer
            pci.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
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
        Debug.Log("SkyBox Button Pressed!");
    }
    public void BlackButton()
    {
        clippingPlane.SetActive(true);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        Debug.Log("Black Button Pressed!");
    }
    public void WhiteButton()
    {
        clippingPlane.SetActive(true);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.white;
        Debug.Log("White Button Pressed!");
    }

    // Methods For Loading And Removing Point Clouds
    public void HidePCClass(int CloudToHide)
    {
        GameObject PointCloudHidden = PCClasses[CloudToHide].cloudClassGO;
        for (int i = 0; i < PointCloudHidden.transform.childCount; i++)
        {
            GameObject instanceInClass = PointCloudHidden.transform.GetChild(i).gameObject;
            instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
        }
        Debug.Log($"Cloud: {CloudToHide} is hidden");
    }
    public void ShowPCClass(int CloudToShow)
    {
        GameObject PointCloudLoaded = PCClasses[CloudToShow].cloudClassGO;
        for (int i = 0; i < PointCloudLoaded.transform.childCount; i++)
        {
            GameObject instanceInClass = PointCloudLoaded.transform.GetChild(i).gameObject;
            instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
        }
        Debug.Log($"Cloud: {CloudToShow} is shown");
    }
    public void HidePCInstance(int CloudToHide)
    {
        GameObject PointCloudHidden = PCInstances[CloudToHide];
        PointCloudHidden.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
        Debug.Log($"Cloud: {CloudToHide} is hidden");
    }
    public void ShowPCInstance(int CloudToShow)
    {
        GameObject PointCloudLoaded = PCInstances[CloudToShow];
        PointCloudLoaded.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
        Debug.Log($"Cloud: {CloudToShow} is shown");
    }

    // Methods For Changing Color Mode Of Point Clouds.
    // NOTE: Ensure that Clouds are Reloaded after conversion
    public void ClassificationConversion()
    {
        // pointMeshConfiguration.material = new Material(Shader.Find("CustomRenderTexture/Classification"));
        // Debug.Log("Showing Classification");

        foreach (GameObject pci in PCInstances)
        {
            pci.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Classification;
        }
        // defaultMeshConfiguration.colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Classification;
        Debug.Log("Showing Classification");
    }
    public void RGBConversion()
    {
        // pointMeshConfiguration.material = new Material(Shader.Find("Custom/PointShader"));
        // Debug.Log("Showing RGB");

        foreach (GameObject pci in PCInstances)
        {
            pci.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.RGBA;
        }

        // defaultMeshConfiguration.colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.RGBA;
        Debug.Log("Showing RGBA");
    }
    public void IntensityConversion()
    {
        // pointMeshConfiguration.material = new Material(Shader.Find("Custom/PointShader"));
        // Debug.Log("Showing Intensity");

        foreach (GameObject pci in PCInstances)
        {
            pci.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Intensity;
        }

        // defaultMeshConfiguration.colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Intensity;
        Debug.Log("Showing Intensity");
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
        if (defaultMeshConfiguration.pointRadius < 5.0f)
        {
            defaultMeshConfiguration.pointRadius += 0.25f;
            defaultMeshConfiguration.reload = true;
            // Debug.Log("Point Radius Increased");
        }
    }
    public void PointSizeDown()
    {
        if (defaultMeshConfiguration.pointRadius > 0.25f)
        {
            defaultMeshConfiguration.pointRadius -= 0.25f;
            defaultMeshConfiguration.reload = true;
            // Debug.Log("Point Radius Decreased");
        }
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
            // Remove Clouds
            foreach (GameObject pci in PCInstances)
            {
                // Remove Clouds
                pci.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                // ShutDown V2 Renderer
                pci.GetComponentInChildren<DynamicPointCloudSet>().PointRenderer.ShutDown();
                // Disable DynamicPointCloudSet Component
                pci.SetActive(false);
            }

            // Change ColorMode
            RGBConversion();

            //Enable Clouds
            foreach (GameObject pci in PCInstances)
            {
                // Enable DynamicPointCloudSet Component
                pci.SetActive(true);
                // Enable Clouds
                pci.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                // Show V2 Renderer
                pci.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
            }
        }

        else if (colorModeDropDown.value == 1)
        {
            //  <- New !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // Remove Clouds
            foreach (GameObject pci in PCInstances)
            {
                // Remove Clouds
                pci.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                // ShutDown V2 Renderer
                pci.GetComponentInChildren<DynamicPointCloudSet>().PointRenderer.ShutDown();
                // Disable DynamicPointCloudSet Component
                pci.SetActive(false);
            }

            // Change ColorMode
            ClassificationConversion();

            //Enable Clouds
            foreach (GameObject pci in PCInstances)
            {
                // Enable DynamicPointCloudSet Component
                pci.SetActive(true);
                // Enable Clouds
                pci.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                // Show V2 Renderer
                pci.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
            }
        }
        else if (colorModeDropDown.value == 2)
        {
            //  <- New !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // Remove Clouds
            foreach (GameObject pci in PCInstances)
            {
                // Remove Clouds
                pci.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                // ShutDown V2 Renderer
                pci.GetComponentInChildren<DynamicPointCloudSet>().PointRenderer.ShutDown();
                // Disable DynamicPointCloudSet Component
                pci.SetActive(false);
            }

            // Change ColorMode
            IntensityConversion();

            //Enable Clouds
            foreach (GameObject pci in PCInstances)
            {
                // Enable DynamicPointCloudSet Component
                pci.SetActive(true);
                // Enable Clouds
                pci.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                // Show V2 Renderer
                pci.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
            }
        }
    }
    
    // Method For Changing Color Mode when selecting cloud with keyboard input
    public void KeyboardCloudSelection()
    {
        // When cloud selected 
        // get cloud instance
        // remove instance
        // get mesh config
        // set color mode to selected
        // reload cloud 

        // hard coded to keyboard buttons:
        if (Input.GetKeyDown(KeyCode.Alpha1) && class1Selected == false)
        {
            SelectCloudClass(1);
            class1Selected = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && class1Selected == true)
        {
            UnSelectCloudClass(1);
            class1Selected = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && class2Selected == false)
        {
            SelectCloudClass(2);
            class2Selected = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && class2Selected == true)
        {
            UnSelectCloudClass(2);
            class2Selected = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && class3Selected == false)
        {
            SelectCloudClass(3);
            class3Selected = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && class3Selected == true)
        {
            UnSelectCloudClass(3);
            class3Selected = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && class4Selected == false)
        {
            SelectCloudClass(4);
            class4Selected = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && class4Selected == true)
        {
            UnSelectCloudClass(4);
            class4Selected = false;
        }  
        
        if (Input.GetKeyDown(KeyCode.Alpha5) && class5Selected == false)
        {
            SelectCloudClass(5);
            class5Selected = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && class5Selected == true)
        {
            UnSelectCloudClass(5);
            class5Selected = false;
        } 
    }

    // Method For Toggling The Various Classes
    public void classToggleChange()
    {
        if (!loadingClassToggles)
        {
            int currentToggle = 0;

            foreach (Toggle toggle in classToggles)
            {
                if (toggle.isOn)
                {
                    ShowPCClass(currentToggle);
                    Debug.Log("Cloud: " + currentToggle + " Toggle is on");
                }
                else if (!toggle.isOn)
                {
                    HidePCClass(currentToggle);
                    Debug.Log("Cloud: " + currentToggle + " Toggle is off");
                }
                currentToggle++;

                if (currentToggle == classToggles.Count)
                {
                    break;
                }
            }
        }
    }

    // Method For Toggling The Various Classes
    public void InstanceToggleChange()
    {
        if (!loadingInstanceToggles)
        {
            int currentToggle = 0;

            foreach (Toggle toggle in InstanceToggles)
            {
                if (toggle.isOn)
                {
                    ShowPCInstance(currentToggle);
                    Debug.Log("Cloud: " + currentToggle + " Toggle is on");
                }
                else if (!toggle.isOn)
                {
                    HidePCInstance(currentToggle);
                    Debug.Log("Cloud: " + currentToggle + " Toggle is off");
                }
                currentToggle++;

                if (currentToggle == InstanceToggles.Count)
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
            instanceUIActive = true;
            instanceUIImage.gameObject.SetActive(true);
            instanceUIButton.image.color = Color.gray;

            LoadAllInstanceToggles();

            foreach (Toggle iToggle in InstanceToggles)
            {
                iToggle.isOn = true;
                iToggle.gameObject.SetActive(true);
            }
        }
        else if (instanceUIActive)
        {
            instanceUIActive = false;
            instanceUIImage.gameObject.SetActive(false);
            instanceUIButton.image.color = Color.white;

            foreach (Toggle iToggle in InstanceToggles)
            {
                iToggle.gameObject.SetActive(false);
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
        loadingClassToggles = false;
    }
    private void LoadAllInstanceToggles()
    {
        // Set Available toggles based on instance amount
        while (InstanceToggles.Count > PCInstances.Count)
        {
            int beforeToggles = InstanceToggles.Count;
            // Debug.Log("Before Removal: " + beforeToggles);
            InstanceToggles[InstanceToggles.Count - 1].isOn = false;
            InstanceToggles[InstanceToggles.Count - 1].gameObject.SetActive(false);
            InstanceToggles.Remove(InstanceToggles[InstanceToggles.Count - 1]);
            int afterToggles = InstanceToggles.Count;
            // Debug.Log("After Removal: " + afterToggles);

            if (beforeToggles == afterToggles) // Avoid infinite Loop
            {
                break;
            }
        }

        int pci = 0;

        foreach (Toggle iToggle in InstanceToggles)
        {
            iToggle.GetComponentInChildren<Text>().text = "Cloud Instance: " + (pci + 1);
            pci++;
        }
        loadingInstanceToggles = false;
    }

    // Method For Loading Selection Buttons Showing Selected Class
    private void LoadClassSelectionButtons()
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
        loadingClassButtons = false;
    }

    // Methods to condense code for the cloud selection method
    private void SelectCloudClass(int cloudClass)
    {
        GameObject PointCloudSelected = PCClasses[cloudClass - 1].cloudClassGO;
        for (int i = 0; i < PointCloudSelected.transform.childCount; i++)
        {
            GameObject instanceInClass = PointCloudSelected.transform.GetChild(i).gameObject;
            previousColor = instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode;
            instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
            instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Selected;
            instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
            classButtons[cloudClass - 1].image.color = new Color(0, 0, 1, 0.4f);
        }
    }
    private void UnSelectCloudClass(int cloudClass)
    {
        GameObject PointCloudUnSelected = PCClasses[cloudClass - 1].cloudClassGO;
        for (int i = 0; i < PointCloudUnSelected.transform.childCount; i++)
        {
            GameObject instanceInClass = PointCloudUnSelected.transform.GetChild(i).gameObject;
            instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
            instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode = previousColor;
            instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
            classButtons[cloudClass - 1].image.color = new Color(1, 1, 1, 0.4f);
        }
    }

    // method for UI buttons to show selected clouds
    public void cloudSelection(int cloudClass)
    {
        if (!loadingClassButtons)
        {
            // Class 1
            if (class1Selected == false && cloudClass == 1)
            {
                SelectCloudClass(1);
                class1Selected = true;
            }
            else if (class1Selected == true && cloudClass == 1)
            {
                UnSelectCloudClass(1);
                class1Selected = false;
            }

            // Class 2
            if (class2Selected == false && cloudClass == 2)
            {
                SelectCloudClass(2);
                class2Selected = true;
            }
            else if (class2Selected == true && cloudClass == 2)
            {
                UnSelectCloudClass(2);
                class2Selected = false;
            }

            // Class 3
            if (class3Selected == false && cloudClass == 3)
            {
                SelectCloudClass(3);
                class3Selected = true;
            }
            else if (class3Selected == true && cloudClass == 3)
            {
                UnSelectCloudClass(3);
                class3Selected = false;
            }

            // Class 4
            if (class4Selected == false && cloudClass == 4)
            {
                SelectCloudClass(4);
                class4Selected = true;
            }
            else if (class4Selected == true && cloudClass == 4)
            {
                UnSelectCloudClass(4);
                class4Selected = false;
            }

            // Class 5
            if (class5Selected == false && cloudClass == 5)
            {
                SelectCloudClass(5);
                class5Selected = true;
            }
            else if (class5Selected == true && cloudClass == 5)
            {
                UnSelectCloudClass(5);
                class5Selected = false;
            }
        }
    }
    
    // Method for Reloading Point Clouds, in case some have not loaded properly
    public void ReloadClouds()
    {
        foreach (GameObject pci in PCInstances)
        {
            // Remove Clouds
            pci.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
            // ShutDown V2 Renderer
            pci.GetComponentInChildren<DynamicPointCloudSet>().PointRenderer.ShutDown();
            // Disable DynamicPointCloudSet Component
            pci.SetActive(false);
            
            // Enable DynamicPointCloudSet Component
            pci.SetActive(true);
            // Enable Clouds
            pci.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
            // Show V2 Renderer
            pci.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
        }
    }
}
