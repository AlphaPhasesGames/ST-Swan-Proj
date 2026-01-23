using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ChangeScenes : MonoBehaviour
{
    public Button devScene1;
    public Button careerPaintScene;

    private void Awake()
    {
        devScene1.onClick.AddListener(MoveToDevScene1);
        careerPaintScene.onClick.AddListener(MoveToCareerPaintScene);
    }

    public void MoveToDevScene1()
    {
        SceneManager.LoadScene("Paint Text Scene");
    }

    public void MoveToCareerPaintScene()
    {
        SceneManager.LoadScene("Career Scene");
    }


}
