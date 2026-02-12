using UnityEngine;

public class BrushPaintVisual : MonoBehaviour
{
    [Header("References")]
    public PaintPalette palette;
    public Renderer paintBlobRenderer;

    [Header("Shader Property")]
    [SerializeField] private string colorProperty = "_BaseColor";

    MaterialPropertyBlock block;

    void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        if (palette != null)
            palette.OnActiveColorChanged += UpdatePaintColor;
    }

    void OnDisable()
    {
        if (palette != null)
            palette.OnActiveColorChanged -= UpdatePaintColor;
    }

    void Start()
    {
        // Apply current colour immediately (important on scene load)
        if (palette != null && palette.activeColor != null)
            UpdatePaintColor(palette.activeColor.value);
    }

    void UpdatePaintColor(Color c)
    {
        c.a = 1f;

        paintBlobRenderer.GetPropertyBlock(block);
        block.SetColor(colorProperty, c);
        paintBlobRenderer.SetPropertyBlock(block);
    }
}
