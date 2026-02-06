using UnityEngine;

public class PaintCore : MonoBehaviour
{
    [Header("Spray Cone")]
    public int sprayRayCount = 12;
    public float sprayAngle = 3.5f;
    public float sprayDistance = 5f;

    [Header("RT (Legacy Access)")]
    public int textureSize = 512;

    [Header("World Brush Size")]
    public float brushWorldSize = 0.25f;

    [Header("Input")]
    public Camera cam;

    [Header("Erase Mode")]
    public bool isErasing = false;

    [Header("Brush Size Mode")]
    public bool useFixedWorldBrushSize = false;
    public float fixedWorldBrushSize = 0.25f;

    [Header("Brush Shapes")]
    public Texture2D circleBrush;
    public Texture2D squareBrush;

    [Header("Palette")]
    public PaintPalette palette;

    [Header("Fire Rate")]
    [Tooltip("Paint strokes per second")]
    public float fireRate = 30f; // strokes per second
    float fireAccumulator = 0f;
    // -------- EVENTS --------
    public event System.Action<PaintMode> OnPaintModeChanged;
    public event System.Action<FireMode> OnFireModeChanged;
    public event System.Action<float> OnBrushSizeChanged;
    public event System.Action<bool> OnEraseModeChanged;
    // -------- MODES --------
    public enum PaintMode { Spray, Precision, SingleRay }
    public PaintMode paintMode = PaintMode.Spray;

    public enum FireMode { Hold, Once }
    public FireMode fireMode = FireMode.Hold;

    // -------- LEGACY COMPAT --------
    public enum PaintSystem { SprayCone, Precision, SingleRay }
    public enum SprayStyle
    {
        Normal,
        Spackle
    }

    public SprayStyle currentSprayStyle = SprayStyle.Normal;

    public void SetSprayStyle(SprayStyle style)
    {
        currentSprayStyle = style;
    }

    public PaintSystem paintSystem =>
        paintMode switch
        {
            PaintMode.Precision => PaintSystem.Precision,
            PaintMode.SingleRay => PaintSystem.SingleRay,
            _ => PaintSystem.SprayCone
        };

    // -------- PAINT --------
    public Color CurrentPaintColor { get; private set; } = Color.black;

    Texture2D brushTex;

    public enum BrushShape { Blob, Square }
    public BrushShape brushShape = BrushShape.Blob;

    void Start()
    {
        if (!cam) cam = Camera.main;

        UpdateBrushTexture();

        if (palette != null)
            palette.OnActiveColorChanged += SetPaintColor;
    }

    // -------- PUBLIC API --------
    public void SetPaintColor(Color c) => CurrentPaintColor = c;

    public void SetPaintMode(PaintMode mode)
    {
        if (paintMode == mode) return;
        paintMode = mode;
        OnPaintModeChanged?.Invoke(mode);
    }

    public void ToggleFireMode()
    {
        fireMode = fireMode == FireMode.Hold ? FireMode.Once : FireMode.Hold;
        OnFireModeChanged?.Invoke(fireMode);
    }

    public void SetBrushShape(BrushShape shape)
    {
        if (brushShape == shape) return;
        brushShape = shape;
        UpdateBrushTexture();
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

    // -------- UPDATE --------
    void Update()
    {
        HandleBrushSizing();
        HandlePaint();
    }

    // -------- PAINTING --------
    void HandlePaint()
    {
        bool inputHeld =
            fireMode == FireMode.Once
                ? Input.GetMouseButtonDown(0)
                : Input.GetMouseButton(0);

        if (!inputHeld)
        {
            fireAccumulator = 0f;
            return;
        }

        // Accumulate time
        fireAccumulator += Time.deltaTime * fireRate;

        int firesThisFrame = Mathf.FloorToInt(fireAccumulator);
        if (firesThisFrame <= 0)
            return;

        fireAccumulator -= firesThisFrame;

        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f)
        );

