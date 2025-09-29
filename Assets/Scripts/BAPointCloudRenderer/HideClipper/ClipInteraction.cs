using UnityEngine;
using System.Collections;

// Script Inspiration https://discussions.unity.com/t/drag-gameobject-with-mouse/1798/8
[RequireComponent(typeof(MeshCollider))]

public class ClipInteraction : MonoBehaviour
{
    // Primitive Variables
    [SerializeField] private GameObject primitiveGO;
    [SerializeField] private Material primitiveMaterial;
    private bool primitiveIsSelected = false;

    // Position Variables

    // Scaling Variables
    private Vector3 screenPoint;
    private Vector3 offset;

    void Start()
    {
        primitiveMaterial = new Material(Shader.Find("Unlit/HideObject")); 
    }

    void Update()
    {
        if (primitiveIsSelected)
        {
            primitiveMaterial.SetFloat("_Toloerance", 1);
        }
        else
        {
            primitiveMaterial.SetFloat("_Toloerance", 0);
        }
    }

    void OnMouseDown()
    {
        primitiveIsSelected = true;

        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }
    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = curPosition;
    }

    void OnMouseUp()
    {
        primitiveIsSelected = false;
    }
}
