using UnityEngine;
using System.IO;
using System.Collections;

public class PaintManagerSaveLoad : MonoBehaviour
{


   // [SerializeField] private PaintSurface_Quad canvasSurface;
    [SerializeField] private string paintFileName = "CanvasPainting.png";
    //  [SerializeField] private GameObject canvasObject;

    [Header("Painting Prefabs")]
    public GameObject framedCanvasPrefabSmall;
    public GameObject framedCanvasPrefabMedium;
    public GameObject framedCanvasPrefablarge;



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            TestRTCopy();

        if (Input.GetKeyDown(KeyCode.I))
        {
            SavePaint();       // legacy          // optional legacy
            SaveGallery();              // saves Gallery.json
            SaveAllPaintingTextures();  // saves Painting_*.png
        }


        if (Input.GetKeyDown(KeyCode.O))
            LoadPaint();
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadGallery();
        }
    }


    void TestRTCopy()
    {
        //RenderTexture paintRT = canvasSurface.GetPaintRT();
        //Texture2D testTex = RenderTextureToTexture2D(paintRT);
       // Debug.Log($"Copied paint RT: {testTex.width} x {testTex.height}");
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

        PaintSurfaceBase activeCanvas = CanvasManager.Instance.ActiveCanvas;

        if (!activeCanvas)
        {
            Debug.LogError("No active canvas set. Cannot save paint.");
            yield break;
        }

        RenderTexture rt = activeCanvas.GetPaintRT();
        Texture2D tex = RenderTextureToTexture2D(rt);
        byte[] pngData = tex.EncodeToPNG();

        string path = Path.Combine(Application.persistentDataPath, paintFileName);
        File.WriteAllBytes(path, pngData);

        Debug.Log("Paint saved from active canvas: " + activeCanvas.name);
    }

    public void LoadPaint()
    {
        StartCoroutine(LoadAfterFrame());
    }

            IEnumerator LoadAfterFrame()
        {
            yield return new WaitForEndOfFrame();

            PaintSurfaceBase activeCanvas = CanvasManager.Instance.ActiveCanvas;

            if (!activeCanvas)
            {
                Debug.LogError("No active canvas set. Cannot load paint.");
                yield break;
            }

                string path = Path.Combine(Application.persistentDataPath, paintFileName);

            if (!File.Exists(path))
            {
                Debug.Log("No saved paint found at: " + path);
                yield break;
            }

            byte[] pngData = File.ReadAllBytes(path);

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(pngData);

            RenderTexture rt = activeCanvas.GetPaintRT();
            Graphics.Blit(tex, rt);

            Debug.Log("Paint loaded into active canvas: " + activeCanvas.name);
        }

    public void SaveGallery()
    {
        PaintingInstance[] instances =
            FindObjectsOfType<PaintingInstance>();

        PaintingSaveWrapper wrapper = new PaintingSaveWrapper();

        foreach (var instance in instances)
        {
            wrapper.paintings.Add(new SavedPaintingData
            {
                size = instance.size,
                position = instance.transform.position,
                rotation = instance.transform.rotation,
                paintFileName = instance.paintFileName
            });
        }

        string json = JsonUtility.ToJson(wrapper, true);
        string path = Path.Combine(Application.persistentDataPath, "Gallery.json");

        File.WriteAllText(path, json);

        Debug.Log($"Gallery saved ({wrapper.paintings.Count} paintings)");
    }

    public void SaveAllPaintingTextures()
    {
        PaintingInstance[] instances =
            FindObjectsOfType<PaintingInstance>();

        foreach (var instance in instances)
        {
            PaintSurfaceBase surface =
                instance.GetComponentInChildren<PaintSurfaceBase>();

            if (!surface)
                continue;

            RenderTexture rt = surface.GetPaintRT();
            Texture2D tex = RenderTextureToTexture2D(rt);
            byte[] pngData = tex.EncodeToPNG();

            string path = Path.Combine(
                Application.persistentDataPath,
                instance.paintFileName
            );

            File.WriteAllBytes(path, pngData);
        }

        Debug.Log($"Saved {instances.Length} painting textures.");
    }

    void ClearExistingPaintings()
    {
        PaintingInstance[] instances =
            FindObjectsOfType<PaintingInstance>();

        foreach (var instance in instances)
        {
            Destroy(instance.gameObject);
        }
    }

    public void LoadGallery()
    {
        string path = Path.Combine(Application.persistentDataPath, "Gallery.json");

        if (!File.Exists(path))
        {
            Debug.LogError("No Gallery.json found.");
            return;
        }

        ClearExistingPaintings();

        string json = File.ReadAllText(path);
        PaintingSaveWrapper wrapper =
            JsonUtility.FromJson<PaintingSaveWrapper>(json);

        if (wrapper == null || wrapper.paintings == null)
        {
            Debug.LogError("Gallery.json is empty or invalid.");
            return;
        }

        foreach (var data in wrapper.paintings)
        {
            LoadSinglePainting(data);
        }

        Debug.Log($"Gallery loaded ({wrapper.paintings.Count} paintings)");
    }

    GameObject GetPrefabForSize(PaintingSize size)
    {
        switch (size)
        {
            case PaintingSize.Small:
                return framedCanvasPrefabSmall;

            case PaintingSize.Medium:
                return framedCanvasPrefabMedium;

            case PaintingSize.Large:
                return framedCanvasPrefablarge;

            default:
                Debug.LogError("Unknown painting size: " + size);
                return null;
        }
    }

    void LoadSinglePainting(SavedPaintingData data)
    {
        GameObject prefab = GetPrefabForSize(data.size);
        if (!prefab)
            return;

        GameObject painting =
            Instantiate(prefab, data.position, data.rotation);

        // Restore PaintingInstance data
        PaintingInstance instance =
            painting.GetComponent<PaintingInstance>();

        if (!instance)
        {
            Debug.LogError("Loaded painting missing PaintingInstance.");
            return;
        }

        instance.size = data.size;
        instance.paintFileName = data.paintFileName;

        // Load paint texture
        PaintSurfaceBase surface =
            painting.GetComponentInChildren<PaintSurfaceBase>();

        if (!surface)
        {
            Debug.LogError("Loaded painting missing PaintSurfaceBase.");
            return;
        }

        string path = Path.Combine(
            Application.persistentDataPath,
            data.paintFileName
        );

        if (!File.Exists(path))
        {
            Debug.LogWarning("Paint file missing: " + data.paintFileName);
            return;
        }

        byte[] pngData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(pngData);

        Graphics.Blit(tex, surface.GetPaintRT());
    }
}
