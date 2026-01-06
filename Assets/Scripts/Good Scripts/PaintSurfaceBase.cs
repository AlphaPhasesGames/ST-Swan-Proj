using UnityEngine;

public abstract class PaintSurfaceBase : MonoBehaviour
{
    protected PaintCore paintCore;

    protected virtual void Awake()
    {
        paintCore = GetComponent<PaintCore>();
    }

    public abstract bool TryGetPaintUV(RaycastHit hit, out Vector2 uv);
}