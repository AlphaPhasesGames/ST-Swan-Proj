using UnityEngine;
using TMPro;
public class SpraySettingsArcade : MonoBehaviour
{
    public PaintCoreOldSystem paintCore;

    public TextMeshProUGUI hardSoftSettings;
    public TextMeshProUGUI stampSpraySettings;
    public TextMeshProUGUI legacySize;
    public TextMeshProUGUI spraySize;

    void Start()
    {
        // paintCore.OnPaintModeChanged += HandlePaintMode;
        //paintCore.OnFireModeChanged += HandleFireMode;
        //paintCore.OnBrushSizeChanged += HandleBrushSize;

        // Initial sync
        //HandlePaintMode(paintCore.paintMode);
        // HandleFireMode(paintCore.fireMode);
        HandleBrushSize(paintCore.GetBrushSizePixels());
    }

    void Update()
    {
        //var surface = paintCore.GetSurfaceUnderCrosshairPublic();
       // legacySize.text = surface ? surface.legacyBrushSize.ToString("0") : "-";
    }

    void HandlePaintMode(PaintCore.PaintMode mode)
    {
        stampSpraySettings.text =
            mode == PaintCore.PaintMode.Precision ? "Rifle" : "Shotgun";
    }

    void HandleFireMode(PaintCore.FireMode mode)
    {
        hardSoftSettings.text =
            mode == PaintCore.FireMode.Once ? "Single Shot" : "Automatic";
    }

    void HandleBrushSize(float size)
    {
        spraySize.text = size.ToString("0.00");
    }
}

