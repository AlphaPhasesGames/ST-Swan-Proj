using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CareerTextManager : MonoBehaviour
{
    public string[] conversation1Text;
    public TextMeshProUGUI onscreenText;
    public CareerMoney money;
    public GameObject textPanal;
    public Button moveForward;
    public Button moreBackwards;
    public int currentIndex;
    public GameObject clientNPC;
    public GameObject playerNPC;
    public GameObject playerModel;
    private void Awake()
    {
       
        moveForward.onClick.AddListener(MoveArrayForward);
        moreBackwards.onClick.AddListener(MoveArrayback);
    }

    public void MoveArrayForward()
    {
        currentIndex++;
        RefreshLine();
    }

    
    public void MoveArrayback()
    {
        currentIndex--;
        RefreshLine();
    }

    void RefreshLine()
    {
        // Safety clamp
        currentIndex = Mathf.Clamp(currentIndex, 0, conversation1Text.Length - 1);

        // Update text
        onscreenText.text = conversation1Text[currentIndex];

        // Toggle speakers
        bool isPlayerLine = currentIndex % 2 == 0;

        playerNPC.SetActive(isPlayerLine);
        clientNPC.SetActive(!isPlayerLine);

        if (currentIndex == 0)
        {
            playerModel.gameObject.SetActive(false);
            textPanal.gameObject.SetActive(true);
        }

        if (currentIndex == conversation1Text.Length - 1)
        {
            EndConversation();
        }
    }

    void EndConversation()
    {
        playerModel.SetActive(true);
        textPanal.SetActive(false);
        clientNPC.SetActive(false);
        playerNPC.SetActive(false);

        money.cashAmount = 100; // add, don’t overwrite
        money.UpdateCashUI();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void StartConversation()
    {
        currentIndex = 0;
        RefreshLine();
        gameObject.SetActive(true); // if your dialogue UI starts hidden
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
