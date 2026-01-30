using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FPSPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed; //= 6f;
    public float jumpForce; //= 5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Jetpack")]
    public bool playerHasJetPack = true;
    public float jetpackForce = 15f;
    public float maxJetpackSpeed = 8f;
    public float normalGravity = -9.8f;
    public float jetpackGravity = -2f;

    [Header("Air Control")]
    public float airControlForce = 25f;
    public float airMaxHorizontalSpeed = 4f;
    public float airDamping = 0.98f;

    [Header("Jetpack Ramping")]
    public float jetpackRampUp = 12f;
    public float jetpackRampDown = 18f;

    [Header("Input")]
    public string horizontalAxis = "Horizontal2";
    public string verticalAxis = "Vertical2";
    public string jumpButton = "Jump";
    public string jetpackButton = "Jetpack";

    private float currentJetpackForce = 0f;

    private Rigidbody rb;
    private float xInput;
    private float zInput;
    private bool isGrounded;

    public GrappleSystem grapple;

    public Transform paintingHoldPoint;
    HoldablePainting heldPainting;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Physics.gravity = Vector3.up * normalGravity;
    }

    void Update()
    {
        xInput = Input.GetAxisRaw(horizontalAxis);
        zInput = Input.GetAxisRaw(verticalAxis);

        if (Input.GetButtonDown(jumpButton) && isGrounded)
        {
            Jump();
        }

        HandleJetpack();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldPainting == null)
            {
                TryPickUp();
            }
            else
            {
                heldPainting.Drop();
                heldPainting = null;
            }
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        Move();
    }

    void Move()
    {
        if (grapple && grapple.IsSwinging) return;

        Vector3 moveDir = transform.right * xInput + transform.forward * zInput;

        if (isGrounded)
        {
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Vector3 targetVel = moveDir.normalized * moveSpeed;

                rb.linearVelocity = new Vector3(
                    targetVel.x,
                    rb.linearVelocity.y,
                    targetVel.z
                );
            }
            else
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x * 0.75f,
                    rb.linearVelocity.y,
                    rb.linearVelocity.z * 0.75f
                );
            }
        }
        else
        {
            if (moveDir.sqrMagnitude > 0.01f)
            {
                rb.AddForce(moveDir.normalized * airControlForce, ForceMode.Acceleration);
            }

            Vector3 horizVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (horizVel.magnitude > airMaxHorizontalSpeed)
            {
                horizVel = horizVel.normalized * airMaxHorizontalSpeed;
                rb.linearVelocity = new Vector3(
                    horizVel.x,
                    rb.linearVelocity.y,
                    horizVel.z
                );
            }

            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x * airDamping,
                rb.linearVelocity.y,
                rb.linearVelocity.z * airDamping
            );
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // =========================
    // JETPACK
    // =========================
    void HandleJetpack()
    {
        if (!playerHasJetPack)
        {
            Physics.gravity = Vector3.up * normalGravity;
            return;
        }

        bool jetpackHeld =
            Input.GetButton(jetpackButton) ||
            Input.GetKey(KeyCode.LeftShift); // dev fallback

        if (jetpackHeld)
        {
            JetpackFly();
        }
        else
        {
            Physics.gravity = Vector3.up * normalGravity;

            currentJetpackForce = Mathf.MoveTowards(
                currentJetpackForce,
                0f,
                jetpackRampDown * Time.fixedDeltaTime
            );
        }
    }

    void JetpackFly()
    {
        Physics.gravity = Vector3.up * jetpackGravity;

        currentJetpackForce = Mathf.MoveTowards(
            currentJetpackForce,
            jetpackForce,
            jetpackRampUp * Time.fixedDeltaTime
        );

        if (rb.linearVelocity.y < maxJetpackSpeed)
        {
            rb.AddForce(Vector3.up * currentJetpackForce, ForceMode.Acceleration);
        }
    }

    void TryPickUp()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            HoldablePainting painting =
                hit.collider.GetComponentInParent<HoldablePainting>();

            if (painting != null)
            {
                painting.PickUp(paintingHoldPoint);
                heldPainting = painting;
            }
        }
    }



}
