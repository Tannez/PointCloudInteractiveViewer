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

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                Debug.Log(hit.transform.name);
                Debug.Log("hit");
            }
        }
    }
}
