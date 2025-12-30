using UnityEngine;

public class RenderTextureTest : MonoBehaviour
{
    public int textureSize = 512;

    private RenderTexture paintRT;
    private Material debugMat;
    private Material blitMat;

    public Camera cam;
    public int brushSize = 64;
    void Start()
    {
        //  Create the RenderTexture
        paintRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.R8);
        paintRT.wrapMode = TextureWrapMode.Clamp;
        paintRT.filterMode = FilterMode.Bilinear;
        paintRT.Create();

        blitMat = new Material(Shader.Find("Hidden/AdditiveBlit"));


        //  Clear it to black
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = paintRT;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = active;

        //  Create a simple unlit material to display it
        debugMat = new Material(Shader.Find("Unlit/Texture"));
        debugMat.mainTexture = paintRT;

        // 4 Apply it to THIS object
        GetComponent<Renderer>().material = debugMat;

        //  Draw a test circle

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (!hit.collider.transform.IsChildOf(transform))
                    return;

                DrawDotAtUV(hit.textureCoord);
            }
        }
    }


    void DrawDotAtUV(Vector2 uv)
    {
        Texture2D dot = CreateDotTexture(brushSize);

        int px = Mathf.RoundToInt(uv.x * paintRT.width);
        int py = Mathf.RoundToInt((1f - uv.y) * paintRT.height);

        RenderTexture.active = paintRT;

        // Convert bottom-left UV space to top-left pixel space
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, paintRT.width, paintRT.height, 0);

        Graphics.DrawTexture(
            new Rect(px - brushSize / 2, py - brushSize / 2, brushSize, brushSize),
            dot,
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
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float value = dist <= radius ? 1f : 0f;
                tex.SetPixel(x, y, new Color(value, value, value, 1));
            }
        }

        tex.Apply();
        return tex;
    }
}
