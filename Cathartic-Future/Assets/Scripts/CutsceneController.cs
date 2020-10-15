using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador de cinemáticas.
/// </summary>
public class CutsceneController : MonoBehaviour
{
    [Tooltip("Cámaras")]
    [SerializeField] Camera[] cams;
    [Tooltip("Referencia al Animator del fundido a negro")]
    [SerializeField] Animator fade;
    [Tooltip("Referencia al Transform del jugador")]
    [SerializeField] Transform player;
    [Tooltip("Posición donde aparecerá el jugador")]
    [SerializeField] Vector3 posToSpawn;

    public void playFadeIn()
    {
        fade.Play("FadeIn");
    }

    public void playFadeOut()
    {
        fade.Play("FadeOut");
    }

    public void swapToCamera(int index)
    {
        
        cams[index+1].enabled = true;
        cams[index].GetComponent<AudioListener>().enabled = false;
        cams[index].enabled = false;
        
        player.position = posToSpawn;
    }

    public void spawnPlayer()
    {
        Quaternion quad = new Quaternion();
        
        //player.rotation = quad.eulerAngles();
    }
}
