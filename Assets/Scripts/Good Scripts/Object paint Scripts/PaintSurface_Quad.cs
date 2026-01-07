using UnityEngine;

public class PaintSurface_Quad : PaintSurfaceBase
{
    public override bool CanPaintHit(RaycastHit hit, Vector3 rayDir)
    {
        // Flat surfaces: simple facing check
        return Vector3.Dot(hit.normal, -rayDir) > 0f;
    }
}