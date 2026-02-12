using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorWheelCenterToggle : MonoBehaviour//, IPointerDownHandler
{
    [Header("References")]
    public PaintCore paintCore;

    public Button blackButton;
    public Button whiteButton;

    [Header("Center Icons")]
    public GameObject blackIcon;
    public GameObject whiteIcon;

    [Header("Outer Wheel (optional)")]
    public GameObject outerSelector;

    [Header("Outer Wheel")]
    public ColorWheelSelectorOuter outerWheel;
    bool IsInColourMode => currentMode == Mode.Colour;

    [Header("Mixing")]
    public PaintMixManager mixManager;
    enum Mode
    {
        Black,
        White,
        Colour
    }

    Mode currentMode = Mode.Black;


    private void Awake()
    {
        blackButton.onClick.AddListener(SetBlack);
        whiteButton.onClick.AddListener(SetWhite);
    }

    void Start()
    {
        SetBlack();

        if (outerWheel != null)
        {
            outerWheel.OnColourSelected += OnOuterColourSelected;
        }
    }

    void OnDestroy()
    {
        if (outerWheel != null)
        {
            outerWheel.OnColourSelected -= OnOuterColourSelected;
        }
    }

    /*
    // -----------------------------
    // CENTER CLICK (BLACK / WHITE)
    // -----------------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentMode == Mode.Black)
            SetWhite();
        else
            SetBlack();


    }
    */
    // -----------------------------
    // MODE SETTERS
    // -----------------------------
    public void SetBlack()
    {
        currentMode = Mode.Black;

        if (mixManager != null)
            mixManager.AssignRawColor(Color.black, "Black");
        else
            paintCore.SetPaintColor(Color.black); // fallback

        UpdateIcons();
        DisableOuter();
    }

    public void SetWhite()
    {
        currentMode = Mode.White;

        if (mixManager != null)
            mixManager.AssignRawColor(Color.white, "White");
        else
            paintCore.SetPaintColor(Color.white); // fallback

        UpdateIcons();
        DisableOuter();
    }


    // -----------------------------
    // CALLED BY OUTER WHEEL
    // -----------------------------
    public void EnterColourMode()
    {
        currentMode = Mode.Colour;

      //  blackIcon.SetActive(false);
      //  whiteIcon.SetActive(false);



        if (outerSelector)
            outerSelector.SetActive(true);
    }


    void OnOuterColourSelected(Color c)
    {
        currentMode = Mode.Colour;

        paintCore.SetPaintColor(c);

        UpdateIcons();

        if (outerSelector)
            outerSelector.SetActive(true);
    }

    void UpdateIcons()
    {
        blackIcon.SetActive(false);
        whiteIcon.SetActive(false);

        if (currentMode == Mode.Black)
            blackIcon.SetActive(true);
        else if (currentMode == Mode.White)
            whiteIcon.SetActive(true);
    }

    void DisableOuter()
    {
        if (outerWheel && outerWheel.highlightRect)
            outerWheel.highlightRect.gameObject.SetActive(false);

        if (outerSelector)
            outerSelector.SetActive(false);
    }
}