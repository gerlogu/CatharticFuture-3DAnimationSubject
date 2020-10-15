using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Selector de niveles.
/// </summary>
public class LevelSelector : MonoBehaviour
{
    [Tooltip("Referencia al Animator del Fundido a Negro")]
    [SerializeField] Animator fade;
    [SerializeField] FadeController fadeController;

    /// <summary>
    /// Carga el juego normal.
    /// </summary>
    public void playJuegoNormal()
    {
        fade.Play("FadeOutLoad");
    }

    public void playAtardecer()
    {
        fadeController.levelIndex = 1;
        playJuegoNormal();
    }

    public void playNoche()
    {
        fadeController.levelIndex = 3;
        playJuegoNormal();
    }

    /// <summary>
    /// Carga la presentación.
    /// </summary>
    public void playPresentacion()
    {
        fadeController.levelIndex = 2;
        fade.Play("FadeOutLoad");
    }
}
