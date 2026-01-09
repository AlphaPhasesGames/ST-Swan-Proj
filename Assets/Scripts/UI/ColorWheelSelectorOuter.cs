using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// ColorWheelSelectorOuter
/// -----------------------
/// Handles click-based selection on the OUTER ring of a radial colour wheel.
/// 
/// - Detects pointer clicks on a UI Image
/// - Converts click position into an angle
/// - Maps that angle to a segment index (0..outerSegmentCount-1)
/// - Ignores the centre of the wheel (dead zone)
/// - Rotates a highlight overlay to show selection
/// - Emits an event when the selection changes
/// 
/// IMPORTANT:
/// This script is intentionally input-agnostic:
/// - No paint system logic
/// - No drag logic (for now)
/// - No platform-specific input
/// 
/// This makes it safe, testable, and easy to extend later.
/// </summary>
public class ColorWheelSelectorOuter : MonoBehaviour, IPointerDownHandler
{
    // -----------------------------
    // VISUAL FEEDBACK
    // -----------------------------

    [Header("Highlight")]
    // RectTransform for a highlight overlay image.
    // This should be a child of the wheel and contain ONE wedge highlight.
    // We rotate this to visually indicate the selected segment.
    public RectTransform highlightRect;

    // -----------------------------
    // OUTPUT / EVENTS
    // -----------------------------

    // Event fired whenever a NEW colour index is selected.
    // Anything can subscribe to this later (paint system, UI preview, sound, etc.)
    // without this script knowing or caring about it.
    public System.Action<int> OnColourSelected;

    // -----------------------------
    // WHEEL SETUP
    // -----------------------------

    [Header("Wheel Setup")]
    // RectTransform of the wheel image itself.
    // Used to convert screen coordinates into local wheel coordinates.
    public RectTransform wheelRect;

    // -----------------------------
    // OUTER RING CONFIGURATION
    // -----------------------------

    [Header("Outer Ring")]
    // Number of segments around the outer ring.
    // Must match the visual design of the wheel texture.
    public int outerSegmentCount = 12;

    [Tooltip("Normalized radius (0–1) where the outer ring starts")]
    // Anything inside this radius is ignored.
    // This prevents clicks on the centre colours (handled later).
    public float innerDeadZone = 0.35f;

    // -----------------------------
    // DEBUG / DEVELOPMENT HELPERS
    // -----------------------------

    [Header("Debug Colours")]
    // Optional array used only for debugging/logging.
    // Lets us confirm that the correct colour is being selected
    // without wiring into the paint system yet.
    public Color[] wheelColours;

    // -----------------------------
    // INTERNAL STATE
    // -----------------------------

    // Stores the last selected index.
    // Used to prevent duplicate events/logs when clicking the same segment.
    private int lastIndex = -1;

    // Public read-only accessor in case other systems want to query the current state.
    public int CurrentIndex => lastIndex;

    // -----------------------------
    // UNITY LIFECYCLE
    // -----------------------------

    void Awake()
    {
        // If no wheelRect was assigned manually,
        // assume this script is attached to the wheel Image itself.
        if (wheelRect == null)
            wheelRect = GetComponent<RectTransform>();

        //Hide the highlight at startup so nothing looks pre-selected.
        if (highlightRect != null)
        highlightRect.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called in the editor whenever values change.
    /// This is purely for catching setup mistakes early.
    /// </summary>
    void OnValidate()
    {
        // Warn if the number of debug colours doesn't match segment count.
        // This avoids silent mismatches that are hard to debug later.
        if (wheelColours != null &&  wheelColours.Length > 0 && wheelColours.Length != outerSegmentCount)
        {
            Debug.LogWarning(
                $"ColorWheelSelectorOuter: wheelColours length ({wheelColours.Length}) " +
                $"does not match outerSegmentCount ({outerSegmentCount})"
            );
        }
    }

    // -----------------------------
    // INPUT HANDLING
    // -----------------------------

    /// <summary>
    /// Called automatically by Unity's EventSystem when the wheel is clicked.
    /// This is the ONLY input entry point for now.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        EvaluatePointer(eventData);
    }

    // -----------------------------
    // CORE LOGIC
    // -----------------------------

