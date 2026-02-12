using UnityEngine;

public class ArcadePaintController : MonoBehaviour
{
    public MouseLook mLook;
    public GameObject colourWheelUI;
    public KeyCode toggleWheelKey = KeyCode.Tab;

    bool wheelOpen;

    void Start() => ApplyWheelState(false);

    void Update()
    {
        if (Input.GetKeyDown(toggleWheelKey))
            ApplyWheelState(!wheelOpen);
    }

    void ApplyWheelState(bool open)
    {
        wheelOpen = open;

        if (colourWheelUI) colourWheelUI.SetActive(open);

        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

        if (mLook) mLook.enabled = !open;
    }
}
