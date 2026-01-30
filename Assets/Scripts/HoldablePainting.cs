using UnityEngine;

public class HoldablePainting : MonoBehaviour
{
    public bool isHeld;

    Rigidbody rb;
    Transform originalParent;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PickUp(Transform holdPoint)
    {
        if (isHeld) return;

        isHeld = true;
        originalParent = transform.parent;

        rb.isKinematic = true;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        if (!isHeld) return;

        isHeld = false;

        transform.SetParent(originalParent);
        //rb.isKinematic = false;
    }
}
