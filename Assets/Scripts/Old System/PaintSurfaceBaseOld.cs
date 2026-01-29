using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]

public abstract class PaintSurfaceBaseOld : MonoBehaviour
{
    [Header("Surface Paint")]
    public int textureSize = 512;

    protected RenderTexture paintRT;
    protected Material paintMat;

    protected virtual void Awake()
    {
        paintMat = GetComponent<Renderer>().material;

        paintRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        paintRT.wrapMode = TextureWrapMode.Clamp;
        paintRT.filterMode = FilterMode.Bilinear;
        paintRT.useMipMap = false;
        paintRT.autoGenerateMips = false;
        paintRT.Create();

        var prev = RenderTexture.active;
        RenderTexture.active = paintRT;
        GL.Clear(false, true, Color.clear);
        RenderTexture.active = prev;

        if (paintMat.HasProperty("_PaintMask"))
            paintMat.SetTexture("_PaintMask", paintRT);
        else
            Debug.LogError($"{name} material missing _PaintMask");
    }

    public abstract bool CanPaintHit(RaycastHit hit, Vector3 rayDir);

    public virtual bool TryGetPaintUV(RaycastHit hit, out Vector2 uv)
    {
        uv = hit.textureCoord;
        return true;
    }

    public void PaintAtUV(Vector2 uv, Texture2D brush, float size)
    {
        uv = new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));

        int px = Mathf.RoundToInt(uv.x * paintRT.width);
        int py = Mathf.RoundToInt(uv.y * paintRT.height);
        int drawSize = Mathf.RoundToInt(size);

        var prev = RenderTexture.active;
        RenderTexture.active = paintRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, paintRT.width, 0, paintRT.height);

        Graphics.DrawTexture(
            new Rect(px - drawSize * 0.5f, py - drawSize * 0.5f, drawSize, drawSize),
            brush
        );

        GL.PopMatrix();
        RenderTexture.active = prev;
    }
}
