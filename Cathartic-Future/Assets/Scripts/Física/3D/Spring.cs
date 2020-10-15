using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring
{
    public float k = 100f;           // Constante de rigidez del muelle (N/m)
    public Node nodeA;               // Primer extremo del muelle
    public Node nodeB;               // Segundo extremo del muelle
    public float defaultSize = 2f;   // Longitud natural de los cilindros en Unity (m)
    public float dRotation = 1f;     // Amortiguamiento debido a la rotación
    public float dDeformation = 10f; // Amortiguamiento debido a la deformación
    public float length0;            // Longitud natural del muelle
    public float length;             // Longitud actual del muelle
    public Vector3 pos;              // Posicion vertical del punto medio del muelle
    public Vector3 dir;              // Direccion del muelle
    public Quaternion rot;           // Rotación

    /// <summary>
    /// Constructor de la Clase
    /// </summary>
    public Spring(Node nodeA, Node nodeB, float dDeformation, float dRotation, float k)
    {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
        this.dDeformation = dDeformation;
        this.dRotation = dRotation;
        this.k = k;
    }
}
