using UnityEngine;
using System.IO;

public class PaintingBin : MonoBehaviour
{
    [Header("Visuals")]
    public Renderer binRenderer;
    public Color normalColor = Color.gray;
    public Color activeColor = Color.red;

    PaintingInstance currentPainting;

    void Start()
    {
        SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        PaintingInstance painting =
            other.GetComponentInParent<PaintingInstance>();

        if (!painting)
            return;

        currentPainting = painting;
        SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        PaintingInstance painting =
            other.GetComponentInParent<PaintingInstance>();

        if (painting != currentPainting)
            return;

        currentPainting = null;
        SetActive(false);
    }

    void SetActive(bool active)
    {
        if (binRenderer)
            binRenderer.material.color =
                active ? activeColor : normalColor;
    }

    void Update()
    {
        if (!currentPainting)
            return;

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            DeleteCurrentPainting();
        }
    }

    void DeleteCurrentPainting()
    {
        if (!currentPainting)
            return;

        // Remove PNG from disk
        string path = Path.Combine(
            Application.persistentDataPath,
            currentPainting.paintFileName
        );

        if (File.Exists(path))
            File.Delete(path);

        // Safety: clear active canvas if this was selected
        PaintSurfaceBase surface =
            currentPainting.GetComponentInChildren<PaintSurfaceBase>();

        if (CanvasManager.Instance.ActiveCanvas == surface)
        {
            CanvasManager.Instance.SetActiveCanvas(null);
        }

        // Destroy painting object
        Destroy(currentPainting.gameObject);

        currentPainting = null;
        SetActive(false);
        FindObjectOfType<PaintManagerSaveLoad>()?.SaveGallery();
        Debug.Log("Painting deleted and removed from save pool.");
    }

}
