using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [TextArea]
    public string tooltipText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipController.Instance.Show(tooltipText);
        Debug.Log("Mouse Over");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.Hide();
        Debug.Log("Mouse Exit");
    }
}
