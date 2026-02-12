using UnityEngine;

public class BrushReticleWorld : MonoBehaviour
{
        public PaintCore paintCore;
        public Transform reticle;
        public Camera cam;

        public float minRadius = 0.02f;
        public float maxRadius = 2.0f;
    [Header("Visual Scale by Mode")]
    public float sprayVisualScale = 4f;
    public float precisionVisualScale = 1.5f;
    public float singleRayVisualScale = 1.0f;
    void Awake()
        {
            if (!cam) cam = Camera.main;
        }

        void Update()
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));

            if (!Physics.Raycast(ray, out RaycastHit hit, paintCore.sprayDistance))
            {
                reticle.gameObject.SetActive(false);
                return;
            }

            PaintSurfaceBase surface =
                hit.collider.GetComponentInParent<PaintSurfaceBase>();

            if (!surface)
            {
                reticle.gameObject.SetActive(false);
                return;
            }

            reticle.gameObject.SetActive(true);

            // Position & orient
            reticle.position = hit.point + hit.normal * 0.001f;
        reticle.rotation = Quaternion.LookRotation(-hit.normal);

        float distance = hit.distance;

            // World-space spray radius
            float radius;

            if (paintCore.paintSystem == PaintCore.PaintSystem.SprayCone)
            {
                radius = Mathf.Tan(paintCore.sprayAngle * Mathf.Deg2Rad) * distance;
            }
            else
            {
                radius = paintCore.BrushWorldRadius;
            }

            radius = Mathf.Clamp(radius, minRadius, maxRadius);

        // Quad scaled uniformly
        reticle.localScale = Vector3.one * radius * 2f * GetVisualScale();
    }

    float GetVisualScale()
    {
        switch (paintCore.paintMode)
        {
            case PaintCore.PaintMode.Precision:
                return precisionVisualScale;

            case PaintCore.PaintMode.SingleRay:
                return singleRayVisualScale;

            case PaintCore.PaintMode.Spray:
            default:
                return sprayVisualScale;
        }
    }

}
