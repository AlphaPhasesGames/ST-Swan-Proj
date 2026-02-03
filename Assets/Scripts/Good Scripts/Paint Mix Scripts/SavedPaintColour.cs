using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SavedPaintColor
{
    public float r, g, b;
    public string name;




    public SavedPaintColor(PaintColour color)
    {
        r = color.value.r;
        g = color.value.g;
        b = color.value.b;
        name = color.name;
    }

    public PaintColour ToPaintColor()
    {
        return new PaintColour(new Color(r, g, b), name);
    }
}




