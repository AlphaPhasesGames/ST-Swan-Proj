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
        if (!Physics.Raycast(ray, out RaycastHit hit, sprayDistance))
            return;

        PaintSurfaceBase surface =
            hit.collider.GetComponentInParent<PaintSurfaceBase>();

        if (!surface) return;
        if (!surface.CanPaintHit(hit, ray.direction)) return;

        float size = brushWorldSize * surface.textureSize;
        size = Mathf.Clamp(size, 1f, surface.textureSize * 0.25f);

        surface.PaintAtWorld(hit, brushTex, size, CurrentPaintColor);
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
                PaintSurfaceBase surface =
                    hit.collider.GetComponentInParent<PaintSurfaceBase>();

                if (!surface) continue;
                if (!surface.CanPaintHit(hit, sprayRay.direction)) continue;

                float size = brushWorldSize * surface.textureSize * 0.5f;
                size = Mathf.Max(1f, size);

                surface.PaintAtWorld(hit, brushTex, size, CurrentPaintColor);
                break;
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
}
