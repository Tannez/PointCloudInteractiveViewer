using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BAPointCloudRenderer.CloudData;
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
        [HideInInspector] public int cloudInstanceInClass = 0;

        // Compare name variables
        private string currentInstanceClassName;
        private string previousInstanceClassName;

        [SerializeField] GameObject CloudLoaderPrefab;
        [SerializeField] GameObject MeshConfigPrefab;

        [SerializeField] public List<Vector3> spawnPositions = new List<Vector3>();

        // Save All Instances in list
        public List<GameObject> allInstances = new List<GameObject>();

        // Pointcloud Instances in Class
        [Serializable]
        public class PCInstances
        {
            public GameObject cloudClassGO;
            public GameObject getCloudInstanceGO;
            public GameObject getCloudLoaderGO;
            // public PointCloudLoader getLoader;
            public GameObject getDynamicLoaderGO;

            public GameObject getMeshConfigGO;

            public PCInstances(GameObject aCloudClassGO, GameObject aCloudInstanceGO, GameObject aCloudLoaderGO, GameObject aDynamicLoaderGO, GameObject aMeshConfigGO)
            {
                // Get Cloud Instance GO's for PC manipulation
                getCloudInstanceGO = aCloudInstanceGO;
                getCloudLoaderGO = aCloudLoaderGO;
                getDynamicLoaderGO = aDynamicLoaderGO;
                getMeshConfigGO = aMeshConfigGO;

                // Make Cloud Instance GO parent of Loader GO's 
                getCloudLoaderGO.transform.SetParent(getCloudInstanceGO.transform);
                getDynamicLoaderGO.transform.SetParent(getCloudInstanceGO.transform);
                getMeshConfigGO.transform.SetParent(getCloudInstanceGO.transform);

                // Make Cloud Instance GO's part of Cloud Class GO
                cloudClassGO = aCloudClassGO;
                getCloudInstanceGO.transform.SetParent(cloudClassGO.transform);

                // Add Cloud Interaction Script to each instance
                getCloudInstanceGO.AddComponent<CloudInteraction>();
                getCloudInstanceGO.AddComponent<BoxCollider>();
                getCloudInstanceGO.tag = "PointCloud";
            }
        }

        // List of Cloud Classes
        public List<PCInstances> pointCloudClasses = new List<PCInstances>(); 
        
        GameObject cloudClassGO;
        PCInstances pCInstances;

        /// <summary>
        /// Creates PointCloudLoader objects for all the point clouds in the given path.
        /// </summary>
        public void LoadAll()
        {
            if (streamingAssetsAsRoot) fullPath = Application.streamingAssetsPath + "/" + path;
            else { fullPath = path; }

            DirectoryInfo dir = new DirectoryInfo(fullPath);
            // Debug.Log("Entering Cloud Loading");

            cloudInstanceInClass = 0;

            foreach (DirectoryInfo sub in dir.GetDirectories())
            {
                // Define first few letters of file name - used to check if instance belongs to same class
                currentInstanceClassName = sub.Name.Substring(0, 2);

                if (currentInstanceClassName != previousInstanceClassName)
                {
                    // Create new class object
                    //Debug.Log("Creating a New Class: " + "Class: " + (cloudClassesInDirectory+1));
                    cloudClassesInDirectory++;
                    cloudClassGO = new GameObject("Class: " + cloudClassesInDirectory);
                    //cloudClassGO.gameObject.GetComponent<BoxCollider>().size = boundingBox.GetBoundsObject().size;
                    cloudInstanceInClass = 0;
                    //Debug.Log("Resetting Instances in Class");
                }

                cloudInstanceInClass++;
                //Debug.Log("Current Cloud sub name class: " + currentInstanceClassName);

                //Debug.Log("Loading Current Cloud Instance(" + (cloudsInDirectory + 1) + ") For Class: " + (cloudClassesInDirectory+1));

                // Create Cloud Instance From Directory
                GameObject cloudInstanceGO = new GameObject("Cloud: " + cloudInstanceInClass);
                //Debug.Log("Created a cloudInstanceGO numbered: " + cloudInstanceInClass);
                GameObject go = new GameObject(sub.Name);
                PointCloudLoader loader = go.AddComponent<PointCloudLoader>();
                GameObject dynamicLoader;
                GameObject meshConfigure;

                // Spawn at specific positions if defined in Inspector
                if (cloudsInDirectory < spawnPositions.Count)
                {
                    dynamicLoader = Instantiate(CloudLoaderPrefab, spawnPositions[cloudsInDirectory], Quaternion.identity);
                    meshConfigure = Instantiate(MeshConfigPrefab);
                }
                else
                {
                    dynamicLoader = Instantiate(CloudLoaderPrefab);
                    meshConfigure = Instantiate(MeshConfigPrefab);
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
                meshConfigure.GetComponent<DefaultMeshConfiguration>().renderCamera = Camera.main;
                dynamicLoader.GetComponent<DynamicPointCloudSet>().meshConfiguration = meshConfigure.GetComponent<DefaultMeshConfiguration>();
                dynamicLoader.GetComponent<DynamicPointCloudSet>().userCamera = Camera.main;


                if (currentInstanceClassName != previousInstanceClassName)
                {
                    // Add to New PointCloudClass 
                
                    // Create Cloud Instance and add to new Class
                    pCInstances = new PCInstances(cloudClassGO, cloudInstanceGO, go, dynamicLoader, meshConfigure);
                    allInstances.Add(pCInstances.getCloudInstanceGO);
                    //Debug.Log("Instance Added. Total Instances = " + allInstances.Count);
                    pointCloudClasses.Add(pCInstances);

                    //Debug.Log("Created New Class: " + cloudClassesInDirectory);
                }

                else if (currentInstanceClassName == previousInstanceClassName)
                {
                    // Add to current PointCloudClasses element

                    // Create Cloud Instance and add to same Class
                    pCInstances = new PCInstances(cloudClassGO, cloudInstanceGO, go, dynamicLoader, meshConfigure);
                    allInstances.Add(pCInstances.getCloudInstanceGO);  
                    //Debug.Log("Instance Added. Total Instances = " + allInstances.Count);

                    //Debug.Log("Appending PointCloud to Same Class");
                }

                previousInstanceClassName = currentInstanceClassName;
                //Debug.Log("Setting previous Cloud sub name class as: " + previousInstanceClassName);
                //Debug.Log("Current Amount Of Point Cloud Classes: " + pointCloudClasses.Count);
                cloudsInDirectory++;
            }

            // Debug.Log("Clouds Loaded: " + cloudsInDirectory);
        }
    }
}

