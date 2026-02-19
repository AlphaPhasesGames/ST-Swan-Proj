using UnityEngine;

public class CareerFixFence : MonoBehaviour
{
    [SerializeField] GameObject brokenFence;
    [SerializeField] GameObject fixedFence;

    bool inRange;
    bool isFixed;

    void Start()
    {
        enabled = false; // no Update until player enters
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        inRange = true;
        enabled = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        inRange = false;
        enabled = false;
    }

    void Update()
    {
        if (isFixed) return;

        if (Input.GetButtonDown("Buy"))
        {
            FixFence();
        }
    }

    void FixFence()
    {
        brokenFence.SetActive(false);
        fixedFence.SetActive(true);
        isFixed = true;
    }
}
