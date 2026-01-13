using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrappleSystem : MonoBehaviour
{
    private Rigidbody rb;

    [Header("References")]
    public Camera cam;
    public Transform firePoint;
    public LineRenderer line;

    [Header("Grapple")]
    public float maxDistance = 25f;
    public LayerMask grappleMask;

    private bool isGrappling;
    private Vector3 grapplePoint;

    [Header("Pull")]
    public float pullForce = 25f;
    public float maxPullSpeed = 15f;
    public KeyCode pullKey = KeyCode.F;

    private bool isPulling;

    [Header("Swing")]
    [Range(0.95f, 1f)]
    public float ropeDamping = 0.995f;

    private float ropeLength;
    private bool ropeAttached;
    private bool ropeWasSlack;

    public bool IsSwinging => ropeAttached && !isPulling && !ropeWasSlack;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;

        line.positionCount = 2;
        line.enabled = false;
    }

    void Update()
    {
        if (isGrappling && Input.GetKey(pullKey))
            StartPull();
        else
            StopPull();

        if (Input.GetKeyDown(KeyCode.E))
            TryGrapple();

        if (Input.GetKeyUp(KeyCode.E))
            ReleaseGrapple();

        if (isGrappling)
            UpdateLine();
    }

    void FixedUpdate()
    {
        if (isPulling)
            PullPlayer();
        else
            HandleSwing();
    }

    // --------------------------------------------------
    // GRAPPLE
    // --------------------------------------------------

    void TryGrapple()
    {
        if (Physics.Raycast(
            cam.transform.position,
            cam.transform.forward,
            out RaycastHit hit,
            maxDistance,
            grappleMask))
        {
            grapplePoint = hit.point;
            ropeLength = Vector3.Distance(rb.position, grapplePoint);
            ropeAttached = true;
            ropeWasSlack = true;
            isGrappling = true;

            line.enabled = true;
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, grapplePoint);
        }
    }

    void ReleaseGrapple()
    {
        isGrappling = false;
        isPulling = false;
        ropeAttached = false;
        rb.useGravity = true;

        line.enabled = false;
    }

    void UpdateLine()
    {
        line.SetPosition(0, firePoint.position);
        line.SetPosition(1, grapplePoint);
    }

    // --------------------------------------------------
    // PULL MODE (ZIP)
    // --------------------------------------------------

    void StartPull()
    {
        if (isPulling) return;

        isPulling = true;
        rb.useGravity = false;
    }

    void StopPull()
    {
        if (!isPulling) return;

        isPulling = false;
        rb.useGravity = true;
    }

    void PullPlayer()
    {
        Vector3 toPoint = grapplePoint - rb.position;
        float distance = toPoint.magnitude;

        if (distance < 1.2f)
        {
            StopPull();
            return;
        }

        Vector3 dir = toPoint.normalized;

        rb.AddForce(dir * pullForce, ForceMode.Acceleration);

        Vector3 forwardVel = Vector3.Project(rb.linearVelocity, dir);

        if (forwardVel.magnitude < 6f)
            forwardVel = dir * 6f;

        if (forwardVel.magnitude > maxPullSpeed)
            forwardVel = forwardVel.normalized * maxPullSpeed;

        rb.linearVelocity = forwardVel;
    }

    // --------------------------------------------------
    // SWING MODE (PHYSICS)
    // --------------------------------------------------

    void HandleSwing()
    {
        if (!ropeAttached) return;

        Vector3 toPlayer = rb.position - grapplePoint;
        float distance = toPlayer.magnitude;

        // Rope slack  free fall
        if (distance <= ropeLength)
        {
            ropeWasSlack = true;
            return;
        }

        Vector3 dir = toPlayer.normalized;

        // Enforce rope length (constraint)
        rb.position = grapplePoint + dir * ropeLength;

        // Remove ONLY radial velocity (this is the key fix)
        Vector3 velocity = rb.linearVelocity;
        Vector3 radialVelocity = Vector3.Project(velocity, dir);
        rb.linearVelocity = velocity - radialVelocity;

        // Optional: snap impulse when rope first goes taut
        if (ropeWasSlack)
        {
            Vector3 swingDir = Vector3.Cross(Vector3.up, dir);
            rb.linearVelocity += swingDir.normalized * 4.5f;
            ropeWasSlack = false;
        }

        // Gravity does ALL the work � do not project or modify it
    }
}
