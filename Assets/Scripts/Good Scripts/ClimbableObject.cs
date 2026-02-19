using UnityEngine;

public class ClimbableObject : MonoBehaviour
{
    [Header("Climb Settings")]
    public FPSPlayerController cont;
    public float climbSpeed = 4f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cont.isTouchingClimbable = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cont.isTouchingClimbable = false;
        }
    }
}
