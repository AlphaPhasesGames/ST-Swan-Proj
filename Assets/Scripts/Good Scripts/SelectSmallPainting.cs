using UnityEngine;

public class SelectSmallPainting : MonoBehaviour
{
    public bool inRange;

    [Header("Spawn")]
    public GameObject paintingSizeSmall;
    public GameObject altSizePaintingMedium;
    public GameObject altSizePaintingLarge;

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
        paintingSizeSmall.gameObject.SetActive(true);
        altSizePaintingMedium.gameObject.SetActive(false);
        altSizePaintingLarge.gameObject.SetActive(false);
    }
}