        for (int i = 0; i < firesThisFrame; i++)
        {
            switch (paintMode)
            {
                case PaintMode.Precision:
                    FirePrecision(ray);
                    break;

                case PaintMode.SingleRay:
                    FireSingleRay(ray);
                    break;

                case PaintMode.Spray:
                default:
                    if (currentSprayStyle == SprayStyle.Normal)
                        FireSprayCone(ray);
                    else
                        FireSprayConeSingleFire(ray); // spackle = chunky / sparse
                    break;


            }
        }


    }


    Color GetFinalPaintColor()
    {
        return isErasing ? new Color(0, 0, 0, 0) : new Color(CurrentPaintColor.r, CurrentPaintColor.g, CurrentPaintColor.b, 1f);
    }

    void FireSingleRay(Ray ray)
    {
        if (!Physics.Raycast(ray, out RaycastHit hit, sprayDistance)) return;

        PaintSurfaceBase surface = hit.collider.GetComponentInParent<PaintSurfaceBase>();
        if (!surface || !surface.CanPaintHit(hit, ray.direction)) return;

        float size = GetBrushSizeForSurface(surface) * surface.textureSize;
        surface.PaintAtWorld(hit, brushTex, size, GetFinalPaintColor(), isErasing);
    }

    void FirePrecision(Ray ray)
    {
        Vector3 o = ray.origin;
        Vector3 f = ray.direction;
        float off = 0.01f;

        FirePrecisionRay(o, f);
        FirePrecisionRay(o, f + cam.transform.right * off);
        FirePrecisionRay(o, f - cam.transform.right * off);
        FirePrecisionRay(o, f + cam.transform.up * off);
        FirePrecisionRay(o, f - cam.transform.up * off);
    }

    void FirePrecisionRay(Vector3 o, Vector3 d)
    {
        if (!Physics.Raycast(o, d, out RaycastHit hit, sprayDistance)) return;

        PaintSurfaceBase surface = hit.collider.GetComponentInParent<PaintSurfaceBase>();
        if (!surface || !surface.CanPaintHit(hit, d)) return;

        float size = Mathf.Clamp(
            GetBrushSizeForSurface(surface) * surface.textureSize,
            1f,
            surface.textureSize * 0.25f
        );

        surface.PaintAtWorld(hit, brushTex, size, GetFinalPaintColor(), isErasing);
    }

    void FireSprayConeSingleFire(Ray ray)
    {
        for (int i = 0; i < sprayRayCount; i++)
        {
            Vector3 dir = GetRandomConeDirection(ray.direction, sprayAngle);
            if (!Physics.Raycast(ray.origin, dir, out RaycastHit hit, sprayDistance)) continue;

            PaintSurfaceBase surface = hit.collider.GetComponentInParent<PaintSurfaceBase>();
            if (!surface || !surface.CanPaintHit(hit, dir)) continue;

            float size = GetBrushSizeForSurface(surface) * surface.textureSize * 0.6f;
            surface.PaintAtWorld(hit, brushTex, size, GetFinalPaintColor(), isErasing);
            break;
        }
    }

    void FireSprayCone(Ray ray)
    {
        for (int i = 0; i < sprayRayCount; i++)
        {
            Vector3 dir = GetRandomConeDirection(ray.direction, sprayAngle);

            if (!Physics.Raycast(ray.origin, dir, out RaycastHit hit, sprayDistance))
                continue;

            PaintSurfaceBase surface = hit.collider.GetComponentInParent<PaintSurfaceBase>();
            if (!surface || !surface.CanPaintHit(hit, dir))
                continue;

            float size = GetBrushSizeForSurface(surface) * surface.textureSize * 0.6f;
            surface.PaintAtWorld(hit, brushTex, size, GetFinalPaintColor(), isErasing);
        }
    }


    // -------- UTIL --------
    void HandleBrushSizing()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        brushWorldSize = Mathf.Clamp(brushWorldSize + scroll * 0.05f, 0.0025f, 2f);
        OnBrushSizeChanged?.Invoke(brushWorldSize);
    }

    float GetBrushSizeForSurface(PaintSurfaceBase surface)
    {
        if (surface.overrideBrushSize) return surface.GetSurfaceBrushSize();
        return useFixedWorldBrushSize ? fixedWorldBrushSize : brushWorldSize;
    }

    Vector3 GetRandomConeDirection(Vector3 forward, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float z = Random.Range(Mathf.Cos(rad), 1f);
        float t = Random.Range(0f, Mathf.PI * 2f);
        float x = Mathf.Sqrt(1 - z * z) * Mathf.Cos(t);
        float y = Mathf.Sqrt(1 - z * z) * Mathf.Sin(t);
        return Quaternion.LookRotation(forward) * new Vector3(x, y, z);
    }

    void UpdateBrushTexture()
    {
        brushTex = brushShape == BrushShape.Blob
            ? CreateBlobTexture(256)
            : squareBrush;
    }

    Texture2D CreateBlobTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        float r = size * 0.5f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / r;
                float a = d < 0.85f ? 1f : Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.85f, 1f, d));
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }

        tex.Apply();
        return tex;
    }

    public Texture2D GetBrushTexture()
    {
        if (!brushTex)
            Debug.LogWarning("[PaintCore] Brush texture requested but not initialised.");

        return brushTex;
    }


    public void SetEraseMode(bool erase)
    {
        if (isErasing == erase) return;

        isErasing = erase;
        Debug.Log("[PaintCore] Erase mode: " + isErasing);
        OnEraseModeChanged?.Invoke(isErasing);
    }

}
