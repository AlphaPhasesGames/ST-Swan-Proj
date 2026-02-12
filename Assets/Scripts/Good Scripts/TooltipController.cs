using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    [Header("References")]
    public RectTransform tooltipRoot;
    public TMP_Text tooltipText;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public Vector2 offset = new Vector2(15f, -15f);
    public float fadeSpeed = 12f;
    public float showDelay = 0.15f;

    float targetAlpha;
    bool isVisible;

    void Awake()
    {
        Instance = this;

        if (tooltipRoot == null)
            tooltipRoot = GetComponent<RectTransform>();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        // Follow mouse while visible or fading
        if (canvasGroup.alpha > 0.001f || targetAlpha > 0f)
        {
            tooltipRoot.position = Input.mousePosition + (Vector3)offset;
        }

        canvasGroup.alpha = Mathf.Lerp(
            canvasGroup.alpha,
            targetAlpha,
            Time.deltaTime * fadeSpeed
        );
    }

    public void Show(string text)
    {
        CancelInvoke();                // cancel any pending hides
        tooltipText.text = text;       // swap text immediately
        targetAlpha = 1f;              // stay visible
        isVisible = true;
    }

    public void Hide()
    {
        CancelInvoke();
        targetAlpha = 0f;
        isVisible = false;
    }

    public void ForceHide()
    {
        CancelInvoke();
        targetAlpha = 0f;
        canvasGroup.alpha = 0f;
        isVisible = false;
    }
}
