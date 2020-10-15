using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador de la presentación.
/// </summary>
public class PresentationController : MonoBehaviour
{
    [Tooltip("Referencia del Animator del personaje")]
    [SerializeField] Animator player;
    [Tooltip("Referencia del Animator auxiliar del personaje")]
    [SerializeField] Animator jumper;
    [Tooltip("Referencia del Animator del fundido de la escena")]
    [SerializeField] Animator fade;

    [SerializeField] AudioClip clip1;
    [SerializeField] AudioClip clip2;

    [SerializeField] AudioSource audioSource;


    /// <summary>
    /// Se inicia la animación de Caminar
    /// </summary>
    public void StartWalking()
    {
        player.SetBool("IsWalking", true);
        player.SetBool("IsIdle", false);
        player.SetBool("IsRunning", false);
    }

    /// <summary>
    /// Se inicia la animación de Correr
    /// </summary>
    public void StartRunning()
    {
        player.SetBool("IsRunning", true);
        player.SetBool("IsWalking", false);
        player.SetBool("IsIdle", false);
    }

    /// <summary>
    /// Se inicia la animación de Saltar
    /// </summary>
    public void StartJumping()
    {
        player.SetBool("IsJumping", true);
        player.SetBool("IsWalking", false);
        player.SetBool("IsIdle", false);
        jumper.SetBool("IsJumping", true);
    }

    /// <summary>
    /// Se inicia la animación de Caída
    /// </summary>
    public void EndJumping()
    {
        player.SetBool("IsFalling", true);
    }

    /// <summary>
    /// Se inicia la animación Neutra
    /// </summary>
    public void StartIdle()
    {
        player.SetBool("IsJumping", false);
        player.SetBool("IsFalling", false);
        player.SetBool("IsCrouching", false);
        player.SetBool("IsIdle", true);
    }

    /// <summary>
    /// Se inicia la animación de Andar Agachado
    /// </summary>
    public void StartCrouching()
    {
        player.SetBool("IsCrouching", true);
        player.SetBool("IsIdle", false);
    }

    /// <summary>
    /// Se inicia la animación de Disparar
    /// </summary>
    public void StartShooting()
    {
        player.SetTrigger("Shoot");
    }

    /// <summary>
    /// Se inicia la animación de Encogerse de hombros
    /// </summary>
    public void StartEncogerseDeHombros()
    {
        player.SetTrigger("Encogerse");
    }

    /// <summary>
    /// Se inicia la animación de Morir
    /// </summary>
    public void StartDeath()
    {
        player.SetTrigger("Death");
    }

    /// <summary>
    /// Cargar el menú de inicio
    /// </summary>
    public void End()
    {
        fade.SetTrigger("LoadLevel");
    }

    /// <summary>
    /// Se inicia la animación de Secarse la FRENTE
    /// </summary>
    public void StartSecar()
    {
        player.SetBool("IsCrouching", false);
        player.SetTrigger("Secar");
    }

    /// <summary>
    /// Se inicia la frase 1
    /// </summary>
    public void StartFrase1()
    {
        player.SetTrigger("Frase1");
        StartCoroutine(PlaySound(clip1, 0.15f, 0F, 0.4f));
    }

    /// <summary>
    /// Se inicia la frase 2
    /// </summary>
    public void StartFrase2()
    {
        player.SetTrigger("Frase2");
        StartCoroutine(PlaySound(clip2, 0.1f, 0F, 0.1f));
    }

    IEnumerator PlaySound(AudioClip clip, float volume, float spacialBlend, float time)
    {
        yield return new WaitForSeconds(time);
        audioSource.spatialBlend = spacialBlend;
        audioSource.volume = volume;
        audioSource.PlayOneShot(clip);
    }
}
