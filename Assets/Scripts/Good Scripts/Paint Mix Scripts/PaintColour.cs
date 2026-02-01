
using UnityEngine;
[System.Serializable]
public class PaintColour
{
    public Color value;
    public string name;

    public PaintColour(Color value, string name = null)
    {
        this.value = value;
        this.name = name;
    }


}
