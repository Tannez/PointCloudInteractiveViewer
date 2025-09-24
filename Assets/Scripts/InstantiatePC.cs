using UnityEngine;

public class InstantiatePC : MonoBehaviour
{
    // Instantiater Game Object
    public GameObject PCInstatiater;

    // Prefabs 
    public GameObject bygværkPC;
    public GameObject terrænPC;

    public bool instatiateByg = false;
    public bool instantiateTerræn = false;

    void Update()
    {
        InstatiateBygværkPC();
        InstatiateTerrænPC();
    }
    public void InstatiateBygværkPC()
    {
        if (instatiateByg)
        {
            Instantiate(bygværkPC, new Vector3(0, 0, 0), Quaternion.identity);
            instatiateByg = false;
            Debug.Log("Bygværk Instantiated");
        }
    }

    public void InstatiateTerrænPC()
    {
        if (instantiateTerræn)
        {
            Instantiate(terrænPC, new Vector3(0, 0, 0), Quaternion.identity);
            instantiateTerræn = false;
            Debug.Log("Terræn Instantiated");
        }
    }
}
