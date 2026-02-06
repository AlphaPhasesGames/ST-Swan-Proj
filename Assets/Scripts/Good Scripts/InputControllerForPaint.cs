using UnityEngine;
using UnityEngine.UI;

public class InputControllerForPaint : MonoBehaviour
{
    [Header("References")]
    public PaintCore paintCore;
    public MouseLook mLook;
    public PaintLineTool lineDraw;
    public GameObject colourWheelUI;

    [Header("UI Buttons")]
    public Button brushButton;        // Precision
    public Button sprayButton;          // Normal spray
    public Button spackleSprayButton;   // Spackle spray
    public Button pencilButton;       // Single Ray
    public Button blobBrushButton;    // Blob Shape
    public Button squareBrushButton;  // Square Shape
    public Button eraseButton;        // Erase toggle
    public Button lineModeButton;
    public GameObject lineModeHighlight;
    [Header("Keyboard")]
    public KeyCode toggleWheelKey = KeyCode.Tab;
    public KeyCode precisionKey = KeyCode.Alpha1;
    public KeyCode sprayKey = KeyCode.Alpha2;
    public KeyCode singleRayKey = KeyCode.Alpha3;
    public KeyCode blobBrushKey = KeyCode.Alpha4;
    public KeyCode squareBrushKey = KeyCode.Alpha5;
    public KeyCode eraseKey = KeyCode.R;
    public KeyCode fireModeKey = KeyCode.Mouse1;

    bool wheelOpen;

    [Header("Selectors")]
    public RectTransform toolSelector;
    public RectTransform shapeSelector;
    public GameObject eraseHighlight;
    public float selectorLerpSpeed = 15f;

    Vector2 toolSelectorTarget;
    Vector2 shapeSelectorTarget;


    [Header("Draw Mode")]
    public DrawMode currentDrawMode = DrawMode.Paint;

    // ---------------- SETUP ----------------

    public enum DrawMode
    {
        Paint,
        Line
    }

    void Awake()
    {
        brushButton?.onClick.AddListener(SetPrecision);
        sprayButton?.onClick.AddListener(SetNormalSpray);
        spackleSprayButton?.onClick.AddListener(SetSpackleSpray);
        pencilButton?.onClick.AddListener(SetSingleRay);

        blobBrushButton?.onClick.AddListener(SetBlobBrush);
        squareBrushButton?.onClick.AddListener(SetSquareBrush);

        eraseButton?.onClick.AddListener(ToggleErase);
        lineModeButton?.onClick.AddListener(ToggleLineMode);
    }

    void Start()
    {
        ApplyWheelState(false);
        ApplyDrawMode();
        if (toolSelector)
            toolSelectorTarget = toolSelector.anchoredPosition;

        if (shapeSelector)
            shapeSelectorTarget = shapeSelector.anchoredPosition;

        if (eraseHighlight)
            eraseHighlight.SetActive(false);
    }

    // ---------------- UPDATE ----------------

    void Update()
    {
        if (!paintCore) return;

        // Smooth selector movement
        if (toolSelector)
        {
            toolSelector.anchoredPosition = Vector2.Lerp(
                toolSelector.anchoredPosition,
                toolSelectorTarget,
                Time.unscaledDeltaTime * selectorLerpSpeed
            );
        }

        if (shapeSelector)
        {
            shapeSelector.anchoredPosition = Vector2.Lerp(
                shapeSelector.anchoredPosition,
                shapeSelectorTarget,
                Time.unscaledDeltaTime * selectorLerpSpeed
            );
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (currentDrawMode == DrawMode.Line && lineDraw)
            {
                lineDraw.TryPlacePoint();
            }
        }


        // Keyboard input
        if (Input.GetKeyDown(precisionKey)) SetPrecision();
        if (Input.GetKeyDown(sprayKey)) SetSpray();
        if (Input.GetKeyDown(singleRayKey)) SetSingleRay();
        if (Input.GetKeyDown(blobBrushKey)) SetBlobBrush();
        if (Input.GetKeyDown(squareBrushKey)) SetSquareBrush();
        if (Input.GetKeyDown(eraseKey)) ToggleErase();
        if (Input.GetKeyDown(fireModeKey)) paintCore.ToggleFireMode();
        if (Input.GetKeyDown(toggleWheelKey)) ToggleColourWheel();



    }

