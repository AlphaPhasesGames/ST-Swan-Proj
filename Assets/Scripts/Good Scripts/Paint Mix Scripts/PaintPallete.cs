using System.Collections.Generic;
using UnityEngine;

public class PaintPalette : MonoBehaviour
{
    public int maxSlots = 6;
    public List<PaintColour> colors = new();

    public PaintColour activeColor;
    public System.Action<Color> OnActiveColorChanged;

    [Header("References")]
    public PaintMixManager mixer;
    public ColorWheelSelectorOuter wheel;

    // Which slot is marked for replacement (right-click)
    public int selectedReplaceSlot = -1;

    void Start()
    {
        if (wheel != null)
            wheel.OnPaintColorSelected += SetActiveColor;
    }

    public void SetActiveColor(PaintColour color)
    {
        if (color == null) return;

        activeColor = new PaintColour(color.value, color.name);
        OnActiveColorChanged?.Invoke(activeColor.value);
    }

    public void SelectSlotForReplacement(int slotIndex)
    {
        selectedReplaceSlot = slotIndex;
    }

    public void AddOrReplaceColor(PaintColour color, int slotIndex = -1)
    {
        if (color == null) return;

        if (slotIndex >= 0 && slotIndex < maxSlots)
        {
            if (slotIndex < colors.Count)
                colors[slotIndex] = new PaintColour(color.value, color.name);
            else
                colors.Add(new PaintColour(color.value, color.name));

            return;
        }

        if (colors.Count < maxSlots)
            colors.Add(new PaintColour(color.value, color.name));
    }

    // Called by Replace button
    public void ReplaceSlotImmediate(int slotIndex)
    {
        if (mixer == null) return;

        PaintColour replacement = mixer.GetCurrentPreviewColour();
        if (replacement == null) return;

        AddOrReplaceColor(replacement, slotIndex);
    }

    // Legacy / external use
    public void AddColor(PaintColour color)
    {
        if (color == null) return;
        colors.Add(new PaintColour(color.value, color.name));
    }
}
