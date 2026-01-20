using UnityEngine;
using System.IO;
using System.Collections;

public class PaintManagerSaveLoad : MonoBehaviour
{


    [SerializeField] private PaintSurface_Quad canvasSurface;
    [SerializeField] private string paintFileName = "CanvasPainting.png";
    //  [SerializeField] private GameObject canvasObject;





    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            TestRTCopy();

        if (Input.GetKeyDown(KeyCode.F5))
            SavePaint();

        if (Input.GetKeyDown(KeyCode.F9))
            LoadPaint();
    }


    void TestRTCopy()
    {
        RenderTexture paintRT = canvasSurface.GetPaintRT();
        Texture2D testTex = RenderTextureToTexture2D(paintRT);
        Debug.Log($"Copied paint RT: {testTex.width} x {testTex.height}");
    }

    /*
    public void SavePaint()
    {
        if (canvasSurface == null)
        {
            Debug.LogError("No PaintSurface_Quad assigned!");
            return;
        }

        RenderTexture paintRT = canvasSurface.GetPaintRT();
        Texture2D tex = RenderTextureToTexture2D(paintRT);
        byte[] pngData = tex.EncodeToPNG();

        string path = Path.Combine(Application.persistentDataPath, paintFileName);
        File.WriteAllBytes(path, pngData);

        Debug.Log("Paint saved to: " + path);
    }
    */
    Texture2D RenderTextureToTexture2D(RenderTexture rt)
    {
        RenderTexture current = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(
            rt.width,
            rt.height,
            TextureFormat.RGBA32,
            false
        );

        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = current;
        return tex;
    }


    public void SavePaint()
    {
        StartCoroutine(SaveAfterFrame());
    }
    public IEnumerator SaveAfterFrame()
    {
        yield return new WaitForEndOfFrame();

        RenderTexture rt = canvasSurface.GetPaintRT();
        Texture2D tex = RenderTextureToTexture2D(rt);
        byte[] pngData = tex.EncodeToPNG();

        string path = Path.Combine(Application.persistentDataPath, paintFileName);
        File.WriteAllBytes(path, pngData);

        Debug.Log("Paint saved to: " + path);
    }

    public void LoadPaint()
    {
        StartCoroutine(LoadAfterFrame());
    }

    IEnumerator LoadAfterFrame()
    {
        yield return new WaitForEndOfFrame();

        string path = Path.Combine(Application.persistentDataPath, paintFileName);

        if (!File.Exists(path))
        {
            Debug.Log("No saved paint found at: " + path);
            yield break;
        }

        byte[] pngData = File.ReadAllBytes(path);

        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(pngData);

        RenderTexture rt = canvasSurface.GetPaintRT();

        // Copy texture into the paint RT
        Graphics.Blit(tex, rt);

        Debug.Log("Paint loaded from: " + path);
    }

}
