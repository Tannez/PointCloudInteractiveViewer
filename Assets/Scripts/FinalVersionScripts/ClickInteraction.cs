using BAPointCloudRenderer.CloudController;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using BAPointCloudRenderer.ObjectCreation;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Linq;
using BAPointCloudRenderer.Loading;
using LLMPCCompanionBubble;

public class ClickInteraction : MonoBehaviour
{
    private static PointCloudControls _pointCloudControls;

    private static PointCloudControls pointCloudControls
    {
        get
        {
            // If already cached and still valid, return it
            if (_pointCloudControls != null)
                return _pointCloudControls;

            // Otherwise, find it in the scene and cache it
            _pointCloudControls = FindFirstObjectByType<PointCloudControls>();

            if (_pointCloudControls == null)
                Debug.LogWarning("PointCloudController not found in scene!");

            return _pointCloudControls;
        }
    }

    public List<DirectoryCloudLoaderFinal.PCInstances> pClasses = new List<DirectoryCloudLoaderFinal.PCInstances>();
    Ray ray;
    bool classSelectedwithMouse = false;
    bool InstanceSelectedwithMouse = false;

    List<int> selectedClasses = new List<int>();
    List<int> selectedInstances = new List<int>();
    int displayedClassSelection;
    int displayedInstanceSelection; 

    void Start()
    {
        //cloudControllerLLM = GameObject.Find("CloudLLMController").GetComponent<CloudControllerLLM>();
        pClasses = pointCloudControls.PCClasses;
    }

    void Update()
    {
        if (pClasses != pointCloudControls.PCClasses)
        {
            pClasses = pointCloudControls.PCClasses;
        }
        if (Input.GetMouseButtonDown(0))
        {
            CloudClickSelection();
        }
    }

    private void CloudClickSelection()
    {
        selectedClasses = new List<int>();
        selectedInstances = new List<int>();
        pointCloudControls.clickedWithMouse = true;

        if (IsPointerOverUI())
        {
            //Debug.Log("Mouse over UI");
            pointCloudControls.keyboardShotcutsEnabled = false;
            return;
        }

        else
        {
            pointCloudControls.keyboardShotcutsEnabled = true;
            StartCoroutine(ClickOnPointCloud());
        }
    }

