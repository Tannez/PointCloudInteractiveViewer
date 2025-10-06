using System.Collections.Generic;
using System.Linq;
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

public class UIController : MonoBehaviour
{
    //Cam Access
    Camera cam;

    //Reference to pointcloud gameobject and its script
    [SerializeField] public List<GameObject> pointClouds = new List<GameObject>();
    [Tooltip("Before running scene, a cloud instantiator is required in hierarchy. Create empty GameObject and add a Cloud Instantiator script to it. Then insert the cloud instantiator here. Remember to set asset path and prefabs in cloud instantiator script")]
    [SerializeField] CloudInstantiator cloudInstantiator;
    [Tooltip("Before running scene, an initial cloud loader is required in hierarchy. Create empty GameObject beneath Cloud Instantiator and add a Dynamic Loader to it. Then insert this cloud loader here.")]
    [SerializeField] GameObject InitialCloudLoader;
    DirectoryLoader directoryLoader;
    [SerializeField] private GameObject clippingPlane;

    // Loaders
    PointCloudLoader pointCloudBygLoader;
    PointCloudLoader pointCloudTerLoader;

    // // Mesh Configurations
    // [SerializeField] private PointMeshConfiguration pointMeshConfiguration;
    [Tooltip("Adds Mesh To points. Insert GameObject Beneath the cloud instantiator, and add the DefaultMeshConfiguration script. Then insert GameObject here.")]
    [SerializeField] private DefaultMeshConfiguration defaultMeshConfiguration;

    [Header("Point Budget")]
    [SerializeField] public uint PCPointBudget;
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

    [Header("Classification Toggles")]
    [SerializeField] private List<Toggle> classToggles = new List<Toggle>();

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

        // Get Cloud objects
        directoryLoader = cloudInstantiator.DirectoryLoaderGO.GetComponent<DirectoryLoader>();
        pointClouds = directoryLoader.pointClouds;

        // Set Initial Point Budget 
        PCPointBudget = InitialCloudLoader.GetComponent<DynamicPointCloudSet>().pointBudget;

        foreach (GameObject cloud in pointClouds)
        {
            cloud.GetComponentInChildren<DynamicPointCloudSet>().pointBudget = PCPointBudget;
        }

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

        // Create Color Mode Listener
        colorModeDropDown.onValueChanged.AddListener(delegate { DropdownColorModeChange(); });
        
