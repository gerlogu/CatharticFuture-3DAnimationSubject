using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers que matan al personaje.
/// </summary>
public class DeathBox : MonoBehaviour
{
    [Tooltip("Referencia al script del personaje")]
    [SerializeField] PlayerBehaviour player;

    /// <summary>
    /// Si el personaje entra en el Trigger, este muere
    /// </summary>
    /// <param name="other">Objeto que colisiona</param>
    private void OnTriggerEnter(Collider other)
    {
        // Si el objeto que ha colisionado tiene el tag "Player", el personaje muere
        if (other.CompareTag("Player"))
        {
            player.Die(); // Se llama a la función que mata al personaje
        }
    }
}
