using UnityEngine;

public class PaintCore : MonoBehaviour
{
    // =======================
    // SPRAY
    // =======================
    [Header("Spray Cone")]
    public int sprayRayCount = 12;
    public float sprayAngle = 3.5f;

    [Header("Paint Distances")]
    public float brushDistance = 15f;        // Precision / SingleRay / Calligraphy
    public float sprayDistance = 5f;         // Spray can
    public float paintballDistance = 25f;    // Paintball gun (used by PaintballGun script, not PaintCore)


    // =======================
    // TEXTURE / SIZE
    // =======================
    [Header("RT (Legacy Access)")]
    public int textureSize = 512;

    [Header("World Brush Size")]
    public float brushWorldSize = 0.25f;

    [Header("Brush Size Mode")]
    public bool useFixedWorldBrushSize = false;
    public float fixedWorldBrushSize = 0.25f;

    // =======================
    // INPUT
    // =======================
    [Header("Input")]
    public Camera cam;

    // =======================
    // ERASE
    // =======================
    [Header("Erase Mode")]
    public bool isErasing = false;

    // =======================
    // BRUSH SHAPES
    // =======================
    [Header("Brush Shapes")]
    public Texture2D circleBrush;
    public Texture2D squareBrush;
    public Texture2D starBrush;
    public Texture2D splatBrush;

    // =======================
    // PALETTE
    // =======================
    [Header("Palette")]
    public PaintPalette palette;

    // =======================
    // FIRE RATE
    // =======================
    [Header("Fire Rate")]
    public float fireRate = 30f;          // Normal spray
    public float spackleFireRate = 8f;    // Spackle spray (slower, chunkier)
    float fireAccumulator = 0f;

    // =======================
    // EVENTS (RESTORED)
    // =======================
    public event System.Action<PaintMode> OnPaintModeChanged;
    public event System.Action<FireMode> OnFireModeChanged;
    public event System.Action<float> OnBrushSizeChanged;
    public event System.Action<bool> OnEraseModeChanged;
    public event System.Action<float> OnSprayAngleChanged;
    public event System.Action<BrushBehaviour> OnBrushBehaviourChanged;

    // =======================
    // MODES
    // =======================
    public enum PaintMode { Spray, Precision, SingleRay }
    public PaintMode paintMode = PaintMode.Spray;

    public enum FireMode { Hold, Once }
    public FireMode fireMode = FireMode.Hold;

    public enum PaintSystem { SprayCone, Precision, SingleRay }

    public PaintSystem paintSystem =>
        paintMode switch
        {
            PaintMode.Precision => PaintSystem.Precision,
            PaintMode.SingleRay => PaintSystem.SingleRay,
            _ => PaintSystem.SprayCone
        };

    public enum SprayStyle { Normal, Spackle }
    public SprayStyle currentSprayStyle = SprayStyle.Normal;

    public enum ScrollMode { BrushSize, SpraySpread }
    public ScrollMode scrollMode;

    // =======================
    // BRUSH BEHAVIOUR
    // =======================
    public enum BrushBehaviour { Normal, Calligraphy }

    [Header("Brush Behaviour")]
    public BrushBehaviour brushBehaviour = BrushBehaviour.Normal;

    // =======================
    // CALLIGRAPHY (ISOLATED)
    // =======================
    [Header("Calligraphy")]
    public float minCalligraphyScale = 0.05f;
    public float maxCalligraphyScale = 1.5f;
    public float slowSpeed = 0.05f;
    public float fastSpeed = 0.6f;
    public float calligraphySmoothing = 12f;

    Vector3 lastCalligraphyHit;
    bool hasLastCalligraphyHit;
    float currentCalligraphyScale = 1f;

    // =======================
    // INTERNAL STATE
    // =======================
    Ray lastPaintRay;
    bool hasLastRay;

    Texture2D brushTex;
    public Color CurrentPaintColor { get; private set; } = Color.black;

    public enum BrushShape { Blob, Square, Star, Splat }
    public BrushShape brushShape = BrushShape.Blob;

    // =======================
    // UNITY
    // =======================
    void Start()
    {
        if (!cam) cam = Camera.main;
        UpdateBrushTexture();

        if (palette != null)
            palette.OnActiveColorChanged += SetPaintColor;
    }

    void Update()
    {
        HandleScroll();
        HandlePaint();
    }

