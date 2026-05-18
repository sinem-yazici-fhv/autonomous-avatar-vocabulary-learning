using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    Quaternion normal;
    Collider[] doorColliders;
    public bool reset;
    bool isOpen;

    void Start()
    {
        normal = transform.rotation;
        doorColliders = GetComponentsInChildren<Collider>();
    }

    public void rote(int side)
    {
        isOpen = true;
        SetDoorCollidersEnabled(false);

        if (side == 1)
        {
            float f = 0;
            f += 200f * Time.deltaTime;
            transform.Rotate(0, f, 0);
        }
        else if (side == 2)
        {
            float f = 0;
            f -= 200f * Time.deltaTime;
            transform.Rotate(0, f, 0);
        }
    }

    void Update()
    {
        if (reset)
        {
            if (Quaternion.Angle(transform.rotation, normal) < 0.5f)
            {
                transform.rotation = normal;
                reset = false;
                isOpen = false;
                SetDoorCollidersEnabled(true);
            }

            transform.rotation = Quaternion.Lerp(transform.rotation, normal, 1.3f * Time.deltaTime);
        }
        else if (!isOpen)
        {
            SetDoorCollidersEnabled(true);
        }
    }

    void SetDoorCollidersEnabled(bool enabled)
    {
        if (doorColliders == null) return;

        foreach (Collider doorCollider in doorColliders)
        {
            if (doorCollider == null || doorCollider.isTrigger) continue;
            doorCollider.enabled = enabled;
        }
    }
}
