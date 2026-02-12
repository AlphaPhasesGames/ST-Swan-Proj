using UnityEngine;

public class HoldablePainting : MonoBehaviour
{
    public bool isHeld;

    Rigidbody rb;
    Transform holdPoint;

    [SerializeField] float holdDistance = 0.6f;
    [SerializeField] float wallPadding = 0.05f;
    [SerializeField] LayerMask collisionMask = ~0; // everything by default

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError($"{name} has no Rigidbody!", this);
    }

    void Update()
    {
        if (!isHeld || holdPoint == null) return;

        Vector3 origin = holdPoint.position;
        Vector3 dir = holdPoint.forward;

        Vector3 targetPos = origin + dir * holdDistance;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, holdDistance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            targetPos = hit.point - dir * wallPadding;
        }

        transform.position = targetPos;
        transform.rotation = holdPoint.rotation;
    }

    public void PickUp(Transform newHoldPoint)
    {
        if (newHoldPoint == null) return;

        isHeld = true;
        holdPoint = newHoldPoint;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false; // optional: prevents weird overlap pushes
        }
    }

    public void Drop()
    {
        isHeld = false;
        holdPoint = null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
    }
}
