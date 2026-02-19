using UnityEngine;
using TMPro;
public class CareerBuyItem : MonoBehaviour
{
    [SerializeField] int itemCost;
    [SerializeField] CareerMoney money;
    [SerializeField] GameObject objectToBuy;
    [SerializeField] GameObject buttonToEnable;
    [SerializeField] GameObject prompt;
    [SerializeField] TextMeshProUGUI promptCost;
    bool inRange;
    bool isBought;

    void Start()
    {
        enabled = false; // only run when player is near
    }

    void Update()
    {
        if (!inRange) return;
        if (isBought) return;

        if (money.cashAmount < itemCost) return;

        if (Input.GetButtonDown("Buy"))
        {
            BuyItem();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        prompt.SetActive(true);
        inRange = true;
        enabled = true;
        promptCost.text = itemCost.ToString();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        prompt.SetActive(false);
        inRange = false;
        enabled = false;
    }

    void BuyItem()
    {
        if (!money.SpendCash(itemCost))
            return;
        prompt.SetActive(false);
        objectToBuy.SetActive(false);
        buttonToEnable.SetActive(true);

        isBought = true;
        enabled = false;
    }
}
