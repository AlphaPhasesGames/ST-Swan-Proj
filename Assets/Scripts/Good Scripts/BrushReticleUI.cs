using UnityEngine;
using UnityEngine.UI;

public class BrushReticleUI : MonoBehaviour
{
    public PaintCore paintCore;
    public RectTransform reticleRect;
    public Camera cam;

    public float minSize = 8f;
    public float maxSize = 400f;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!reticleRect) reticleRect = GetComponent<RectTransform>();
        Debug.Log("RETICLE AWAKE");
    }

    void Update()
    {
        UpdateReticle();
        Debug.Log("RETICLE UPDATE");
    }


    void UpdateReticle()
    {
        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f)
        );

        if (!Physics.Raycast(ray, out RaycastHit hit, paintCore.sprayDistance))
        {
            reticleRect.sizeDelta = Vector2.zero;
            return;
        }

        PaintSurfaceBase surface =
            hit.collider.GetComponent<PaintSurfaceBase>()
            ?? hit.collider.GetComponentInParent<PaintSurfaceBase>();

        if (!surface)
        {
            reticleRect.sizeDelta = Vector2.zero;
            return;
        }

        // Must be MeshCollider for UV-aware sizing
        if (!(hit.collider is MeshCollider))
        {
            reticleRect.sizeDelta = Vector2.one * minSize;
            return;
        }

        float pixelSize;

        if (paintCore.paintSystem == PaintCore.PaintSystem.SprayCone)
        {
            pixelSize = paintCore.CalculateBrushSizeFromWorldPublic(hit);
        }
        else
        {
            pixelSize = surface.GetLegacyBrushSize();
        }


        float distance = hit.distance;

        // Invert so closer = bigger
        float distanceScale = paintCore.sprayDistance / distance;

        // Clamp so it never explodes or vanishes
        distanceScale = Mathf.Clamp(distanceScale, 0.25f, 1.5f);

        // Apply distance scaling
        pixelSize *= distanceScale;

        reticleRect.sizeDelta =
            Vector2.one * Mathf.Clamp(pixelSize, minSize, maxSize);
    }

    /*
    void UpdateReticle()
    {
        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f)
        );

        if (!Physics.Raycast(ray, out RaycastHit hit, paintCore.sprayDistance))
        {
           // reticleRect.gameObject.SetActive(false);
            return;
        }

        reticleRect.gameObject.SetActive(true);

        float pixelSize;

        if (paintCore.paintSystem == PaintCore.PaintSystem.SprayCone)
        {
            pixelSize = paintCore.CalculateBrushSizeFromWorldPublic(hit);
        }
        else
        {
            PaintSurfaceBase surface = hit.collider.GetComponent<PaintSurfaceBase>();
             ?? hit.collider.GetComponentInParent<PaintSurfaceBase>();
            pixelSize = surface
                ? surface.GetLegacyBrushSize()
                : paintCore.baseBrushSize;
        }

        float uiSize = Mathf.Clamp(pixelSize, minSize, maxSize);
        reticleRect.sizeDelta = Vector2.one * uiSize;

        Debug.Log("Reticle update running");

        Ray ray2 = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f)
        );

        if (!Physics.Raycast(ray, out RaycastHit hit2, paintCore.sprayDistance))
        {
            Debug.Log("NO HIT");
            reticleRect.gameObject.SetActive(false);
            return;
        }

        Debug.Log("HIT: " + hit.collider.name);
        reticleRect.gameObject.SetActive(true);
    }
    */

}

