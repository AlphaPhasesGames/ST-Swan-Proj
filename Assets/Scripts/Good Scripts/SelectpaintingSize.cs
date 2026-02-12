using UnityEngine;

public class SelectpaintingSize : MonoBehaviour
{
    public bool inRange;

    [Header("Spawn")]
    public GameObject paintingSize;
    public GameObject altSizePainting;
    public GameObject altSizePainting2;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            inRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            inRange = false;
    }

    private void Update()
    {
        if (!inRange)
            return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SpawnPainting();
        }
    }

    void SpawnPainting()
    {
        paintingSize.SetActive(true);
        altSizePainting.SetActive(false);
        altSizePainting2.SetActive(false);

        //  THIS is the important line
        PaintSurfaceBase paintSurface =
     paintingSize.GetComponentInChildren<PaintSurfaceBase>();

        if (!paintSurface)
        {
            Debug.LogError("Active canvas has no PaintSurfaceBase.");
            return;
        }

        CanvasManager.Instance.SetActiveCanvas(paintSurface);

        Debug.Log("Active canvas set to: " + paintingSize.name);
    }
}

