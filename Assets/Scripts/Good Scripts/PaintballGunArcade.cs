using UnityEngine;

public class PaintballGunArcade : MonoBehaviour
{
    public Camera cam;
    public float maxDistance = 10f;

    [Header("Paint Source")]
    public PaintCoreOldSystem paintCore;

    // Reuse the same paint logic
    PaintBallObjectArcade painter;

    void Awake()
    {
        if (!cam) cam = Camera.main;

        // Create a hidden helper instance
        GameObject helper = new GameObject("ArcadePaintHelper");
        helper.hideFlags = HideFlags.HideAndDontSave;

        painter = helper.AddComponent<PaintBallObjectArcade>();
    }

    void Update()
    {
        if (!paintCore) return;

        bool fireInput =
            paintCore.fireMode == PaintCoreOldSystem.FireMode.Once
                ? Input.GetMouseButtonDown(0)
                : Input.GetMouseButton(0);

        if (!fireInput) return;

        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f)
        );

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            return;

        // Inject current paint settings
        painter.brush = paintCore.GetBrushTexture();
        painter.size = paintCore.GetBrushSizePixels();
        painter.paintColor = paintCore.GetPaintColor();

        painter.PaintFromHit(hit);
    }
}
