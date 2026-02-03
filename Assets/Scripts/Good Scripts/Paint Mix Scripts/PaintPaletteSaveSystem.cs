using UnityEngine;
using System.IO;

public class PaintPaletteSaveSystem : MonoBehaviour
{
    public PaintPalette palette;
    public PaintPaletteSavedColoursUI paletteUI;

    string SavePath => Path.Combine(Application.persistentDataPath, "palette.json");

    void Start()
    {
        Load();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            Save();

        if (Input.GetKeyDown(KeyCode.Y))
            Load();
    }

    public void Save()
    {
        PaintPaletteSaveData data = new();

        foreach (var c in palette.colors)
            data.colors.Add(new SavedPaintColor(c));

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
            return;

        var json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<PaintPaletteSaveData>(json);

        if (data == null || data.colors == null)
            return;

        palette.colors.Clear();

        foreach (var saved in data.colors)
            palette.AddColor(saved.ToPaintColor());

        paletteUI.Rebuild();
    }

    public void ResetPalette()
    {
        // 1. Clear runtime palette
        palette.colors.Clear();

        // 2. Save empty palette to disk
        PaintPaletteSaveData data = new();
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));

        // 3. Rebuild UI
        paletteUI.Rebuild();
    }

}
