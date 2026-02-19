using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FPSPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public bool isTouchingClimbable;
    private float currentSpeed;
    public float jumpForce; //= 5f;
    private bool isSprinting = false;
    [Header("Stance")]
    public CapsuleCollider capsule;
    public Transform playerCamera;

    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float proneHeight = 0.5f;

    public float stanceTransitionSpeed = 10f;

    private float targetHeight;

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
    public string horizontalAxis = "Move X";
    public string verticalAxis = "Move Y";
    public string horizontalAxis2 = "Horizontal";
    public string verticalAxis2 = "Vertical";
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


 
    enum Stance { Stand, Crouch, Prone }
    Stance currentStance = Stance.Stand;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Physics.gravity = Vector3.up * normalGravity;

        targetHeight = standHeight;

        capsule.height = standHeight;
        capsule.center = new Vector3(0, standHeight / 2f, 0);
    }

    void Update()
    {

        if (isTouchingClimbable && Input.GetButtonDown(jumpButton))
        {
            ClimbingFunction();
        }

        float x1 = Input.GetAxis(horizontalAxis);   // Android / mobile stick
        float z1 = Input.GetAxis(verticalAxis);

        float x2 = Input.GetAxis(horizontalAxis2);  // Keyboard / default
        float z2 = Input.GetAxis(verticalAxis2);

        xInput = Mathf.Abs(x1) > 0.01f ? x1 : x2;
        zInput = Mathf.Abs(z1) > 0.01f ? z1 : z2;

        if (Input.GetButtonDown(jumpButton) && isGrounded)
        {
            Jump();
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
{
            isSprinting = !isSprinting;
        }

        currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        HandleJetpack();
        HandleStanceInput();
        UpdateStance();
        if (Input.GetButtonDown("Fire1"))
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

       // bool touchingClimbable = currentClimbable != null && !isGrounded;
        bool climbHeld = Input.GetButton(jumpButton);

        if (grapple && grapple.IsSwinging) return;

        Vector3 moveDir = transform.right * xInput + transform.forward * zInput;

        if (isGrounded)
        {
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Vector3 targetVel = moveDir.normalized * currentSpeed;

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

        if (isTouchingClimbable) return;

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

    void ClimbingFunction()
    {
        
        Physics.gravity = Vector3.up * jetpackGravity;

        currentJetpackForce = Mathf.MoveTowards(
            currentJetpackForce,
            jetpackForce,
            jetpackRampUp * Time.deltaTime
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

    void HandleStanceInput()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetStance(currentStance == Stance.Crouch ? Stance.Stand : Stance.Crouch);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SetStance(currentStance == Stance.Prone ? Stance.Stand : Stance.Prone);
        }
    }


    void UpdateStance()
    {
        float currentHeight = capsule.height;

        float newHeight = Mathf.Lerp(
            currentHeight,
            targetHeight,
            Time.deltaTime * stanceTransitionSpeed
        );

        capsule.height = newHeight;

        capsule.center = new Vector3(0, newHeight / 2f, 0);

        // Move camera with body
        if (playerCamera)
        {
            Vector3 camPos = playerCamera.localPosition;
            camPos.y = newHeight - 0.1f;
            playerCamera.localPosition = camPos;
        }
    }
    void SetStance(Stance newStance)
    {
        currentStance = newStance;
        isSprinting = false;
        switch (currentStance)
        {
            case Stance.Stand: targetHeight = standHeight; break;
            case Stance.Crouch: targetHeight = crouchHeight; break;
            case Stance.Prone: targetHeight = proneHeight; break;
        }
    }


}
