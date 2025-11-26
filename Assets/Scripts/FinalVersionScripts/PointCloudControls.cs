using System.Collections.Generic;
using System.Collections;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.Edl;
using BAPointCloudRenderer.ObjectCreation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BAPointCloudRenderer.Controllers;
using System.Linq;
using LLMPCCompanionBubble;

public class PointCloudControls : MonoBehaviour
{
    // LLM access
    [Header("AI Interface")]
    [SerializeField] Button ShowAIAssistantButton;
    [SerializeField] GameObject LLMMenu; 
    [HideInInspector] public bool showAIAssistantButtonActive = false;
    private static PointCloudAI _pointCloudAI;
    private static PointCloudAI pointCloudAI
    {
        get
        {
            // If already cached and still valid, return it
            if (_pointCloudAI != null)
                return _pointCloudAI;

            // Otherwise, find it in the scene and cache it
            _pointCloudAI = FindFirstObjectByType<PointCloudAI>();

            if (_pointCloudAI == null)
                Debug.LogWarning("PointCloudCompanion not found in scene!");

            return _pointCloudAI;
        }
    }

    //Cam Access
    Camera cam;
    CameraController camcontrol;

    //Reference to pointcloud gameobject and its script
    [Header("Cloud Instantiation Components")]

    [Tooltip("Before running scene, a cloud instantiator is required in hierarchy. Create empty GameObject and add a Cloud Instantiator script to it. Then insert the cloud instantiator here. Remember to set asset path and prefabs in cloud instantiator script")]
    [SerializeField] CloudCreator cloudCreator;
    [Tooltip("Before running scene, an initial cloud loader is required in hierarchy. Create empty GameObject beneath Cloud Instantiator and add a Dynamic Loader to it. Then insert this cloud loader here.")]
    [SerializeField] GameObject InitialCloudLoader;
    [SerializeField] private GameObject clippingPlane;
    DirectoryCloudLoaderFinal directoryCloudLoaderFinal;
    // // Mesh Configurations
    // [SerializeField] private PointMeshConfiguration pointMeshConfiguration;
    [Tooltip("Adds Mesh To points. Insert GameObject Beneath the cloud instantiator, and add the DefaultMeshConfiguration script. Then insert GameObject here.")]
    [SerializeField] private DefaultMeshConfiguration defaultMeshConfiguration;
    [SerializeField] public List<DirectoryCloudLoaderFinal.PCInstances> PCClasses = new List<DirectoryCloudLoaderFinal.PCInstances>();
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
    [SerializeField] public Toggle EDLToggle;
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
    private bool loadingClassInstanceToggles = true;
    [Tooltip("These Toggles are only shown if the Class Button in the Classification Section has been pressed. They will be displayed on an instance menu on the right.")]
    [SerializeField] private List<Toggle> InstanceTogglesClass = new List<Toggle>();

    [Header("Instance UI")]
    [Tooltip("This UI Image refers to the one used when using the class buttons")]
    [SerializeField] private Image instanceUIImageClass;
    [HideInInspector] public bool instanceUIActive = false;
    public bool classInstanceUIActive = false;

    [Header("Color Mode Dropdown")]
    [SerializeField] public TMP_Dropdown colorModeDropDown;

    [Header("Exploded View Control")]
    [SerializeField] public Slider ExplodedViewSlider;

    // selection mode variables
    [Header("Class Buttons")]
    [Tooltip("Use this button to allow for class selection using the keyboard buttons (1,2,3,4,5)")]
    [SerializeField] bool keyboardClassSelection = true;
    public bool keyboardShotcutsEnabled = true;
    [Tooltip("Class buttons within the Classification Section of the Main UI")]
    [SerializeField] List<Button> classButtons = new List<Button>();
    BAPointCloudRenderer.ObjectCreation.ColorMode previousClassColor;
    public bool[] classSelected = new bool[10];
    //private List<int> activeClasses = new List<int>();
    public bool[] classInstanceSelected = new bool[10];
    public bool[] classHidden = new bool[10]; // used to disable cloud selection when toggled off
    public int activeClassInstanceInMenu = 0;

    [Header("Instance Buttons")]
    [Tooltip("Buttons available within the Instance Menu. Only shown when specific Class is selected")]
    [SerializeField] List<Button> ClassInstancesButtons = new List<Button>();
    BAPointCloudRenderer.ObjectCreation.ColorMode previousClassInstanceColor;
    BAPointCloudRenderer.ObjectCreation.ColorMode previousClassPriorityColor;

    [HideInInspector] public int availableInstancesInClass = 0;

    [HideInInspector] public bool clickedWithMouse = false;

