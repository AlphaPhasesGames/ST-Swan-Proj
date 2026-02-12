using UnityEngine;
using UnityEngine.UI;
public class ButtonToSpawnPainting : MonoBehaviour
{
    public bool inRange;
    public MouseLook mLook;
    [Header("Spawn")]
    public GameObject framedCanvasPrefabSmall;
    public GameObject framedCanvasPrefabMedium;
    public GameObject framedCanvasPrefablarge;
    public Transform spawnPoint;
    bool panelOpen;
    [Header("Paint Source")]
    private PaintSurfaceBase sourcePainting;
    public GameObject chooseSizePanal;
    public Button smallPaintingButton;
    public Button mediumPaintingButton;
    public Button largePaintingButton;
    public Button closePanalButton;
    private void Awake()
    {
        smallPaintingButton.onClick.AddListener(SpawnPaintingSmall);
        mediumPaintingButton.onClick.AddListener(SpawnPaintingMedium);
        largePaintingButton.onClick.AddListener(SpawnPaintingKLarge);
        closePanalButton.onClick.AddListener(ClosePanal);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            inRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        inRange = false;
        SetChooseSizePanel(false);
    }

    private void Update()
    {
        if (!inRange)
            return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SetChooseSizePanel(!panelOpen);
        }
    }


    void SetChooseSizePanel(bool open)
    {
        panelOpen = open;

        if (chooseSizePanal)
            chooseSizePanal.SetActive(open);

        mLook.enabled = !open; //  THIS IS THE FIX

        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void SpawnPaintingSmall()
    {
        SpawnPainting(framedCanvasPrefabSmall, PaintingSize.Small);
    }

    void SpawnPaintingMedium()
    {
        SpawnPainting(framedCanvasPrefabMedium, PaintingSize.Medium);
    }

    void SpawnPaintingKLarge()
    {
        SpawnPainting(framedCanvasPrefablarge, PaintingSize.Large);
    }

    public void ClosePanal()
    {
        chooseSizePanal.gameObject.SetActive(false);
       
    }

    void SpawnPainting(GameObject prefab, PaintingSize size)
    {
        sourcePainting = CanvasManager.Instance.ActiveCanvas;

        if (!sourcePainting)
        {
            Debug.LogError("No active canvas set in CanvasManager.");
            return;
        }

        // Generate unique paint file name
        string fileName = $"Painting_{System.Guid.NewGuid()}.png";

        // Spawn the frame
        GameObject newFrame =
            Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // Copy paint
        PaintSurfaceBase targetSurface =
            newFrame.GetComponentInChildren<PaintSurfaceBase>();

        if (!targetSurface)
        {
            Debug.LogError("Missing PaintSurfaceBase on framed canvas.");
            return;
        }

        targetSurface.CopyPaintFrom(
            sourcePainting,
            copyTriplanar: true,
            copyUV: true
        );

        //  Tag the painting instance
        PaintingInstance instance =
            newFrame.GetComponent<PaintingInstance>();

        if (!instance)
        {
            Debug.LogError("Painting prefab missing PaintingInstance component.");
            return;
        }

        instance.size = size;
        instance.paintFileName = fileName;

        ClosePanal();

        Debug.Log($"Painting spawned: {size} | {fileName}");
    }

}
