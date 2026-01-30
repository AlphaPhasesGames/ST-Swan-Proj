using UnityEngine;

public class PaintCore : MonoBehaviour
{
    [Header("Spray Cone")]
    public int sprayRayCount = 12;
    public float sprayAngle = 3.5f;
    public float sprayDistance = 5f;

    [Header("RT")]
    public int textureSize = 512;

    [Header("World Brush Size")]
    public float brushWorldSize = 0.25f;

    [Header("Input")]
    public Camera cam;


    [Header("Brush Size Mode")]
    public bool useFixedWorldBrushSize = false;

    [Tooltip("Used when Fixed World Brush Size is enabled")]
    public float fixedWorldBrushSize = 0.25f;


    public enum PaintMode
    {
        Spray,
        Precision
    }

    public PaintMode paintMode = PaintMode.Spray;

    public enum FireMode
    {
        Hold,
        Once
    }

    public FireMode fireMode = FireMode.Hold;

    [Header("Paint Colour")]
    public Color CurrentPaintColor { get; private set; } = Color.black;

    public void SetPaintColor(Color c)
    {
        CurrentPaintColor = c;
    }

    // Legacy UI compatibility
    public enum PaintSystem
    {
        SprayCone,
        Precision
    }



    public PaintSystem paintSystem =>
        paintMode == PaintMode.Precision
            ? PaintSystem.Precision
            : PaintSystem.SprayCone;

    Texture2D brushTex;

    void Start()
    {
        if (!cam) cam = Camera.main;

        // Same brush for both modes – hardness comes from texture
        brushTex = CreateBlobTexture(256);
    }

    void Update()
    {
       // HandleModeKeys();
       // HandleFireModeToggle();
        HandleBrushSizing();
        HandlePaint();
    }

    void HandleModeKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            paintMode = PaintMode.Precision;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            paintMode = PaintMode.Spray;
    }

    void HandleFireModeToggle()
    {
        if (Input.GetMouseButtonDown(1))
        {
            fireMode = fireMode == FireMode.Hold
                ? FireMode.Once
                : FireMode.Hold;
        }
    }

    void HandlePaint()
    {
        bool paintInput =
            fireMode == FireMode.Once
                ? Input.GetMouseButtonDown(0)
                : Input.GetMouseButton(0);

        if (!paintInput) return;

        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f)
        );

        switch (paintMode)
        {
            case PaintMode.Precision:
                FirePrecision(ray);
                break;

            case PaintMode.Spray:
                FireSprayCone(ray);
                break;
        }
    }

    void FirePrecision(Ray ray)
    {
        Vector3 origin = ray.origin;
        Vector3 forward = ray.direction;

        float offset = 0.01f; // seam sensitivity (tweak later)

        Vector3 right = cam.transform.right * offset;
        Vector3 up = cam.transform.up * offset;

        // Main ray
        FirePrecisionRay(origin, forward);

        // Seam helper rays
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

            // DEBUG: draw the ray in the Scene view (magenta baseline)
            Debug.DrawRay(sprayRay.origin, sprayRay.direction * sprayDistance, Color.magenta, 0.1f);

            RaycastHit[] hits = Physics.RaycastAll(sprayRay, sprayDistance);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            bool paintedThisRay = false;

            foreach (RaycastHit hit in hits)
            {
                PaintSurfaceBase surface =
                    hit.collider.GetComponentInParent<PaintSurfaceBase>();

                if (!surface) continue;
                if (!surface.CanPaintHit(hit, sprayRay.direction)) continue;

                // DEBUG: show successful paint ray (green) and the hit normal (cyan)
                Debug.DrawRay(sprayRay.origin, sprayRay.direction * hit.distance, Color.green, 0.2f);
                Debug.DrawRay(hit.point, hit.normal * 0.25f, Color.cyan, 0.2f);

                float worldSize = GetBrushSizeForSurface(surface);
                float size = worldSize * surface.textureSize * 0.5f;
                size *= 1.2f;

                surface.PaintAtWorld(hit, brushTex, size, CurrentPaintColor);

                //  Notify coverage system
                IPaintCoverage coverage =
    hit.collider.GetComponentInParent<IPaintCoverage>();

                if (coverage != null)
                {
                    coverage.RegisterPaintHit(hit);
                }

                paintedThisRay = true;
                break;
            }

            // DEBUG: if this ray never painted anything, tint it red so it stands out
            if (!paintedThisRay)
            {
                Debug.DrawRay(sprayRay.origin, sprayRay.direction * sprayDistance, Color.red, 0.2f);
            }
        }
    }

    void HandleBrushSizing()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        brushWorldSize += scroll * 0.05f;
        brushWorldSize = Mathf.Clamp(brushWorldSize, 0.01f, 2f);
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

    //  THIS IS THE ONLY CHANGE 
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
                float t = Vector2.Distance(new Vector2(x, y), c) / r;

                // Tight, crisp edge but NOT aliased
                float edge = 0.9f; // tweak 0.88 – 0.95
                float a = Mathf.SmoothStep(1f, 0f,
                    Mathf.InverseLerp(edge, 1f, t));

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }

        tex.Apply(false, false);
        return tex;
    }

    public Texture2D GetBrushTexture() => brushTex;

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

    void FirePrecisionRay(Vector3 origin, Vector3 dir)
    {
        if (!Physics.Raycast(origin, dir, out RaycastHit hit, sprayDistance))
            return;

        Debug.DrawRay(origin, dir.normalized * hit.distance, Color.green, 0.2f);
        Debug.DrawRay(hit.point, hit.normal * 0.25f, Color.cyan, 0.2f);

        PaintSurfaceBase surface =
            hit.collider.GetComponentInParent<PaintSurfaceBase>();

        if (!surface) return;
        if (!surface.CanPaintHit(hit, dir)) return;

        float worldSize = GetBrushSizeForSurface(surface);
        float size = worldSize * surface.textureSize;
        size = Mathf.Clamp(size, 1f, surface.textureSize * 0.25f);

        surface.PaintAtWorld(hit, brushTex, size, CurrentPaintColor);
    }

    float GetBrushSizeForSurface(PaintSurfaceBase surface)
    {
        //  Per-surface override wins
        if (surface.overrideBrushSize)
            return surface.GetSurfaceBrushSize();

        //  Global fixed mode
        if (useFixedWorldBrushSize)
            return fixedWorldBrushSize;

        //  Default adaptive behaviour (your current logic)
        return brushWorldSize;
    }

}
