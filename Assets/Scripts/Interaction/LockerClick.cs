using UnityEngine;

public class LockerClick : MonoBehaviour
{
    public GameObject jacket;
    private bool isOpen = false;

    public bool HasJacket => jacket != null;

    void OnMouseDown()
    {
        if (!isOpen)
        {
            jacket.SetActive(true);
            isOpen = true;
        }
        else
        {
            jacket.SetActive(false);
            isOpen = false;
        }
    }
}
