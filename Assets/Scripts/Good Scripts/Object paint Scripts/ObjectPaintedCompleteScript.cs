using UnityEngine;

public class ObjectPaintedCompleteScript : MonoBehaviour
{

    public PaintCoverageMesh paintMeshScript;
    public bool runOnce;
    public GameObject tick;
    private void Update()
    {
        if (!runOnce)
        {
            if (paintMeshScript.CoveragePercent > 53)
            {
                tick.gameObject.SetActive(true);
            }
        }

    }

}
