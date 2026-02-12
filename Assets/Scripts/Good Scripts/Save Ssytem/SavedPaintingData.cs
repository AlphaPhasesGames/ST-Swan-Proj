using UnityEngine;

[System.Serializable]
public class SavedPaintingData
{
    public PaintingSize size;
    public Vector3 position;
    public Quaternion rotation;
    public string paintFileName;
}