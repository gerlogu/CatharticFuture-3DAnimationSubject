using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase de los objetos que transforman los nodos de
/// la bandera a nodos fijos
/// </summary>
public class Fixer2D : MonoBehaviour
{
    /// <summary>
    /// Función que ejecuta antes del primer fotograma de la pantalla
    /// </summary>
    public void Awake()
    {
        this.gameObject.tag = "Fixer"; // Antes del primer fotograma, se convierte el tag de este objeto a Fixer
                                       // para que sea detectado por el script MassSpring en la función Start
    }

    /// <summary>
    /// Función que se ejecuta en el primer fotograma de la pantalla
    /// </summary>
    public void Start()
    {
        this.GetComponent<MeshRenderer>().enabled = false; // Hacemos invisible el objeto
    }
}
