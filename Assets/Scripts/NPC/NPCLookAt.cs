using UnityEngine;

public class NPCLookAt : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Nur Y-Rotation ändern, X und Z behalten
            Vector3 currentEuler = transform.rotation.eulerAngles;
            float targetY = targetRotation.eulerAngles.y;
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(currentEuler.x, targetY, currentEuler.z),
                Time.deltaTime * 3f
            );
        }
    }
}