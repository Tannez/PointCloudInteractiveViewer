using BAPointCloudRenderer.CloudController;
using UnityEngine;
using System.Collections.Generic;
using BAPointCloudRenderer.ObjectCreation;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CloudInteraction : MonoBehaviour
{
    UIInstanceController uIInstanceController;
    public List<DirectoryInstanceLoader.PCInstances> pClasses = new List<DirectoryInstanceLoader.PCInstances>();
    // Primitive Variables
    [SerializeField] private GameObject primitiveGO;
    [SerializeField] private Material primitiveMaterial;
    Ray ray;
    bool classSelectedwithMouse = false;

    bool[] cIInClassSelected = new bool[10];

    List<int> selectedClasses = new List<int>();
    List<int> selectedInstances = new List<int>();
    int displayedClassSelection;
    int displayedInstanceSelection;

    void Start()
    {
        uIInstanceController = GameObject.Find("UIInstanceControl").GetComponent<UIInstanceController>();
        pClasses = uIInstanceController.PCClasses;


        primitiveGO = gameObject;
        primitiveMaterial = new Material(Shader.Find("Unlit/HideObject"));
    }

    void Update()
    {
        CloudClickSelection();
    }

    // First attempt to get mouse click to work with bounds. Only worked on first instance
    // private void CloudClickSelectionSingleInstance()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    //     var renderer = gameObject.GetComponentInChildren<BAPointCloudRenderer.CloudController.AbstractPointCloudSet>();
    //     Bounds bounds = renderer.GetTightBoundingBoxBounds();

    //     if (bounds.IntersectRay(ray))
    //     {
    //             Debug.Log("Clicked on " + renderer.name);
    //             Debug.Log("Clicked on " + renderer.gameObject.transform.parent.name);
    //     }
    //     }
    // }

    private void CloudClickSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectedClasses = new List<int>();
            selectedInstances = new List<int>();

            if (IsPointerOverUI())
            {
                //Debug.Log("Mouse over UI");
                return;
            }

            else
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

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
                            string clickClassTarget = renderer.gameObject.transform.parent.transform.parent.name;
                            string clickInstanceTarget = renderer.gameObject.transform.parent.name;
                            //Debug.Log("Clicked cloud: " + clickInstanceTarget + " of Class: " + clickClassTarget);

                            // Class Selection
                            // if (clickClassTarget == "Class: 1" && clickInstanceTarget == "Cloud: 1")
                            // {
                            //     uIInstanceController.ShowClassInstanceUI(1);
                            //     uIInstanceController.SelectCloudClassInstance(1, 1);
                            // }
                            // else if (clickClassTarget == "Class: 1" && clickInstanceTarget == "Cloud: 2")
                            // {
                            //     uIInstanceController.ShowClassInstanceUI(1);
                            //     uIInstanceController.SelectCloudClassInstance(1, 2);
                            // }
                            if (clickClassTarget == "Class: 1" && classSelectedwithMouse == false)
                            {
                                selectedClasses.Add(1);
                                classSelectedwithMouse = true;
                                //Debug.Log("Added Class: 1");
                            }
                            else if (clickClassTarget == "Class: 2" && classSelectedwithMouse == false)
                            {
                                selectedClasses.Add(2);
                                classSelectedwithMouse = true;
                                //Debug.Log("Added Class: 2");
                            }
                            else if (clickClassTarget == "Class: 3" && classSelectedwithMouse == false)
                            {
                                selectedClasses.Add(3);
                                classSelectedwithMouse = true;
                                //Debug.Log("Added Class: 3");
                            }
                            else if (clickClassTarget == "Class: 4" && classSelectedwithMouse == false)
                            {
                                selectedClasses.Add(4);
                                classSelectedwithMouse = true;
                                //Debug.Log("Added Class: 4");
                            }
                            else if (clickClassTarget == "Class: 5" && classSelectedwithMouse == false)
                            {
                                selectedClasses.Add(5);
                                classSelectedwithMouse = true;
                                //Debug.Log("Added Class: 5");
                            }

                            if (clickInstanceTarget == "Cloud: 1")
                            {
                                selectedInstances.Add(1);
                                //Debug.Log("Added Cloud: 1");
                            }
                            else if (clickInstanceTarget == "Cloud: 2")
                            {
                                selectedInstances.Add(2);
                                //Debug.Log("Added Cloud: 2");
                            }
                            else if (clickInstanceTarget == "Cloud: 3")
                            {
                                selectedInstances.Add(3);
                                //Debug.Log("Added Cloud: 3");
                            }
                            else if (clickInstanceTarget == "Cloud: 4")
                            {
                                selectedInstances.Add(4);
                                //Debug.Log("Added Cloud: 4");
                            }
                            else if (clickInstanceTarget == "Cloud: 5")
                            {
                                selectedInstances.Add(5);
                                //Debug.Log("Added Cloud: 5");
                            }

                            classSelectedwithMouse = false;
                        }
                    }
                }

                int classesSelected = selectedClasses.Count;
                int instancesSelected = selectedInstances.Count;

                displayedClassSelection = selectedClasses[classesSelected-1];
                displayedInstanceSelection = selectedInstances[instancesSelected-1];

                //Debug.Log("Displayed Class: " + displayedClassSelection);
                //Debug.Log("Displayed Class: " + displayedInstanceSelection);

                ShowClickedCloud(displayedClassSelection, displayedInstanceSelection);
            }
        }
    }

    private void ShowClickedCloud(int cloudClass, int cloudInstance)
    {
        if (uIInstanceController.classSelected[cloudClass - 1] == false)
        {
            uIInstanceController.ShowClassInstanceUI(cloudClass);
            uIInstanceController.cloudClassInstanceSelection(cloudInstance);
        }
        else if (uIInstanceController.classSelected[cloudClass - 1] == true)
        {
            if (uIInstanceController.classInstanceSelected[cloudInstance - 1] == false)
            {
                uIInstanceController.cloudClassInstanceSelection(cloudInstance);
            }
            else if (uIInstanceController.classInstanceSelected[cloudInstance - 1] == true)
            {
                uIInstanceController.cloudClassInstanceSelection(cloudInstance);
                uIInstanceController.ShowClassInstanceUI(cloudClass); 
            }
        }  
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
