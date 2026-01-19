using UnityEngine;

public class PaintSurface_Quad : PaintSurfaceBase
{
    private Renderer rend;

    protected override void Awake()
    {
        rend = GetComponent<Renderer>();

        // Important: ensure this quad has its own material BEFORE base grabs it
        rend.material = new Material(rend.material);

        base.Awake(); // base creates paintRT and assigns _PaintMask on paintMat

        // Optional: if the visible shader is URP/Lit, also show the RT on the base map
        if (rend.material.HasProperty("_BaseMap"))
            rend.material.SetTexture("_BaseMap", paintRT);

        // Optional fallback for non-URP shaders
        if (rend.material.HasProperty("_MainTex"))
            rend.material.SetTexture("_MainTex", paintRT);

        if (rend.material.HasProperty("_UseUVPaint"))
            rend.material.SetFloat("_UseUVPaint", 1f);
    }

    public override bool CanPaintHit(RaycastHit hit, Vector3 rayDir)
    {
        return Vector3.Dot(hit.normal, -rayDir) > 0f;
    }

    public RenderTexture GetPaintRT() => paintRT;

    public override void PaintAtWorld(RaycastHit hit, Texture2D brush, float size, Color paintColor)
    {
        PaintAtUV(hit.textureCoord, brush, size, paintColor);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Quick proof the displayed RT is the same one we're painting into
            var prev = RenderTexture.active;
            RenderTexture.active = paintRT;
            GL.Clear(true, true, Color.red);
            RenderTexture.active = prev;
        }
    }
#endif
}
