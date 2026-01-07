using UnityEngine;

public class PaintSurface_Mesh : PaintSurfaceBase
{
    public override bool CanPaintHit(RaycastHit hit, Vector3 rayDir)
    {
        // Default mesh behaviour
        return Vector3.Dot(hit.normal, -rayDir) > 0f;
    }
}