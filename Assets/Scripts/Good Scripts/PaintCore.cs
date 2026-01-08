
using UnityEngine;

public class PaintCore : MonoBehaviour
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

    [Header("System")]
    public PaintSystem paintSystem = PaintSystem.SprayCone;

    [Header("Spray Behaviour")]
    public SpraySizeMode spraySizeMode = SpraySizeMode.Distance;

    public enum PaintSystem
    {
        LegacyStamp,
        SprayCone
    }

    public enum SpraySizeMode
    {
        Constant,   // same size no matter distance
        Distance    // grows with distance (shotgun)
    }


    void Start()
    {
        if (!cam) cam = Camera.main;

        // brushTex = CreateBlobTexture(baseBrushSize);
        brushTex = CreateBlobTexture(256); // high-res master brush
    }

    void Update()
    {


        // Switch paint system
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            paintSystem = PaintSystem.LegacyStamp;
            Debug.Log("Paint System: LEGACY STAMP");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            paintSystem = PaintSystem.SprayCone;
            Debug.Log("Paint System: SPRAY CONE");
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            if (paintSystem == PaintSystem.SprayCone)
            {
                brushWorldSize += 0.05f;
                brushWorldSize = Mathf.Clamp(brushWorldSize, -20f, 2f);
            }

            if (paintSystem == PaintSystem.LegacyStamp)
            {
                PaintSurfaceBase surface = GetSurfaceUnderCrosshair();
                if (surface)
                {
                    surface.legacyBrushSize += 1f;
                    surface.legacyBrushSize = Mathf.Clamp(surface.legacyBrushSize, 1f, 512f);
                }
            }
        }
        else if (scroll < 0f)
        {
            if (paintSystem == PaintSystem.SprayCone)
            {
                brushWorldSize -= 0.05f;
                brushWorldSize = Mathf.Clamp(brushWorldSize, -20f, 2f);
            }

            if (paintSystem == PaintSystem.LegacyStamp)
            {
                PaintSurfaceBase surface = GetSurfaceUnderCrosshair();
                if (surface)
                {
                    surface.legacyBrushSize -= 1f;   //  THIS WAS THE BUG
                    surface.legacyBrushSize = Mathf.Clamp(surface.legacyBrushSize, 1f, 512f);
                }
            }
        }
        /* // redundant code to make a shotgun but i'm alreayd doing this with hard and soft and just named them wrong
        if (Input.GetKeyDown(KeyCode.Q))
        {
            spraySizeMode =
                spraySizeMode == SpraySizeMode.Distance
                    ? SpraySizeMode.Constant
                    : SpraySizeMode.Distance;

            Debug.Log("Spray Size Mode: " + spraySizeMode);
        }
        */
        HandleFireModeToggle();
        HandlePaint();

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
                PaintSurfaceBase surface = hit.collider.GetComponent<PaintSurfaceBase>();
                if (!surface)
                    continue;

                if (!surface.CanPaintHit(hit, sprayRay.direction))
                    continue;

                if (!surface.TryGetPaintUV(hit, out Vector2 uv))
                    continue;

                //float size = CalculateBrushSizeFromWorld(hit); //  change size of paint at distance

                float size = spraySizeMode == SpraySizeMode.Distance
                ? CalculateBrushSizeFromWorld(hit)   // shotgun
                : baseBrushSize;                     // normal gun

                // allow negative sprayWorldSize, but never draw invalid rects
                float safeSize = Mathf.Max(1f, Mathf.Abs(size));

                surface.PaintAtUV(uv, brushTex, safeSize);

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
        // brushWorldSiz how big the spray should be in world units - worldPerUV is how much world space one UV unit covers and this changes with distance and surface orientation
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


    void HandlePaint()
    {
        bool paintInput =
            fireMode == FireMode.Once
                ? Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1")
                : Input.GetMouseButton(0) || Input.GetButton("Fire1");

        if (!paintInput) return;

        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
        );

        switch (paintSystem)
        {
            case PaintSystem.LegacyStamp:
                FireLegacy(ray);
                break;

            case PaintSystem.SprayCone:
                FireSprayCone(ray);
                break;
        }
    }


    void FireLegacy(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, sprayDistance);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            PaintSurfaceBase surface = hit.collider.GetComponent<PaintSurfaceBase>();
            if (!surface)
                continue;

            //  LEGACY GUARD
            if (!surface.allowLegacyPaint)
                continue;

            if (!surface.CanPaintHit(hit, ray.direction))
                continue;

            if (!surface.TryGetPaintUV(hit, out Vector2 uv))
                continue;

            surface.PaintAtUV(uv, brushTex, surface.GetLegacyBrushSize());
        }

    }

    void HandleFireModeToggle()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Fire2"))
        {
            fireMode = fireMode == FireMode.Spray
                ? FireMode.Once
                : FireMode.Spray;

            Debug.Log("Fire Mode: " + fireMode);
        }
    }
    PaintSurfaceBase GetSurfaceUnderCrosshair()
    {
        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, sprayDistance))
            return hit.collider.GetComponent<PaintSurfaceBase>();

        return null;
    }

    public PaintSurfaceBase GetSurfaceUnderCrosshairPublic()
    {
        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, sprayDistance))
            return hit.collider.GetComponentInParent<PaintSurfaceBase>(); // IMPORTANT

        return null;
    }

    public Texture2D GetBrushTexture()
    {
        return brushTex;
    }


    public float CalculateBrushSizeFromWorldPublic(RaycastHit hit)
    {
        return CalculateBrushSizeFromWorld(hit);
    }
}


























