using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    public Button mainMenyButton;
    public GameObject optionsPanal;
    public MouseLook mLook;
    [Header("Sensitivity Sliders")]
    public Slider mouseSlider;
    public Slider controllerSlider;

    public float mouseSensitivity = 1f;
    public float controllerSensitivity = 1f;
    public static bool MenuOpen;
    private void Awake()
    {
        mainMenyButton.onClick.AddListener(LoadMainMenu);
    }

    void Start()
    {
        // Load saved values (or defaults)
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 150f);
        controllerSensitivity = PlayerPrefs.GetFloat("ControllerSensitivity", 120f);

        mouseSlider.value = mouseSensitivity;
        controllerSlider.value = controllerSensitivity;

        mouseSlider.onValueChanged.AddListener(SetMouseSensitivity);
        controllerSlider.onValueChanged.AddListener(SetControllerSensitivity);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MenuOpen = !optionsPanal.activeSelf;
            optionsPanal.SetActive(MenuOpen);

            if (MenuOpen)
                UnlockCursor();
            else
                LockCursor();
        }
    }


    public void SetMouseSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        mLook.RefreshSensitivity();
    }

    public void SetControllerSensitivity(float value)
    {
        PlayerPrefs.SetFloat("ControllerSensitivity", value);
        mLook.RefreshSensitivity();
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }


    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
       // mLook.enabled = false;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //mLook.enabled = true;
    }

}
