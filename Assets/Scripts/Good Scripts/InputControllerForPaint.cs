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
    public Button brushButton;
    public Button sprayButton;
    public Button spackleSprayButton;
    public Button pencilButton;
    public Button blobBrushButton;
    public Button shapesBrushButton;
    public Button squareBrushButton;
    public Button starBrushButton;
    public Button splatBrushButton;
    public Button eraseButton;
    public Button lineModeButton;
    public Button calligraphyModeButton;
    public Button setSpraySizeButton;
    public Button setSprayAngleButton;
    public Button shapesClosePanal;
    public Button paintBallGunButon;
    public Button smudgeButton;
    public Button waterColourButton;
    public Button diluteButton;

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
    public KeyCode smudgeKey = KeyCode.Alpha8;
    public KeyCode waterColourKey = KeyCode.Alpha9;

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

    public DrawMode currentDrawMode = DrawMode.Paint;

    public enum DrawMode { Paint, Line }

    [SerializeField] private ParticleSystem hoseCore;
    [SerializeField] private ParticleSystem hoseSpread;

    [Header("Hose Materials")]
    [SerializeField] private Material hoseTrailMaterial;
    [SerializeField] private Material hoseBlobMaterial;

    [Header("Hose Blobs")]
    [SerializeField] private PaintBlob hoseBlobPrefab;
    public Transform hoseMuzzle;
    [SerializeField] private float hoseStartSpeed = 50f;

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
        smudgeButton.onClick.AddListener(SetSmudge);
        waterColourButton.onClick.AddListener(SetWatercolour);
        diluteButton.onClick.AddListener(DilutePaint);
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

        paintCore.palette.OnActiveColorChanged += OnPaintColourChanged;
    }

    void Update()
    {
        if (Input.GetButtonDown("ToggleWheel"))
            ToggleColourWheel();

        if (!paintCore) return;

        bool fireHeld = Input.GetButton("Fire 1");
        bool fireDown = Input.GetButtonDown("Fire 1");

        paintCore.SetFireInput(fireHeld, fireDown);

        HandleHoseVFX(fireHeld);

        if (toolSelector)
            toolSelector.anchoredPosition = Vector2.Lerp(
                toolSelector.anchoredPosition,
                toolSelectorTarget,
                Time.unscaledDeltaTime * selectorLerpSpeed);



        if (shapeSelector)
            shapeSelector.anchoredPosition = Vector2.Lerp(
                shapeSelector.anchoredPosition,
                shapeSelectorTarget,
                Time.unscaledDeltaTime * selectorLerpSpeed);

        if (Input.GetMouseButtonDown(0))
        {
            if (currentDrawMode == DrawMode.Line && lineDraw)
                lineDraw.TryPlacePoint();
        }

        if (Input.GetKeyDown(precisionKey)) SetPrecision();
        if (Input.GetKeyDown(sprayKey)) SetNormalSpray();
        if (Input.GetKeyDown(singleRayKey)) SetSingleRay();
        if (Input.GetKeyDown(blobBrushKey)) SetBlobBrush();
        if (Input.GetKeyDown(squareBrushKey)) SetSquareBrush();
        if (Input.GetKeyDown(eraseKey)) ToggleErase();
        if (Input.GetKeyDown(fireModeKey)) paintCore.ToggleFireMode();
        //if (Input.GetKeyDown(toggleWheelKey)) ToggleColourWheel();
        if (Input.GetKeyDown(calligraphyKey)) SetCalligraphy();
        if (Input.GetKeyDown(paintBallGunKey)) EnablePaintballGun();
        if (Input.GetKeyDown(smudgeKey)) SetSmudge();
        if (Input.GetKeyDown(waterColourKey)) SetWatercolour();
    }

    // ---------------- TOOLS ----------------

    void SetPrecision()
    {
        DisableErase();
        DisableLineMode();

        SetBrushModel(paintBrushFine);

        paintCore.SetBrushBehaviour(PaintCore.BrushBehaviour.Normal);
        paintCore.SetPaintMode(PaintCore.PaintMode.Precision);

        MoveToolSelectorTo(brushButton);
    }

    void SetNormalSpray()
    {
        DisableErase();
        DisableLineMode();

        SetBrushModel(null);

        paintCore.SetBrushBehaviour(PaintCore.BrushBehaviour.Normal);
        paintCore.SetPaintMode(PaintCore.PaintMode.Spray);
        paintCore.SetSprayStyle(PaintCore.SprayStyle.Normal);

        paintballGunLogic.SetSprayVisual(PaintballGun.SprayVisual.SprayCan);

        ForceBlobBrush();
        MoveToolSelectorTo(sprayButton);
    }

    void SetSpackleSpray()
    {
        DisableErase();
        DisableLineMode();

        SetBrushModel(null);

        paintCore.SetPaintMode(PaintCore.PaintMode.Spray);
        paintCore.SetSprayStyle(PaintCore.SprayStyle.Spackle);

        paintballGunLogic.SetSprayVisual(PaintballGun.SprayVisual.SprayCan);

        ForceBlobBrush();
        MoveToolSelectorTo(spackleSprayButton);
    }

    void DilutePaint()
    {
        DisableErase();
        DisableLineMode();

        SetBrushModel(paintBrushFine);

        paintCore.SetBrushBehaviour(PaintCore.BrushBehaviour.Dilute);
        paintCore.SetPaintMode(PaintCore.PaintMode.SingleRay);

        ForceBlobBrush();
        MoveToolSelectorTo(diluteButton);
    }

    void SetSmudge()
    {
        DisableErase();
        DisableLineMode();

        SetBrushModel(paintBrushSuperFine);

        paintCore.SetBrushBehaviour(PaintCore.BrushBehaviour.Smudge);
        paintCore.SetPaintMode(PaintCore.PaintMode.SingleRay);

        ForceBlobBrush();
        MoveToolSelectorTo(smudgeButton);
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

        SetBrushModel(null);

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

    // ---------------- SHAPES ----------------

    void SetBlobBrush()
    {
        DisableErase();
        DisableLineMode();
        paintCore.SetBrushShape(PaintCore.BrushShape.Blob);
        MoveShapeSelectorTo(blobBrushButton);
    }

    void SetSquareBrush()
    {
        DisableErase();
        DisableLineMode();
        paintCore.SetBrushShape(PaintCore.BrushShape.Square);
        MoveShapeSelectorTo(squareBrushButton);
    }

    void SetSplatBrush()
    {
        DisableErase();
        DisableLineMode();
        paintCore.SetBrushShape(PaintCore.BrushShape.Splat);
        MoveShapeSelectorTo(splatBrushButton);
    }

    void SetStarBrush()
    {
        DisableErase();
        DisableLineMode();
        paintCore.SetBrushShape(PaintCore.BrushShape.Star);
        MoveShapeSelectorTo(starBrushButton);
    }

    void ForceBlobBrush()
    {
        paintCore.SetBrushShape(PaintCore.BrushShape.Blob);
        MoveShapeSelectorTo(blobBrushButton);
    }

    // ---------------- WATERCOLOUR ----------------

    void SetWatercolour()
    {
        DisableErase();
        DisableLineMode();

        SetBrushModel(paintBrushFine);

        paintCore.SetBrushBehaviour(PaintCore.BrushBehaviour.Watercolour);
        paintCore.SetPaintMode(PaintCore.PaintMode.SingleRay);

        ForceBlobBrush();
        MoveToolSelectorTo(waterColourButton);
    }

    // ---------------- UI / STATE ----------------

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

    void ToggleErase()
    {
        bool newState = !paintCore.isErasing;
        paintCore.SetEraseMode(newState);

        if (eraseHighlight)
            eraseHighlight.SetActive(newState);
    }

    void DisableErase()
    {
        if (!paintCore.isErasing) return;

        paintCore.SetEraseMode(false);

        if (eraseHighlight)
            eraseHighlight.SetActive(false);
    }

    void ToggleColourWheel() => ApplyWheelState(!wheelOpen);

    void ApplyWheelState(bool open)
    {
        wheelOpen = open;
        colourWheelUI?.SetActive(open);

        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

        if (mLook) mLook.enabled = !open;

        if (!open && TooltipController.Instance != null)
            TooltipController.Instance.ForceHide();

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
        if (currentDrawMode != DrawMode.Line) return;

        currentDrawMode = DrawMode.Paint;

        if (lineModeHighlight)
            lineModeHighlight.SetActive(false);

        if (lineDraw)
            lineDraw.CancelLine();
    }

    void ApplyDrawMode()
    {
        paintCore.SetInputEnabled(
            currentDrawMode == DrawMode.Paint &&
            !wheelOpen
        );
    }

    void SetBrushModel(GameObject activeBrush)
    {
        paintBrushFine.SetActive(false);
        paintBrushSuperFine.SetActive(false);

        if (activeBrush != null)
            activeBrush.SetActive(true);
    }

    public void OpenShapesPanel()
    {
        shapesOverlay.SetActive(true);
        shapesPanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseShapesPanel()
    {
        shapesPanel.SetActive(false);
        shapesOverlay.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void SetSprayAngle()
    {
        paintCore.SetScrollMode(PaintCore.ScrollMode.SpraySpread);
    }

    void SetSpraySize()
    {
        paintCore.SetScrollMode(PaintCore.ScrollMode.BrushSize);
    }

    void HandleHoseVFX(bool isFiring)
    {
        if (!hoseCore) return;

        if (isFiring)
        {
            if (!hoseCore.isEmitting) hoseCore.Play(true);
            if (hoseSpread && !hoseSpread.isPlaying) hoseSpread.Play(true);
        }
        else
        {
            hoseCore.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            if (hoseSpread)
                hoseSpread.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
    void SpawnBlob()
    {
        if (!hoseBlobPrefab || !hoseMuzzle) return;

        PaintBlob blob = Instantiate(
            hoseBlobPrefab,
            hoseMuzzle.position,
            hoseMuzzle.rotation
        );

        float size =
            (paintCore.useFixedWorldBrushSize
            ? paintCore.fixedWorldBrushSize
            : paintCore.brushWorldSize)
            * Random.Range(0.85f, 1.15f);

        blob.Init(paintCore, size);
        //blob.SetColor(paintCore.GetFinalPaintColor());
        Rigidbody rb = blob.GetComponent<Rigidbody>();

        Vector3 dir = hoseMuzzle.forward +
                      Random.insideUnitSphere * 0.02f;

        Vector3 velocity = dir.normalized * hoseStartSpeed;

        rb.linearVelocity = velocity;
    }

    void ApplyColourToVFX(Color c)
    {
        //  Mesh particles  material colour
        if (hoseBlobMaterial)
            hoseBlobMaterial.SetColor("_BaseColor", c);

        //  Trail material
        if (hoseTrailMaterial)
            hoseTrailMaterial.SetColor("_BaseColor", c);

        //  OPTIONAL: if you still use StartColor for alpha control
        if (hoseCore)
        {
            var main = hoseCore.main;
            main.startColor = c;
        }

        if (hoseSpread)
        {
            var main = hoseSpread.main;
            main.startColor = c;
        }
    }


    void OnPaintColourChanged(Color _)
    {
        ApplyColourToVFX(paintCore.GetFinalPaintColor());
    }



}
