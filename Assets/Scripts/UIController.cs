using System.Collections.Generic;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.Edl;
using BAPointCloudRenderer.ObjectCreation;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    //Cam Access
    Camera cam;

    //Reference to pointcloud gameobject and its script
    [Tooltip("Insert GameObject with a Dynamic Loader Script attached to it")]
    [SerializeField] private GameObject pointClouds;

    [SerializeField] private PointMeshConfiguration pointMeshConfiguration;
    [SerializeField] private DefaultMeshConfiguration defaultMeshConfiguration;


    private DynamicPointCloudSet dynamicPointCloudScript;

    [Header("Values")]
    [SerializeField] public uint PCPointBudget;
    [SerializeField] public float EDLRadiusUI { get { return edlCamera._edlRadius; } set { edlCamera._edlRadius = EDLRadiusSlider.value; } }
    [SerializeField] public float EDLExpScaleUI { get { return edlCamera._edlExpScale; } set { edlCamera._edlExpScale = EDLExpScaleSlider.value; } }
    [SerializeField] public float EDLScaleUI { get { return edlCamera._edlScale; } set { edlCamera._edlScale = EDLScaleSlider.value; } }


    //UI Components
    [Header("Point Budget Slider")]
    [SerializeField] private Slider pointBudgetSlider;
    [SerializeField] private TextMeshProUGUI pointBudgetSliderText;

    [Header("EDL GameObjects")]
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

    //Exploded View GO's 
    private GameObject pointCloudByg;
    private GameObject pointCloudTer; 



    [Header("Exploded View Control")]
    [SerializeField] private Slider ExplodedViewSlider;

    void Start()
    {
        // Access Main Camera
        cam = Camera.main;

        dynamicPointCloudScript = pointClouds.GetComponent<DynamicPointCloudSet>();

        PCPointBudget = dynamicPointCloudScript.pointBudget;

        // Get EDL values from EdlCamera Script
        EDLRadiusUI = edlCamera.EdlRadius;
        EDLExpScaleUI = edlCamera.EdlExpScale;
        EDLScaleUI = edlCamera.EdlScale;

        // Exploded View 
        pointCloudByg = GameObject.Find("KalkværkPCLoader");
        pointCloudTer = GameObject.Find("KalkværkPCLoader2");

        pointCloudByg.transform.position = new Vector3(1.27f, -4.43f, 1.0f);
        pointCloudTer.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

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
    }

    void Update()
    {

    }

    public void PCValueChangeCheck()
    {
        PCPointBudget = (uint)pointBudgetSlider.value;
        pointBudgetSliderText.text = PCPointBudget.ToString();
        Debug.Log(dynamicPointCloudScript.PointRenderer.GetPointCount());
    }
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

    public void ExplodedViewSpread()
    {
        GameObject pointCloudByg = GameObject.Find("KalkværkPCLoader");
        GameObject pointCloudTer = GameObject.Find("KalkværkPCLoader2");

        pointCloudByg.transform.position = new Vector3(1.275f, -4.43f - ExplodedViewSlider.value * 2, 1.0f);
        pointCloudTer.transform.position = new Vector3(0.0f, 0.0f + ExplodedViewSlider.value * 2, 0.0f);
    }

    public void SkyBoxButton()
    {
        cam.clearFlags = CameraClearFlags.Skybox;
        Debug.Log("SkyBox Button Pressed!");
    }
    public void BlackButton()
    {
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        Debug.Log("Black Button Pressed!");
    }
    public void WhiteButton()
    {
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.white;
        Debug.Log("White Button Pressed!");
    }

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
}
