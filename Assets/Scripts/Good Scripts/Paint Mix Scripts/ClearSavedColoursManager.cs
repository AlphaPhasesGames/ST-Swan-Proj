using UnityEngine;
using UnityEngine.UI;
public class ClearSavedColoursManager : MonoBehaviour
{
    public GameObject clearColoursPanal;
    public Button yesClearButton;
    public Button noDontClearButton;

    public Button closePanal;

    
    public PaintPaletteSaveSystem saveSystemForPaint;


    private void Awake()
    {
        yesClearButton.onClick.AddListener(ClearColours);
        noDontClearButton.onClick.AddListener(ClosePanalAfterNo);
        closePanal.onClick.AddListener(OpenPanal);

        clearColoursPanal.SetActive(false);
    }

    public void ClearColours()
    {
        saveSystemForPaint.ResetPalette();
        clearColoursPanal.gameObject.SetActive(false);
    }

    public void ClosePanalAfterNo()
    {
        clearColoursPanal.gameObject.SetActive(false);
       
    }

    public void OpenPanal()
    {
       
        clearColoursPanal.gameObject.SetActive(true);
    }
}
