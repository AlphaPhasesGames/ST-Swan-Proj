using UnityEngine;

public class InputControllerForPaint : MonoBehaviour
{
    [Header("References")]
    public PaintCore paintCore;
    public MouseLook mLook;
    public GameObject colourWheelUI;

    [Header("Debug Keys")]
    public KeyCode toggleWheelKey = KeyCode.Tab;
    public KeyCode precisionKey = KeyCode.Alpha1;
    public KeyCode sprayKey = KeyCode.Alpha2;
    public KeyCode fireModeKey = KeyCode.Mouse1;

    bool wheelOpen;

    void Start()
    {
        ApplyWheelState(false); // start in gameplay mode
    }

    void Update()
    {
        if (!paintCore) return;

        // --- Paint mode ---
        if (Input.GetKeyDown(precisionKey))
            paintCore.SetPaintMode(PaintCore.PaintMode.Precision);

        if (Input.GetKeyDown(sprayKey))
            paintCore.SetPaintMode(PaintCore.PaintMode.Spray);

        // --- Fire mode ---
        if (Input.GetKeyDown(fireModeKey))
            paintCore.ToggleFireMode();

        // --- Colour wheel toggle ---
        if (Input.GetKeyDown(toggleWheelKey))
            ToggleColourWheel();
    }

    void ToggleColourWheel()
    {
        ApplyWheelState(!wheelOpen);
    }

    void ApplyWheelState(bool open)
    {
        wheelOpen = open;

        // UI
        if (colourWheelUI)
            colourWheelUI.SetActive(open);

        // Cursor
        Cursor.visible = open;
        Cursor.lockState = open
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        // Player look
        if (mLook)
            mLook.enabled = !open;

        // Painting
        if (paintCore)
            paintCore.enabled = !open;
    }
}
