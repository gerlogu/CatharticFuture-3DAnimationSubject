using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador del fundido.
/// </summary>
public class FadeController : MonoBehaviour
{
    [Tooltip("Índice del nivel")]
    public int levelIndex;

    /// <summary>
    /// Carga el nivel deseado.
    /// </summary>
    public void LoadLevel()
    {
        SceneManager.LoadSceneAsync(levelIndex);
    }
}
