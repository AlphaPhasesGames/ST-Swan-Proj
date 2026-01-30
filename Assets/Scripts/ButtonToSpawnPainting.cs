using UnityEngine;

public class ButtonToSpawnPainting : MonoBehaviour
{
    public bool inRange;

    [Header("Spawn")]
    public GameObject framedCanvasPrefab;
    public Transform spawnPoint;

    [Header("Paint Source")]
    public PaintSurfaceBase sourceBig;

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
        // 1. Spawn the framed canvas prefab
        GameObject newFrame =
            Instantiate(framedCanvasPrefab, spawnPoint.position, spawnPoint.rotation);

        // 2. Find the paint surface on the canvas child
        PaintSurfaceBase targetSmall =
            newFrame.GetComponentInChildren<PaintSurfaceBase>();

        if (!sourceBig || !targetSmall)
        {
            Debug.LogError("Missing PaintSurfaceBase reference.");
            return;
        }

        // 3. Transfer the paint
        targetSmall.CopyPaintFrom(
            sourceBig,
            copyTriplanar: true,
            copyUV: true
        );

        Debug.Log("Painting spawned and transferred.");
    }
}
