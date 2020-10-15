using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool fixedNode;           // Indica si es un nodo fijo o puede moverse
    public float mass = 5f;          // Masa del nodo
    public Vector3 vel;              // Velocidad del nodo
    public float dAbsolute = 0.01f;  // Factor de amortiguamiento del muelle absoluto
    public Vector3 pos;              // Posición del nodo
    public Vector3 force;            // Fuerza que sufre el nodo
    public int index;                // Índice del nodo
    public Vector3 posAnterior;      // Posición anterior del nodo
    float margin = 4;                // Valor mínimo de la velocidad al colisionar

    /// <summary>
    /// Constructor de la Clase
    /// </summary>
    public Node(Vector3 pos, float mass, float dAbsolute, int index)
    {
        this.pos       = pos;
        this.mass      = mass;
        this.dAbsolute = dAbsolute; // Factor de amortiguamiento del muelle absoluto
        this.index     = index;
        posAnterior = this.pos;
    }

    /// <summary>
    /// Segundo constructor de la Clase (con Fixers)
    /// </summary>
    public Node(Vector3 pos, float mass, float dAbsolute, GameObject[] fixer, int index)
    {
        this.pos       = pos;
        this.mass      = mass;
        this.dAbsolute = dAbsolute; // Factor de amortiguamiento del muelle absoluto
        this.index     = index;
        posAnterior = this.pos;
        // Comprueba si hay objetos con una componente Fixer
        this.InitFixerState(pos, fixer);
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

    public string ToString()
    {
        return "[<color=orange>Index</color>: " + index + " | <color=orange>Position</color>: " + pos + " | <color=orange>Mass</color>: " + mass + "]";
    }

    /// <summary>
    /// Función que comprueba si el nodo está colisionando con algo
    /// </summary>
    /// <param name="collision">Collider con el que colisiona el nodo</param>
    /// <param name="position">Posición del nodo</param>
    /// <param name="permanent">Determina si es una colisión permanente o no</param>
    public void CheckCollision(BoxCollider collision, Vector3 position, bool permanent)
    {
        // Se comprueba si el nodo está contenido en el collider
        if (collision.bounds.Contains(position))
        {
            // Si la velocidad no se encuentra comprendida entre -4 y 4, 
            // se invierte la velocidad actual
            if (vel.y > margin || vel.y < -margin)
            {
                vel = -vel;
            }
            else
            {
                // Se comprueba si la colisión es permanente
                if (permanent)
                {
                    // Si la velocidad se encuentra comprendida entre -4 y 4, la velocidad se iguala a 0
                    vel = Vector3.zero;
                    fixedNode = true; // Al ser permanente, volvemos el nodo fijo
                }
                else
                {
                    // Hacemos el cálculo absoluto de la velocidad del nodo (Para elementos como las colchonetas)
                    vel.x = Mathf.Abs(vel.x);
                    vel.y = Mathf.Abs(vel.y);
                    vel.z = Mathf.Abs(vel.z);
                }
            }
        }
    }

    /// <summary>
    /// Comprueba si el personaje está colisionando con el nodo
    /// </summary>
    /// <param name="collision">Collider del personaje</param>
    /// <param name="position">Posición del nodo</param>
    /// <returns></returns>
    public bool CheckCollisionPlayer(BoxCollider collision, Vector3 position)
    {
        return collision.bounds.Contains(position);
    }
}
