using UnityEngine;

public class InputControllerForPaint : MonoBehaviour
{
    [Header("References")]
    public PaintCore paintCore;
    public MouseLook mLook;
    public GameObject colourWheelUI;

    [Header("Keyboard Keys")]
    public KeyCode toggleWheelKey = KeyCode.Tab;
    public KeyCode precisionKey = KeyCode.Alpha1;
    public KeyCode sprayKey = KeyCode.Alpha2;
    public KeyCode fireModeKey = KeyCode.Mouse1;

    [Header("Controller Inputs")]
    public string toggleWheelButton = "ToggleWheel";
    public string precisionButton = "PrecisionMode";
    public string sprayButton = "SprayMode";
    public string fireButton = "Fire";

    bool wheelOpen;

    void Start()
    {
        ApplyWheelState(false);
    }

    void Update()
    {
        if (!paintCore) return;

        // --- Paint modes ---
        if (Input.GetKeyDown(precisionKey) || Input.GetButtonDown(precisionButton))
            paintCore.SetPaintMode(PaintCore.PaintMode.Precision);

        if (Input.GetKeyDown(sprayKey) || Input.GetButtonDown(sprayButton))
            paintCore.SetPaintMode(PaintCore.PaintMode.Spray);

        // --- Fire mode ---
        if (Input.GetKeyDown(fireModeKey) || Input.GetButtonDown(fireButton))
            paintCore.ToggleFireMode();

        // --- Colour wheel ---
        if (Input.GetKeyDown(toggleWheelKey) || Input.GetButtonDown(toggleWheelButton))
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