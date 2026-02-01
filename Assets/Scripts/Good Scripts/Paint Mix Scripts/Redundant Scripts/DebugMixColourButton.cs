using UnityEngine;
using UnityEngine.UI;

public class DebugMixedColourButton : MonoBehaviour
{
    public PaintMixManager mixer;
    public PaintPalette palette;

    public Button button;
    public Image buttonImage;

    //public Image previewImage;

    PaintColour currentColor;

    void Awake()
    {
        button.onClick.AddListener(ApplyColor);
    }

    void Start()
    {
        mixer.OnMixedColorCreated += SetColor;
    }


    private void Update()
    {
        if (mixer != null && mixer.mixedColor != null)
        {
            Color c = mixer.mixedColor.value;
            c.a = 1f;
            buttonImage.color = c;
        }
    }
    void SetColor(PaintColour color)
    {
        currentColor = color;

        Color c = color.value;
        c.a = 1f;                 // FORCE alpha
        buttonImage.color = c;
    }

    void ApplyColor()
    {
        if (currentColor != null)
        {
            palette.SetActiveColor(currentColor);
        }
        Debug.Log("ButtonPushed");
    }
}
