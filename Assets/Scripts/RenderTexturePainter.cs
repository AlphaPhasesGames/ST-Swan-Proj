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

    RenderTexture paintRT;
    Material blitMat;
    Texture2D brushTex;
    Material paintMat;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        // Get LIVE material instance
        paintMat = GetComponent<Renderer>().material;

        // ----- CREATE RENDER TEXTURE (BUILD SAFE) -----
        paintRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        paintRT.wrapMode = TextureWrapMode.Clamp;
        paintRT.filterMode = FilterMode.Bilinear;
        paintRT.useMipMap = false;
        paintRT.autoGenerateMips = false;
        paintRT.Create();

        // Clear RT to black
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = paintRT;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = prev;

        // ----- BLIT MATERIAL -----
        Shader blitShader = Shader.Find("Unlit/AdditiveBlit");
        if (blitShader == null)
        {
            Debug.LogError("Unlit/AdditiveBlit shader NOT FOUND (will not paint in build)");
            enabled = false;
            return;
        }

        blitMat = new Material(blitShader);

        // ----- BRUSH TEXTURE -----
        brushTex = CreateDotTexture(brushSize);

        // ----- ASSIGN RT TO PAINT SHADER -----
        if (paintMat.HasProperty("_PaintMask"))
        {
            paintMat.SetTexture("_PaintMask", paintRT);
        }
        else
        {
            Debug.LogError("Material does not have _PaintMask (wrong shader?)");
            enabled = false;
        }
    }

    void Update()
    {
        bool mouseFire = Input.GetMouseButtonDown(0);
        bool controllerFire = Input.GetButtonDown("Fire1");

        if (!mouseFire && !controllerFire)
            return;

        Vector3 screenPos = mouseFire
            ? Input.mousePosition
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (!hit.collider.transform.IsChildOf(transform))
                return;

            DrawDotAtUV(hit.textureCoord);
        }
    }

    void DrawDotAtUV(Vector2 uv)
    {
        int px = Mathf.RoundToInt(uv.x * paintRT.width);
        int py = Mathf.RoundToInt((1f - uv.y) * paintRT.height);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = paintRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, paintRT.width, paintRT.height, 0);

        Graphics.DrawTexture(
            new Rect(px - brushSize / 2, py - brushSize / 2, brushSize, brushSize),
            brushTex,
            blitMat
        );

        GL.PopMatrix();
        RenderTexture.active = prev;
    }

    Texture2D CreateDotTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float value = dist <= radius ? 1f : 0f;
                tex.SetPixel(x, y, new Color(value, value, value, 1f));
            }
        }

        tex.Apply(false, true);
        return tex;
    }
}
