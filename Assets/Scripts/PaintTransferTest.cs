using UnityEngine;

public class PaintTransferTest : MonoBehaviour
{
    public PaintSurfaceBase sourceBig;
    public PaintSurfaceBase targetSmall;

    [ContextMenu("Transfer Paint Big -> Small")]
    public void Transfer()
    {
        if (!sourceBig || !targetSmall)
        {
            Debug.LogError("Assign sourceBig and targetSmall.");
            return;
        }

        // If your portable canvas only needs UV, you can copy only UV:
        // targetSmall.CopyPaintFrom(sourceBig, copyTriplanar:false, copyUV:true);

        // Or copy everything:
        targetSmall.CopyPaintFrom(sourceBig, copyTriplanar: true, copyUV: true);

        Debug.Log("Transfer done.");
    }
}
