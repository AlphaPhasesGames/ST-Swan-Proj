using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public abstract class PaintSurfaceBase : MonoBehaviour
{
    [Header("Surface Paint")]
    public int textureSize = 512;

    protected RenderTexture paintRT;
    protected Material paintMat;

    [Header("Stamp Mode")]
    public bool hardStamp = false;

    [Header("Stamp Material (assign in inspector)")]
    [SerializeField] private Material stampMat;

    [Header("Legacy Paint")]
    public float legacyBrushSize = 64f;
    public float legacyMinSize = 1f;
    public float legacyMaxSize = 256f;

    public bool allowLegacyPaint = true;
    public bool allowSprayPaint = true;

    // Triplanar RTs
    protected RenderTexture paintRT_PosX;
    protected RenderTexture paintRT_NegX;
    protected RenderTexture paintRT_PosY;
    protected RenderTexture paintRT_NegY;
    protected RenderTexture paintRT_PosZ;
    protected RenderTexture paintRT_NegZ;

    // Controls world tiling density (bigger = repeats more)
    [Header("Triplanar Scale")]
    public float triplanarTiling = 1f; // try 1..4

    protected virtual void Awake()
    {
        if (!stampMat)
        {
            Debug.LogError($"{name}: Stamp material not assigned!");
            enabled = false;
            return;
        }
      
        // Surface material (this is the visible shader that samples paint)
        paintMat = GetComponent<Renderer>().material;
        if (paintMat.HasProperty("_WorldScale"))
            paintMat.SetFloat("_WorldScale", triplanarTiling);
        // Existing UV paint RT (keep unchanged)
        paintRT = CreatePaintRT();


        if (paintMat.HasProperty("_PaintMask"))
        {
            paintMat.SetTexture("_PaintMask", paintRT);
        }


        var bounds = GetComponent<Renderer>().localBounds;

        paintMat.SetVector("_BoundsMin", bounds.min);
        paintMat.SetVector("_BoundsSize", bounds.size);

        /*
        if (paintMat.HasProperty("_PaintMask"))
            paintMat.SetTexture("_PaintMask", paintRT);
        else
            Debug.LogError($"{name} material missing _PaintMask");
        */
        // Triplanar RTs (created, but not yet used by the surface shader)
        paintRT_PosX = CreatePaintRT();
        paintRT_NegX = CreatePaintRT();
        paintRT_PosY = CreatePaintRT();
        paintRT_NegY = CreatePaintRT();
        paintRT_PosZ = CreatePaintRT();
        paintRT_NegZ = CreatePaintRT();

        if (paintMat.HasProperty("_PaintPosX"))
        {
            paintMat.SetTexture("_PaintPosX", paintRT_PosX);
            paintMat.SetTexture("_PaintNegX", paintRT_NegX);
            paintMat.SetTexture("_PaintPosY", paintRT_PosY);
            paintMat.SetTexture("_PaintNegY", paintRT_NegY);
            paintMat.SetTexture("_PaintPosZ", paintRT_PosZ);
            paintMat.SetTexture("_PaintNegZ", paintRT_NegZ);
        }
    }

    RenderTexture CreatePaintRT()
    {
        var rt = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.filterMode = FilterMode.Bilinear;
        rt.useMipMap = false;
        rt.autoGenerateMips = false;
        rt.Create();

        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(false, true, Color.clear);
        RenderTexture.active = prev;

        return rt;
    }

    public virtual float GetLegacyBrushSize()
    {
        return Mathf.Clamp(legacyBrushSize, legacyMinSize, legacyMaxSize);
    }

    public abstract bool CanPaintHit(RaycastHit hit, Vector3 rayDir);

    public virtual bool TryGetPaintUV(RaycastHit hit, out Vector2 uv)
    {
        uv = hit.textureCoord;
        return true;
    }

    // UV paint (unchanged)
    public void PaintAtUV(Vector2 uv, Texture2D brush, float size, Color paintColor)
    {
        uv = new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));

        int px = Mathf.RoundToInt(uv.x * paintRT.width);
        int py = Mathf.RoundToInt(uv.y * paintRT.height);
        int drawSize = Mathf.RoundToInt(size);
        float half = drawSize * 0.5f;

        float x = Mathf.Clamp(px - half, 0, paintRT.width - drawSize);
        float y = Mathf.Clamp(py - half, 0, paintRT.height - drawSize);

        Rect rect = new Rect(x, y, drawSize, drawSize);

        var prev = RenderTexture.active;
        RenderTexture.active = paintRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, paintRT.width, 0, paintRT.height);

        stampMat.SetTexture("_MainTex", brush);
        stampMat.SetColor("_Color", paintColor);
        stampMat.SetFloat("_HardStamp", hardStamp ? 1f : 0f);

        Graphics.DrawTexture(rect, brush, stampMat);

        GL.PopMatrix();
        RenderTexture.active = prev;
    }


    // New: triplanar stamping entry point using hit info
    public void PaintAtWorld(RaycastHit hit, Texture2D brush, float size, Color paintColor)
    {
        PaintAtWorld(hit.point, hit.normal, brush, size, paintColor);
    }

    public void PaintAtWorld(Vector3 worldPos, Vector3 normal, Texture2D brush, float size, Color paintColor)
    {
        //  LOCAL normal so rotation is respected AND matches shader
        Vector3 nL = transform.InverseTransformDirection(normal).normalized;

        float ax = Mathf.Abs(nL.x);
        float ay = Mathf.Abs(nL.y);
        float az = Mathf.Abs(nL.z);

        // Pick dominant LOCAL axis
        if (ax >= ay && ax >= az)
        {
            PaintOnPlane(
                nL.x >= 0 ? paintRT_PosX : paintRT_NegX,
                worldPos,
                Axis.X,
                nL, // pass local normal for mirroring
                brush,
                size,
                paintColor
            );
        }
        else if (ay >= ax && ay >= az)
        {
            PaintOnPlane(
                nL.y >= 0 ? paintRT_PosY : paintRT_NegY,
                worldPos,
                Axis.Y,
                nL,
                brush,
                size,
                paintColor
            );
        }
        else
        {
            PaintOnPlane(
                nL.z >= 0 ? paintRT_PosZ : paintRT_NegZ,
                worldPos,
                Axis.Z,
                nL,
                brush,
                size,
                paintColor
            );
        }
    }

 

    //  IMPORTANT: mirroring here MUST MATCH YOUR SHADER
    void PaintOnPlane(RenderTexture rt, Vector3 worldPos, Axis axis, Vector3 localNormal,
                      Texture2D brush, float size, Color color)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        Bounds b = GetComponent<Renderer>().localBounds;

        Vector2 uv;

        switch (axis)
        {
            case Axis.X:
                // shader uses uvX = p.zy
                uv = new Vector2(
                    Mathf.InverseLerp(b.min.z, b.max.z, localPos.z),
                    Mathf.InverseLerp(b.min.y, b.max.y, localPos.y)
                );

                // shader mirrors NEG X: float2(1 - uvX.x, uvX.y)
                if (localNormal.x < 0) uv.x = 1f - uv.x;
                break;

            case Axis.Y:
                // shader uses uvY = p.xz
                uv = new Vector2(
                    Mathf.InverseLerp(b.min.x, b.max.x, localPos.x),
                    Mathf.InverseLerp(b.min.z, b.max.z, localPos.z)
                );

                // shader mirrors NEG Y: float2(uvY.x, 1 - uvY.y)
                if (localNormal.y < 0) uv.y = 1f - uv.y;
                break;

            default: // Z
                     // shader uses uvZ = p.xy
                uv = new Vector2(
                    Mathf.InverseLerp(b.min.x, b.max.x, localPos.x),
                    Mathf.InverseLerp(b.min.y, b.max.y, localPos.y)
                );

                // shader mirrors NEG Z: float2(1 - uvZ.x, uvZ.y)
                if (localNormal.z < 0) uv.x = 1f - uv.x;
                break;
        }

        Stamp(rt, uv, brush, size, color);
    }


    void Stamp(RenderTexture targetRT, Vector2 uv, Texture2D brush, float size, Color color)
    {
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        int px = Mathf.RoundToInt(uv.x * targetRT.width);
        int py = Mathf.RoundToInt(uv.y * targetRT.height);
        int drawSize = Mathf.RoundToInt(size);
        float half = drawSize * 0.5f;

        //  clamp inside RT
        float x = Mathf.Clamp(px - half, -half, targetRT.width - half);
        float y = Mathf.Clamp(py - half, -half, targetRT.height - half);

        Rect rect = new Rect(x, y, drawSize, drawSize);

        //float x = Mathf.Clamp(px - half, 0, targetRT.width - drawSize);
        //float y = Mathf.Clamp(py - half, 0, targetRT.height - drawSize);

        //Rect rect = new Rect(x, y, drawSize, drawSize);

        var prev = RenderTexture.active;
        RenderTexture.active = targetRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, targetRT.width, 0, targetRT.height);

        stampMat.SetTexture("_MainTex", brush);
        stampMat.SetColor("_Color", color);
        stampMat.SetFloat("_HardStamp", hardStamp ? 1f : 0f);

        Graphics.DrawTexture(rect, brush, stampMat);

        GL.PopMatrix();
        RenderTexture.active = prev;
    }


    RenderTexture GetPaintRTForNormal(Vector3 normal)
    {
        Vector3 n = normal.normalized;

        if (Mathf.Abs(n.x) > Mathf.Abs(n.y) && Mathf.Abs(n.x) > Mathf.Abs(n.z))
            return n.x > 0 ? paintRT_PosX : paintRT_NegX;

        if (Mathf.Abs(n.y) > Mathf.Abs(n.x) && Mathf.Abs(n.y) > Mathf.Abs(n.z))
            return n.y > 0 ? paintRT_PosY : paintRT_NegY;

        return n.z > 0 ? paintRT_PosZ : paintRT_NegZ;
    }

    public RenderTexture GetPaintPosX() => paintRT_PosX;
    public RenderTexture GetPaintNegX() => paintRT_NegX;
    public RenderTexture GetPaintPosY() => paintRT_PosY;
    public RenderTexture GetPaintNegY() => paintRT_NegY;
    public RenderTexture GetPaintPosZ() => paintRT_PosZ;
    public RenderTexture GetPaintNegZ() => paintRT_NegZ;



    enum Axis { X, Y, Z }

    void PaintOnPlane(RenderTexture rt, Vector3 worldPos, Axis axis,
                      Texture2D brush, float size, Color color)
    {
        Vector3 local = transform.InverseTransformPoint(worldPos);
        Bounds b = GetComponent<Renderer>().localBounds;

        Vector2 uv;

        switch (axis)
        {
            case Axis.X:
                uv = new Vector2(
                    Mathf.InverseLerp(b.min.z, b.max.z, local.z),
                    Mathf.InverseLerp(b.min.y, b.max.y, local.y)
                );
                break;

            case Axis.Y:
                uv = new Vector2(
                    Mathf.InverseLerp(b.min.x, b.max.x, local.x),
                    Mathf.InverseLerp(b.min.z, b.max.z, local.z)
                );
                break;

            default: // Z
                uv = new Vector2(
                    Mathf.InverseLerp(b.min.x, b.max.x, local.x),
                    Mathf.InverseLerp(b.min.y, b.max.y, local.y)
                );
                break;
        }

        Stamp(rt, uv, brush, size, color);
    }

}
