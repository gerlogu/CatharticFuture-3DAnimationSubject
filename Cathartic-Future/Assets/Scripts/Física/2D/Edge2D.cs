using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase de las aristas para facilitar la creación de los muelles
/// </summary>
public class Edge2D
{
    public int vertexA;  // Vértice A
    public int vertexB;  // Vértice B
    public int vertexC;  // Vértice C a tener en cuenta
    List<int> auxList;   // Lista auxiliar para ordenar los vértices A y B


    /// <summary>
    /// Constructor de la clase
    /// </summary>
    public Edge2D(int vertexA, int vertexB, int vertexC)
    {
        auxList = new List<int> { vertexA, vertexB }; // Inicializamos la lista auxiliar
        auxList.Sort();                               // Ordenamos los vértices de la lista

        this.vertexA = auxList[0];                    // El vértice A es igual al primer valor de la lista
        this.vertexB = auxList[1];                    // El vértice B es igual al segundo valor de la lista
        this.vertexC = vertexC;                       // El vértice C es el mismo que se introduce
    }
}
