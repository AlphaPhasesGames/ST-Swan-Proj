using UnityEngine;

public class PaintBallObjectArcade : MonoBehaviour
{
    public Texture2D brush;
    public float size = 64f;
    public Color paintColor = Color.black;

    // NEW: paint directly from a raycast hit
    public void PaintFromHit(RaycastHit hit)
    {
        var surface =
            hit.collider.GetComponent<PaintSurfaceBase>()
            ?? hit.collider.GetComponentInParent<PaintSurfaceBase>();

        if (!surface)
            return;

        surface.PaintAtWorld(
            hit,
            brush ? brush : Texture2D.whiteTexture,
            size,
            paintColor
        );
    }
}
