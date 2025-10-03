using System.Collections.Generic;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.Edl;
using BAPointCloudRenderer.Loading;
using BAPointCloudRenderer.ObjectCreation;
using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class OLDUIController : MonoBehaviour
{
    //Cam Access
    Camera cam;

    //Reference to pointcloud gameobject and its script
    [Tooltip("Insert GameObject with a Dynamic Loader Script attached to it")]
    // [SerializeField] public List<GameObject> pointClouds = new List<GameObject>();
    // [SerializeField] CloudInstantiator cloudInstantiator;
    // DirectoryLoader directoryLoader;
    [SerializeField] private GameObject clippingPlane;

    // Loaders
    PointCloudLoader pointCloudBygLoader;
    PointCloudLoader pointCloudTerLoader;

    // Mesh Configurations
    [SerializeField] private PointMeshConfiguration pointMeshConfiguration;
    [SerializeField] private DefaultMeshConfiguration defaultMeshConfiguration;

    [Header("Point Budget Values")]
    [SerializeField] public uint PCPointBudget1;
    [SerializeField] public uint PCPointBudget2;
    [SerializeField] public float EDLRadiusUI { get { return edlCamera._edlRadius; } set { edlCamera._edlRadius = EDLRadiusSlider.value; } }
    [SerializeField] public float EDLExpScaleUI { get { return edlCamera._edlExpScale; } set { edlCamera._edlExpScale = EDLExpScaleSlider.value; } }
    [SerializeField] public float EDLScaleUI { get { return edlCamera._edlScale; } set { edlCamera._edlScale = EDLScaleSlider.value; } }


    //UI Components
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

    //Exploded View GO's 
    private GameObject pointCloudByg;
    private GameObject pointCloudTer;

    void Start()
    {
        // Access Main Camera
        cam = Camera.main;

        // // Exploded View 
        pointCloudByg = GameObject.Find("KalkværkPCLoader");
        pointCloudTer = GameObject.Find("KalkværkPCLoader2");

        pointCloudByg.transform.position = new Vector3(1.27f, -4.43f, 1.0f);
        pointCloudTer.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

        pointCloudBygLoader = pointCloudByg.GetComponent<PointCloudLoader>();
        pointCloudTerLoader = pointCloudTer.GetComponent<PointCloudLoader>();

        // Point Budget 
        PCPointBudget1 = pointCloudByg.GetComponent<DynamicPointCloudSet>().pointBudget;
        PCPointBudget2 = pointCloudTer.GetComponent<DynamicPointCloudSet>().pointBudget;

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

        // Slider Listeners 
        pointBudgetSlider.onValueChanged.AddListener(delegate { PCValueChangeCheck(); });
        EDLRadiusSlider.onValueChanged.AddListener(delegate { EDLRadiusChangeCheck(); });
        EDLExpScaleSlider.onValueChanged.AddListener(delegate { EDLExpScaleChangeCheck(); });
        EDLScaleSlider.onValueChanged.AddListener(delegate { EDLScaleChangeCheck(); });
        ExplodedViewSlider.onValueChanged.AddListener(delegate { ExplodedViewSpread(); });

        // Color Mode 
        colorModeDropDown.onValueChanged.AddListener(delegate { DropdownColorModeChange(); });
    }

    void Update()
    {

    }

    // Method For Changing Point Budget Of Point Clouds
    public void PCValueChangeCheck()
    {
        pointCloudByg = GameObject.Find("KalkværkPCLoader");
        pointCloudTer = GameObject.Find("KalkværkPCLoader2");
        PointCloudLoader pointCloudBygLoader = GameObject.Find("N028_kalkværksvej_class_1_bygværk_converted").GetComponent<PointCloudLoader>();
        PointCloudLoader pointCloudTerLoader = GameObject.Find("N028_kalkværksvej_class_2_terræn_converted").GetComponent<PointCloudLoader>();

        // Remove Clouds
        pointCloudBygLoader.RemovePointCloud();
        pointCloudTerLoader.RemovePointCloud();

        // ShutDown V2 Renderer
        pointCloudByg.GetComponent<DynamicPointCloudSet>().PointRenderer.ShutDown();
        pointCloudTer.GetComponent<DynamicPointCloudSet>().PointRenderer.ShutDown();

        // Disable DynamicPointCloudSet Component
        pointCloudByg.SetActive(false);
        pointCloudTer.SetActive(false);

        // Change Value Of Point Budget
        PCPointBudget1 = (uint)pointBudgetSlider.value;
        pointCloudByg.GetComponent<DynamicPointCloudSet>().pointBudget = (uint)pointBudgetSlider.value;
        PCPointBudget2 = (uint)pointBudgetSlider.value;
        pointCloudTer.GetComponent<DynamicPointCloudSet>().pointBudget = (uint)pointBudgetSlider.value;
        pointBudgetSliderText.text = PCPointBudget1.ToString();

        // Enable DynamicPointCloudSet
        pointCloudByg.SetActive(true);
        pointCloudTer.SetActive(true); pointCloudBygLoader.RemovePointCloud();
        pointCloudTerLoader.RemovePointCloud();

        // // Show V2 Renderer
        pointCloudBygLoader.LoadPointCloud();
        pointCloudTerLoader.LoadPointCloud();

        // Load Clouds
        pointCloudByg.GetComponent<DynamicPointCloudSet>().ReInitialize();
        pointCloudTer.GetComponent<DynamicPointCloudSet>().ReInitialize();
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

    // Method For Explod View 
    public void ExplodedViewSpread()
    {
        GameObject pointCloudByg = GameObject.Find("KalkværkPCLoader");
        GameObject pointCloudTer = GameObject.Find("KalkværkPCLoader2");

        pointCloudByg.transform.position = new Vector3(1.275f, -4.43f - ExplodedViewSlider.value * 2, 1.0f);
        pointCloudTer.transform.position = new Vector3(0.0f, 0.0f + ExplodedViewSlider.value * 2, 0.0f);
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
    public void HidePC(string CloudToHide)
    {
        GameObject PointCloudHidden = GameObject.Find(CloudToHide);
        PointCloudHidden.GetComponent<PointCloudLoader>().RemovePointCloud();
        Debug.Log($"{CloudToHide} is hidden");
    }
    public void ShowPC(string CloudToShow)
    {
        GameObject PointCloudLoaded = GameObject.Find(CloudToShow);
        PointCloudLoaded.GetComponent<PointCloudLoader>().LoadPointCloud();
        Debug.Log($"{CloudToShow} is shown");
    }

    // Methods For Changing Color Mode Of Point Clouds.
    // NOTE: Ensure that Clouds are Reloaded after conversion
    public void ClassificationConversion()
    {
        // pointMeshConfiguration.material = new Material(Shader.Find("CustomRenderTexture/Classification"));
        // Debug.Log("Showing Classification");

        defaultMeshConfiguration.colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Classification;
        Debug.Log("Showing Classification");
    }
    public void RGBConversion()
    {
        // pointMeshConfiguration.material = new Material(Shader.Find("Custom/PointShader"));
        // Debug.Log("Showing RGB");
        defaultMeshConfiguration.colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.RGBA;
        Debug.Log("Showing Classification");
    }
    public void IntensityConversion()
    {
        // pointMeshConfiguration.material = new Material(Shader.Find("Custom/PointShader"));
        // Debug.Log("Showing RGB");
        defaultMeshConfiguration.colorMode = BAPointCloudRenderer.ObjectCreation.ColorMode.Intensity;
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
            // OLD !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                pointCloudByg = GameObject.Find("KalkværkPCLoader");
                pointCloudTer = GameObject.Find("KalkværkPCLoader2");
                PointCloudLoader pointCloudBygLoader = GameObject.Find("N028_kalkværksvej_class_1_bygværk_converted").GetComponent<PointCloudLoader>();
                PointCloudLoader pointCloudTerLoader = GameObject.Find("N028_kalkværksvej_class_2_terræn_converted").GetComponent<PointCloudLoader>();
                
                // Remove Clouds
                pointCloudBygLoader.RemovePointCloud();
                pointCloudTerLoader.RemovePointCloud();

                // ShutDown V2 Renderer
                pointCloudByg.GetComponent<DynamicPointCloudSet>().PointRenderer.ShutDown();
                pointCloudTer.GetComponent<DynamicPointCloudSet>().PointRenderer.ShutDown();

                // Disable DynamicPointCloudSet Component
                pointCloudByg.SetActive(false);
                pointCloudTer.SetActive(false);

                // Change ColorMode
                RGBConversion(); 

                // Enable DynamicPointCloudSet
                pointCloudByg.SetActive(true);
                pointCloudTer.SetActive(true);
                pointCloudBygLoader.RemovePointCloud();
                pointCloudTerLoader.RemovePointCloud();

                // // Show V2 Renderer
                pointCloudBygLoader.LoadPointCloud();
                pointCloudTerLoader.LoadPointCloud();

                // Load Clouds
                pointCloudByg.GetComponent<DynamicPointCloudSet>().ReInitialize();
                pointCloudTer.GetComponent<DynamicPointCloudSet>().ReInitialize();
        }

        else if (colorModeDropDown.value == 1)
        {
            // OLD !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                pointCloudByg = GameObject.Find("KalkværkPCLoader");
                pointCloudTer = GameObject.Find("KalkværkPCLoader2");
                PointCloudLoader pointCloudBygLoader = GameObject.Find("N028_kalkværksvej_class_1_bygværk_converted").GetComponent<PointCloudLoader>();
                PointCloudLoader pointCloudTerLoader = GameObject.Find("N028_kalkværksvej_class_2_terræn_converted").GetComponent<PointCloudLoader>();

                // Remove Clouds
                pointCloudBygLoader.RemovePointCloud();
                pointCloudTerLoader.RemovePointCloud();

                // ShutDown V2 Renderer
                pointCloudByg.GetComponent<DynamicPointCloudSet>().PointRenderer.ShutDown();
                pointCloudTer.GetComponent<DynamicPointCloudSet>().PointRenderer.ShutDown();

                // Disable DynamicPointCloudSet Component
                pointCloudByg.SetActive(false);
                pointCloudTer.SetActive(false);

                // Change ColorMode
                ClassificationConversion();

                // Enable DynamicPointCloudSet
                pointCloudByg.SetActive(true);
                pointCloudTer.SetActive(true);
                pointCloudBygLoader.RemovePointCloud();
                pointCloudTerLoader.RemovePointCloud();

                // // Show V2 Renderer
                pointCloudBygLoader.LoadPointCloud();
                pointCloudTerLoader.LoadPointCloud();

                // Load Clouds
                pointCloudByg.GetComponent<DynamicPointCloudSet>().ReInitialize();
                pointCloudTer.GetComponent<DynamicPointCloudSet>().ReInitialize();
        }
        else if (colorModeDropDown.value == 2)
        {
            // OLD !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                pointCloudByg = GameObject.Find("KalkværkPCLoader");
                pointCloudTer = GameObject.Find("KalkværkPCLoader2");
                PointCloudLoader pointCloudBygLoader = GameObject.Find("N028_kalkværksvej_class_1_bygværk_converted").GetComponent<PointCloudLoader>();
                PointCloudLoader pointCloudTerLoader = GameObject.Find("N028_kalkværksvej_class_2_terræn_converted").GetComponent<PointCloudLoader>();

                // Remove Clouds
                pointCloudBygLoader.RemovePointCloud();
                pointCloudTerLoader.RemovePointCloud();

                // ShutDown V2 Renderer
                pointCloudByg.GetComponent<DynamicPointCloudSet>().PointRenderer.ShutDown();
                pointCloudTer.GetComponent<DynamicPointCloudSet>().PointRenderer.ShutDown();

                // Disable DynamicPointCloudSet Component
                pointCloudByg.SetActive(false);
                pointCloudTer.SetActive(false);

                // Change ColorMode
                IntensityConversion();

                // Enable DynamicPointCloudSet
                pointCloudByg.SetActive(true);
                pointCloudTer.SetActive(true);
                pointCloudBygLoader.RemovePointCloud();
                pointCloudTerLoader.RemovePointCloud();

                // // Show V2 Renderer
                pointCloudBygLoader.LoadPointCloud();
                pointCloudTerLoader.LoadPointCloud();

                // Load Clouds
                pointCloudByg.GetComponent<DynamicPointCloudSet>().ReInitialize();
                pointCloudTer.GetComponent<DynamicPointCloudSet>().ReInitialize();
        }
    }
}

