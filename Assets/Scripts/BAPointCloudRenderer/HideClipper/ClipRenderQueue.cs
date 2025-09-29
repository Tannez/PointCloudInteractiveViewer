using UnityEngine;

// Sets the Render queue of an object's material on Awake. This will instance
// the materials, so the script won't interfere with other renderers that
// reference the same materials.

[AddComponentMenu("Rendering/ClipRenderQueue")]

public class ClipRenderQueue : MonoBehaviour
{
    [SerializeField] protected int[] m_queues = new int[] { 3000 };

    protected void Awake()
    {
        Material[] materials = GetComponent<Renderer>().materials;
        for (int i = 0; i < materials.Length && i < m_queues.Length; i++)
        {
            materials[i].renderQueue = m_queues[i];
        }
    }
}
