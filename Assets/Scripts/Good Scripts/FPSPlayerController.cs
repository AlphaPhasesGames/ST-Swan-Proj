
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

    private Rigidbody rb;
    private float xInput;
    private float zInput;
    private bool isGrounded;
    public GrappleSystem grapple;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Left stick OR WASD
        xInput = Input.GetAxisRaw("Horizontal"); // using GetAxisRaw becuse using GetAxis was not zeroing out on stop.
        zInput = Input.GetAxisRaw("Vertical");

        

        // Jump 
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
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
        Vector3 moveDir = transform.right * xInput + transform.forward * zInput;

        // Grounded = snappy control
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
                // HARD STOP when grounded
                rb.linearVelocity = new Vector3(
                    0f,
                    rb.linearVelocity.y,
                    0f
                );
            }
            return;
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}


