
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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Left stick OR WASD
        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        // Jump (Space or A)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // Fire (Mouse 0 or Right Trigger)
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("FIRE / PAINT");
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
            rb.linearVelocity = new Vector3(
                velocity.x,
                rb.linearVelocity.y,
                velocity.z
            );
        }
        else
        {
            // HARD STOP when no input
            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}


/*using UnityEngine;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
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

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("CLICK");
        }

      
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        Vector3 moveDir = transform.right * xInput + transform.forward * zInput;
        Vector3 velocity = moveDir.normalized * moveSpeed;

        rb.linearVelocity = new Vector3(
            velocity.x,
            rb.linearVelocity.y,
            velocity.z
        );
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
*/