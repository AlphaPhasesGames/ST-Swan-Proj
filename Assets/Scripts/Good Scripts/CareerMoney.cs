using UnityEngine;
using TMPro;

public class CareerMoney : MonoBehaviour
{
    public int cashAmount;
    public TextMeshProUGUI onScreenCashAmount;

    void Start()
    {
        UpdateCashUI(); // set initial value once
    }

    public void AddCash(int amount)
    {
        cashAmount += amount;
        UpdateCashUI();
    }

    public bool SpendCash(int amount)
    {
        if (cashAmount < amount)
            return false;

        cashAmount -= amount;
        UpdateCashUI();
        return true;
    }

    public void UpdateCashUI()
    {
        onScreenCashAmount.text = cashAmount.ToString();
    }
}
