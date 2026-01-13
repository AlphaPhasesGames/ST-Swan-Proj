using UnityEngine;

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        line.positionCount = 2;
        line.enabled = false;
    }

    void Update()
    {
        if (isGrappling && Input.GetKey(pullKey))
        {
            StartPull();
        }
        else
        {
            StopPull();
        }

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
    }

    void TryGrapple()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, maxDistance, grappleMask))
        {
            grapplePoint = hit.point;
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
        rb.useGravity = true;

        line.enabled = false;
    }

    void UpdateLine()
    {
        line.SetPosition(0, firePoint.position);
        line.SetPosition(1, grapplePoint);
    }

    void PullPlayer()
    {
        Vector3 toPoint = grapplePoint - transform.position;
        float distance = toPoint.magnitude;
        /*
        if (distance < 1.2f)
        {
            rb.velocity = Vector3.zero;
            ReleaseGrapple();
            return;
        }
            */
        if (distance < 1.2f)
        {
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            return;
        }
    
        Vector3 dir = toPoint.normalized;

        // Pull forward
        rb.AddForce(dir * pullForce, ForceMode.Acceleration);

        // Remove sideways (tangential) velocity
        Vector3 vel = rb.velocity;
        Vector3 forwardVel = Vector3.Project(vel, dir);

        // Ensure minimum pull speed
        if (forwardVel.magnitude < 6f)
        {
            forwardVel = dir * 6f;
        }

        // Clamp speed
        if (forwardVel.magnitude > maxPullSpeed)
        {
            forwardVel = forwardVel.normalized * maxPullSpeed;
        }

        rb.velocity = forwardVel;
    }



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

}
