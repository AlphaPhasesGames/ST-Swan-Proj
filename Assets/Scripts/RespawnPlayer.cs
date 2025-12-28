using UnityEngine;

public class RespawnPlayer : MonoBehaviour
{
    public GameObject restartPosition;
    public GameObject player;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.transform.position = restartPosition.transform.position;
        }
    }


}
