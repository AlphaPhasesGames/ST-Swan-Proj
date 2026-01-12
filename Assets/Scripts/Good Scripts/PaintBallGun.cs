using UnityEngine;

public class PaintballGun : MonoBehaviour
{
    public GameObject paintballPrefab;
    public Transform firePoint;
    public float fireForce = 20f;

    [Header("Paint Source")]
    public PaintCore paintCore; // assign in Inspector

    void Update()
    {
        if(paintCore.fireMode == PaintCore.FireMode.Once)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Fire();
            }
        }

        if (paintCore.fireMode == PaintCore.FireMode.Hold)
        {
            if (Input.GetMouseButton(0))
            {
                Fire();
            }
        }

    }

    void Fire()
    {
        GameObject ball = Instantiate(
            paintballPrefab,
            firePoint.position,
            firePoint.rotation
        );
       
        // ---- MOVE IT ----
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.linearVelocity = firePoint.forward * fireForce;

        // ---- OPTION A: INJECT BRUSH DATA ----
        PaintBallObject pbo = ball.GetComponent<PaintBallObject>();
        if (pbo != null && paintCore != null)
        {
            pbo.brushTex = paintCore.GetBrushTexture();   // we'll add this
            pbo.worldBrushSize = paintCore.brushWorldSize;
            pbo.textureSize = paintCore.textureSize;
        }
        Debug.Log(paintCore.GetBrushTexture());
    }
}
