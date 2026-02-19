using UnityEngine;
using System.Collections;

public class KnockOnDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] AudioSource knockKnock;
    [SerializeField] CareerTextManager textMan;
    [SerializeField] GameObject npcClient;

    bool playerInRange;
    bool isProcessing;

    const string USE_INPUT = "Buy";

    void Update()
    {
        if (!playerInRange || isProcessing)
            return;

        if (Input.GetButtonDown(USE_INPUT))
        {
            StartCoroutine(KnockRoutine());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    IEnumerator KnockRoutine()
    {
        isProcessing = true;

        knockKnock?.Play();

        yield return new WaitForSeconds(1.5f);

        textMan.enabled = true;
        textMan.StartConversation();

        isProcessing = false;
    }
}
