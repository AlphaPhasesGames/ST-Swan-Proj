using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SpraySettingsUI : MonoBehaviour
{

    public TextMeshProUGUI hardSoftSettings;
    public TextMeshProUGUI stampSpraySettings;

    public PaintCore paintCore;

    public TextMeshProUGUI legacySize;
    public TextMeshProUGUI spraySize;

    public TextMeshProUGUI gunType; // Shotgun / Rifle

    private void Update()
    {
        var surface = paintCore.GetSurfaceUnderCrosshairPublic();

        if (surface != null)
            legacySize.text = surface.legacyBrushSize.ToString("0");
        else
            legacySize.text = "-";

        spraySize.text = paintCore.brushWorldSize.ToString("0.00");

        stampSpraySettings.text =
        paintCore.paintSystem == PaintCore.PaintSystem.LegacyStamp
        ? "Rifle"
        : "Shotgun";

        hardSoftSettings.text = paintCore.fireMode == PaintCore.FireMode.Once
        ? "Single Shot"
        : "Automatic";

       // gunType.text = paintCore.spraySizeMode == PaintCore.SpraySizeMode.Constant
        //? "Rifle"
       // : "Shotgun";
    }


    public void SetPaintToHard()
    {
        if (paintCore.paintSystem == PaintCore.PaintSystem.LegacyStamp)
        {

            hardSoftSettings.text = ("Hard paint");
        }

        if (paintCore.paintSystem == PaintCore.PaintSystem.SprayCone)
        {

            hardSoftSettings.text = ("Soft Paint");
        }

    }

    public void SetPaintToSpray()
    {

        if (paintCore.fireMode == PaintCore.FireMode.Once)
        {

            stampSpraySettings.text = ("Stamp");
        }

        if (paintCore.fireMode == PaintCore.FireMode.Spray)
        {

            stampSpraySettings.text = ("Spray");
        }

    }



}


