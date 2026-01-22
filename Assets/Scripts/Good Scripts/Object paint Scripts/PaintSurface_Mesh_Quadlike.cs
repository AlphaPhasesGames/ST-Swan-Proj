using UnityEngine;

public class PaintSurface_Mesh_QuadLike : PaintSurfaceBase
{
    private Renderer rend;

    protected override void Awake()
    {
        rend = GetComponent<Renderer>();
        rend.material = new Material(rend.material);
        base.Awake();

        if (rend.material.HasProperty("_BaseMap"))
            rend.material.SetTexture("_BaseMap", paintRT);

        if (rend.material.HasProperty("_MainTex"))
            rend.material.SetTexture("_MainTex", paintRT);

        if (rend.material.HasProperty("_UseUVPaint"))
            rend.material.SetFloat("_UseUVPaint", 1f);
    }

    public override bool CanPaintHit(RaycastHit hit, Vector3 rayDir)
    {
        return Vector3.Dot(hit.normal, -rayDir) > 0f;
    }

    public override void PaintAtWorld(
        RaycastHit hit,
        Texture2D brush,
        float size,
        Color paintColor
    )
    {
        PaintAtUV(hit.textureCoord, brush, size, paintColor);
    }
}
