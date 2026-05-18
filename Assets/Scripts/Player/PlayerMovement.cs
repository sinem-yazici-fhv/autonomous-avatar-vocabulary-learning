using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 80f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private float yVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        yVelocity = -1f; 
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal"); 
        float z = Input.GetAxis("Vertical");   
    
        transform.Rotate(Vector3.up * x * rotationSpeed * Time.deltaTime);
      
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        
        Vector3 move = forward * z * speed;      
        if (controller.isGrounded)
        {
            yVelocity = -1f; 
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
        }

        move.y = yVelocity;

        
        controller.Move(move * Time.deltaTime);

        Vector3 rot = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0, rot.y, 0);
    }
}