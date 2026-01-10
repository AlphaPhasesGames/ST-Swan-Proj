using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ColorWheelSelectorOuter : MonoBehaviour, IPointerDownHandler
{
    // -----------------------------
    // VISUAL FEEDBACK
    // -----------------------------

    [Header("Highlight")]
    public RectTransform highlightRect;

    // -----------------------------
    // OUTPUT / EVENTS (optional)
    // -----------------------------

    // Still index-based for UI / sound if you want it
    public System.Action<int> OnColourSelected;

    // -----------------------------
    // WHEEL SETUP
    // -----------------------------

    [Header("Wheel Setup")]
    public RectTransform wheelRect;

    // -----------------------------
    // OUTER RING CONFIGURATION
    // -----------------------------

    [Header("Outer Ring")]
    public int outerSegmentCount = 12;

    [Range(0f, 1f)]
    public float innerDeadZone = 0.35f;

    // -----------------------------
    // PALETTE (RUNTIME SAFE)
    // -----------------------------

    [Header("Palette (12 colours)")]
    public Color[] wheelColours = new Color[]
    {
        new Color(1f, 0f, 0f),   // 0 Red
        new Color(1f, 0.5f, 0f), // 1 Orange
        new Color(1f, 1f, 0f),   // 2 Yellow
        new Color(0.5f, 1f, 0f), // 3 Yellow-Green
        new Color(0f, 1f, 0f),   // 4 Green
        new Color(0f, 1f, 0.5f), // 5 Green-Cyan
        new Color(0f, 1f, 1f),   // 6 Cyan
        new Color(0f, 0.5f, 1f), // 7 Blue-Cyan
        new Color(0f, 0f, 1f),   // 8 Blue
        new Color(0.5f, 0f, 1f), // 9 Blue-Magenta
        new Color(1f, 0f, 1f),   // 10 Magenta
        new Color(1f, 0f, 0.5f), // 11 Magenta-Red
    };

    // -----------------------------
    // INTERNAL STATE
    // -----------------------------

    private int lastIndex = -1;
    public int CurrentIndex => lastIndex;

    // -----------------------------
    // UNITY LIFECYCLE
    // -----------------------------

    void Awake()
    {
        if (wheelRect == null)
            wheelRect = GetComponent<RectTransform>();

        wheelRect.gameObject.SetActive(false);

        if (highlightRect != null)
            highlightRect.gameObject.SetActive(false);

        if (wheelColours == null || wheelColours.Length != outerSegmentCount)
        {
            Debug.LogError(
                "ColorWheelSelectorOuter: wheelColours must contain EXACTLY 12 colours."
            );
        }
    }

    // -----------------------------
    // INPUT
    // -----------------------------

    public void OnPointerDown(PointerEventData eventData)
    {
        EvaluatePointer(eventData);
    }

    // -----------------------------
    // CORE LOGIC
    // -----------------------------

    void EvaluatePointer(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            wheelRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
            return;

        float radius = Mathf.Min(
            wheelRect.rect.width,
            wheelRect.rect.height
        ) * 0.5f;

        Vector2 normalized = localPoint / radius;
        float distance = normalized.magnitude;

        if (distance < innerDeadZone || distance > 1f)
            return;

        // 0° at top, clockwise
        float angle = Mathf.Atan2(normalized.x, normalized.y) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        float segmentAngle = 360f / outerSegmentCount;

        // Center click inside wedge
        angle += segmentAngle * 0.5f;

        int index = Mathf.FloorToInt(angle / segmentAngle);
        index = Mathf.Clamp(index, 0, outerSegmentCount - 1);

        if (index == lastIndex)
            return;

        lastIndex = index;

        // -----------------------------
        // VISUAL FEEDBACK
        // -----------------------------

        if (highlightRect != null)
        {
            highlightRect.localEulerAngles =
                new Vector3(0f, 0f, -index * segmentAngle);

            highlightRect.gameObject.SetActive(true);
        }

        // -----------------------------
        // APPLY PAINT COLOUR (OPTION A)
        // -----------------------------

        Color selected = wheelColours[index];
        PaintSurfaceBase.SetPaintColor(selected);

        Debug.Log($"Paint colour set to: {selected}");

        // Optional index event (UI / sound etc.)
        OnColourSelected?.Invoke(index);
    }

#if UNITY_EDITOR
    // -----------------------------
    // DEBUG GIZMOS
    // -----------------------------

    void OnDrawGizmos()
    {
        if (wheelRect == null)
            wheelRect = GetComponent<RectTransform>();

        if (wheelRect == null)
            return;

        Vector3 center = wheelRect.position;
        float outerRadius = wheelRect.rect.width * 0.5f * wheelRect.lossyScale.x;
        float innerRadius = outerRadius * innerDeadZone;

        Gizmos.color = Color.red;
        DrawCircle(center, innerRadius);

        Gizmos.color = Color.white;
        DrawCircle(center, outerRadius);
    }

    void DrawCircle(Vector3 center, float radius)
    {
        const int steps = 64;
        Vector3 prev = center + Vector3.right * radius;

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            float a = t * Mathf.PI * 2f;

            Vector3 next = center +
                new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;

            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}
