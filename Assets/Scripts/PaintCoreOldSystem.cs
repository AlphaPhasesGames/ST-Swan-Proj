using UnityEngine;

public class PaintCoreOldSystem : MonoBehaviour
{
    [Header("RT / Surface")]
    [Tooltip("Should match your paint surface RT size (often 512).")]
    public int textureSize = 512;

    [Header("Brush Stamp Size (Pixels)")]
    [Tooltip("Default stamp size in pixels on the paint texture.")]
    public float brushSizePixels = 32f;

    [Header("Paint Colour")]
    public Color paintColor = Color.black;

    //[Header("Fire Mode")]
    public enum FireMode { Hold, Once }
    public FireMode fireMode = FireMode.Hold;

    [Header("Brush Shape Assets (Optional)")]
    public Texture2D squareBrush;
    public Texture2D starBrush;
    public Texture2D splatBrush;

    [Header("Procedural Blob Settings")]
    public int baseBrushSize = 64;

    public enum BrushShape { Blob, Square, Star, Splat }
    public BrushShape brushShape = BrushShape.Blob;

    private Texture2D brushTex;

    void Awake()
    {
        UpdateBrushTexture();
    }

    // ----------------------
    // Public API used by gun UI
    // ----------------------

    public Texture2D GetBrushTexture()
    {
        return brushTex;
    }

    public float GetBrushSizePixels()
    {
        return brushSizePixels;
    }

    public Color GetPaintColor()
    {
        return paintColor;
    }

    public int GetTextureSize()
    {
        return textureSize;
    }

    public void SetBrushShape(BrushShape shape)
    {
        brushShape = shape;
        UpdateBrushTexture();
    }

    // ----------------------
    // Brush selection / generation
    // ----------------------

    void UpdateBrushTexture()
    {
        switch (brushShape)
        {
            case BrushShape.Blob:
                brushTex = CreateBlobTexture(baseBrushSize);
                break;

            case BrushShape.Square:
                brushTex = squareBrush;
                break;

            case BrushShape.Star:
                brushTex = starBrush;
                break;

            case BrushShape.Splat:
                brushTex = splatBrush;
                break;
        }

        // Safety fallback so paint never "disappears"
        if (!brushTex)
        {
            Debug.LogWarning($"[PaintCoreOldSystem] Missing brush for {brushShape}. Falling back to Blob.");
            brushTex = CreateBlobTexture(baseBrushSize);
        }
    }

    Texture2D CreateBlobTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 c = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float r = size * 0.5f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float t = Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), c) / r);
                float a = Mathf.SmoothStep(1f, 0f, t);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }

        tex.Apply(false, false);
        return tex;
    }
}
