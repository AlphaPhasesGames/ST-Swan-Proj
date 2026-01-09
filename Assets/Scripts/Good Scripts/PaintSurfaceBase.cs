using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public abstract class PaintSurfaceBase : MonoBehaviour
{
    // --------------------------------------------------
    // PAINT RT (OPTION A)
    // --------------------------------------------------

    [Header("Paint RT")]
    public int textureSize = 512;

    protected RenderTexture paintRT;
    protected Material surfaceMat;
    protected Material drawMat;

    // --------------------------------------------------
    // LEGACY SUPPORT (DO NOT REMOVE)
    // --------------------------------------------------

    [Header("Legacy Paint")]
    public float legacyBrushSize = 64f;
    public float legacyMinSize = 1f;
    public float legacyMaxSize = 256f;

    public bool allowLegacyPaint = true;
    public bool allowSprayPaint = true;

    public virtual float GetLegacyBrushSize()
    {
        return Mathf.Clamp(legacyBrushSize, legacyMinSize, legacyMaxSize);
    }

    // --------------------------------------------------
    // COLOUR SYSTEM (INDEX + COLOR)
    // --------------------------------------------------

    // Existing code still uses indices
    public static int CurrentColourIndex = 0;

    // Actual paint colour used by Option A
    public static Color CurrentPaintColor = Color.red;

    // Optional palette (assign in Inspector or elsewhere)
    public static Color[] GlobalPalette;

    // --------------------------------------------------
    // UNITY LIFECYCLE
    // --------------------------------------------------

    protected virtual void Awake()
    {
        surfaceMat = GetComponent<Renderer>().material;

        // Create RenderTexture
        paintRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        paintRT.wrapMode = TextureWrapMode.Clamp;
        paintRT.filterMode = FilterMode.Bilinear;
        paintRT.useMipMap = false;
        paintRT.autoGenerateMips = false;
        paintRT.Create();

        // Clear RT ONCE
        var prev = RenderTexture.active;
        RenderTexture.active = paintRT;
        GL.Clear(false, true, Color.clear);
        RenderTexture.active = prev;

        // Bind RT to surface shader
        surfaceMat.SetTexture("_PaintTex", paintRT);

        // Create draw material
        drawMat = new Material(Shader.Find("Custom/PaintRT_DrawColor"));
    }

    // --------------------------------------------------
    // PAINTING
    // --------------------------------------------------

    public abstract bool CanPaintHit(RaycastHit hit, Vector3 rayDir);

    public virtual bool TryGetPaintUV(RaycastHit hit, out Vector2 uv)
    {
        uv = hit.textureCoord;
        return true;
    }

    public void PaintAtUV(Vector2 uv, Texture2D brush, float size)
    {
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        int px = Mathf.RoundToInt(uv.x * paintRT.width);
        int py = Mathf.RoundToInt(uv.y * paintRT.height);
        int drawSize = Mathf.RoundToInt(size);

        drawMat.SetColor("_Color", CurrentPaintColor);

        var prev = RenderTexture.active;
        RenderTexture.active = paintRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, paintRT.width, 0, paintRT.height);

        Graphics.DrawTexture(
            new Rect(
                px - drawSize * 0.5f,
                py - drawSize * 0.5f,
                drawSize,
                drawSize
            ),
            brush,
            drawMat
        );

        GL.PopMatrix();
        RenderTexture.active = prev;
    }

    // --------------------------------------------------
    // COMPATIBILITY API (IMPORTANT)
    // --------------------------------------------------

    // Called by ColorWheelSelectorOuter
    public static void SetColourIndex(int index)
    {
        CurrentColourIndex = index;

        // If a palette exists, map index  color
        if (GlobalPalette != null &&
            index >= 0 &&
            index < GlobalPalette.Length)
        {
            CurrentPaintColor = GlobalPalette[index];
        }
    }

    // Optional direct colour set (newer code can use this)
    public static void SetPaintColor(Color c)
    {
        CurrentPaintColor = c;
    }
}
