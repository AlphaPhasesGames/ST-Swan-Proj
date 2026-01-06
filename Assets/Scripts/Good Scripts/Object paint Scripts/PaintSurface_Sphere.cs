using UnityEngine;

public class PaintSurface_Sphere : PaintSurfaceBase
{
    public override bool TryGetPaintUV(RaycastHit hit, out Vector2 uv)
    {
        uv = hit.textureCoord;
        return true;
    }
}