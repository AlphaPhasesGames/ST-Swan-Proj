using UnityEngine;

public class PaintBlob : MonoBehaviour
{
    PaintCore paintCore;

    Texture2D brushTex;
    Color paintColor;
    float brushSize;
    bool erase;
    bool dilute;

    Rigidbody rb;
    bool hasPainted;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Init(PaintCore core, float worldSize)
    {
        paintCore = core;

        brushTex = core.GetBrushTexture();
        paintColor = core.GetFinalPaintColor();
        brushSize = worldSize;
        erase = core.isErasing;
        dilute = core.brushBehaviour == PaintCore.BrushBehaviour.Dilute;

        hasPainted = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void OnCollisionEnter(Collision col)
    {
        if (hasPainted || !paintCore) return;

        PaintSurfaceBase surface = col.collider.GetComponentInParent<PaintSurfaceBase>();
        if (!surface) return;

        hasPainted = true;

        ContactPoint contact = col.GetContact(0);

        if (col.collider.Raycast(
                new Ray(contact.point + contact.normal * 0.01f, -contact.normal),
                out RaycastHit hit,
                0.05f))
        {
            float size = brushSize * surface.textureSize;
            Debug.Log(hit.textureCoord);
            surface.PaintAtWorld(
                hit,
                brushTex,
                size,
                paintColor,
                erase,
                dilute
            );
        }

        ReturnToPool();
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false);
    }
}