    [Header("Zoom-To Menu")]
    [SerializeField] Image zoomMenuBackgroundImage;
    private bool showZoomMenu = false;
    [SerializeField] Button showZoomButton;
    [SerializeField] Button[] zoomMenuButton = new Button[5];
    private bool[] zoomToButtonActive = new bool[5];


    void Start()
    {
        // Access Main Camera
        cam = Camera.main;
        camcontrol = cam.GetComponent<CameraController>();

        // Get Cloud objects
        directoryCloudLoaderFinal = cloudCreator.DirectoryLoaderGO.GetComponent<DirectoryCloudLoaderFinal>();
        PCClasses = directoryCloudLoaderFinal.pointCloudClasses;

        // Set Initial Point Budget 
        PCPointBudget = InitialCloudLoader.GetComponent<DynamicPointCloudSet>().pointBudget;

        StartCoroutine(SetPointBudget());

        pointBudgetSlider.value = PCPointBudget;
        pointBudgetSliderText.text = PCPointBudget.ToString();

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
        ZoomToDefault();
    }

    void Update()
    {
        // Use keyboard numbers to select classes
        if (keyboardClassSelection && keyboardShotcutsEnabled == true)
        {
            KeyboardCloudSelection();
        }

        // Use keyboard to reload clouds in case they are not loading properly
        if (Input.GetKeyDown(KeyCode.R) && keyboardShotcutsEnabled == true)
        {
            StartCoroutine(ReloadClouds());
        }

        // reset clouds and turn of menu
        if (Input.GetKeyDown(KeyCode.Escape) && keyboardShotcutsEnabled == true && classInstanceUIActive == true)
        {
            ShowClassInstanceUI(activeClassInstanceInMenu);
            ResetClassToggles();
        }
    }

    // Method For Changing Point Budget Of Point Clouds
    public void PCValueChangeCheck()
    {
        // stop loading
        StartCoroutine(StopLoadingClouds());

        // Change Value Of Point Budget
        PCPointBudget = (uint)pointBudgetSlider.value;

        // set point budget for each cloud
        StartCoroutine(SetPointBudget());

        pointBudgetSliderText.text = PCPointBudget.ToString();

        // start loading
        StartCoroutine(StartLoadingClouds());
    }
    //Coroutine to set the point budget
    IEnumerator SetPointBudget()
    {
        for (int cloudClassIter = 0; cloudClassIter < PCClasses.Count; cloudClassIter++)
        {
            DirectoryCloudLoaderFinal.PCInstances cloud = PCClasses[cloudClassIter];
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;

                if (instanceInClass.name.StartsWith("Cloud:") && classHidden[cloudClassIter] == false)
                {
                    // Change Point Budget
                    instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().pointBudget = PCPointBudget;
                }
            }
        }
        yield return null;
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

        foreach (DirectoryCloudLoaderFinal.PCInstances cloud in PCClasses)
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
        //Debug.Log($"Cloud: {CloudToHide + 1} is hidden");
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
        for (int cloudClassIter = 0; cloudClassIter < PCClasses.Count; cloudClassIter++)
        {
            DirectoryCloudLoaderFinal.PCInstances cloud = PCClasses[cloudClassIter];
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

        for (int cloudClassIter = 0; cloudClassIter < PCClasses.Count; cloudClassIter++)
        {
            DirectoryCloudLoaderFinal.PCInstances cloud = PCClasses[cloudClassIter];
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
        for (int cloudClassIter = 0; cloudClassIter < PCClasses.Count; cloudClassIter++)
        {
            DirectoryCloudLoaderFinal.PCInstances cloud = PCClasses[cloudClassIter];
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
        for (int cloudClassIter = 0; cloudClassIter < PCClasses.Count; cloudClassIter++)
        {
            DirectoryCloudLoaderFinal.PCInstances cloud = PCClasses[cloudClassIter];
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;
                if (instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius < 5.0f)
                {
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius += 0.25f;
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                }
            }
        }
    }
    public void PointSizeDown()
    {
        for (int cloudClassIter = 0; cloudClassIter < PCClasses.Count; cloudClassIter++)
        {
            DirectoryCloudLoaderFinal.PCInstances cloud = PCClasses[cloudClassIter];
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;
                if (instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius > 0.25f)
                {
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().pointRadius -= 0.25f;
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                }
            }
        }
    }

