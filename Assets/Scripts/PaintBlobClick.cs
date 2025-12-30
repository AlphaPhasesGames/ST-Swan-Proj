using UnityEngine;

public class PaintBlobClick : MonoBehaviour
{
    private Material mat;
    public float paintRadius = 0.25f;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.SetFloat("_PaintRadius", paintRadius);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                mat.SetVector("_PaintPos", hit.point);
            }
        }
    }
}
