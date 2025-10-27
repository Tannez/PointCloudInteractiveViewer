using BAPointCloudRenderer.CloudController;
using UnityEngine;
using System.Collections.Generic;
using BAPointCloudRenderer.ObjectCreation;

public class CloudInteraction : MonoBehaviour
{
    UIInstanceController uIInstanceController;
    public List<DirectoryInstanceLoader.PCInstances> pClasses = new List<DirectoryInstanceLoader.PCInstances>();
    // Primitive Variables
    [SerializeField] private GameObject primitiveGO;
    [SerializeField] private Material primitiveMaterial;

    // private bool primitiveIsSelected = false;

    // Position Variables

    // Scaling Variables
    private Vector3 screenPoint;
    private Vector3 offset;

    void Start()
    {
        uIInstanceController = GameObject.Find("UIInstanceControl").GetComponent<UIInstanceController>();
        pClasses = uIInstanceController.PCClasses;


        primitiveGO = gameObject;
        primitiveMaterial = new Material(Shader.Find("Unlit/HideObject"));
    }

    void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // var renderer = gameObject.GetComponentInChildren<BAPointCloudRenderer.CloudController.AbstractPointCloudSet>();
        // Bounds bounds = renderer.GetTightBoundingBoxBounds();

        // if (bounds.IntersectRay(ray))
        // {
        //         Debug.Log("Clicked on " + renderer.name);
        //         Debug.Log("Clicked on " + renderer.gameObject.transform.parent.name);
        // }
        // }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            foreach (DirectoryInstanceLoader.PCInstances clouds in pClasses)
            {
                for (int i = 0; i < clouds.cloudClassGO.transform.childCount; i++)
                {
                    GameObject instanceInClass = clouds.cloudClassGO.transform.GetChild(i).gameObject;
                    var renderer = instanceInClass.GetComponentInChildren<AbstractPointCloudSet>();

                    Bounds b = renderer.GetTightBoundingBoxBounds();

                    // Convert to world space
                    b.center = renderer.transform.TransformPoint(b.center);
                    b.extents = Vector3.Scale(b.extents, renderer.transform.lossyScale);
                    
                    if (b.IntersectRay(ray))
                    {
                        //Debug.Log("Clicked cloud: " + renderer.gameObject.transform.parent.name);
                        if (uIInstanceController.classInstanceUIActive == false)
                        {
                            switch (renderer.gameObject.transform.parent.transform.parent.name)
                            {
                                case "Class: 1":
                                    uIInstanceController.ShowClassInstanceUI(1);
                                    uIInstanceController.classInstanceUIActive = true;
                                    break;
                                case "Class: 2":
                                    uIInstanceController.ShowClassInstanceUI(2);
                                    uIInstanceController.classInstanceUIActive = true;
                                    break;
                                case "Class: 3":
                                    uIInstanceController.ShowClassInstanceUI(3);
                                    uIInstanceController.classInstanceUIActive = true;
                                    break;
                                case "Class: 4":
                                    uIInstanceController.ShowClassInstanceUI(4);
                                    uIInstanceController.classInstanceUIActive = true;
                                    break;
                                case "Class: 5":
                                    uIInstanceController.ShowClassInstanceUI(5);
                                    uIInstanceController.classInstanceUIActive = true;
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
