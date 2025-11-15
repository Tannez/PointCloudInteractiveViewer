using BAPointCloudRenderer.CloudController;
using UnityEngine;
using System.Collections.Generic;
using BAPointCloudRenderer.ObjectCreation;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Linq;

public class CloudInteraction : MonoBehaviour
{
    UIInstanceController uIInstanceController;
    public List<DirectoryInstanceLoader.PCInstances> pClasses = new List<DirectoryInstanceLoader.PCInstances>();
    Ray ray;
    bool classSelectedwithMouse = false;
    bool InstanceSelectedwithMouse = false;

    List<int> selectedClasses = new List<int>();
    List<int> selectedInstances = new List<int>();
    int displayedClassSelection;
    int displayedInstanceSelection; 

    void Start()
    {
        uIInstanceController = GameObject.Find("UIInstanceControl").GetComponent<UIInstanceController>();
        pClasses = uIInstanceController.PCClasses;
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
            uIInstanceController.clickedWithMouse = true;

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

                        // Convert to world space so that each bounding box is tested in world coordinates,
                        // matching where the clouds actually are in the scene.
                        b.center = renderer.transform.TransformPoint(b.center);
                        b.extents = Vector3.Scale(b.extents, renderer.transform.lossyScale);

                        if (b.IntersectRay(ray))
                        {
                            string clickClassTarget = renderer.gameObject.transform.parent.transform.parent.name;
                            string clickInstanceTarget = renderer.gameObject.transform.parent.name;

                            
                            if (clickClassTarget == "Class: 1" && classSelectedwithMouse == false && uIInstanceController.classHidden[0] == false)
                            {
                                selectedClasses.Add(1);
                                classSelectedwithMouse = true;
                                //Debug.Log("Added Class: 1");
                            }
                            else if (clickClassTarget == "Class: 2" && classSelectedwithMouse == false && uIInstanceController.classHidden[1] == false)
                            {
                                selectedClasses.Add(2);
                                classSelectedwithMouse = true;
                                //Debug.Log("Added Class: 2");
                            }
                            else if (clickClassTarget == "Class: 3" && classSelectedwithMouse == false && uIInstanceController.classHidden[2] == false)
                            {
                                selectedClasses.Add(3);
                                classSelectedwithMouse = true;
                                //Debug.Log("Added Class: 3");
                            }
                            else if (clickClassTarget == "Class: 4" && classSelectedwithMouse == false && uIInstanceController.classHidden[3] == false)
                            {
                                selectedClasses.Add(4);
                                classSelectedwithMouse = true;
                                //Debug.Log("Added Class: 4");
                            }
                            else if (clickClassTarget == "Class: 5" && classSelectedwithMouse == false && uIInstanceController.classHidden[4] == false)
                            {
                                selectedClasses.Add(5);
                                classSelectedwithMouse = true;
                                //Debug.Log("Added Class: 5");
                            }
                            

                            if (clickInstanceTarget == "Cloud: 1" && InstanceSelectedwithMouse == false)
                            {
                                selectedInstances.Add(1);
                                InstanceSelectedwithMouse = true;
                                //Debug.Log("Added Cloud: 1");
                            }
                            else if (clickInstanceTarget == "Cloud: 2" && InstanceSelectedwithMouse == false)
                            {
                                selectedInstances.Add(2);
                                InstanceSelectedwithMouse = true;
                                //Debug.Log("Added Cloud: 2");
                            }
                            else if (clickInstanceTarget == "Cloud: 3" && InstanceSelectedwithMouse == false)
                            {
                                selectedInstances.Add(3);
                                InstanceSelectedwithMouse = true;
                                //Debug.Log("Added Cloud: 3");
                            }
                            else if (clickInstanceTarget == "Cloud: 4" && InstanceSelectedwithMouse == false)
                            {
                                selectedInstances.Add(4);
                                InstanceSelectedwithMouse = true;
                                //Debug.Log("Added Cloud: 4");
                            }
                            else if (clickInstanceTarget == "Cloud: 5" && InstanceSelectedwithMouse == false)
                            {
                                selectedInstances.Add(5);
                                InstanceSelectedwithMouse = true;
                                //Debug.Log("Added Cloud: 5");
                            }

                            classSelectedwithMouse = false;
                            InstanceSelectedwithMouse = false;
                        }
                    }
                }

                int classesSelected = selectedClasses.Count;
                int instancesSelected = selectedInstances.Count;

                if (classesSelected > 0 && instancesSelected > 0) // Must have a selected target
                {
                    displayedClassSelection = selectedClasses[classesSelected - 1];
                    displayedInstanceSelection = selectedInstances[instancesSelected - 1];

                    //Debug.Log("Displayed Class: " + displayedClassSelection);
                    //Debug.Log("Displayed Class: " + displayedInstanceSelection);
                    ShowClickedCloud(displayedClassSelection, displayedInstanceSelection);
                }
                // else if (classesSelected >= 0 && instancesSelected > 0) // Must have a selected target
                // {
                //     displayedInstanceSelection = selectedInstances[instancesSelected - 1];

                //     //Debug.Log("Displayed Class: " + displayedClassSelection);
                //     //Debug.Log("Displayed Class: " + displayedInstanceSelection);
                //     uIInstanceController.cloudClassInstanceSelection(instancesSelected);
                // }
                else if (classesSelected == 0 && instancesSelected == 0)
                {
                    uIInstanceController.ResetSelection();
                }
            }  
        }
        uIInstanceController.clickedWithMouse = false;
    }

    private void ShowClickedCloud(int cloudClass, int cloudInstance)
    {
        // IF CLASS INSTANCE UI HAS NOT BEEN OPENED: OPEN IT AND MARK INSTANCE IN CLASS
        if (uIInstanceController.classInstanceUIActive == false && uIInstanceController.clickedWithMouse == true)
        {
            uIInstanceController.ShowClassInstanceUI(cloudClass);
            uIInstanceController.cloudClassInstanceMouseSelection(cloudClass, cloudInstance);
            return;
        }

        // IF CLASS INSTANCE UI IS OPEN: CHECK INSTANCE STATUS
        // IF INSTANCE SELECTED WHILE OTHER INSTANCE IS ACTIVE
        else if (uIInstanceController.classInstanceUIActive == true && uIInstanceController.classInstanceSelected.Contains(true) && uIInstanceController.clickedWithMouse == true)
        {
            // MARK INSTANCE
            uIInstanceController.cloudClassInstanceMouseSelection(uIInstanceController.activeClassInstanceInMenu, cloudInstance);

            // IF NO INSTANCES ARE NO LONGER ACTIVE: CLOSE CLASS INSTANCE UI 
            if (!uIInstanceController.classInstanceSelected.Contains(true))
            {
                uIInstanceController.ShowClassInstanceUI(uIInstanceController.activeClassInstanceInMenu);
            }
            return;
        }
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
