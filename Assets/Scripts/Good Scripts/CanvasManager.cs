using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;
    
    public PaintSurfaceBase ActiveCanvas { get; private set; }



    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetActiveCanvas(PaintSurfaceBase canvas)
    {
        ActiveCanvas = canvas;
    }
}

