using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 150f;
    public float controllerSensitivity = 120f;

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
        // --- Input ---
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        float stickX = Input.GetAxis("Cont X") * controllerSensitivity * Time.deltaTime;
        float stickY = Input.GetAxis("Cont Y") * controllerSensitivity * Time.deltaTime;

        float lookX = mouseX + stickX;
        float lookY = mouseY + stickY;

        // --- Pitch (camera only) ---
        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // --- Yaw (player body only) ---
        playerBody.Rotate(Vector3.up * lookX);
    }
}
