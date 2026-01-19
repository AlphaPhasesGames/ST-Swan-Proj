using UnityEngine;

public class PaintSurface_Quad : PaintSurfaceBase
{
    public override bool CanPaintHit(RaycastHit hit, Vector3 rayDir)
    {
        return Vector3.Dot(hit.normal, -rayDir) > 0f;
    }

    public RenderTexture GetPaintRT()
    {
        return paintRT;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            DebugFillPaintRT();
            DebugShowPaintRT();
        }
    }
    public void DebugShowPaintRT()
    {
        var r = GetComponent<Renderer>();
        r.material.mainTexture = paintRT;
    }

    public void DebugFillPaintRT()
    {
        RenderTexture rt = paintRT;

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        GL.Clear(false, true, Color.red);

        RenderTexture.active = prev;
    }
    public override void PaintAtWorld(
        RaycastHit hit,
        Texture2D brush,
        float size,
        Color paintColor)
    {
        PaintAtUV(hit.textureCoord, brush, size, paintColor);
    }
}


/*
public class PaintSurface_Quad : PaintSurfaceBase
{
    private RenderTexture bakeRT;

    protected override void Awake()
    {
        base.Awake();

        bakeRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        bakeRT.Create();
    }

    public void BakeFinalPaint()
    {
        // IMPORTANT: use the SAME material instance that shows the paint
        Material liveMat = GetComponent<Renderer>().material;

        // Ensure the paint RT is bound
        if (liveMat.HasProperty("_PaintMask"))
            liveMat.SetTexture("_PaintMask", paintRT);

        Graphics.Blit(null, bakeRT, liveMat);
    }

    public RenderTexture GetBakeRT()
    {
        return bakeRT;
    }

    public override bool CanPaintHit(RaycastHit hit, Vector3 rayDir)
    {
        return Vector3.Dot(hit.normal, -rayDir) > 0f;
    }
}*/