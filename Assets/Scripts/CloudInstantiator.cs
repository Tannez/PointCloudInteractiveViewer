using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.Loading;
using UnityEngine;

public class CloudInstantiator : MonoBehaviour
{
    [Header("UI Connection")]
    [SerializeField] UIController uIContoller;

    [Header("Point Cloud Directory")]
    [SerializeField] public string DirectoryPath;
    [SerializeField] public GameObject DirectoryLoaderPrefab;
    [HideInInspector] public GameObject DirectoryLoaderGO;

    [Header("Cloud Loader")]
    [SerializeField] public GameObject InitialCloudLoader;
    [SerializeField] public GameObject CloudLoaderPrefab;

    void Awake()
    {
        if (DirectoryPath != null)
        {
            // Instatiate Directory Loader On Awake And Set Path + Cloud Loader
            DirectoryLoaderGO = Instantiate(DirectoryLoaderPrefab);
            DirectoryLoaderGO.GetComponent<DirectoryLoader>().path = DirectoryPath;
            DirectoryLoaderGO.GetComponent<DirectoryLoader>().pointset = InitialCloudLoader.GetComponent<DynamicPointCloudSet>();

            // Load Clouds From Directory
            DirectoryLoaderGO.GetComponent<DirectoryLoader>().LoadAll();
        }
        else
        {
            Debug.Log("No Path Set For Directory Loader");
        }
    }
}
