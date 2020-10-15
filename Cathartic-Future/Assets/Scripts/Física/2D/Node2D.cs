using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase de los nodos de la bandera
/// </summary>
public class Node2D
{
    public bool fixedNode;           // Indica si es un nodo fijo o puede moverse
    public float mass = 5f;          // Masa del nodo
    public Vector3 vel;              // Velocidad del nodo
    public float dAbsolute = 0.01f;  // Factor de amortiguamiento del muelle absoluto
    public Vector3 pos;              // Posición del nodo
    public Vector3 force;            // Fuerza que sufre el nodo
    public Vector3 posAnterior;      // Posición anterior del nodo

    /// <summary>
    /// Constructor de la Clase
    /// </summary>
    public Node2D(Vector3 pos, float mass, float dAbsolute)
    {
        this.pos = pos;
        this.mass = mass;
        this.dAbsolute = dAbsolute; // Factor de amortiguamiento del muelle absoluto
        vel = Vector3.zero;
        posAnterior = this.pos;
    }

    /// <summary>
    /// Segundo constructor de la Clase
    /// </summary>
    public Node2D(Vector3 pos, float mass, float dAbsolute, GameObject[] fixer, int index)
    {
        this.pos       = pos;
        this.mass      = mass;
        this.dAbsolute = dAbsolute; // Factor de amortiguamiento del muelle absoluto
        vel = Vector3.zero;
        posAnterior = this.pos;
        if(index == 1)
        {
            // Comprueba si hay objetos con una componente Fixer
            this.InitFixerState(pos, fixer);
        }
        
    }

    /// <summary>
    /// Inizializa el buleano fixedNode en función de si el nodo colisiona
    /// con un objeto fixer
    /// </summary>
    public void InitFixerState(Vector3 globalPos, GameObject[] fixer)
    {
        foreach (GameObject f in fixer)
        {
            if (f.GetComponent<BoxCollider>().bounds.Contains(globalPos))
            {
                fixedNode = true; // Si se encuentra el nodo en el area de un Fixer, este nodo se vuelve fijo
            }
        }
    }
}
