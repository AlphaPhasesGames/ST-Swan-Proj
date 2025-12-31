using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PaintRTBinder : MonoBehaviour
{
    public int textureSize = 512;

    RenderTexture paintRT;
    Material mat;

    void Awake()
    {
        // Create RT
        paintRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        paintRT.wrapMode = TextureWrapMode.Clamp;
        paintRT.filterMode = FilterMode.Bilinear;
        paintRT.Create();

        // Clear RT
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = paintRT;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = active;

        // Bind to LIVE material instance
        mat = GetComponent<Renderer>().material;
        mat.SetTexture("_PaintMask", paintRT);

        Debug.Log("PaintRT bound at runtime: " + paintRT);
    }

    public RenderTexture GetRT()
    {
        return paintRT;
    }
}
