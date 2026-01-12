using UnityEngine;

public class ColorWheelToPaintCore : MonoBehaviour
{
    public ColorWheelSelectorOuter wheel;
    public PaintCore paintCore;

    void Awake()
    {
        wheel.OnColourSelected += HandleColour;
    }

    void OnDestroy()
    {
        wheel.OnColourSelected -= HandleColour;
    }

    void HandleColour(Color c)
    {
        paintCore.SetPaintColor(c);
    }
}
