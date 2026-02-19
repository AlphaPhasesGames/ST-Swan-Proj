using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public abstract class PaintSurfaceBase : MonoBehaviour
{
    [Header("Surface Paint")]
    public int textureSize = 1024;

    protected RenderTexture paintRT;
    protected Material paintMat;
    public abstract RenderTexture GetPaintRT();
    [Header("Stamp Mode")]
    public bool hardStamp = false;
    //public bool isDiluting;
    [Header("Legacy Paint")]
    public float legacyBrushSize = 64f;
    public float legacyMinSize = 1f;
    public float legacyMaxSize = 256f;

    public bool allowLegacyPaint = true;
    public bool allowSprayPaint = true;

    // Triplanar RTs
    protected RenderTexture paintRT_PosX, paintRT_NegX;
    protected RenderTexture paintRT_PosY, paintRT_NegY;
    protected RenderTexture paintRT_PosZ, paintRT_NegZ;

    [Header("Triplanar Scale")]
    public float triplanarTiling = 1f;

    [Header("Brush Size Override")]
    public bool overrideBrushSize = false;

    [Tooltip("World-space brush size for this object")]
    public float surfaceBrushWorldSize = 0.25f;

    public RenderTexture GetUVPaintRT() => paintRT;

    [Header("Stamp Materials (assign in inspector)")]
    [SerializeField] private Material stampMatPaint; // Custom/PaintStampColor
    [SerializeField] private Material stampMatErase; // Custom/PaintStampErase
    [SerializeField] private Material stampMatWater;
    [SerializeField] private Material stampMatDilute;
    private Texture2D smudgeBuffer;

    // ------------------- UNITY LIFECYCLE -------------------

    protected virtual void Awake()
    {
        if (!stampMatPaint)
            stampMatPaint = Resources.Load<Material>("Paint/Stamp_Paint");

        if (!stampMatErase)
            stampMatErase = Resources.Load<Material>("Paint/Stamp_Erase");

        if (!stampMatWater)
            stampMatWater = Resources.Load<Material>("Paint/Stamp_Paint_Water");

        if (!stampMatPaint || !stampMatErase || !stampMatWater || !stampMatDilute)
        {
            Debug.LogError($"{name}: Stamp materials missing");
            enabled = false;
            return;
        }

        paintMat = GetComponent<Renderer>().material;

        if (paintMat.HasProperty("_WorldScale"))
            paintMat.SetFloat("_WorldScale", triplanarTiling);

        paintRT = CreatePaintRT();

        if (paintMat.HasProperty("_PaintMask"))
            paintMat.SetTexture("_PaintMask", paintRT);

        var bounds = GetComponent<Renderer>().localBounds;
        if (paintMat.HasProperty("_BoundsMin")) paintMat.SetVector("_BoundsMin", bounds.min);
        if (paintMat.HasProperty("_BoundsSize")) paintMat.SetVector("_BoundsSize", bounds.size);

        paintRT_PosX = CreatePaintRT();
        paintRT_NegX = CreatePaintRT();
        paintRT_PosY = CreatePaintRT();
        paintRT_NegY = CreatePaintRT();
        paintRT_PosZ = CreatePaintRT();
        paintRT_NegZ = CreatePaintRT();

        if (paintMat.HasProperty("_PaintPosX"))
        {
            paintMat.SetTexture("_PaintPosX", paintRT_PosX);
            paintMat.SetTexture("_PaintNegX", paintRT_NegX);
            paintMat.SetTexture("_PaintPosY", paintRT_PosY);
            paintMat.SetTexture("_PaintNegY", paintRT_NegY);
            paintMat.SetTexture("_PaintPosZ", paintRT_PosZ);
            paintMat.SetTexture("_PaintNegZ", paintRT_NegZ);
        }
    }

    RenderTexture CreatePaintRT()
    {
        var rt = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.filterMode = FilterMode.Trilinear;
        rt.useMipMap = false;
        rt.autoGenerateMips = false;
        rt.Create();

        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(false, true, Color.clear);
        RenderTexture.active = prev;

        return rt;
    }

    // ------------------- ABSTRACT -------------------

    public abstract bool CanPaintHit(RaycastHit hit, Vector3 rayDir);

    public virtual bool TryGetPaintUV(RaycastHit hit, out Vector2 uv)
    {
        uv = hit.textureCoord;
        return true;
    }

    public virtual float GetLegacyBrushSize() =>
        Mathf.Clamp(legacyBrushSize, legacyMinSize, legacyMaxSize);

    public float GetSurfaceBrushSize() =>
        Mathf.Max(0.001f, surfaceBrushWorldSize);

    // ------------------- PUBLIC API (NEW CORE) -------------------

    public void PaintAtUV(Vector2 uv, Texture2D brush, float size, Color color, bool erase)
    {
        bool precision = size <= 2.5f;
        paintRT.filterMode = precision ? FilterMode.Trilinear : FilterMode.Bilinear;

        Stamp(paintRT, uv, brush, size, color, erase, false);
    }

    public virtual void PaintAtWorld(
        RaycastHit hit,
        Texture2D brush,
        float size,
        Color color,
        bool erase,
        bool dilute
    )
    {
        if (TryGetPaintUV(hit, out var uv))
            Stamp(paintRT, uv, brush, size, color, erase, dilute);

        PaintAtWorld(hit.point, hit.normal, brush, size, color, erase, dilute);
    }

    public virtual void PaintAtWorld(
    Vector3 worldPos,
    Vector3 normal,
    Texture2D brush,
    float size,
    Color color,
    bool erase,
    bool dilute
)
    {
        bool precision = size <= 2.5f;

        SetRTFiltering(paintRT_PosX, precision);
        SetRTFiltering(paintRT_NegX, precision);
        SetRTFiltering(paintRT_PosY, precision);
        SetRTFiltering(paintRT_NegY, precision);
        SetRTFiltering(paintRT_PosZ, precision);
        SetRTFiltering(paintRT_NegZ, precision);

        Vector3 nL = transform.InverseTransformDirection(normal).normalized;

        float wx = Mathf.Abs(nL.x);
        float wy = Mathf.Abs(nL.y);
        float wz = Mathf.Abs(nL.z);
        float sum = wx + wy + wz;
        if (sum < 0.0001f) return;

        wx /= sum;
        wy /= sum;
        wz /= sum;

        PaintOnPlane(nL.x >= 0 ? paintRT_PosX : paintRT_NegX, worldPos, Axis.X, nL, brush, size * wx, color, erase, dilute);
        PaintOnPlane(nL.y >= 0 ? paintRT_PosY : paintRT_NegY, worldPos, Axis.Y, nL, brush, size * wy, color, erase, dilute);
        PaintOnPlane(nL.z >= 0 ? paintRT_PosZ : paintRT_NegZ, worldPos, Axis.Z, nL, brush, size * wz, color, erase, dilute);
    }


    enum Axis { X, Y, Z }

    void PaintOnPlane(
        RenderTexture rt,
        Vector3 worldPos,
        Axis axis,
        Vector3 localNormal,
        Texture2D brush,
        float size,
        Color color,
        bool erase,
        bool dilute
    )
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        Bounds b = GetComponent<Renderer>().localBounds;

        Vector2 uv;

        switch (axis)
        {
            case Axis.X:
                uv = new Vector2(
                    Mathf.InverseLerp(b.min.z, b.max.z, localPos.z),
                    Mathf.InverseLerp(b.min.y, b.max.y, localPos.y)
                );
                if (localNormal.x < 0) uv.x = 1f - uv.x;
                break;

            case Axis.Y:
                uv = new Vector2(
                    Mathf.InverseLerp(b.min.x, b.max.x, localPos.x),
                    Mathf.InverseLerp(b.min.z, b.max.z, localPos.z)
                );
                if (localNormal.y < 0) uv.y = 1f - uv.y;
                break;

            default:
                uv = new Vector2(
                    Mathf.InverseLerp(b.min.x, b.max.x, localPos.x),
                    Mathf.InverseLerp(b.min.y, b.max.y, localPos.y)
                );
                if (localNormal.z < 0) uv.x = 1f - uv.x;
                break;
        }

        Stamp(rt, uv, brush, size, color, erase, dilute);
    }

    // ------------------- CORE STAMP (FIXED) -------------------

    void Stamp(RenderTexture targetRT, Vector2 uv, Texture2D brush,
           float size, Color paintColor, bool erase, bool dilute)
    {
        if (!targetRT || !brush) return;

        Material mat;

        if (erase)
        {
            mat = stampMatErase;
        }
        else if (dilute)
        {
            mat = stampMatDilute;
        }
        else if (paintColor.a < 1f) // watercolour
        {
            mat = stampMatWater;
        }
        else
        {
            mat = stampMatPaint;
        }
        if (!mat) return;

        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        float px = uv.x * targetRT.width;
        float py = uv.y * targetRT.height;

        float drawSize = Mathf.Max(1f, size);
        float half = drawSize * 0.5f;

        Rect rect = new Rect(px - half, py - half, drawSize, drawSize);

        var prev = RenderTexture.active;
        RenderTexture.active = targetRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, targetRT.width, 0, targetRT.height);

        mat.SetTexture("_MainTex", brush);

        if (!erase)
        {
            //  IMPORTANT FIX:
            // Do NOT force alpha here — premultiplied alpha depends on brush alpha
            mat.SetColor("_Color", paintColor);
            mat.SetFloat("_HardStamp", hardStamp ? 1f : 0f);
        }

        Graphics.DrawTexture(rect, brush, mat);

        GL.PopMatrix();
        RenderTexture.active = prev;
    }

    void SetRTFiltering(RenderTexture rt, bool precision)
    {
        if (!rt) return;
        rt.filterMode = precision ? FilterMode.Point : FilterMode.Trilinear;
    }

    // ------------------- BACKWARD COMPATIBILITY -------------------

    public void PaintAtUV(Vector2 uv, Texture2D brush, float size, Color color)
    {
        PaintAtUV(uv, brush, size, color, false);
    }

    public virtual void PaintAtWorld(
     RaycastHit hit,
     Texture2D brush,
     float size,
     Color color
 )
    {
        PaintAtWorld(hit, brush, size, color, false, false);
    }

    public virtual void PaintAtWorld(
        Vector3 worldPos,
        Vector3 normal,
        Texture2D brush,
        float size,
        Color color
    )
    {
        PaintAtWorld(worldPos, normal, brush, size, color, false, false);
    }


    public void CopyPaintFrom(
    PaintSurfaceBase source,
    bool copyTriplanar = true,
    bool copyUV = true
)
    {
        if (source == null)
        {
            Debug.LogError($"{name}: CopyPaintFrom source is null");
            return;
        }

        // --- UV paint copy ---
        if (copyUV && source.paintRT != null && paintRT != null)
        {
            if (!paintRT.IsCreated())
                paintRT.Create();

            Graphics.Blit(source.paintRT, paintRT);
        }

        // --- Triplanar paint copy ---
        if (copyTriplanar)
        {
            BlitSafe(source.paintRT_PosX, paintRT_PosX);
            BlitSafe(source.paintRT_NegX, paintRT_NegX);
            BlitSafe(source.paintRT_PosY, paintRT_PosY);
            BlitSafe(source.paintRT_NegY, paintRT_NegY);
            BlitSafe(source.paintRT_PosZ, paintRT_PosZ);
            BlitSafe(source.paintRT_NegZ, paintRT_NegZ);
        }
    }

    static void BlitSafe(RenderTexture src, RenderTexture dst)
    {
        if (src == null || dst == null) return;
        if (!dst.IsCreated()) dst.Create();
        Graphics.Blit(src, dst);
    }

    //Sampling colour from the canvas, then re-applying it slightly offset in the direction of movement.
    public void ApplySmudgeAtUV(
        Vector2 uv,
        Vector2 delta,
        float worldBrushSize,
        float strength,
        float sampleDistance,
        float falloffPower)
    {
        if (paintRT == null) return;

        int texSize = paintRT.width;

        if (smudgeBuffer == null || smudgeBuffer.width != texSize)
        {
            smudgeBuffer = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        }

        var prev = RenderTexture.active;
        RenderTexture.active = paintRT;

        smudgeBuffer.ReadPixels(new Rect(0, 0, texSize, texSize), 0, 0);
        smudgeBuffer.Apply();

        int centerX = Mathf.RoundToInt(uv.x * texSize);
        int centerY = Mathf.RoundToInt(uv.y * texSize);

        int radius = Mathf.Max(1, Mathf.RoundToInt(worldBrushSize * texSize * 0.5f));

        Vector2 pixelDelta = delta * texSize * sampleDistance;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int px = centerX + x;
                int py = centerY + y;

                if (px < 0 || px >= texSize || py < 0 || py >= texSize)
                    continue;

                float dist = Mathf.Sqrt(x * x + y * y);
                float t = Mathf.Clamp01(1f - (dist / radius));
                t = Mathf.Pow(t, falloffPower);

                int sampleX = Mathf.Clamp(
                    Mathf.RoundToInt(px - pixelDelta.x),
                    0, texSize - 1
                );

                int sampleY = Mathf.Clamp(
                    Mathf.RoundToInt(py - pixelDelta.y),
                    0, texSize - 1
                );

                Color current = smudgeBuffer.GetPixel(px, py);
                Color sampled = smudgeBuffer.GetPixel(sampleX, sampleY);

                Color blended = Color.Lerp(current, sampled, strength * t);

                smudgeBuffer.SetPixel(px, py, blended);
            }
        }

        smudgeBuffer.Apply();
        Graphics.Blit(smudgeBuffer, paintRT);

        RenderTexture.active = prev;
    }

}
