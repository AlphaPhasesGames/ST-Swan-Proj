using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This class controls the entire paint mixing UI and logic
public class PaintMixManager : MonoBehaviour
{
    // ============================
    // PAINT DATA (THE ACTUAL COLOURS)
    // ============================

    [Header("Input Colours")]
    public PaintPaletteSavedColoursUI paletteUI;
    // The paint stored in Slot A
    public PaintColour colorA;
   
    // The paint stored in Slot B
    public PaintColour colorB;

    // ============================
    // UI BUTTONS
    // ============================

    // Button the player presses to arm Slot A
    public Button slotAButton;

    // Button the player presses to arm Slot B
    public Button slotBButton;

    // Button that mixes A + B
    public Button mixColours;

    // Button that commits the mixed colour to the palette
    public Button selectNewColour;

    public Button replaceColourButton;
    // ============================
    // MIX PANEL STATE
    // ============================

    // The whole panel GameObject (can be turned on/off)
    public GameObject paintMixPanalObject;

    // Button that opens / closes the mix panel
    public Button openMixPanel;

    // Tracks whether the panel is currently open
    public bool mixPanelOpen;

    [Header("Palette Actions")]
    public Button saveColourButton;


    // ============================
    // PALETTE REFERENCE
    // ============================

    [Header("References")]

    // The palette that stores saved colours
    public PaintPalette palette;

    // ============================
    // SLOT UI IMAGES
    // ============================

    // UI image that shows Slot A's colour
    public Image colourSelected1;

    // UI image that shows Slot B's colour
    public Image colourSelected2;

    // UI image that shows the final mixed colour
    public Image finalMixedColours;

    // ============================
    // SLIDER + TEXT
    // ============================

    // Text that displays the slider value (0–100)
    public TextMeshProUGUI mixValueText;

    // Slider that controls how much A vs B is mixed
    public Slider valueSlider;

    // ============================
    // SLOT STATE TRACKING
    // ============================

    // Has Slot A actually been filled yet?
    public bool slotAFilled = false;

    // Has Slot B actually been filled yet?
    public bool slotBFilled = false;

    // Has the player pressed "Mix" yet?
    bool hasMixed = false;

    // ============================
    // MIX VALUE (INTERNAL LOGIC)
    // ============================

    // This is ALWAYS between 0 and 1
    // (UI uses 0–100, we convert it)
    [Range(0f, 1f)]
    public float mixAmount = 0.5f;

    // ============================
    // MIX RESULT
    // ============================

    [Header("Result (Preview)")]

    // The resulting mixed paint colour
    public PaintColour mixedColor;

    // The colour wheel that sends colour selections
    public ColorWheelSelectorOuter wheel;

    // Event fired when a mixed colour is committed
    public System.Action<PaintColour> OnMixedColorCreated;

    // ============================
    // SLOT SELECTION STATE
    // ============================

    // Which slot (if any) is currently armed
    public enum MixSlot
    {
        None, // No slot selected
        A,    // Slot A armed
        B     // Slot B armed
    }

    // ============================
    // UNITY LIFECYCLE
    // ============================

    private void Awake()
    {
        // Hook UI buttons to their functions
        slotAButton.onClick.AddListener(SelectSlotA);
        slotBButton.onClick.AddListener(SelectSlotB);
        mixColours.onClick.AddListener(MixPaints);
        selectNewColour.onClick.AddListener(SelectMixedColour);
        saveColourButton.onClick.AddListener(SaveActiveColour);
        openMixPanel.onClick.AddListener(OpenMixPanalFunction);
        replaceColourButton.onClick.AddListener(ReplaceSelectedSlot);
        // Panel starts closed
        mixPanelOpen = false;
    }

    // Which slot is currently armed
    public MixSlot activeSlot = MixSlot.None;

    void Start()
    {
        // Listen for colour wheel selections
        if (wheel != null)
            wheel.OnPaintColorSelected += AssignColor;

        // Mixed preview starts white
        if (finalMixedColours != null)
            finalMixedColours.color = Color.white;

        // Slider setup (UI range = 0–100)
        if (valueSlider != null)
        {
            valueSlider.minValue = 0f;
            valueSlider.maxValue = 100f;
            valueSlider.value = mixAmount * 100f;
            valueSlider.onValueChanged.AddListener(OnMixAmountChanged);
        }

        // Reset slot visuals to white
        ResetSlotUI();
    }

    // ============================
    // UPDATE (ONLY DEBUG / PREVIEW)
    // ============================

    void Update()
    {
        // Live preview mixing (optional)
        if (colorA != null && colorB != null)
        {
            mixedColor = PaintMixer.Mix(colorA, colorB, mixAmount);
        }
        /*
        // DEBUG KEYS (optional dev shortcuts)
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SaveSlotAColor();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SaveSlotBColor();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SaveMixColor();
        */
    }

