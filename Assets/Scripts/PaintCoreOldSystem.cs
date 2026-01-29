using UnityEngine;

public class PaintCoreOldSystem : MonoBehaviour
{
    [Header("Spray Cone")]
    public int sprayRayCount = 12;
    public float sprayAngle = 3.5f;
    public float sprayDistance = 5f;

    [Header("RT")]
    public int textureSize = 512;

    [Header("Brush")]
    public int baseBrushSize = 64;

    [Header("World Brush")]
    public float brushWorldSize = 0.25f;

    [Header("Input")]
    public Camera cam;

    //  private RenderTexture paintRT;
    private Texture2D brushTex;
    //  private Material paintMat;

    public enum FireMode { Spray, Once }
    public FireMode fireMode = FireMode.Spray;

    void Start()
    {
        if (!cam) cam = Camera.main;

        brushTex = CreateBlobTexture(baseBrushSize);

        /*
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

        brushTex = CreateBlobTexture(baseBrushSize);

        if (paintMat.HasProperty("_PaintMask"))
            paintMat.SetTexture("_PaintMask", paintRT);
        else
            Debug.LogError("Paint material missing _PaintMask");
        */
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Fire2"))
            fireMode = fireMode == FireMode.Spray ? FireMode.Once : FireMode.Spray;

        bool paintInput =
            fireMode == FireMode.Once
                ? Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1")
                : Input.GetMouseButton(0) || Input.GetButton("Fire1");

        if (!paintInput) return;

        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
        );

        FireSprayCone(ray);
    }

    void FireSprayCone(Ray centerRay)
    {
        for (int i = 0; i < sprayRayCount; i++)
        {
            Vector3 dir = GetRandomConeDirection(centerRay.direction, sprayAngle);
            Ray sprayRay = new Ray(centerRay.origin, dir);

            RaycastHit[] hits = Physics.RaycastAll(sprayRay, sprayDistance);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                PaintSurfaceBaseOld surface = hit.collider.GetComponent<PaintSurfaceBaseOld>();
                if (!surface)
                    continue;

                if (!surface.CanPaintHit(hit, sprayRay.direction))
                    continue;

                if (!surface.TryGetPaintUV(hit, out Vector2 uv))
                    continue;

                float size = CalculateBrushSizeFromWorld(hit);
                surface.PaintAtUV(uv, brushTex, size);

                break; // THIS IS THE IMPORTANT PART
            }
        }
    }



    float CalculateBrushSizeFromWorld(RaycastHit hit)
    {
        MeshCollider mc = hit.collider as MeshCollider;
        if (!mc || !mc.sharedMesh) return baseBrushSize;

        Mesh m = mc.sharedMesh;
        int tri = hit.triangleIndex * 3;
        if (tri + 2 >= m.triangles.Length) return baseBrushSize;

        Transform t = hit.collider.transform;

        Vector3 v0 = t.TransformPoint(m.vertices[m.triangles[tri]]);
        Vector3 v1 = t.TransformPoint(m.vertices[m.triangles[tri + 1]]);
        Vector3 v2 = t.TransformPoint(m.vertices[m.triangles[tri + 2]]);

        Vector2 uv0 = m.uv[m.triangles[tri]];
        Vector2 uv1 = m.uv[m.triangles[tri + 1]];
        Vector2 uv2 = m.uv[m.triangles[tri + 2]];

        float worldArea = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
        float uvArea = Mathf.Abs(
            (uv1.x - uv0.x) * (uv2.y - uv0.y) -
            (uv2.x - uv0.x) * (uv1.y - uv0.y)
        ) * 0.5f;

        if (uvArea < 0.00001f) return baseBrushSize;

        float worldPerUV = Mathf.Sqrt(worldArea / uvArea);
        float pixelSize = (brushWorldSize / worldPerUV) * textureSize;

        return Mathf.Clamp(pixelSize, 4f, textureSize * 0.5f);
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

    Vector3 GetRandomConeDirection(Vector3 forward, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float z = Random.Range(Mathf.Cos(rad), 1f);
        float theta = Random.Range(0f, Mathf.PI * 2f);
        float x = Mathf.Sqrt(1 - z * z) * Mathf.Cos(theta);
        float y = Mathf.Sqrt(1 - z * z) * Mathf.Sin(theta);
        return Quaternion.LookRotation(forward) * new Vector3(x, y, z);
    }
}