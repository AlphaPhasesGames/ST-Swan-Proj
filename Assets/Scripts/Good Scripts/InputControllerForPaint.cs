using UnityEngine;
using UnityEngine.UI;

public class InputControllerForPaint : MonoBehaviour
{
    [Header("References")]
    public PaintCore paintCore;
    public MouseLook mLook;
    public PaintballGun paintballGunLogic;
    public PaintLineTool lineDraw;
    public GameObject colourWheelUI;

    [Header("UI Buttons")]
    public Button brushButton;        // Precision
    public Button sprayButton;          // Normal spray
    public Button spackleSprayButton;   // Spackle spray
    public Button pencilButton;       // Single Ray
    public Button blobBrushButton;    // Blob Shape
    public Button shapesBrushButton;  // Square Shape
    public Button squareBrushButton;  // Square Shape
    public Button starBrushButton;  // Square Shape
    public Button splatBrushButton;  // Square Shape
    public Button eraseButton;        // Erase toggle
    public Button lineModeButton;
    public Button calligraphyModeButton;
    public Button setSpraySizeButton;
    public Button setSprayAngleButton;
    public Button shapesClosePanal;
    public Button paintBallGunButon;
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
    public KeyCode calligraphyKey = KeyCode.Alpha6;
    public KeyCode paintBallGunKey = KeyCode.Alpha7;
    bool wheelOpen;

    [Header("Panels")]
    public GameObject shapesPanel;
    public GameObject shapesOverlay;

    [Header("Selectors")]
    public RectTransform toolSelector;
    public RectTransform shapeSelector;
    public GameObject eraseHighlight;
    public float selectorLerpSpeed = 15f;

    Vector2 toolSelectorTarget;
    Vector2 shapeSelectorTarget;

    [Header("PaintToolModels")]
    public GameObject paintballGun;
    public GameObject paintBrushFine;
    public GameObject sprayCan;
    public GameObject paintBrushSuperFine;
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

        shapesBrushButton?.onClick.AddListener(OpenShapesPanel);
        blobBrushButton?.onClick.AddListener(SetBlobBrush);
        squareBrushButton?.onClick.AddListener(SetSquareBrush);
        starBrushButton?.onClick.AddListener(SetStarBrush);
        splatBrushButton?.onClick.AddListener(SetSplatBrush);
        shapesClosePanal?.onClick.AddListener(CloseShapesPanel);
        eraseButton?.onClick.AddListener(ToggleErase);
        lineModeButton?.onClick.AddListener(ToggleLineMode);
        calligraphyModeButton?.onClick.AddListener(SetCalligraphy);
        setSprayAngleButton.onClick.AddListener(SetSprayAngle);
        setSpraySizeButton.onClick.AddListener(SetSpraySize);
        paintBallGunButon.onClick.AddListener(EnablePaintballGun);
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
        if (Input.GetKeyDown(calligraphyKey)) SetCalligraphy();
        if (Input.GetKeyDown(paintBallGunKey)) EnablePaintballGun();
        //if(Input.GetKeyDown(star))


    }

    // ---------------- TOOL MODES ----------------

    void SetPrecision()
    {
        DisableErase();
        DisableLineMode();

        SetBrushModel(paintBrushFine);

        paintCore.SetBrushBehaviour(PaintCore.BrushBehaviour.Normal);
        paintCore.SetPaintMode(PaintCore.PaintMode.Precision);

        MoveToolSelectorTo(brushButton);
    }


    void SetSpray()
    {
        DisableErase();
        DisableLineMode();

        SetBrushModel(null); // hides both brushes

        paintCore.SetBrushBehaviour(PaintCore.BrushBehaviour.Normal);
        paintCore.SetPaintMode(PaintCore.PaintMode.Spray);
        paintballGunLogic.SetSprayVisual(PaintballGun.SprayVisual.SprayCan);

        ForceBlobBrush();
        MoveToolSelectorTo(sprayButton);
    }


    void SetSingleRay()
    {
        DisableErase();
        DisableLineMode();

        paintCore.SetBrushBehaviour(PaintCore.BrushBehaviour.Normal);
        paintCore.SetPaintMode(PaintCore.PaintMode.SingleRay);
        SetBrushModel(paintBrushSuperFine);
        ForceBlobBrush();
        MoveToolSelectorTo(pencilButton);
    }
    void EnablePaintballGun()
    {
        DisableErase();
        DisableLineMode();

        SetBrushModel(null); //  THIS is the missing piece

        paintCore.SetPaintMode(PaintCore.PaintMode.Spray);
        paintballGunLogic.SetSprayVisual(PaintballGun.SprayVisual.PaintballGun);

        MoveToolSelectorTo(paintBallGunButon);
    }



    void SetCalligraphy()
    {
        DisableErase();
        DisableLineMode();

        SetBrushModel(paintBrushSuperFine);

        paintCore.SetBrushBehaviour(PaintCore.BrushBehaviour.Calligraphy);
        paintCore.SetPaintMode(PaintCore.PaintMode.SingleRay);

        ForceBlobBrush();
        MoveToolSelectorTo(calligraphyModeButton);
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

    void SetSplatBrush()
    {
        DisableErase();
        paintCore.SetBrushShape(PaintCore.BrushShape.Splat);
        MoveShapeSelectorTo(splatBrushButton);
        DisableLineMode();
    }

    void SetStarBrush()
    {
        DisableErase();
        paintCore.SetBrushShape(PaintCore.BrushShape.Star);
        MoveShapeSelectorTo(starBrushButton);
        DisableLineMode();
    }

    void ForceBlobBrush()
    {
        paintCore.SetBrushShape(PaintCore.BrushShape.Blob);
        MoveShapeSelectorTo(blobBrushButton);

    }
    public void OpenShapesPanel()
    {
        shapesOverlay.SetActive(true);
        shapesPanel.SetActive(true);
    }

    public void CloseShapesPanel()
    {
        shapesPanel.SetActive(false);
        shapesOverlay.SetActive(false);
    }

    void SetSprayAngle()
    {
        paintCore.SetScrollMode(PaintCore.ScrollMode.SpraySpread);
    }

    void SetSpraySize()
    {
        paintCore.SetScrollMode(PaintCore.ScrollMode.BrushSize);
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

        //  HARD KILL TOOLTIPS WHEN UI CLOSES
        if (!open && TooltipController.Instance != null)
        {
            TooltipController.Instance.ForceHide();
        }

        ApplyDrawMode();
    }

    void ToggleLineMode()
    {
        bool enableLine = currentDrawMode != DrawMode.Line;

        currentDrawMode = enableLine ? DrawMode.Line : DrawMode.Paint;

        if (lineModeHighlight)
            lineModeHighlight.SetActive(enableLine);
        SetBrushModel(paintBrushSuperFine);
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
        paintCore.SetSprayStyle(PaintCore.SprayStyle.Normal);

        //  THIS is the only thing that matters
        paintballGunLogic.SetSprayVisual(PaintballGun.SprayVisual.SprayCan);

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

    void SetBrushModel(GameObject activeBrush)
    {
        paintBrushFine.SetActive(false);
        paintBrushSuperFine.SetActive(false);

        if (activeBrush != null)
            activeBrush.SetActive(true);
    }

}
