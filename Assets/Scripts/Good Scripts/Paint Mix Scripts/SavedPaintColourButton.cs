using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SavedPaintColourButton : MonoBehaviour, IPointerClickHandler
{
    public Image swatchImage;

    PaintColour color;
    PaintPalette palette;
    int slotIndex;

    public void Init(PaintColour color, PaintPalette palette, int slotIndex)
    {
        this.color = color;
        this.palette = palette;
        this.slotIndex = slotIndex;

        swatchImage.color = color != null
            ? new Color(color.value.r, color.value.g, color.value.b, 1f)
            : Color.clear;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // LEFT CLICK  select only
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (color != null)
            {
                palette.SetActiveColor(color);
            }
        }

        // RIGHT CLICK  IMMEDIATE REPLACE (NO SAVE BUTTON)
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            palette.SelectSlotForReplacement(slotIndex);
        }
    }
}
