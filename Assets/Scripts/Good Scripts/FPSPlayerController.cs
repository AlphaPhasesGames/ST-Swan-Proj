using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FPSPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;

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

    private Rigidbody rb;
    private float xInput;
    private float zInput;
    private bool isGrounded;

    public GrappleSystem grapple;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Physics.gravity = Vector3.up * normalGravity;
    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        zInput = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        HandleJetpack();
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
        Vector3 moveDir = transform.right * xInput + transform.forward * zInput;

        if (isGrounded && !grapple.IsSwinging)
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
                    0f,
                    rb.linearVelocity.y,
                    0f
                );
            }
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
        if (!playerHasJetPack || isGrounded)
        {
            Physics.gravity = Vector3.up * normalGravity;
            return;
        }

        bool jetpackTrigger = false;

#if UNITY_EDITOR
        jetpackTrigger = Input.GetKey(KeyCode.LeftShift);
#elif UNITY_ANDROID
        jetpackTrigger = Input.GetButton("JetpackAndroid");
#else
        jetpackTrigger = Input.GetButton("Jetpack");
#endif

        if (jetpackTrigger)
        {
            JetpackFly();
        }
        else
        {
            Physics.gravity = Vector3.up * normalGravity;
        }
    }

    void JetpackFly()
    {
        Physics.gravity = Vector3.up * jetpackGravity;

        if (rb.linearVelocity.y < maxJetpackSpeed)
        {
            rb.AddForce(Vector3.up * jetpackForce, ForceMode.Acceleration);
        }
    }
}
