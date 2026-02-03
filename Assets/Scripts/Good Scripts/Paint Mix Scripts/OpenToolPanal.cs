using UnityEngine;
using UnityEngine.UI;
public class OpenToolPanal : MonoBehaviour
{
    public Button openTools;
    public GameObject toolPanel;
    private void Awake()
    {
        openTools.onClick.AddListener(TogglePanel);
        toolPanel.SetActive(false); // start closed
    }

    public void TogglePanel()
    {
        toolPanel.SetActive(!toolPanel.activeSelf);
    }

}
