using UnityEngine;

public class PaintSurface_Sphere : PaintSurfaceBase
{
    [Range(0f, 1f)]
    public float facingThreshold = 0.15f;

    public override bool CanPaintHit(RaycastHit hit, Vector3 rayDir)
    {
        float facing = Vector3.Dot(hit.normal, -rayDir);
        return facing > facingThreshold;
    }
}