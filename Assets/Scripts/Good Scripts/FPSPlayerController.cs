
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FPSPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public Vector3 velocity;
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private float xInput;
    private float zInput;
    private bool isGrounded;

    public float jetpackAcceleration = 5f;
    public float maxJetpackSpeed = 8f;
    private float currentJetpackSpeed = 0f;
    public bool playerHasJetPack;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerHasJetPack = true;
    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        zInput = Input.GetAxisRaw("Vertical");

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        if (playerHasJetPack && Input.GetButton("Fire7"))
        {
            Jetpack();
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        Vector3 moveDir = transform.right * xInput + transform.forward * zInput;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Vector3 velocity = moveDir.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(velocity.x,rb.linearVelocity.y,velocity.z);
        }
        else
        {
            
            rb.linearVelocity = new Vector3(0f,rb.linearVelocity.y,0f);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }


    public void Hover()
    {
        velocity.y = 0f;
        rb.useGravity = false;
    }

    public void DebugPlayerFly()
    {
        rb.useGravity = false;
        if (isGrounded && velocity.y <= 0f)
        {
            velocity.y = 1f;
        }
    }

    public void LetPlayerFall()
    {
        rb.useGravity = true;
       // velocity.y += gravity * Time.deltaTime;
    }


    void Jetpack()
    {
        // Apply upward force
        rb.AddForce(Vector3.up * jetpackAcceleration, ForceMode.Acceleration);

        // Clamp vertical speed
        if (rb.velocity.y > maxJetpackSpeed)
        {
            rb.velocity = new Vector3(
                rb.velocity.x,
                maxJetpackSpeed,
                rb.velocity.z
            );
        }
    }
}


