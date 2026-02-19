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

    float xRotation = 0f;

    void Start()
    {
        LoadSensitivity();

        if (!OptionsMenu.MenuOpen)
            LockCursor();
    }

    void Update()
    {
        //  Stop looking if menu is open
        if (OptionsMenu.MenuOpen)
            return;

        Look();
    }

    void LoadSensitivity()
    {
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 150f);
        controllerSensitivity = PlayerPrefs.GetFloat("ControllerSensitivity", 120f);
    }

    public void RefreshSensitivity()
    {
        LoadSensitivity();
    }

    void Look()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        float stickX = Input.GetAxis(lookXAxis);
        float stickY = Input.GetAxis(lookYAxis);

        if (Mathf.Abs(stickX) < stickDeadzone) stickX = 0f;
        if (Mathf.Abs(stickY) < stickDeadzone) stickY = 0f;

        stickX *= controllerSensitivity * Time.deltaTime;
        stickY *= controllerSensitivity * Time.deltaTime;

        float lookX = mouseX + stickX;
        float lookY = mouseY + stickY;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * lookX);
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}