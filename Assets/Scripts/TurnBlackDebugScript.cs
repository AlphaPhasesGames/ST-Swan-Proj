using UnityEngine;

public class TurnBlackDebugScript : MonoBehaviour
{
    public Material mat;

    void Start()
    {
        // Get the live material instance
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            mat.SetFloat("_PaintStength", 1);
        }
    }
}
