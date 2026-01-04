using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RenderTexturePainter : MonoBehaviour
{
    [Header("RT")]
    public int textureSize = 512;

    [Header("Brush")]
    public int brushSize = 64;

    [Header("Input")]
    public Camera cam;

    private RenderTexture paintRT;
    private Material blitMat;
    private Texture2D brushTex;
    private Material paintMat;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        // Get the object's paint material (must use Custom/PaintRT_Mask)
        paintMat = GetComponent<Renderer>().material;

        // Create RT
        paintRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        paintRT.wrapMode = TextureWrapMode.Clamp;
        paintRT.filterMode = FilterMode.Bilinear;
        paintRT.Create();

        // Clear RT to black
        var active = RenderTexture.active;
        RenderTexture.active = paintRT;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = active;

        // Blit material for additive draw
        blitMat = new Material(Shader.Find("Hidden/AdditiveBlit"));
        // Cache brush texture (don’t recreate per click)
        brushTex = CreateDotTexture(brushSize);

        // Hook RT into the shader
        if (paintMat.HasProperty("_PaintMask"))
            paintMat.SetTexture("_PaintMask", paintRT);
        else
            Debug.LogError("Paint material does not have _PaintMask. Assign Custom/PaintRT_Mask shader.");
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Only paint THIS object (or its children)
                if (!hit.collider.transform.IsChildOf(transform))
                    return;

                DrawDotAtUV(hit.textureCoord);
            }
        }
    }

    void DrawDotAtUV(Vector2 uv)
    {
        int px = Mathf.RoundToInt(uv.x * paintRT.width);
        int py = Mathf.RoundToInt((1f - uv.y) * paintRT.height);

        RenderTexture.active = paintRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, paintRT.width, paintRT.height, 0);

        Graphics.DrawTexture(
            new Rect(px - brushSize / 2, py - brushSize / 2, brushSize, brushSize),
            brushTex,
            blitMat
        );

        GL.PopMatrix();
        RenderTexture.active = null;
    }

    Texture2D CreateDotTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.R8, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float value = dist <= radius ? 1f : 0f;
                tex.SetPixel(x, y, new Color(value, value, value, 1));
            }

        tex.Apply();
        return tex;
    }
}
