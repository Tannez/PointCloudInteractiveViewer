using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BAPointCloudRenderer.ObjectCreation;
using UnityEngine;

namespace BAPointCloudRenderer.CloudController {
    /// <summary>
    /// Use this loader, if you have several pointcloud-folders in the same directory and want to import all of them to your program.
    /// To  import them, create a DirectoryLoader and press the "Load Directory" Button in the Editor (or call the function LoadAll).
    /// 
    /// Streaming Assets support provided by Pablo Vidaurre
    /// </summary>
    [ExecuteInEditMode]
    public class DirectoryInstanceLoader : MonoBehaviour {

        /// <summary>
        /// Path of the directory containing the point clouds
        /// </summary>
        public string path;
        /// <summary>
        /// When true, the cloudPath is relative to the streaming assets directory
        /// </summary>
        public bool streamingAssetsAsRoot = false;

        /// <summary>
        /// The PointSetController
        /// </summary>
        public AbstractPointCloudSet pointset;

        //may include streaming assets path
        private string fullPath;

        // Count Cloud Loaders Created for Cloud Instantiator <- Added
        [HideInInspector] public int cloudsInDirectory = 0;
        [HideInInspector] public int cloudClassesInDirectory = 0;

        // Compare name variables
        private string currentInstanceClassName;
        private string previousInstanceClassName;

        [SerializeField] GameObject CloudLoaderPrefab;

        [SerializeField] public List<Vector3> spawnPositions = new List<Vector3>();


        // Pointcloud Instances in Class
        [Serializable]
        public class PCInstances
        {
            public GameObject cloudClassGO;
            public GameObject getCloudInstanceGO;
            public GameObject getCloudLoaderGO;
            // public PointCloudLoader getLoader;
            public GameObject getDynamicLoaderGO;

            public PCInstances(GameObject aCloudClassGO, GameObject aCloudInstanceGO, GameObject aCloudLoaderGO, GameObject aDynamicLoaderGO)
            {
                // Get Cloud Instance GO's for PC manipulation
                getCloudInstanceGO = aCloudInstanceGO;
                getCloudLoaderGO = aCloudLoaderGO;
                getDynamicLoaderGO = aDynamicLoaderGO;

                // Make Cloud Instance GO parent of Loader GO's 
                getCloudLoaderGO.transform.SetParent(getCloudInstanceGO.transform);
                getDynamicLoaderGO.transform.SetParent(getCloudInstanceGO.transform);

                // Make Cloud Instance GO's part of Cloud Class GO
                cloudClassGO = aCloudClassGO;
                getCloudInstanceGO.transform.SetParent(cloudClassGO.transform);
            }
        }

        // List of Cloud Classes
        public List<PCInstances> pointCloudClasses = new List<PCInstances>(); 

        /// <summary>
        /// Creates PointCloudLoader objects for all the point clouds in the given path.
        /// </summary>
        public void LoadAll()
        {
            if (streamingAssetsAsRoot) fullPath = Application.streamingAssetsPath + "/" + path;
            else { fullPath = path; }

            // Create first instance of class
            GameObject cloudClassGO = null;
            PCInstances pCInstances = null;

            DirectoryInfo dir = new DirectoryInfo(fullPath);
            foreach (DirectoryInfo sub in dir.GetDirectories())
            {
                Debug.Log("Loading Current Cloud: " + cloudsInDirectory);

                // Define first few letters of new sub (ignore on first iteration)
                currentInstanceClassName = sub.Name.Substring(0,2);
                Debug.Log("Current Cloud sub name class: " + currentInstanceClassName);

                // If new class name is same as previous iteration
                if (currentInstanceClassName == previousInstanceClassName)
                {
                    // Add to current PointCloudClasses element 
                    pointCloudClasses.Add(pCInstances);
                    Debug.Log("Appending PointCloud to Same Class");
                }
                else
                {
                    // else create new element in PointCloudClasses 
                    cloudClassGO = new GameObject("Class: " + (cloudClassesInDirectory + 1));
                    cloudClassesInDirectory++;
                    Debug.Log("Appending PointCloud to New Class");
                }

                // Create Cloud Instance From Directory
                GameObject cloudInstanceGO = new GameObject("Cloud: " + (cloudsInDirectory + 1));
                GameObject go = new GameObject(sub.Name);
                PointCloudLoader loader = go.AddComponent<PointCloudLoader>();
                GameObject dynamicLoader;

                // Spawn at specific positions if defined in Inspector
                if (cloudsInDirectory < spawnPositions.Count)
                {
                    dynamicLoader = Instantiate(CloudLoaderPrefab, spawnPositions[cloudsInDirectory], Quaternion.identity);
                }
                else
                {
                    dynamicLoader = Instantiate(CloudLoaderPrefab);
                }

                // Connect Values to Components
                if (streamingAssetsAsRoot)
                {
                    loader.streamingAssetsAsRoot = true;
                    loader.cloudPath = path + sub.Name;
                }
                else
                {
                    loader.cloudPath = sub.FullName;
                }

                loader.setController = dynamicLoader.GetComponent<DynamicPointCloudSet>();
                dynamicLoader.GetComponent<DynamicPointCloudSet>().meshConfiguration = GameObject.Find("MeshConfig").GetComponent<DefaultMeshConfiguration>();
                dynamicLoader.GetComponent<DynamicPointCloudSet>().userCamera = Camera.main;

                // Create Cloud Instance and add to Class
                pCInstances = new PCInstances(cloudClassGO, cloudInstanceGO, go, dynamicLoader);
                
                previousInstanceClassName = currentInstanceClassName;
                Debug.Log("Previous Cloud sub name class: " + previousInstanceClassName);
                cloudsInDirectory++;
            }

            Debug.Log("Clouds Loaded: " + cloudsInDirectory);
        }
    }
}

