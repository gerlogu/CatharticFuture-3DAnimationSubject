using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activador de las físicas.
/// -
/// OBJETIVO: para optimizar el rendimiento, hay objetos que
/// solo actualizarán sus físicas cuando el jugador se encuentre
/// en un área determinada.
/// </summary>
public class PhysicsActivator : MonoBehaviour
{
    #region Variables Inicializables
    [Tooltip("Script de los objetos 3D")]
    [SerializeField] MassSpring[] massSprings;
    [Tooltip("Script de los objetos 2D")]
    [SerializeField] MassSpring2D[] massSprings2D;
    #endregion

    bool activated = false; // Determina si se encuentra el jugador en el Área o no (para evitar repetir cálculos)

    /// <summary>
    /// Comprueba si el personaje se encuentra en el área para que el objeto
    /// actualice las físicas
    /// </summary>
    /// <param name="other">Collider del Player</param>
    private void OnTriggerEnter(Collider other)
    {
        // Si el jugador se encuentra en el área
        if (other.CompareTag("Player") && !activated)
        {
            // Se activan las físicas en TODOS los objetos introducidos
            // en este script
            foreach(MassSpring m in massSprings)
            {
                m.Unpause();
            }
            foreach (MassSpring2D m in massSprings2D)
            {
                m.Unpause();
            }
            activated = true; // Se actualiza el bool
        }
    }

    /// <summary>
    /// Comprueba si el personaje deja de encontrarse en el área, para que el objeto
    /// deje de actualizar las físicas
    /// </summary>
    /// <param name="other">Collider del Player</param>
    private void OnTriggerExit(Collider other)
    {
        // Si el jugador ha salido del área
        if (other.CompareTag("Player") && activated)
        {
            // Se desactivan las físicas en TODOS los objetos introducidos
            // en este script
            foreach (MassSpring m in massSprings)
            {
                m.Pause();
            }
            foreach (MassSpring2D m in massSprings2D)
            {
                m.Pause();
            }
            activated = false; // Se actualiza el bool
        }
    }
}
