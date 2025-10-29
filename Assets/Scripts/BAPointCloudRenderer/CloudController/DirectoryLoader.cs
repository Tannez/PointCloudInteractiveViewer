using System;
using System.Collections.Generic;
using System.IO;
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
    public class DirectoryLoader : MonoBehaviour {

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

        [SerializeField] GameObject CloudLoaderPrefab;

        [SerializeField] public List<Vector3> spawnPositions = new List<Vector3>();

        public List<GameObject> pointClouds = new List<GameObject>();

        /// <summary>
        /// Creates PointCloudLoader objects for all the point clouds in the given path.
        /// </summary>
        // public void LoadAll() // NOT ORIGINAL
        // {
        //     if (streamingAssetsAsRoot) fullPath = Application.streamingAssetsPath + "/" + path;
        //     else { fullPath = path; }

        //     DirectoryInfo dir = new DirectoryInfo(fullPath);
        //     foreach (DirectoryInfo sub in dir.GetDirectories())
        //     {
        //         GameObject parentGO = new GameObject("Cloud: " + (cloudsInDirectory + 1));
        //         GameObject go = new GameObject(sub.Name);
        //         PointCloudLoader loader = go.AddComponent<PointCloudLoader>();
        //         GameObject dynamicLoader;

        //         if (cloudsInDirectory < spawnPositions.Count)
        //         {
        //             dynamicLoader = Instantiate(CloudLoaderPrefab, spawnPositions[cloudsInDirectory], Quaternion.identity);
        //         }
        //         else
        //         {
        //             dynamicLoader = Instantiate(CloudLoaderPrefab);
        //         }

        //         go.transform.SetParent(parentGO.transform);
        //         dynamicLoader.transform.SetParent(parentGO.transform);

        //         pointClouds.Add(parentGO);

        //         if (streamingAssetsAsRoot)
        //         {
        //             loader.streamingAssetsAsRoot = true;
        //             loader.cloudPath = path + sub.Name;
        //         }
        //         else
        //         {
        //             loader.cloudPath = sub.FullName;
        //         }

        //         //loader.setController = pointset;
        //         loader.setController = dynamicLoader.GetComponent<DynamicPointCloudSet>();
        //         dynamicLoader.GetComponent<DynamicPointCloudSet>().meshConfiguration = GameObject.Find("MeshConfig").GetComponent<DefaultMeshConfiguration>();
        //         dynamicLoader.GetComponent<DynamicPointCloudSet>().userCamera = Camera.main;

        //         cloudsInDirectory++;
        //     }

        //     Debug.Log("Clouds Loaded: " + cloudsInDirectory);
        // }
    
        public void LoadAll() { // Original
        if (streamingAssetsAsRoot) fullPath = Application.streamingAssetsPath + "/" + path;
        else { fullPath = path; }

        DirectoryInfo dir = new DirectoryInfo(fullPath);
        foreach (DirectoryInfo sub in dir.GetDirectories()) {
            GameObject go = new GameObject(sub.Name);
            PointCloudLoader loader = go.AddComponent<PointCloudLoader>();
            if (streamingAssetsAsRoot)
            {
                loader.streamingAssetsAsRoot = true;
                loader.cloudPath = path + sub.Name;
            }
            else
            {
                loader.cloudPath = sub.FullName;
            }

            loader.setController = pointset;
    }
}

    }
}
