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
        Cursor.visible = true;
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked &&
            Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Look();
    }

    void Look()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        float stickX = Input.GetAxis("Cont X") * controllerSensitivity * Time.deltaTime;
        float stickY = Input.GetAxis("Cont Y") * controllerSensitivity * Time.deltaTime;

        float lookX = mouseX + stickX;
        float lookY = mouseY + stickY;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * lookX);
    }
}