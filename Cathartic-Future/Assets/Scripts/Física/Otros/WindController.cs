using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador del viento.
/// </summary>
public class WindController : MonoBehaviour
{
    #region Variables Inicializables
    [Tooltip("Tiempo máximo entre cada cambio en la dirección del viento")]
    [SerializeField] float maxTimer = 3;
    [Tooltip("Referencia de los objetos 3D")]
    [SerializeField] MassSpring[] _3dObjects;
    [Tooltip("Referencia de los objetos 2D")]
    [SerializeField] MassSpring2D[] _2dObjects;
    #endregion

    #region Variables Privadas
    float timer;                     // Tiempo que queda para el próximo cambio en la dirección del viento
    bool oppositeDirection = false;  // Determina si se cambia la dirección
    bool firstLoop = false;          // Determina si se ha realizado la primera iteración
    List<Vector3> originalVectors;   // Dirección del viento de los objetos 3D
    List<Vector3> originalVectors2D; // Dirección del viento de los objetos 2D
    #endregion

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        timer = maxTimer;                        // Se inicializa el timer
        originalVectors = new List<Vector3>();   // Inicializamos la lista de las direcciones de los objetos 3D
        originalVectors2D = new List<Vector3>(); // Inicializamos la lista de las direcciones de los objetos 2D

        // Recorremos los objetos 3D y rellenamos la lista con las direccion del viento de cada objeto
        for (int i = 0; i < _3dObjects.Length; i++)
        {
            originalVectors.Add(_3dObjects[i].direction);
        }

        // Recorremos los objetos 2D y rellenamos la lista
        for (int i = 0; i < _2dObjects.Length; i++)
        {
            originalVectors2D.Add(_2dObjects[i].direction);
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // Si el contador llega a cero
        if (timer <= 0)
        {
            // Se invierte la dirección del viento
            oppositeDirection = !oppositeDirection;
            // Primer loop finalizado
            firstLoop = true;
            // Se reinicia el contador
            timer = maxTimer*Random.Range(maxTimer, maxTimer*1.5f);
        }
        else
        {
            // Se actualiza el contador
            timer -= Time.deltaTime;
        }

        // Dirección 1
        if (oppositeDirection && firstLoop)
        {
            // Se actualiza la dirección del viento en todos los objetos 3D
            for (int i = 0; i < originalVectors.Count; i++)
            {
                // Se actualiza la dirección mediante una interpolación para suavizar el tránsito de una direccón a otra
                _3dObjects[i].direction = Vector3.Lerp(_3dObjects[i].direction, -originalVectors[i], Time.deltaTime/2);
            }

            // Se actualiza la dirección del viento en todos los objetos 2D
            for (int i = 0; i < originalVectors2D.Count; i++)
            {
                // Se actualiza la dirección mediante una interpolación para suavizar el tránsito de una direccón a otra
                _2dObjects[i].direction = Vector3.Lerp(_2dObjects[i].direction, -originalVectors2D[i], Time.deltaTime / 2);
            }
        }
        // Dirección 2
        else if(firstLoop)
        {
            // Se actualiza la dirección del viento en todos los objetos 3D
            for (int i = 0; i < originalVectors.Count; i++)
            {
                // Se actualiza la dirección mediante una interpolación para suavizar el tránsito de una direccón a otra
                _3dObjects[i].direction = Vector3.Lerp(_3dObjects[i].direction, originalVectors[i], Time.deltaTime/2);
            }

            // Se actualiza la dirección del viento en todos los objetos 2D
            for (int i = 0; i < originalVectors2D.Count; i++)
            {
                // Se actualiza la dirección mediante una interpolación para suavizar el tránsito de una direccón a otra
                _2dObjects[i].direction = Vector3.Lerp(_2dObjects[i].direction, originalVectors2D[i], Time.deltaTime / 2);
            }
        }
    }
}
