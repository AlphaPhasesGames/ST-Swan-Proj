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

    [Header("Air Control")]
    public float airControlForce = 25f;
    public float airMaxHorizontalSpeed = 4f;
    public float airDamping = 0.98f;


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

        if (grapple.IsSwinging) return;

        if (isGrounded)
        {
            // GROUND — snappy and decisive
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
            // AIR / JETPACK — soft, controlled
            if (moveDir.sqrMagnitude > 0.01f)
            {
                rb.AddForce(moveDir.normalized * airControlForce, ForceMode.Acceleration);
            }

            // Cap horizontal air speed
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

            // Gentle air damping
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
        Vector3 antiGravity = -Physics.gravity;
        rb.AddForce(antiGravity, ForceMode.Acceleration);

        if (rb.linearVelocity.y < maxJetpackSpeed)
        {
            rb.AddForce(Vector3.up * jetpackForce, ForceMode.Acceleration);
        }
    }

}
