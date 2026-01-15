using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    private RigidbodyConstraints defaultConstraints;
    public bool IsSwinging => ropeAttached && !isPulling && !ropeWasSlack;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        defaultConstraints = rb.constraints;
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
            ropeLength = Mathf.Max(
    Vector3.Distance(rb.position, grapplePoint),
    0.5f
);
            ropeAttached = true;
            ropeWasSlack = true;
            isGrappling = true;

            line.enabled = true;
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, grapplePoint);

            GetLineLength();
        }
    }

    void ReleaseGrapple()
    {
        isGrappling = false;
        isPulling = false;
        ropeAttached = false;
        ropeWasSlack = false;

        rb.useGravity = true;
        line.enabled = false;

        // IMPORTANT: delay constraint restore
        StartCoroutine(RestoreConstraintsNextPhysics());
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

        // Keep upright: no head-over-heels, but yaw is allowed
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        Vector3 toPlayer = rb.position - grapplePoint;
        float distance = toPlayer.magnitude;

        // Rope slack = free fall
        if (distance <= ropeLength)
        {
            ropeWasSlack = true;
            return;
        }

        Vector3 dir = toPlayer.normalized;

        // Enforce rope length
        rb.position = grapplePoint + dir * ropeLength;

        // Remove ONLY radial velocity (your good fix)
        Vector3 vel = rb.linearVelocity;
        Vector3 radialVel = Vector3.Project(vel, dir);
        rb.linearVelocity = vel - radialVel;

        // --- NEW: keep swing in camera-aligned vertical plane ---
        Vector3 planeNormal = cam.transform.right;

        // project position onto plane through grapplePoint
        Vector3 p = rb.position;
        float off = Vector3.Dot(p - grapplePoint, planeNormal);
        rb.position = p - planeNormal * off;

        // re-enforce rope length after plane projection (keeps it tight)
        Vector3 newDir = (rb.position - grapplePoint).normalized;
        rb.position = grapplePoint + newDir * ropeLength;

        // remove out-of-plane velocity
        rb.linearVelocity -= Vector3.Project(rb.linearVelocity, planeNormal);

        // Optional: snap impulse when rope first goes taut
        if (ropeWasSlack)
        {
            Vector3 swingDir = Vector3.Cross(Vector3.up, newDir);
            rb.linearVelocity += swingDir.normalized * 4.5f;
            ropeWasSlack = false;
        }
    }

    public IEnumerator RestoreConstraintsNextPhysics()
    {
        yield return new WaitForFixedUpdate();

        // Gentle carry-through boost (optional but delicious)
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            rb.linearVelocity += rb.linearVelocity.normalized * 1.2f;
        }

        rb.constraints = defaultConstraints;
    }

    float GetLineLength()
    {
        float length = 0f;

        for (int i = 0; i < line.positionCount - 1; i++)
        {
            length += Vector3.Distance(
                line.GetPosition(i),
                line.GetPosition(i + 1)
            );
        }

        return length;
    }

    /*
    void OnGUI()
    {
        if (line == null || line.positionCount < 2) return;

        float length = Vector3.Distance(
            line.GetPosition(0),
            line.GetPosition(1)
        );

        GUI.Label(
            new Rect(20, 20, 1500, 1000),
            $"Rope Length: {length:F2}"
        );
    }
    */
    void OnGUI()
    {
        if (!ropeAttached) return;

        float physicsDist = Vector3.Distance(rb.position, grapplePoint);

        GUI.Label(
            new Rect(20, 20, 1500, 1000),
            $"ropeLength (stored): {ropeLength:F3}\n" +
            $"physics distance: {physicsDist:F3}\n" +
            $"delta: {(physicsDist - ropeLength):F4}"
        );
    }

}
