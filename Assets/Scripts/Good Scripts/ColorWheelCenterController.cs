using UnityEngine;
using UnityEngine.EventSystems;

public class ColorWheelCenterController : MonoBehaviour, IPointerDownHandler
{
    [Header("References")]
    public PaintCore paintCore;

    [Header("Center Icons")]
    public GameObject blackIcon;
    public GameObject whiteIcon;

    [Header("Outer Wheel (optional)")]
    public GameObject outerSelector;

    enum Mode
    {
        Black,
        White,
        Colour
    }

    Mode currentMode = Mode.Black;

    void Start()
    {
        SetBlack();
    }

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

    // -----------------------------
    // MODE SETTERS
    // -----------------------------

    public void SetBlack()
    {
        currentMode = Mode.Black;

        paintCore.SetPaintColor(Color.black);

        blackIcon.SetActive(true);
        whiteIcon.SetActive(false);

        if (outerSelector)
            outerSelector.SetActive(false);
    }

    public void SetWhite()
    {
        currentMode = Mode.White;

        paintCore.SetPaintColor(Color.white);

        blackIcon.SetActive(false);
        whiteIcon.SetActive(true);

        if (outerSelector)
            outerSelector.SetActive(false);
    }

    // -----------------------------
    // CALLED BY OUTER WHEEL
    // -----------------------------

    public void EnterColourMode()
    {
        currentMode = Mode.Colour;

        blackIcon.SetActive(false);
        whiteIcon.SetActive(false);

        if (outerSelector)
            outerSelector.SetActive(true);
    }
}