    // =======================
    // INPUT
    // =======================
    void HandleScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        if (scrollMode == ScrollMode.BrushSize)
        {
            brushWorldSize = Mathf.Clamp(brushWorldSize + scroll * 0.05f, 0.0025f, 2f);
            OnBrushSizeChanged?.Invoke(brushWorldSize);
        }
        else if (scrollMode == ScrollMode.SpraySpread)
        {
            sprayAngle = Mathf.Clamp(sprayAngle + scroll * 2f, 0.5f, 25f);
            OnSprayAngleChanged?.Invoke(sprayAngle);
        }
    }

    void HandlePaint()
    {
        if (fireMode == FireMode.Once)
        {
            if (Input.GetMouseButtonDown(0))
                FireByPaintMode(GetCenterRay());
            return;
        }

        if (!Input.GetMouseButton(0))
        {
            hasLastRay = false;
            fireAccumulator = 0f;
            ResetCalligraphy();
            return;
        }

        Ray ray = GetCenterRay();

        if (paintMode == PaintMode.Spray)
        {
                float rate =
                currentSprayStyle == SprayStyle.Spackle
                ? spackleFireRate
                : fireRate;

            fireAccumulator += Time.deltaTime * rate;
            int fires = Mathf.FloorToInt(fireAccumulator);
            fireAccumulator -= fires;

            for (int i = 0; i < fires; i++)
                FireByPaintMode(ray);

            return;
        }

        if (brushBehaviour == BrushBehaviour.Calligraphy)
            UpdateCalligraphy(ray);

        if (!hasLastRay)
        {
            FireByPaintMode(ray);
            lastPaintRay = ray;
            hasLastRay = true;
            return;
        }

        float dist = Vector3.Distance(lastPaintRay.origin, ray.origin);
        float step = brushWorldSize * 0.5f;

        // BASE CALLIGRAPHY DENSITY BOOST (your original logic)
        if (brushBehaviour == BrushBehaviour.Calligraphy)
        {
            step *= 0.00f;
        }

        float minStep = 0.0025f;

        if (brushBehaviour == BrushBehaviour.Calligraphy)
        {
            minStep *= 0.25f;
        }

        step = Mathf.Max(step, minStep);

        if (brushBehaviour == BrushBehaviour.Calligraphy)
        {
            step *= Mathf.Lerp(0.25f, 1f, currentCalligraphyScale);
        }

        int steps = Mathf.CeilToInt(dist / step);

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            Ray lerpRay = new Ray(
                Vector3.Lerp(lastPaintRay.origin, ray.origin, t),
                Vector3.Slerp(lastPaintRay.direction, ray.direction, t)
            );
            FireByPaintMode(lerpRay);
        }

        lastPaintRay = ray;
    }

    // =======================
    // CALLIGRAPHY
    // =======================
    void UpdateCalligraphy(Ray ray)
    {
        // Calligraphy uses BRUSH distance (as requested)
        if (!Physics.Raycast(ray, out RaycastHit hit, brushDistance))
            return;

        if (!hasLastCalligraphyHit)
        {
            lastCalligraphyHit = hit.point;
            hasLastCalligraphyHit = true;
            return;
        }

        float dist = Vector3.Distance(lastCalligraphyHit, hit.point);
        float speed = dist / Mathf.Max(Time.deltaTime, 0.0001f);

        float normalizedSpeed = Mathf.Clamp01((speed - slowSpeed) / (fastSpeed - slowSpeed));
        float t = 1f - normalizedSpeed;
        t = Mathf.Pow(t, 50f);

        float target = Mathf.Lerp(minCalligraphyScale, maxCalligraphyScale, t);

        float growSpeed = 0.8f;
        float shrinkSpeed = 0.8f;
        float speedcal = target > currentCalligraphyScale ? growSpeed : shrinkSpeed;

        currentCalligraphyScale = Mathf.MoveTowards(
            currentCalligraphyScale,
            target,
            speedcal * Time.deltaTime
        );

        lastCalligraphyHit = hit.point;
    }

    void ResetCalligraphy()
    {
        hasLastCalligraphyHit = false;
        currentCalligraphyScale = 1f;
    }

    // =======================
    // SIZE
    // =======================
    float GetFinalBrushSize(PaintSurfaceBase surface)
    {
        float baseWorld = GetBrushSizeForSurface(surface);

        if (brushBehaviour == BrushBehaviour.Calligraphy &&
            paintMode != PaintMode.Spray)
        {
            return baseWorld * currentCalligraphyScale * surface.textureSize;
        }

        return baseWorld * surface.textureSize;
    }

    float GetBrushSizeForSurface(PaintSurfaceBase surface)
    {
        if (surface.overrideBrushSize)
            return surface.GetSurfaceBrushSize();

        return useFixedWorldBrushSize ? fixedWorldBrushSize : brushWorldSize;
    }

    public float BrushWorldRadius
    {
        get
        {
            if (paintSystem == PaintSystem.SprayCone)
                return Mathf.Tan(sprayAngle * Mathf.Deg2Rad) * sprayDistance; // spray can radius uses sprayDistance

            return useFixedWorldBrushSize ? fixedWorldBrushSize : brushWorldSize;
        }
    }

    // =======================
    // FIRE
    // =======================
    void FireSingleRay(Ray ray)
    {
        // Precision / SingleRay / Calligraphy all use brushDistance
        if (!Physics.Raycast(ray, out RaycastHit hit, brushDistance)) return;

        PaintSurfaceBase surface = hit.collider.GetComponentInParent<PaintSurfaceBase>();
        if (!surface || !surface.CanPaintHit(hit, ray.direction)) return;

        surface.PaintAtWorld(hit, brushTex, GetFinalBrushSize(surface), GetFinalPaintColor(), isErasing);
    }

    void FireSpray(Ray ray)
    {
        // Spray uses sprayDistance
        for (int i = 0; i < sprayRayCount; i++)
        {
            Vector3 dir = GetRandomConeDirection(ray.direction, sprayAngle);
            if (!Physics.Raycast(ray.origin, dir, out RaycastHit hit, sprayDistance)) continue;

            PaintSurfaceBase surface = hit.collider.GetComponentInParent<PaintSurfaceBase>();
            if (!surface || !surface.CanPaintHit(hit, dir)) continue;

            float size = GetBrushSizeForSurface(surface) * surface.textureSize * 0.6f;
            surface.PaintAtWorld(hit, brushTex, size, GetFinalPaintColor(), isErasing);
        }
    }

    void FireByPaintMode(Ray ray)
    {
        if (paintMode == PaintMode.Spray)
            FireSpray(ray);
        else
            FireSingleRay(ray);
    }

    // =======================
    // PUBLIC API (RESTORED)
    // =======================
    public void SetPaintColor(Color c) => CurrentPaintColor = c;
    public void SetPaintColour(Color c) => SetPaintColor(c);

    public void SetPaintMode(PaintMode mode)
    {
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
        brushShape = shape;
        UpdateBrushTexture();
    }

    public void SetBrushBehaviour(BrushBehaviour behaviour)
    {
        brushBehaviour = behaviour;
        ResetCalligraphy();
        OnBrushBehaviourChanged?.Invoke(behaviour);
    }

    public void SetScrollMode(ScrollMode mode) => scrollMode = mode;

    public void SetEraseMode(bool erase)
    {
        isErasing = erase;
        OnEraseModeChanged?.Invoke(isErasing);
    }

    public void SetSprayStyle(SprayStyle style) => currentSprayStyle = style;

    public Texture2D GetBrushTexture() => brushTex;

    public PaintSurfaceBase GetSurfaceUnderCrosshairPublic()
    {
        float dist = GetRaycastDistance();
        if (Physics.Raycast(GetCenterRay(), out RaycastHit hit, dist))
            return hit.collider.GetComponentInParent<PaintSurfaceBase>();
        return null;
    }

    // =======================
    // DISTANCE HELPERS
    // =======================
    float GetRaycastDistance()
    {
        // Spray mode uses sprayDistance
        if (paintMode == PaintMode.Spray)
            return sprayDistance;

        // Precision / SingleRay / Calligraphy use brushDistance
        return brushDistance;
    }

    // Optional: makes it easy for PaintballGun to query this
    public float GetPaintballDistance() => paintballDistance;

    // =======================
    // UTILS
    // =======================
    Ray GetCenterRay() =>
        cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

    Color GetFinalPaintColor() =>
        isErasing ? Color.clear : new Color(CurrentPaintColor.r, CurrentPaintColor.g, CurrentPaintColor.b, 1f);

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
        brushTex = brushShape switch
        {
            BrushShape.Square => squareBrush,
            BrushShape.Star => starBrush,
            BrushShape.Splat => splatBrush,
            _ => circleBrush
        };
    }
}
