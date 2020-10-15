using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la carga del menú de inicio.
/// </summary>
public class LevelManager : MonoBehaviour
{
    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadSceneAsync(0); // Carga el menú de Inicio
        }
    }
}
