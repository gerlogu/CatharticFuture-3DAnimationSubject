using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nodo para los objetos unidimensionales
/// </summary>
public class Node1D : MonoBehaviour
{
    public bool fixedNode;                  // Indica si es un nodo fijo o puede moverse
    public float mass = 5f;                 // Masa del nodo
    public Vector3 vel;                     // Velocidad del nodo
    public float dAbsolute = 0.01f;         // Factor de amortiguamiento del muelle absoluto
    [HideInInspector] public Vector3 pos;   // Posición del nodo
    [HideInInspector] public Vector3 force; // Fuerza que sufre el nodo


    /// <summary>
    /// Método Start.
    /// </summary>
    public void Start()
    {
        pos = transform.position;
    }

    /// <summary>
    /// Método Update.
    /// </summary>
    void Update()
    {
        transform.position = pos;
    }
}