    /// <summary>
    /// Converts pointer position into a segment index.
    /// This is where all the math lives.
    /// </summary>
    void EvaluatePointer(PointerEventData eventData)
    {
        Vector2 localPoint;

        // Convert screen-space pointer position into local-space coordinates
        // relative to the wheel's RectTransform.
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            wheelRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
            return;

        // Normalize local coordinates so the wheel radius is ~1.
        // This makes the logic resolution-independent.
        float radius = Mathf.Min(
      wheelRect.rect.width,
      wheelRect.rect.height
  ) * 0.5f;

        Vector2 normalized = localPoint / radius;

        // Distance from centre of the wheel.
        float distance = normalized.magnitude;

        // Ignore:
        // - Clicks inside the centre dead zone
        // - Clicks outside the wheel entirely
        if (distance < innerDeadZone || distance > 1f)
            return;

        // Calculate angle from the centre.
        // We intentionally swap parameters so:
        // - 0 degrees is at the TOP (12 o'clock)
        // - Angle increases CLOCKWISE
        float angle = Mathf.Atan2(normalized.x, normalized.y) * Mathf.Rad2Deg;

        // Ensure angle is always in the range 0–360
        if (angle < 0)
            angle += 360f;

        // Determine how wide each segment is.
       
        float segmentAngle = 360f / outerSegmentCount;

        // shift angle so clicks land in the CENTER of a wedge
        angle -= segmentAngle * -0.5f;

        // Convert angle into a segment index.
        // Clamp is used defensively to avoid floating-point edge cases.
        int index = Mathf.Clamp(
            Mathf.FloorToInt(angle / segmentAngle),
            0,
            outerSegmentCount - 1
        );

        // If the selection hasn't changed, do nothing.
        if (index == lastIndex)
            return;

        // Update internal state.
        lastIndex = index;

        // -----------------------------
        // VISUAL FEEDBACK
        // -----------------------------

        // Rotate the highlight overlay to match the selected segment.
        if (highlightRect != null)
        {
            float rotation = -index * segmentAngle;
            highlightRect.localEulerAngles = new Vector3(0, 0, rotation);
            highlightRect.gameObject.SetActive(true);
        }

        // -----------------------------
        // DEBUG OUTPUT
        // -----------------------------

        // Log the selected colour if a debug array is provided.
        if (wheelColours != null && index < wheelColours.Length)
        {
            Color selected = wheelColours[index];

            // Push colour into paint system
            PaintSurfaceBase.SetGlobalPaintColor(selected);

            Debug.Log($"Selected Colour: {selected}");
        }

        // Notify any listeners that the selection changed.
        OnColourSelected?.Invoke(index);

        // General debug log for index selection.
        Debug.Log($"Outer Wheel Selected: Index {index}");
    }


    // debug sections
#if UNITY_EDITOR
void OnDrawGizmos()
{
    if (wheelRect == null)
        wheelRect = GetComponent<RectTransform>();

    if (wheelRect == null)
        return;

    int segments = outerSegmentCount;
    if (segments <= 0)
        return;

    Vector3 center = wheelRect.position;

    float outerRadius = wheelRect.rect.width * 0.5f * wheelRect.lossyScale.x;
    float innerRadius = outerRadius * innerDeadZone;

    float segmentAngle = 360f / segments;

    // --- Draw dead zone ---
    Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
    DrawCircle(center, innerRadius);

    // --- Draw outer boundary ---
    Gizmos.color = Color.white;
    DrawCircle(center, outerRadius);

    // --- Draw segments ---
    for (int i = 0; i < segments; i++)
    {
        float angleDeg = -i * segmentAngle + 90f;
        float rad = angleDeg * Mathf.Deg2Rad;

        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            center + dir * innerRadius,
            center + dir * outerRadius
        );

        // --- Label ---
        Vector3 labelPos = center + dir * ((innerRadius + outerRadius) * 0.5f);

        Handles.color = Color.yellow;
        Handles.Label(labelPos, i.ToString());
    }
}

    void DrawCircle(Vector3 center, float radius)
    {
        const int steps = 64;
        Vector3 prev = center + Vector3.right * radius;

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            float angle = t * Mathf.PI * 2f;

            Vector3 next = center + new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0f) * radius;

            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    public Color[] palette = new Color[]
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


   

#endif

}
