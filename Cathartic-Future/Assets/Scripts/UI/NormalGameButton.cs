using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Controlador de los botones.
/// </summary>
public class NormalGameButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerClickHandler
{
    #region Variables Inicializables
    [Tooltip("Referencia del Audio Source")]
    [SerializeField] AudioSource audioSource;
    [Tooltip("Volumen")]
    [SerializeField] float volume;
    [Tooltip("Referencia del Audio Source 2")]
    [SerializeField] AudioSource audioToTurnOff;
    [Tooltip("Botón para desactivar")]
    [SerializeField] NormalGameButton buttonToShutDown;
    [Tooltip("Botón para desactivar")]
    [SerializeField] bool shutDownAll;
    [SerializeField] NormalGameButton secondButtonToShutDown;
    [SerializeField] VideoPlayerScript videoPlayer;
    [SerializeField] VideoPlayerScript videoPlayerToStop;
    [SerializeField] int levelIndex;
    [SerializeField] FadeController fadeController;
    #endregion

    #region Variables Privadas
    private bool activated = false;
    private bool canPlay = true;
    #endregion

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        if (shutDownAll)
        {
            pointerEnter();
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (activated)
        {
            if (!shutDownAll)
            {
                if (audioToTurnOff.volume > 0)
                {
                    audioToTurnOff.volume -= 0.08f * Time.deltaTime;
                }
                else
                {
                    if (audioSource.volume < volume && canPlay)
                    {
                        audioSource.volume += 0.08f * Time.deltaTime;
                        buttonToShutDown.activated = false;
                        
                        
                    }else if (!canPlay)
                    {
                        audioSource.volume -= 0.08f * Time.deltaTime;
                    }
                }
            }
            else
            {
                if (audioToTurnOff.volume > 0)
                {
                    audioToTurnOff.volume -= 0.08f * Time.deltaTime;
                }
                else
                {
                    if (audioSource.volume > 0)
                    {
                        audioSource.volume -= 0.08f * Time.deltaTime;
                    }
                    else
                    {
                        activated = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Función que se ejecuta cuando el ratón se encuentra encima del botón.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerEnter();
    }

    /// <summary>
    /// Función que se ejecuta cuando se hace click sobre el botón.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        canPlay = false;
        buttonToShutDown.activated = false;
        videoPlayerToStop.StopPlayingVideo();
        videoPlayer.StopPlayingVideo();
        activated = true;
    }

    /// <summary>
    /// Cuando se selecciona el botón con el ratón
    /// </summary>
    private void pointerEnter()
    {
        activated = true;
        buttonToShutDown.activated = false;
        if (shutDownAll)
        {
            secondButtonToShutDown.activated = false;
            videoPlayerToStop.StopPlayingVideo();
            videoPlayer.StopPlayingVideo();
        }
        else
        {
            videoPlayer.StartPlayingVideo();
            videoPlayerToStop.StopPlayingVideo();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       Debug.Log("OnPointerExit");
    }

    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("OnSelected");
    }
}
