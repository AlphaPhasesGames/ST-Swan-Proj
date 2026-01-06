using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 150f;
    public float controllerSensitivity = 120f;

    public Transform playerBody;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Controller right stick input
        float stickX = Input.GetAxis("Cont X") * controllerSensitivity * Time.deltaTime;
        float stickY = Input.GetAxis("Cont Y") * controllerSensitivity * Time.deltaTime;

        // Combine inputs
        float lookX = mouseX + stickX;
        float lookY = mouseY + stickY;

        // Vertical rotation
        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal rotation
        playerBody.Rotate(Vector3.up * lookX);
    }
}