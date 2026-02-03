using UnityEngine;
using UnityEngine.UI;
public class SwitchToEraseMode : MonoBehaviour
{
    public Button toggleEraseButton;
    
    public PaintCore pCore;


    private void Awake()
    {
        toggleEraseButton.onClick.AddListener(ToggleErase);
    }

    void ToggleErase()
    {
        bool newEraseState = !pCore.isErasing;
        pCore.SetEraseMode(newEraseState);
    }
}


