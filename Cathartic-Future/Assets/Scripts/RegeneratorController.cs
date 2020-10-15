using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador del regenerador de salud.
/// </summary>
public class RegeneratorController : MonoBehaviour
{

    [Tooltip("BoxCollider del Objeto")]
    [SerializeField] BoxCollider trigger;
    [Tooltip("Animator del objeto")]
    [SerializeField] Animator anim;
    private bool activated = false;

    /// <summary>
    /// Se comprueba si el personaje está en el área
    /// </summary>
    /// <param name="other">Player</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activated)
        {
            activated = true;
            anim.SetBool("Activated", true);
        }
    }

    /// <summary>
    /// Se comprueba si el personaje sale del área
    /// </summary>
    /// <param name="other">Player</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && activated)
        {
            activated = false;
            anim.SetBool("Activated", false);
        }
    }
}
