using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class ObjectClick : MonoBehaviour
{
    public string correctTag = "Chair";
    private bool taskActive = false;

    // NPCInteraction registriert sich hier
    public Action onCorrect;
    public Action onWrong;

    void Update()
    {
        if (!taskActive) return;
        if (Input.GetMouseButtonDown(0))
        {
            // NEU: UI Klick ignorieren
            if (EventSystem.current.IsPointerOverGameObject())
                return;
                
            HandleClick();
        }
    }

    void HandleClick()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (correctTag == "Jacket" && hit.collider.GetComponentInParent<LockerClick>() != null)
            {
                return;
            }

            if (hit.collider.CompareTag(correctTag))
            {
                taskActive = false;
                onCorrect?.Invoke();
            }
            else
            {
                onWrong?.Invoke();
            }
        }
    }
    
    public void StartTask(string tag)
    {
        correctTag = tag; // welches Objekt gesucht wird
        taskActive = true;
    }

    public void StopTask()
    {
        taskActive = false;
    }
}
