using System.Collections.Generic;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.Loading;
using UnityEngine;

public class CloudInstanceInstantiatiorLLM : MonoBehaviour
{
    [Header("UI Connection")]
    [SerializeField] CloudControllerLLM cloudControllerLLM;

    [Header("Point Cloud Directory")]
    [SerializeField] public string DirectoryPath;
    [SerializeField] public GameObject DirectoryLoaderPrefab;
    [HideInInspector] public GameObject DirectoryLoaderGO;

    [Header("Cloud Loader")]
    [SerializeField] public GameObject InitialCloudLoader;
    [SerializeField] public GameObject CloudLoaderPrefab;

    [Header("Mesh Config")]
    [SerializeField] public GameObject InitialMeshConfig;
    [SerializeField] public GameObject MeshConfigPrefab;

    [Header("Spawn Positions")]
    [SerializeField] public bool useSpawnPositions = true;
    [SerializeField] public List<Vector3> cloudSpawnPositions = new List<Vector3>();

    void Awake()
    {
        if (DirectoryPath != null)
        {
            // Instatiate Directory Loader On Awake And Set Path + Cloud Loader
            DirectoryLoaderGO = Instantiate(DirectoryLoaderPrefab);
            DirectoryLoaderGO.GetComponent<DirectoryInstanceLoaderLLM>().path = DirectoryPath;
            DirectoryLoaderGO.GetComponent<DirectoryInstanceLoaderLLM>().pointset = InitialCloudLoader.GetComponent<DynamicPointCloudSet>();

            // Add Spawn Positions From Inspector To Created Directory Loader
            if (useSpawnPositions)
            {
                foreach (Vector3 SP in cloudSpawnPositions)
                {
                    DirectoryLoaderGO.GetComponent<DirectoryInstanceLoaderLLM>().spawnPositions.Add(SP);
                }
            }
                // Load Clouds From Directory
                DirectoryLoaderGO.GetComponent<DirectoryInstanceLoaderLLM>().LoadAll();
        }
        else
        {
            Debug.Log("No Path Set For Directory Loader");
        }
    }
}