    // ============================
    // SAVING COLOURS
    // ============================

    public void SelectSlotA() { activeSlot = MixSlot.A; }
    public void SelectSlotB() { activeSlot = MixSlot.B; }

    public void SaveSlotAColor()
    {
        if (!mixPanelOpen || colorA == null || palette == null)
            return;

        PaintColour committed = new PaintColour(colorA.value, colorA.name);
        palette.AddColor(committed);
        palette.SetActiveColor(committed);
    }

    public void SaveSlotBColor()
    {
        if (!mixPanelOpen || colorB == null || palette == null)
            return;

        PaintColour committed = new PaintColour(colorB.value, colorB.name);
        palette.AddColor(committed);
        palette.SetActiveColor(committed);
    }

   

    // ============================
    // COLOUR ASSIGNMENT (THE IMPORTANT BIT)
    // ============================

    public void AssignColor(PaintColour color)
    {
        Debug.Log($"AssignColor called | panelOpen={mixPanelOpen} | slot={activeSlot}");

        // Do nothing if panel is closed
        //if (!mixPanelOpen)
        //    return;

        // Do nothing if no slot is armed
        if (activeSlot == MixSlot.None)
        {
            if (palette != null)
                palette.SetActiveColor(color);

            return;
        }

        if (color == null)
            return;

        // Create a copy so we don't mutate shared data
        PaintColour copy = new PaintColour(color.value, color.name);

        if (activeSlot == MixSlot.A)
        {
            colorA = copy;
            slotAFilled = true;
        }
        else if (activeSlot == MixSlot.B)
        {
            colorB = copy;
            slotBFilled = true;
        }

        // Disarm slot after fill
        activeSlot = MixSlot.None;

        ResetMixed();
        UpdateColourUI();
    }

    // ============================
    // UI UPDATES
    // ============================

    void UpdateColourUI()
    {
        if (slotAFilled && colourSelected1 != null)
        {
            Color c = colorA.value;
            c.a = 1f;
            colourSelected1.color = c;
        }

        if (slotBFilled && colourSelected2 != null)
        {
            Color c = colorB.value;
            c.a = 1f;
            colourSelected2.color = c;
        }
    }

    void UpdateMixedUI()
    {
        if (!hasMixed || mixedColor == null || finalMixedColours == null)
            return;

        Color c = mixedColor.value;
        c.a = 1f;
        finalMixedColours.color = c;
    }

    // ============================
    // MIX LOGIC
    // ============================

    public void MixPaints()
    {
        if (colorA == null || colorB == null)
            return;

        mixedColor = PaintMixer.Mix(colorA, colorB, mixAmount);
        hasMixed = true;

        UpdateMixedUI();
    }

    public void ResetMixed()
    {
        hasMixed = false;
        mixedColor = null;

        if (finalMixedColours != null)
            finalMixedColours.color = Color.white;
    }

    // ============================
    // SLIDER
    // ============================

    void OnMixAmountChanged(float value)
    {
        // Convert UI (0–100) to logic (0–1)
        mixAmount = value / 100f;

        mixValueText.text = Mathf.RoundToInt(value).ToString();

        if (hasMixed)
            MixPaints();
    }

    // ============================
    // PANEL OPEN / CLOSE
    // ============================

    public void OpenMixPanalFunction()
    {
        mixPanelOpen = !mixPanelOpen;
        paintMixPanalObject.SetActive(mixPanelOpen);
    }

    // ============================
    // RESET SLOT UI
    // ============================

    void ResetSlotUI()
    {
        if (colourSelected1 != null)
            colourSelected1.color = Color.white;

        if (colourSelected2 != null)
            colourSelected2.color = Color.white;

        slotAFilled = false;
        slotBFilled = false;
    }

    public PaintColour GetCurrentPreviewColour()
    {
        if (mixedColor != null)
            return mixedColor;

        if (colorA != null && !slotBFilled)
            return colorA;

        return null;
    }

    public void ReplaceSelectedSlot()
    {
        if (palette == null)
            return;

        if (palette.selectedReplaceSlot < 0)
            return;

        PaintColour replacement =
            mixedColor != null
                ? mixedColor          //  use mixed preview FIRST
                : palette.activeColor; // fallback only

        if (replacement == null)
            return;

        palette.AddOrReplaceColor(
            replacement,
            palette.selectedReplaceSlot
        );

        paletteUI.Rebuild();
    }

    public void SaveActiveColour()
    {
        if (palette == null || palette.activeColor == null)
            return;

        palette.AddColor(palette.activeColor);
        paletteUI.Rebuild();
    }


    public void SelectMixedColour()
    {
        if (mixedColor == null || palette == null)
            return;

        // Assign to brush ONLY (no saving)
        palette.SetActiveColor(mixedColor);
    }

    public void AssignRawColor(Color c, string name)
    {
        AssignColor(new PaintColour(c, name));
    }
}
