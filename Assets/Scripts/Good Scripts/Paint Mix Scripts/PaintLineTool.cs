using UnityEngine;

public class PaintLineTool : MonoBehaviour
{
    public float rayDistance = 10f;
    public PaintCore paintCore;

    private PaintSurfaceBase surface;
    private Vector2 uvA;
    private Vector2 uvB;
    private bool hasFirstPoint = false;

    [Header("Line Settings")]
    [Range(0.001f, 0.1f)]
    public float lineThickness = 0.02f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPlacePoint();
        }
    }

    void TryPlacePoint()
    {
        Ray ray = paintCore.cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance))
            return;

        PaintSurfaceBase hitSurface =
            hit.collider.GetComponentInParent<PaintSurfaceBase>();

        if (!hitSurface)
            return;

        // Get paintable UV
        if (!hitSurface.TryGetPaintUV(hit, out Vector2 uv))
            return;

        if (!hasFirstPoint)
        {
            surface = hitSurface;
            uvA = uv;
            hasFirstPoint = true;
        }
        else
        {
            if (hitSurface != surface)
            {
                // optional: cancel or reset
                return;
            }

            uvB = uv;
            PaintLine();
            hasFirstPoint = false;
        }
    }

    void PaintLine()
    {
        Texture2D brush = paintCore.GetBrushTexture();
        Color color = paintCore.CurrentPaintColor;

        //float worldSize = surface.GetSurfaceBrushSize();
       // float size = worldSize * surface.textureSize * lineThickness;
        //size = Mathf.Max(1f, size);

        //Code based on spray size -Test later

        float worldSize = surface.GetSurfaceBrushSize();
        float size = worldSize * surface.textureSize * lineThickness;
        size = Mathf.Max(1f, size);


        float dist = Vector2.Distance(uvA, uvB);
        int steps = Mathf.CeilToInt(dist * surface.textureSize);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 uv = Vector2.Lerp(uvA, uvB, t);

            surface.PaintAtUV(uv, brush, size, color);
        }
    }
}
