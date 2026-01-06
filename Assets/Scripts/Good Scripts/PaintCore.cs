using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class PaintCore : MonoBehaviour
{

    [Header("Spray Cone")]
    public int sprayRayCount = 12;
    public float sprayAngle = 3.5f; // degrees
    public float sprayDistance = 5f;


    [Header("RT")]
    public int textureSize = 512;

    [Header("Brush")]
    public int baseBrushSize = 64; // fallback only

    [Header("World Brush")]
    public float brushWorldSize = 0.25f; // meters

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
        if (cam == null)
            cam = Camera.main;

        paintMat = GetComponent<Renderer>().material;

        // Create RenderTexture
        paintRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        paintRT.wrapMode = TextureWrapMode.Clamp;
        paintRT.filterMode = FilterMode.Bilinear;
        paintRT.useMipMap = false;
        paintRT.autoGenerateMips = false;
        paintRT.Create();

        // Clear RT (transparent = unpainted)
        var prev = RenderTexture.active;
        RenderTexture.active = paintRT;
        GL.Clear(false, true, Color.clear);
        RenderTexture.active = prev;

        // Create base brush texture
        brushTex = CreateBlobTexture(baseBrushSize);

        // Hook RT into paint shader
        if (paintMat.HasProperty("_PaintMask"))
            paintMat.SetTexture("_PaintMask", paintRT);
        else
            Debug.LogError("Paint material missing _PaintMask property.");
    }

    void Update()
    {
        // Toggle fire mode
        if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Fire2"))
        {
            fireMode = fireMode == FireMode.Spray ? FireMode.Once : FireMode.Spray;
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

        /* if (!Physics.Raycast(ray, out RaycastHit hit))
             return;

         if (!hit.collider.transform.IsChildOf(transform))
             return;
         TryPaintFromHit(hit);
        */

        FireSprayCone(ray);
       
    }

    // ========================= PAINTING =========================

    bool TryPaintFromHit(RaycastHit hit)
    {
        Vector2 uv = hit.textureCoord;

        float dynamicBrushSize = CalculateBrushSizeFromWorld(hit);
        DrawDotAtUV(uv, dynamicBrushSize);

        return true;
    }

    void DrawDotAtUV(Vector2 uv, float size)
    {
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        int px = Mathf.RoundToInt(uv.x * paintRT.width);
        int py = Mathf.RoundToInt(uv.y * paintRT.height);

        int drawSize = Mathf.RoundToInt(size);

        var prev = RenderTexture.active;
        RenderTexture.active = paintRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, paintRT.width, 0, paintRT.height);

        Graphics.DrawTexture(
            new Rect(px - drawSize * 0.5f, py - drawSize * 0.5f, drawSize, drawSize),
            brushTex,
            new Rect(0, 0, 1, 1),
            0, 0, 0, 0,
            Color.white
        );

        GL.PopMatrix();
        RenderTexture.active = prev;
    }

    // ========================= WORLD  UV =========================

    float CalculateBrushSizeFromWorld(RaycastHit hit)
    {
        MeshCollider meshCol = hit.collider as MeshCollider;
        if (meshCol == null || meshCol.sharedMesh == null)
            return baseBrushSize;

        Mesh mesh = meshCol.sharedMesh;
        int triIndex = hit.triangleIndex * 3;

        if (triIndex + 2 >= mesh.triangles.Length)
            return baseBrushSize;

        Vector3[] verts = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] tris = mesh.triangles;

        Transform t = hit.collider.transform;

        Vector3 v0 = t.TransformPoint(verts[tris[triIndex]]);
        Vector3 v1 = t.TransformPoint(verts[tris[triIndex + 1]]);
        Vector3 v2 = t.TransformPoint(verts[tris[triIndex + 2]]);

        Vector2 uv0 = uvs[tris[triIndex]];
        Vector2 uv1 = uvs[tris[triIndex + 1]];
        Vector2 uv2 = uvs[tris[triIndex + 2]];

        float worldArea =
            Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;

        float uvArea =
            Mathf.Abs(
                (uv1.x - uv0.x) * (uv2.y - uv0.y) -
                (uv2.x - uv0.x) * (uv1.y - uv0.y)
            ) * 0.5f;

        if (uvArea < 0.00001f)
            return baseBrushSize;

        float worldPerUV = Mathf.Sqrt(worldArea / uvArea);
        float brushUVSize = brushWorldSize / worldPerUV;
        float pixelSize = brushUVSize * textureSize;

        return Mathf.Clamp(pixelSize, 4f, textureSize * 0.5f);
    }

    // ========================= BRUSH =========================

    Texture2D CreateBlobTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                //float alpha = dist <= radius ? 1f : 0f;
               

                float t = Mathf.Clamp01(dist / radius);
                float alpha = Mathf.SmoothStep(1f, 0f, t);

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply(false, false);
        return tex;
    }

    void FireSprayCone(Ray centerRay)
    {
        for (int i = 0; i < sprayRayCount; i++)
        {
            // Random direction inside a cone
            Vector3 dir = GetRandomConeDirection(
                centerRay.direction,
                sprayAngle
            );

            Ray sprayRay = new Ray(centerRay.origin, dir);

            if (Physics.Raycast(sprayRay, out RaycastHit hit, sprayDistance))
            {
                if (!hit.collider.transform.IsChildOf(transform))
                    continue;

                TryPaintFromHit(hit);
            }
        }
    }


    Vector3 GetRandomConeDirection(Vector3 forward, float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;

        float z = Random.Range(Mathf.Cos(angleRad), 1f);
        float theta = Random.Range(0f, Mathf.PI * 2f);
        float x = Mathf.Sqrt(1 - z * z) * Mathf.Cos(theta);
        float y = Mathf.Sqrt(1 - z * z) * Mathf.Sin(theta);

        Vector3 localDir = new Vector3(x, y, z);

        return Quaternion.LookRotation(forward) * localDir;
    }

}