    // Method For Changing Color Mode Via DropDown In UI
    public void DropdownColorModeChange()
    {
        if (colorModeDropDown.value == 0)
        {
            // Reset Selects
            UnSelectOnConversion();

            // Change ColorMode
            RGBConversion();
        }

        else if (colorModeDropDown.value == 1)
        {
            // Reset Selects
            UnSelectOnConversion();

            // Change ColorMode
            ClassificationConversion();
        }

        else if (colorModeDropDown.value == 2)
        {
            // Reset Selects
            UnSelectOnConversion();

            // Change ColorMode
            IntensityConversion();
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
            StartCoroutine(UnSelectCloudClass(1));
            classSelected[0] = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && classSelected[1] == false)
        {
            SelectCloudClass(2);
            classSelected[1] = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && classSelected[1] == true)
        {
            StartCoroutine(UnSelectCloudClass(2));
            classSelected[1] = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && classSelected[2] == false)
        {
            SelectCloudClass(3);
            classSelected[2] = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && classSelected[2] == true)
        {
            StartCoroutine(UnSelectCloudClass(3));
            classSelected[2] = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && classSelected[3] == false)
        {
            SelectCloudClass(4);
            classSelected[3] = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && classSelected[3] == true)
        {
            StartCoroutine(UnSelectCloudClass(4));
            classSelected[3] = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5) && classSelected[4] == false)
        {
            SelectCloudClass(5);
            classSelected[4] = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && classSelected[4] == true)
        {
            StartCoroutine(UnSelectCloudClass(5));
            classSelected[4] = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha6) && classSelected[5] == false)
        {
            SelectCloudClass(6);
            classSelected[5] = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6) && classSelected[5] == true)
        {
            StartCoroutine(UnSelectCloudClass(6));
            classSelected[5] = false;
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
    }
    // Method To Set All Toggles Active Again
    public void ResetClassToggles()
    {
        int classTogglesIter = 0;
        foreach (Toggle ctoggle in classToggles)
        {
            ctoggle.isOn = true;
            classHidden[classTogglesIter] = false;
            classTogglesIter++;
        }
        StartCoroutine(ReloadClouds());
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

    // Method that hides the class instance ui and deprioritises all point clouds
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

        for (int i = 19; i > (instancesInClass - 1); i--)
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
                //iToggle.GetComponentInChildren<Text>().text = $"Class {cloudClass}; Instance: " + (pciT + 1); // Default name
                iToggle.GetComponentInChildren<Text>().text = PCClasses[cloudClass - 1].cloudClassGO.transform.GetChild(pciT).GetChild(0).name.Substring(6);

                if (iToggle.GetComponentInChildren<Text>().text.Length <= 0)
                {
                    iToggle.GetComponentInChildren<Text>().text = classToggles[cloudClass - 1].GetComponentInChildren<Text>().text;
                }

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
        loadingClassToggles = true;
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
            }
        }

        string clickPrompt = "User has clicked and selected the button for cloud instance: " + cloudInstance + " within the menu for class: " + cloudClass + ". \nPlease let the user know only available information about this cloud instance";
        pointCloudAI.SendChatMessage(clickPrompt);
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

        string clickPrompt = "User has clicked on cloud class: " + cloudClass + " instance: " + cloudInstance + " directly with the mouse.\n Please let them know what cloud they have currently selected and provide them with information about it.";
        pointCloudAI.SendChatMessage(clickPrompt);
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
        for (int i = 1; i <= PCClasses.Count; i++)
        {
            StartCoroutine(UnSelectCloudClass(i));
        }
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
                    // Save color
                    previousClassPriorityColor = instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode;
                    // Do not Mark class 
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().prioritiseCloud = false;
                    // Reload Mesh
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().reload = true;
                }
                else if (selected == true) // if cloud class has been selected
                {
                    // Make Button interactable
                    classButtons[currentClassInHierarchy].interactable = true;
                    classButtons[currentClassInHierarchy].gameObject.SetActive(true);
                    // Save color 
                    previousClassPriorityColor = instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().colorMode;
                    // Mark class 
                    instanceInClass.GetComponentInChildren<DefaultMeshConfiguration>().prioritiseCloud = true;
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
        if (showAIAssistantButtonActive == false)
        {
            // Show LLM menu
            LLMMenu.gameObject.SetActive(true);
            ShowAIAssistantButton.image.color = new Color(0, 0, 1, 0.4f);
            keyboardShotcutsEnabled = false;
            showAIAssistantButtonActive = true;
            return;
        }

        if (showAIAssistantButtonActive == true)
        {            
            // Hide LLM menu
            LLMMenu.gameObject.SetActive(false);
            ShowAIAssistantButton.image.color = new Color(1, 1, 1, 0.4f);
            keyboardShotcutsEnabled = true;
            showAIAssistantButtonActive = false;
            return;
        }
    }

    // Method for Reloading Point Clouds, in case some have not loaded properly
    public IEnumerator ReloadClouds()
    {

        for (int currentClass = 0; currentClass < PCClasses.Count; currentClass++)
        {
            DirectoryCloudLoaderFinal.PCInstances cloud = PCClasses[currentClass];
            for (int i = 0; i < cloud.cloudClassGO.transform.childCount; i++)
            {
                GameObject instanceInClass = cloud.cloudClassGO.transform.GetChild(i).gameObject;

                if (instanceInClass.name.StartsWith("Cloud:") && classHidden[currentClass] == false)
                {
                    // Remove Clouds
                    instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                    // ShutDown V2 Renderer
                    instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().StopRendering();

                    // Enable Clouds
                    instanceInClass.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();
                    // Show V2 Renderer
                    instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();
                }
            }
        }
        yield return null;
    }

    // Method for Reloading specific Point Clous
    public IEnumerator ReloadCloudClass(int cloudClass)
    {
        DirectoryCloudLoaderFinal.PCInstances cloudClassLoaderGO = PCClasses[cloudClass - 1];
        for (int i = 0; i < cloudClassLoaderGO.cloudClassGO.transform.childCount; i++)
        {
            GameObject instanceInClass = cloudClassLoaderGO.cloudClassGO.transform.GetChild(i).gameObject;

            if (instanceInClass.name.StartsWith("Cloud:") && classHidden[cloudClass - 1] == false)
            {
                // Remove Clouds
                instanceInClass.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();
                // ShutDown V2 Renderer
                instanceInClass.GetComponentInChildren<DynamicPointCloudSet>().StopRendering();
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
            DirectoryCloudLoaderFinal.PCInstances cloud = PCClasses[currentClass];
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
        yield return null;
    }

    // Coroutine to start loading point clouds 
    IEnumerator StartLoadingClouds()
    {
        for (int currentClass = 0; currentClass < PCClasses.Count; currentClass++)
        {
            DirectoryCloudLoaderFinal.PCInstances cloud = PCClasses[currentClass];
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
        yield return null;
    }

    public void ShowZoomMenuUI()
    {
        if (showZoomMenu == false)
        {
            showZoomMenu = true;
            showZoomButton.image.color = new Color(0, 0, 1, 0.4f);
            zoomMenuBackgroundImage.gameObject.SetActive(true);
            return;
        }
        else if (showZoomMenu == true)
        {
            showZoomMenu = false;
            for (int i = 0; i < zoomToButtonActive.Count(); i++)
            {
                if (zoomToButtonActive[i] == true)
                {
                    zoomMenuButton[i].image.color = new Color(255, 255, 255);
                    zoomToButtonActive[i] = false;
                }
            }
            showZoomButton.image.color = new Color(1, 1, 1, 0.4f);
            zoomMenuBackgroundImage.gameObject.SetActive(false);
            return;
        }
    }

    public void ZoomToClassButton(int cloudClass)
    {
        camcontrol.CameraClassTranslation(cloudClass);

        if (zoomToButtonActive[cloudClass - 1] == true) // reset if same button click
        {
            zoomMenuButton[cloudClass - 1].image.color = new Color(255, 255, 255);
            zoomToButtonActive[cloudClass - 1] = false;
            classToggles[0].isOn = true;
            classToggles[1].isOn = true;
            ZoomToDefault();
            StartCoroutine(ReloadClouds());
            return;
        }

        if (zoomToButtonActive.Contains(true)) // reset button selection coloring when switching between views
        {
            for (int i = 0; i < zoomToButtonActive.Count(); i++)
            {
                if (zoomToButtonActive[i] == true)
                {
                    zoomMenuButton[i].image.color = new Color(255, 255, 255);
                    zoomToButtonActive[i] = false;
                }
            }
        }

        if (cloudClass == 1 && zoomToButtonActive[0] == false)
        {
            classToggles[0].isOn = true;
            classToggles[1].isOn = true;
            zoomToButtonActive[0] = true;
            zoomMenuButton[0].image.color = new Color(0, 0, 125);
            return;
        }
        if (cloudClass > 1 && cloudClass < 7 && zoomToButtonActive[cloudClass - 1] == false)
        {
            classToggles[0].isOn = false;
            classToggles[1].isOn = false;
            zoomToButtonActive[cloudClass - 1] = true;
            zoomMenuButton[cloudClass - 1].image.color = new Color(0, 0, 125);
            return;
        }
    }

    // Method for positioning camerea to individual classes (class int over 0 as it does not use indecies)
    public void ZoomToClassLLM(int cloudClass)
    {
        camcontrol.CameraClassTranslation(cloudClass);
    }
    public void ZoomToDefault()
    {
        camcontrol.MoveToDefaultPosition();
    }
}
