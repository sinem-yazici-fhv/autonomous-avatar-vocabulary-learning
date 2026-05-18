using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public int side;
    public DoorOpen d;

    void OnTriggerEnter(Collider other)
    {
        d.rote(side);
    }

    void OnTriggerStay(Collider other)
    {
        d.rote(side);
    }

    void OnTriggerExit(Collider c)
    {
        d.reset = true;
    }
}