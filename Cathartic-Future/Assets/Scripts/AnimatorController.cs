using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animator Controller auxiliar del jugador
/// </summary>
public class AnimatorController : MonoBehaviour
{
    [Tooltip("Referencia del script del jugador")]
    [SerializeField] PlayerBehaviour player;
    
    /// <summary>
    /// Inicia la animación de salto
    /// </summary>
    public void StartJump()
    {
        player.endedJump = true;
        player.isImpulsing = true;
        this.GetComponent<Animator>().Play("EndJump");
    }

    /// <summary>
    /// Concluye la animación de salto
    /// </summary>
    public void EndJump()
    {
        player.isImpulsing = false;
    }

}
