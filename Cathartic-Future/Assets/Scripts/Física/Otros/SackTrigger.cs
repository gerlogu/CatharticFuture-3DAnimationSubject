using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger que activa la caída del saco
/// </summary>
public class SackTrigger : MonoBehaviour
{
    [Tooltip("MassSpring del objeto")]
    [SerializeField] MassSpring _MS;
    bool activated = false;

    /// <summary>
    /// Comprobamos si el personaje ha entrado en el área
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si el objeto que ha entrado en el Trigger
        // es el jugador si se se puede activar
        if (other.CompareTag("Player") && !activated)
        {
            _MS.ResetFixed(); // Todos los nodos dejan de ser fijos
            activated = true; // Se actualiza el booleano para que no vuelva a ejecutarse esta función
        }
    }
}
