using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 150f;
    public float controllerSensitivity = 120f;

    [Header("Controller")]
    public string lookXAxis = "Look X";
    public string lookYAxis = "Look Y";
    [Range(0f, 0.3f)]
    public float stickDeadzone = 0.1f;

    [Header("References")]
    public Transform playerBody;
    public Camera cam;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
    }

    void Look()
    {
        // ---------- Mouse ----------
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ---------- Controller ----------
        float stickX = Input.GetAxis(lookXAxis);
        float stickY = Input.GetAxis(lookYAxis);

        if (Mathf.Abs(stickX) < stickDeadzone) stickX = 0f;
        if (Mathf.Abs(stickY) < stickDeadzone) stickY = 0f;

        stickX *= controllerSensitivity * Time.deltaTime;
        stickY *= controllerSensitivity * Time.deltaTime;

        // ---------- Combined ----------
        float lookX = mouseX + stickX;
        float lookY = mouseY + stickY;

        // ---------- Pitch (camera only) ----------
        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // ---------- Yaw (player body only) ----------
        playerBody.Rotate(Vector3.up * lookX);
    }
}
