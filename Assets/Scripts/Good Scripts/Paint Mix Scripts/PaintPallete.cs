using System.Collections.Generic;
using UnityEngine;

public class PaintPalette : MonoBehaviour
{
    public List<PaintColour> colors = new List<PaintColour>();
    public PaintColour activeColor;

    public System.Action<Color> OnActiveColorChanged;

    [Header("References")]
    public PaintMixManager mixer;
    public ColorWheelSelectorOuter wheel;

    void Start()
    {
        if (wheel != null)
        {
            wheel.OnPaintColorSelected += SetActiveColor;
        }
    }

    public void SetActiveColor(PaintColour color)
    {
        activeColor = new PaintColour(color.value, color.name);

        //  Notify listeners
        OnActiveColorChanged?.Invoke(activeColor.value);
    }

    public void AddColor(PaintColour color)
    {
        colors.Add(new PaintColour(color.value, color.name));
    }
}
