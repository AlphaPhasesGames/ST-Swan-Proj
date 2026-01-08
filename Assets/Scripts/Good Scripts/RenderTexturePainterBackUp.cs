using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class RenderTexturePainterBackUp : MonoBehaviour
{
    [Header("RT")]
    public int textureSize = 512;


    [Header("Brush")]
    public int brushSize = 64;

    [Header("Input")]
    public Camera cam;

    private RenderTexture paintRT;
    private Texture2D brushTex;
    private Material paintMat;



    public enum FireMode
    {
        Spray,
        Once
    }

    public FireMode fireMode = FireMode.Spray;

    void Start()
    {



        if (cam == null) cam = Camera.main;

        paintMat = GetComponent<Renderer>().material;

        // Create RenderTexture
        paintRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        paintRT.wrapMode = TextureWrapMode.Clamp;
        paintRT.filterMode = FilterMode.Bilinear;
        paintRT.useMipMap = false;
        paintRT.autoGenerateMips = false;
        paintRT.Create();



        // Clear RT to black (unpainted)
        var prev = RenderTexture.active;
        RenderTexture.active = paintRT;
        //GL.Clear(false, true, Color.black);
        GL.Clear(false, true, Color.clear);
        RenderTexture.active = prev;

        // Create blob brush
        brushTex = CreateBlobTexture(brushSize);

        // Hook RT into paint shader
        if (paintMat.HasProperty("_PaintMask"))
            paintMat.SetTexture("_PaintMask", paintRT);
        else
            Debug.LogError("Paint material does not have _PaintMask. Assign Custom/PaintRT_Mask.");
    }

    void Update()
    {
        // Toggle fire mode (RMB / Fire2)
        if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Fire2"))
        {
            fireMode = fireMode == FireMode.Spray
                ? FireMode.Once
                : FireMode.Spray;

            Debug.Log("Fire mode: " + fireMode);
        }

        bool paintInput =
            fireMode == FireMode.Once
                ? (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
                : (Input.GetMouseButton(0) || Input.GetButton("Fire1"));

        if (!paintInput)
            return;

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (!hit.collider.transform.IsChildOf(transform))
            return;

        DrawDotAtUV(hit.textureCoord);
    }

    void DrawDotAtUV(Vector2 uv)
    {
        // Safety clamp
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        int px = Mathf.RoundToInt(uv.x * paintRT.width);
        int py = Mathf.RoundToInt(uv.y * paintRT.height);

        var prev = RenderTexture.active;
        RenderTexture.active = paintRT;

        GL.PushMatrix();
        // Y-up pixel space (matches UVs)
        GL.LoadPixelMatrix(0, paintRT.width, 0, paintRT.height);

        Graphics.DrawTexture(
     new Rect(px - brushSize / 2, py - brushSize / 2, brushSize, brushSize),
     brushTex,
     new Rect(0, 0, 1, 1),
     0, 0, 0, 0,
     new Color(1, 1, 1, 1)
 );

        GL.PopMatrix();
        RenderTexture.active = prev;
    }

    Texture2D CreateBlobTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float t = Mathf.Clamp01(dist / radius);

                // Soft blob falloff
                //float v = Mathf.SmoothStep(1f, 0f, t);
                //float v = t < 0.6f ? 1f : Mathf.SmoothStep(1f, 0f, (t - 0.6f) / 0.4f);
                //float v = Mathf.Pow(Mathf.SmoothStep(1f, 0f, t), 0.5f);
                float v = dist <= radius ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, v));
            }

        tex.Apply(false, false);
        return tex;
    }


}
