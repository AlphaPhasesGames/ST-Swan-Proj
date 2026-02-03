using UnityEngine;

public class PaintCore : MonoBehaviour
{
    [Header("Spray Cone")]
    public int sprayRayCount = 12;
    public float sprayAngle = 3.5f;
    public float sprayDistance = 5f;

    [Header("RT (Legacy Access)")]
    public int textureSize = 512;   // REQUIRED for old tools

    [Header("World Brush Size")]
    public float brushWorldSize = 0.25f;

    [Header("Input")]
    public Camera cam;

    [Header("Erase Mode")]
    public bool isErasing = false;

    [Header("Brush Size Mode")]
    public bool useFixedWorldBrushSize = false;
    public float fixedWorldBrushSize = 0.25f;

    [Header("Palette")]
    public PaintPalette palette;

    // ---------------- MODES ----------------

    public enum PaintMode { Spray, Precision }
    public PaintMode paintMode = PaintMode.Spray;

    public enum FireMode { Hold, Once }
    public FireMode fireMode = FireMode.Hold;

    // --------- LEGACY UI COMPAT ---------

    public enum PaintSystem
    {
        SprayCone,
        Precision
    }

    public PaintSystem paintSystem =>
        paintMode == PaintMode.Precision
            ? PaintSystem.Precision
            : PaintSystem.SprayCone;

    // ------------------------------------

    [Header("Paint Colour")]
    public Color CurrentPaintColor { get; private set; } = Color.black;

    Texture2D brushTex;

    // ---------------- SETUP ----------------

    void Start()
    {
        if (!cam) cam = Camera.main;

        brushTex = CreateBlobTexture(256);

        if (palette != null)
            palette.OnActiveColorChanged += SetPaintColor;
    }

    // ---------------- PUBLIC API ----------------

    public void SetPaintColor(Color c)
    {
        CurrentPaintColor = c;
    }

    public Texture2D GetBrushTexture()
    {
        return brushTex;
    }

    public PaintSurfaceBase GetSurfaceUnderCrosshairPublic()
    {
        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, sprayDistance))
            return hit.collider.GetComponentInParent<PaintSurfaceBase>();

        return null;
    }

    public void SetPaintMode(PaintMode mode)
    {
        paintMode = mode;
    }

    public void ToggleFireMode()
    {
        fireMode = fireMode == FireMode.Hold
            ? FireMode.Once
            : FireMode.Hold;
    }

    public void SetEraseMode(bool erase)
    {
        isErasing = erase;
        Debug.Log($"[PaintCore] Erase mode set to: {isErasing}");
    }

    // ---------------- UPDATE ----------------

    void Update()
    {
        HandleBrushSizing();
        HandlePaint();
    }

    // ---------------- PAINTING ----------------

    Color GetFinalPaintColor()
    {
        Color c = CurrentPaintColor;
        if (!isErasing) c.a = 1f;
        else c = new Color(0, 0, 0, 0);
        return c;
    }

    void HandlePaint()
    {
        bool paintInput =
            fireMode == FireMode.Once
                ? Input.GetMouseButtonDown(0)
                : Input.GetMouseButton(0);

        if (!paintInput) return;

        Debug.Log($"[PaintCore] Paint triggered | Erasing: {isErasing} | Mode: {paintMode}");

        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f)
        );

        if (paintMode == PaintMode.Precision)
            FirePrecision(ray);
        else
            FireSprayCone(ray);
    }

    void FirePrecision(Ray ray)
    {
        Vector3 origin = ray.origin;
        Vector3 forward = ray.direction;

        float offset = 0.01f;
        Vector3 right = cam.transform.right * offset;
        Vector3 up = cam.transform.up * offset;

        FirePrecisionRay(origin, forward);
        FirePrecisionRay(origin, forward + right);
        FirePrecisionRay(origin, forward - right);
        FirePrecisionRay(origin, forward + up);
        FirePrecisionRay(origin, forward - up);
    }

    void FireSprayCone(Ray centerRay)
    {
        for (int i = 0; i < sprayRayCount; i++)
        {
            Vector3 dir = GetRandomConeDirection(centerRay.direction, sprayAngle);
            Ray sprayRay = new Ray(centerRay.origin, dir);

            Debug.DrawRay(
                sprayRay.origin,
                sprayRay.direction * sprayDistance,
                Color.magenta,
                0.1f
            );

            RaycastHit[] hits = Physics.RaycastAll(sprayRay, sprayDistance);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                PaintSurfaceBase surface =
                    hit.collider.GetComponentInParent<PaintSurfaceBase>();

                if (!surface) continue;
                if (!surface.CanPaintHit(hit, sprayRay.direction)) continue;

                float worldSize = GetBrushSizeForSurface(surface);
                float size = worldSize * surface.textureSize * 0.5f * 1.2f;

                Color finalColor = GetFinalPaintColor();

                Debug.Assert(
                    !isErasing || finalColor.a == 0f,
                    "Erase mode active but alpha is not zero!"
                );

                surface.PaintAtWorld(hit, brushTex, size, finalColor, isErasing);

                IPaintCoverage coverage =
                    hit.collider.GetComponentInParent<IPaintCoverage>();
                coverage?.RegisterPaintHit(hit);

                break;
            }
        }
    }

    void FirePrecisionRay(Vector3 origin, Vector3 dir)
    {
        if (!Physics.Raycast(origin, dir, out RaycastHit hit, sprayDistance))
            return;

        PaintSurfaceBase surface =
            hit.collider.GetComponentInParent<PaintSurfaceBase>();

        if (!surface) return;
        if (!surface.CanPaintHit(hit, dir)) return;

        float worldSize = GetBrushSizeForSurface(surface);
        float size = Mathf.Clamp(
            worldSize * surface.textureSize,
            1f,
            surface.textureSize * 0.25f
        );

        surface.PaintAtWorld(hit, brushTex, size, GetFinalPaintColor());
    }

    // ---------------- UTIL ----------------

    void HandleBrushSizing()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        brushWorldSize += scroll * 0.05f;
        brushWorldSize = Mathf.Clamp(brushWorldSize, 0.0025f, 2f);
    }

    float GetBrushSizeForSurface(PaintSurfaceBase surface)
    {
        if (surface.overrideBrushSize)
            return surface.GetSurfaceBrushSize();

        if (useFixedWorldBrushSize)
            return fixedWorldBrushSize;

        return brushWorldSize;
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

    Texture2D CreateBlobTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        Vector2 c = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float r = size * 0.5f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / r;
                float a = (d < 0.85f)
                    ? 1f
                    : Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.85f, 1f, d));

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }

        tex.Apply(false, false);
        return tex;
    }
}
