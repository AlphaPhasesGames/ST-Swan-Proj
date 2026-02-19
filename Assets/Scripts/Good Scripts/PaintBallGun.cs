using UnityEngine;

public class PaintballGun : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject paintballPrefab;
    //public PaintBallObject pBallObject;
    public Transform firePoint;
    public float fireForce = 20f;

    [Header("VFX")]
    public ParticleSystem sprayParticles;
    public Animator brushAnimator;
    public Animator fineBrushAnimator;
    [Header("Paint Source")]
    public PaintCore paintCore;

    [Header("Tool Models")]
    public GameObject brush;
    public GameObject sprayCan;
    public GameObject paintBallGun;
    public string fire = "Fire 1";
    // ---------------- VISUAL STATE ----------------
    bool IsPaintballGunActive =>
    IsSprayMode &&
    currentSprayVisual == SprayVisual.PaintballGun;
    public enum SprayVisual
    {
        SprayCan,
        PaintballGun
    }

    [SerializeField]
    private SprayVisual currentSprayVisual = SprayVisual.SprayCan;

    bool IsSprayMode =>
        paintCore != null &&
        paintCore.paintMode == PaintCore.PaintMode.Spray;

    // ---------------- UNITY ----------------

    void Start()
    {
        if (sprayParticles)
            sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (paintCore)
        {
            paintCore.OnPaintModeChanged += HandlePaintModeChanged;
            HandlePaintModeChanged(paintCore.paintMode);
        }
    }

    void Update()
    {
        // Brush animation (non-spray)
        HandleBrushInput();

        // Nothing else runs unless we're in spray mode
        if (!IsSprayMode) return;

        // ---------------- SPRAY (both visuals)
        if (paintCore.fireMode == PaintCore.FireMode.Hold)
        {
            if (Input.GetButtonDown(fire));
            StartSpray();

            if (Input.GetButtonUp(fire))
                StopSpray();
        }
        else
        {
            if (Input.GetButtonDown(fire))
                StartSpray();
        }

        // ---------------- PROJECTILES (paintball gun ONLY)
        if (!IsPaintballGunActive) return;

        if (paintCore.fireMode == PaintCore.FireMode.Hold)
        {
            if (Input.GetButtonDown(fire))
                Fire();
        }
        else
        {
            if (Input.GetButtonDown(fire))
                FireSingleShot();
        }
    }


    // ---------------- SPRAY FLOW ----------------

    void StartSpray()
    {
        if (sprayParticles)
        {
            UpdateSprayColour();
            sprayParticles.Play();
        }
    }

    void StopSpray()
    {
        if (sprayParticles)
            sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void FireSingleShot()
    {
        if (sprayParticles)
        {
            UpdateSprayColour();
            sprayParticles.Play();
            sprayParticles.Stop();
        }

        Fire();
    }

    // ---------------- FIRING ----------------

    void Fire()
    {
        GameObject ball = Instantiate(
            paintballPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb)
            rb.linearVelocity = firePoint.forward * fireForce;

        PaintBallObject pbo = ball.GetComponent<PaintBallObject>();
        if (pbo && paintCore)
        {
            pbo.brushTex = paintCore.GetBrushTexture();
            pbo.worldBrushSize = paintCore.brushWorldSize;
            pbo.textureSize = paintCore.textureSize;

            //  THIS IS THE IMPORTANT LINE
            pbo.maxDistance = paintCore.GetPaintballDistance();
        }
    }


    // ---------------- VISUAL CONTROL ----------------

    void HandlePaintModeChanged(PaintCore.PaintMode mode)
    {
        if (mode != PaintCore.PaintMode.Spray)
        {
            // Brush / non-spray mode
            if (sprayParticles)
                sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            if (brushAnimator)
            {
                brushAnimator.ResetTrigger("paint");
                brushAnimator.SetTrigger("stopPaint");
            }

            if (fineBrushAnimator)
            {
                fineBrushAnimator.ResetTrigger("paintFine");
                fineBrushAnimator.SetTrigger("fineReset");
            }

            //brush.SetActive(true);
            sprayCan.SetActive(false);
            paintBallGun.SetActive(false);
            return;
        }

        // Spray mode
        brush.SetActive(false);

        sprayCan.SetActive(currentSprayVisual == SprayVisual.SprayCan);
        paintBallGun.SetActive(currentSprayVisual == SprayVisual.PaintballGun);
    }

    public void SetSprayVisual(SprayVisual visual)
    {
        currentSprayVisual = visual;

        if (IsSprayMode)
            HandlePaintModeChanged(PaintCore.PaintMode.Spray);
    }

    void UpdateSprayColour()
    {
        if (!sprayParticles || paintCore == null) return;

        var main = sprayParticles.main;
        Color c = paintCore.CurrentPaintColor;
        main.startColor = new Color(c.r, c.g, c.b, 0.6f);
    }

    void OnDestroy()
    {
        if (paintCore)
            paintCore.OnPaintModeChanged -= HandlePaintModeChanged;
    }

    void HandleBrushInput()
    {
        if (IsSprayMode) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (brushAnimator)
            {
                brushAnimator.ResetTrigger("stopPaint");
                brushAnimator.SetTrigger("paint");
            }

            if (fineBrushAnimator)
            {
                fineBrushAnimator.ResetTrigger("fineReset");
                fineBrushAnimator.SetTrigger("paintFine");
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (brushAnimator)
            {
                brushAnimator.ResetTrigger("paint");
                brushAnimator.SetTrigger("stopPaint");
            }

            if (fineBrushAnimator)
            {
                fineBrushAnimator.ResetTrigger("paintFine");
                fineBrushAnimator.SetTrigger("fineReset");
            }
        }
    }


}
