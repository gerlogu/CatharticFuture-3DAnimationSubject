using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador del menú de inicio.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    #region Variables Inicializables
    [Tooltip("Referencia al Animator del menú")]
    [SerializeField] Animator anim;
    [Tooltip("Referencia al AudioSource del menú")]
    [SerializeField] AudioSource audioSource;
    [Tooltip("Referencia al Animator del panel que muestra los controles")]
    [SerializeField] Animator controles;
    #endregion

    #region Variables Privadas
    private bool started = false; // Determina si ha empezado la carga
    #endregion

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (!started && Input.anyKeyDown)
        {
            started = true;
            anim.SetBool("Started", true);
            audioSource.Play();
        }
    }

    /// <summary>
    /// Abre el panel con los niveles disponibles.
    /// </summary>
    public void SelectLevelSelection()
    {
        anim.SetBool("IsLevelSelection", true);
    }

    /// <summary>
    /// Abre el menú de inicio.
    /// </summary>
    public void SelectMainMenu()
    {
        anim.SetBool("IsLevelSelection", false);
        anim.SetBool("IsSettingsMenu", false);
    }

    public void SelectLevelState()
    {
        anim.SetBool("IsLevelSelection", false);
        anim.SetBool("IsLevelStateSelection", true);
        anim.SetBool("IsSettingsMenu", false);
    }

    /// <summary>
    /// Abre el menú de controles.
    /// </summary>
    public void SelectControlsMenu()
    {
        controles.SetBool("IsShowing", true);
    }

    /// <summary>
    /// Oculta el menú de controles.
    /// </summary>
    public void HideControlsMenu()
    {
        controles.SetBool("IsShowing", false);
    }

    /// <summary>
    /// Cierra el juego.
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
