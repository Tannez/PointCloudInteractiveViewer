using UnityEngine;

[RequireComponent(typeof(MeshCollider))]

public class CloudInteraction : MonoBehaviour
{
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
        primitiveGO = gameObject;
        primitiveMaterial = new Material(Shader.Find("Unlit/HideObject"));
    }

    // void Update()
    // {

    // }

    void OnMouseDown()
    {
        // primitiveIsSelected = true;

        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }
    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = curPosition;
    }

    // void OnMouseUp()
    // {
    //     // primitiveIsSelected = false;
    // }
}
