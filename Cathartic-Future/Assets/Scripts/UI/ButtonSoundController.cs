using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador del sonido de los botones del menú.
/// </summary>
public class ButtonSoundController : MonoBehaviour
{
    [Tooltip("Audio Source que reproduce el sonido deseado")]
    [SerializeField] AudioSource audioSource;
    [Tooltip("Sonido que se desea reproducir")]
    [SerializeField] AudioClip audioClip;
    [Tooltip("Sonido cuando se hacer click en un botón")]
    [SerializeField] AudioClip clickedAudioClip;

    /// <summary>
    /// Reproduce el sonido normal.
    /// </summary>
    public void PlaySound()
    {
        audioSource.volume = 0.1f;
        audioSource.PlayOneShot(audioClip);
    }

    /// <summary>
    /// Reproduce el sonido al presionar un botón.
    /// </summary>
    public void PlayClickedSound()
    {
        audioSource.volume = 0.3f;
        audioSource.PlayOneShot(clickedAudioClip);
    }
}
