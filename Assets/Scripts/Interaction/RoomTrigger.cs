using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public NPCOverlay npcOverlay;

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (npcOverlay == null) return;
            npcOverlay.PlayerLeftRoom();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (npcOverlay == null) return;
            npcOverlay.PlayerEnteredRoom();
        }
    }
} 