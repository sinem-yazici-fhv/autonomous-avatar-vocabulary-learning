using UnityEngine;

public class BillboardScript : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // Nur Y-Achse zur Kamera drehen — bleibt gerade aufrecht!
        Vector3 direction = mainCamera.transform.position - transform.position;
        direction.y = 0; // Y ignorieren → kein Kippen!
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-direction);
        }
    }
}