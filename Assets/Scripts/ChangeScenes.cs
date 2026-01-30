using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ChangeScenes : MonoBehaviour
{
    public Button devScene1;
    public Button careerPaintScene;
    public Button creativePaintScene;
    public Button arcadePaintScene;
    private void Awake()
    {
        devScene1.onClick.AddListener(MoveToDevScene1);
        careerPaintScene.onClick.AddListener(MoveToCareerPaintScene);
        creativePaintScene.onClick.AddListener(MoveToCreativePaintScene);
        arcadePaintScene.onClick.AddListener(MoveToArcadePaintScene);
    }

    public void MoveToDevScene1()
    {
        SceneManager.LoadScene("Paint Text Scene");
    }

    public void MoveToCareerPaintScene()
    {
        SceneManager.LoadScene("Career Scene");
    }

    public void MoveToCreativePaintScene()
    {
        SceneManager.LoadScene("Creatiive Scene");
    }

    public void MoveToArcadePaintScene()
    {
        SceneManager.LoadScene("Arcade");
    }
}
