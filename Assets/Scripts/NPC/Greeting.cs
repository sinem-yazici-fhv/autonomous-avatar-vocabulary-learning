using System.Collections;
using UnityEngine;

public class Greeting : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("Greet"); 
    }
}