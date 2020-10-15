using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// Gestiona la reproducción de los vídeos del menú.
/// </summary>
public class VideoPlayerScript : MonoBehaviour
{
    #region Variables Públicas
    [Tooltip("Imagen inicial")]
    public RawImage rawImage;
    [Tooltip("Referencia al VideoPlayer")]
    public VideoPlayer videoPlayer;
    [Tooltip("Referencia al Audio Source")]
    public AudioSource audioSource;
    #endregion

    #region Variables Privadas
    private bool started = false;
    private Texture black;
    #endregion

    /// <summary>
    /// Se llama antes del Start
    /// </summary>
    private void Awake()
    {
        black = rawImage.texture; // Se guarda la imagen inicial
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        videoPlayer.Prepare(); // Se prepara el VideoPlayer
    }

    /// <summary>
    /// Comienza la reproducción del vídeo
    /// </summary>
    public void StartPlayingVideo()
    {
        // Se considera a la animación por Comenzada
        started = true;

        // Se inicia el VideoPlayer
        StartCoroutine(PlayVideo());
    }

    /// <summary>
    /// Función que detiene la reproducción de un vídeo
    /// </summary>
    public void StopPlayingVideo()
    {
        // Se para la corutina
        StopCoroutine("PlayVideo");

        // Se pausa el VideoPlayer
        videoPlayer.Pause();
        // Se para el VideoPlayer
        videoPlayer.Stop();
        // Se para el sonido
        audioSource.Stop();

        // Se reinician los atributos
        started = false;
        rawImage.texture = black;
    }

    /// <summary>
    /// Ejecuta el video.
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayVideo()
    {
        if (started)
        {
            videoPlayer.Prepare();
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.25f);
            while (!videoPlayer.isPrepared)
            {
                
                yield return waitForSeconds;
                break;
            }
            while (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
                audioSource.Play();
                yield return waitForSeconds;
                break;
            }
            rawImage.color = new Color(1, 1, 1, 1);
            rawImage.texture = videoPlayer.texture;
        }
        else
        {
            videoPlayer.Pause();
            videoPlayer.Stop();
            audioSource.Stop();
            rawImage.color = new Color(1, 1, 1, 1);
            rawImage.texture = black;
            started = false;
            yield return -1;
        }
        
    }
}