    // ---------------- TOOL MODES ----------------

    void SetPrecision()
    {
        DisableErase();
        DisableLineMode();
        paintCore.SetPaintMode(PaintCore.PaintMode.Precision);
        MoveToolSelectorTo(brushButton);
    }

    void SetSpray()
    {
        DisableErase();
        paintCore.SetPaintMode(PaintCore.PaintMode.Spray);
        ForceBlobBrush();
        MoveToolSelectorTo(sprayButton);
    }

    void SetSingleRay()
    {
        DisableErase();
        paintCore.SetPaintMode(PaintCore.PaintMode.SingleRay);
        ForceBlobBrush();
        MoveToolSelectorTo(pencilButton);
    }

    // ---------------- BRUSH SHAPES ----------------

    void SetBlobBrush()
    {
        DisableErase();
        paintCore.SetBrushShape(PaintCore.BrushShape.Blob);
        MoveShapeSelectorTo(blobBrushButton);
        DisableLineMode();
    }

    void SetSquareBrush()
    {
        DisableErase();
        paintCore.SetBrushShape(PaintCore.BrushShape.Square);
        MoveShapeSelectorTo(squareBrushButton);
        DisableLineMode();
    }

    void ForceBlobBrush()
    {
        paintCore.SetBrushShape(PaintCore.BrushShape.Blob);
        MoveShapeSelectorTo(blobBrushButton);

    }

    // ---------------- ERASE ----------------

    void ToggleErase()
    {
        if (!paintCore) return;

        bool newState = !paintCore.isErasing;
        paintCore.SetEraseMode(newState);

        if (eraseHighlight)
            eraseHighlight.SetActive(newState);
    }

    void DisableErase()
    {
        if (paintCore.isErasing)
        {
            paintCore.SetEraseMode(false);
            if (eraseHighlight)
                eraseHighlight.SetActive(false);
        }
    }

    // ---------------- SELECTOR HELPERS ----------------

    void MoveToolSelectorTo(Button button)
    {
        if (!toolSelector || !button) return;
        toolSelectorTarget = button.GetComponent<RectTransform>().anchoredPosition;
    }

    void MoveShapeSelectorTo(Button button)
    {
        if (!shapeSelector || !button) return;
        shapeSelectorTarget = button.GetComponent<RectTransform>().anchoredPosition;
    }

    // ---------------- COLOUR WHEEL ----------------

    void ToggleColourWheel()
    {
        ApplyWheelState(!wheelOpen);
    }

    void ApplyWheelState(bool open)
    {
        wheelOpen = open;
        colourWheelUI?.SetActive(open);

        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

        if (mLook) mLook.enabled = !open;
        ApplyDrawMode();
    }

    void ToggleLineMode()
    {
        bool enableLine = currentDrawMode != DrawMode.Line;

        currentDrawMode = enableLine ? DrawMode.Line : DrawMode.Paint;

        if (lineModeHighlight)
            lineModeHighlight.SetActive(enableLine);

        if (!enableLine && lineDraw)
            lineDraw.CancelLine();
        DisableErase();
        
        ApplyDrawMode();
    }

    void DisableLineMode()
    {
        if (currentDrawMode == DrawMode.Line)
        {
            currentDrawMode = DrawMode.Paint;
            if (lineModeHighlight)
                lineModeHighlight.SetActive(false);

            if (lineDraw)
                lineDraw.CancelLine();
        }
    }

    void ApplyDrawMode()
    {
        if (!paintCore) return;

        // Paint only when in Paint mode AND wheel is closed
        paintCore.enabled =
            currentDrawMode == DrawMode.Paint &&
            !wheelOpen;
    }

    void SetNormalSpray()
    {
        DisableErase();
        DisableLineMode();

        paintCore.SetPaintMode(PaintCore.PaintMode.Spray);
        paintCore.SetSprayStyle(PaintCore.SprayStyle.Normal); //  if you have this

        ForceBlobBrush();
        MoveToolSelectorTo(sprayButton);
    }

    void SetSpackleSpray()
    {
        DisableErase();
        DisableLineMode();

        paintCore.SetPaintMode(PaintCore.PaintMode.Spray);
        paintCore.SetSprayStyle(PaintCore.SprayStyle.Spackle); // 

        ForceBlobBrush();
        MoveToolSelectorTo(spackleSprayButton);
    }

}