        // Set Available toggles based on class amount
        while (classToggles.Count > pointClouds.Count)
        {
            int beforeToggles = classToggles.Count;
            Debug.Log("Before Removal: " + beforeToggles);
            classToggles[classToggles.Count - 1].isOn = false;
            classToggles[classToggles.Count - 1].gameObject.SetActive(false);
            classToggles.Remove(classToggles[classToggles.Count - 1]);
            int afterToggles = classToggles.Count;
            Debug.Log("After Removal: " + afterToggles);

            if (beforeToggles == afterToggles) // Avoid infinite Loop
            {
                break;
            }
        }
    }

    void Update()
    {

    }

    // Method For Changing Point Budget Of Point Clouds
    public void PCValueChangeCheck()
    {
        // Remove Clouds
        foreach (GameObject cloud in pointClouds)
        {
            // Remove Clouds
            cloud.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();

            // ShutDown V2 Renderer
            cloud.GetComponentInChildren<DynamicPointCloudSet>().PointRenderer.ShutDown();

            // Disable DynamicPointCloudSet Component
            cloud.SetActive(false);
        }

        // Change Value Of Point Budget
        PCPointBudget = (uint)pointBudgetSlider.value;


        foreach (GameObject cloud in pointClouds)
        {
            cloud.GetComponentInChildren<DynamicPointCloudSet>().pointBudget = PCPointBudget;
        }

        pointBudgetSliderText.text = PCPointBudget.ToString();

        //Enable Clouds
        foreach (GameObject cloud in pointClouds)
        {
            // Enable DynamicPointCloudSet Component
            cloud.SetActive(true);

            // Enable Clouds
            cloud.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();

            // Show V2 Renderer
            cloud.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();

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

    // Method For Explod View 
    public void ExplodedViewSpread()
    {
        int cloudRuns = 0;

        foreach (GameObject cloud in pointClouds)
        {
            cloud.transform.position = new Vector3(0.0f, 0.0f + ExplodedViewSlider.value * (pointClouds.Count - cloudRuns) / 10, 0.0f);
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
    public void HidePC(int CloudToHide)
    {
        GameObject PointCloudHidden = pointClouds[CloudToHide];
        PointCloudHidden.GetComponent<PointCloudLoader>().RemovePointCloud();
        Debug.Log($"{CloudToHide} is hidden");
    }
    public void ShowPC(int CloudToShow)
    {
        GameObject PointCloudLoaded = pointClouds[CloudToShow];
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
            //  <- New !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            foreach (GameObject cloud in pointClouds)
            {
                // Remove Clouds
                cloud.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();

                // ShutDown V2 Renderer
                cloud.GetComponentInChildren<DynamicPointCloudSet>().PointRenderer.ShutDown();

                // Disable DynamicPointCloudSet Component
                cloud.SetActive(false);
            }

            // Change ColorMode
            RGBConversion();

            // Enable Clouds
            foreach (GameObject cloud in pointClouds)
            {
                // Enable DynamicPointCloudSet Component
                cloud.SetActive(true);

                // Enable Clouds
                cloud.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();

                // Show V2 Renderer
                cloud.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();

            }
        }

        else if (colorModeDropDown.value == 1)
        {
            //  <- New !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            foreach (GameObject cloud in pointClouds)
            {
                // Remove Clouds
                cloud.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();

                // ShutDown V2 Renderer
                cloud.GetComponentInChildren<DynamicPointCloudSet>().PointRenderer.ShutDown();

                // Disable DynamicPointCloudSet Component
                cloud.SetActive(false);
            }

            // Change ColorMode
            ClassificationConversion();

            // Enable Clouds
            foreach (GameObject cloud in pointClouds)
            {
                // Enable DynamicPointCloudSet Component
                cloud.SetActive(true);

                // Enable Clouds
                cloud.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();

                // Show V2 Renderer
                cloud.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();

            }
        }
        else if (colorModeDropDown.value == 2)
        {
            //  <- New !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            foreach (GameObject cloud in pointClouds)
            {
                // Remove Clouds
                cloud.GetComponentInChildren<PointCloudLoader>().RemovePointCloud();

                // ShutDown V2 Renderer
                cloud.GetComponentInChildren<DynamicPointCloudSet>().PointRenderer.ShutDown();

                // Disable DynamicPointCloudSet Component
                cloud.SetActive(false);
            }

            // Change ColorMode
            IntensityConversion();

            // Enable Clouds
            foreach (GameObject cloud in pointClouds)
            {
                // Enable DynamicPointCloudSet Component
                cloud.SetActive(true);

                // Enable Clouds
                cloud.GetComponentInChildren<PointCloudLoader>().LoadPointCloud();

                // Show V2 Renderer
                cloud.GetComponentInChildren<DynamicPointCloudSet>().ReInitialize();

            }
        }
    }

    // Method For Toggling The Various Classes
    // public void classToggleChange()
    // {
    //     int currentToggle = 0;

    //     foreach (Toggle toggle in classToggles)
    //     {
    //         if (toggle.isOn)
    //         {
    //             ShowPC(currentToggle);
    //             Debug.Log("Cloud: " + currentToggle + " is on");
    //         }
    //         else if (!toggle.isOn)
    //         {
    //             HidePC(currentToggle);
    //             Debug.Log("Cloud: " + currentToggle + " is off");
    //         }
    //         currentToggle++;
    //     }
    // }
}
