using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class OptionsMenu : MonoBehaviour
{

    public bool menuOpen;
    public Button mainMenyButton;
    public GameObject optionsPanal;
    private void Awake()
    {
        mainMenyButton.onClick.AddListener(MenuOpen);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            optionsPanal.SetActive(!optionsPanal.activeSelf);
        }
    }

    public void MenuOpen()
    {
        SceneManager.LoadScene("Main Menu");
    }
}