    private void ShowClickedCloud(int cloudClass, int cloudInstance)
    {
        // IF CLASS INSTANCE UI HAS NOT BEEN OPENED: OPEN IT AND MARK INSTANCE IN CLASS
        if (pointCloudControls.classInstanceUIActive == false && pointCloudControls.clickedWithMouse == true)
        {
            pointCloudControls.ShowClassInstanceUI(cloudClass);
            pointCloudControls.cloudClassInstanceMouseSelection(cloudClass, cloudInstance);
            return;
        }

        // IF CLASS INSTANCE UI IS OPEN: CHECK INSTANCE STATUS
        // IF INSTANCE SELECTED WHILE OTHER INSTANCE IS ACTIVE
        if (pointCloudControls.classInstanceUIActive == true && pointCloudControls.classInstanceSelected.Contains(true) && pointCloudControls.clickedWithMouse == true)
        {
            // MARK INSTANCE
            pointCloudControls.cloudClassInstanceMouseSelection(pointCloudControls.activeClassInstanceInMenu, cloudInstance);
            return;
        }

        // IF NO INSTANCE SELECTED BUT MENU IS OPEN
        if (pointCloudControls.classInstanceUIActive == true && pointCloudControls.clickedWithMouse == true)
        {
            // MARK INSTANCE
            pointCloudControls.cloudClassInstanceMouseSelection(pointCloudControls.activeClassInstanceInMenu, cloudInstance);
            return;
        }

        // IF NO INSTANCES ARE ACTIVE ANYMORE: CLOSE CLASS INSTANCE UI 
        if (!pointCloudControls.classInstanceSelected.Contains(true))
        {
            pointCloudControls.ShowClassInstanceUI(pointCloudControls.activeClassInstanceInMenu);
            return;
        }
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void CheckClickedCloudClass(AbstractPointCloudSet renderer)
    {
        string clickClassTarget = renderer.gameObject.transform.parent.transform.parent.name;

        if (clickClassTarget == "Class: 1" && classSelectedwithMouse == false && pointCloudControls.classHidden[0] == false)
        {
            selectedClasses.Add(1);
            classSelectedwithMouse = true;
            //Debug.Log("Added Class: 1");
        }
        else if (clickClassTarget == "Class: 2" && classSelectedwithMouse == false && pointCloudControls.classHidden[1] == false)
        {
            selectedClasses.Add(2);
            classSelectedwithMouse = true;
            //Debug.Log("Added Class: 2");
        }
        else if (clickClassTarget == "Class: 3" && classSelectedwithMouse == false && pointCloudControls.classHidden[2] == false)
        {
            selectedClasses.Add(3);
            classSelectedwithMouse = true;
            //Debug.Log("Added Class: 3");
        }
        else if (clickClassTarget == "Class: 4" && classSelectedwithMouse == false && pointCloudControls.classHidden[3] == false)
        {
            selectedClasses.Add(4);
            classSelectedwithMouse = true;
            //Debug.Log("Added Class: 4");
        }
        else if (clickClassTarget == "Class: 5" && classSelectedwithMouse == false && pointCloudControls.classHidden[4] == false)
        {
            selectedClasses.Add(5);
            classSelectedwithMouse = true;
            //Debug.Log("Added Class: 5");
        }
        else if (clickClassTarget == "Class: 6" && classSelectedwithMouse == false && pointCloudControls.classHidden[5] == false)
        {
            selectedClasses.Add(6);
            classSelectedwithMouse = true;
            //Debug.Log("Added Class: 6");
        }

        classSelectedwithMouse = false;
        return;
    }
    private void CheckClickedCloudInstance(AbstractPointCloudSet renderer)
    {
        string clickInstanceTarget = renderer.gameObject.transform.parent.name;

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

        InstanceSelectedwithMouse = false;
        return;
    }

    public IEnumerator ClickOnPointCloud()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            for (int classInstance = 0; classInstance < pClasses.Count; classInstance++)
            {
                DirectoryCloudLoaderFinal.PCInstances clouds = pClasses[classInstance];
                for (int i = 0; i < clouds.cloudClassGO.transform.childCount; i++)
                {
                    GameObject instanceInClass = clouds.cloudClassGO.transform.GetChild(i).gameObject;
                    AbstractPointCloudSet renderer = instanceInClass.GetComponentInChildren<AbstractPointCloudSet>();
                    Bounds b = renderer.GetTightBoundingBoxBounds();

                    // Convert to world space so that each bounding box is tested in world coordinates,
                    // matching where the clouds actually are in the scene.
                    b.center = renderer.transform.TransformPoint(b.center);
                    b.extents = Vector3.Scale(b.extents, renderer.transform.lossyScale);

                    if (b.IntersectRay(ray))
                    {
                        CheckClickedCloudClass(renderer);
                        CheckClickedCloudInstance(renderer);
                    }
                }
            }

            int classesSelected = selectedClasses.Count;
            int instancesSelected = selectedInstances.Count;

            if (classesSelected > 0 && instancesSelected > 0) // Must have a selected target
            {
                displayedClassSelection = selectedClasses[classesSelected/2];
                displayedInstanceSelection = selectedInstances[instancesSelected/2];

                //Debug.Log("Displayed Class: " + displayedClassSelection);
                //Debug.Log("Displayed Class: " + displayedInstanceSelection);
                ShowClickedCloud(displayedClassSelection, displayedInstanceSelection);
            }
            
            else if (classesSelected == 0 && instancesSelected == 0)
            {
                pointCloudControls.ResetSelection();
                pointCloudControls.StartCoroutine(pointCloudControls.ReloadClouds());
            }

        pointCloudControls.clickedWithMouse = false;
        yield return null;
    }
}

