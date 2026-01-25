using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ChangeScenes : MonoBehaviour
{
    public Button devScene1;
    public Button careerPaintScene;
    public Button creativePaintScene;
    private void Awake()
    {
        devScene1.onClick.AddListener(MoveToDevScene1);
        careerPaintScene.onClick.AddListener(MoveToCareerPaintScene);
        creativePaintScene.onClick.AddListener(MoveToCreativePaintScene);
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
}
