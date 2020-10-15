using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spring para los objetos unidimensionales.
/// </summary>
public class Spring1D : MonoBehaviour
{

    #region Variables Inicializables
    [Tooltip("Constante de rigidez del muelle (N/m)")]
    public float k = 100f;
    [Tooltip("Primer extremo del muelle")]
    public Node1D nodeA;
    [Tooltip("Segundo extremo del muelle")]
    public Node1D nodeB;
    [Tooltip("Longitud natural de los cilindros en Unity (m)")]
    [SerializeField] float defaultSize = 2f;
    [Tooltip("Amortiguamiento debido a la rotación")]
    public float dRotation = 1f;
    [Tooltip("Amortiguamiento debido a la deformación")]
    public float dDeformation = 10f;
    #endregion

    #region Variables Ocultas
    [HideInInspector] public float length0;  // Longitud natural del muelle
    [HideInInspector] public float length;   // Longitud actual del muelle
    [HideInInspector] public Vector3 pos;    // Posicion vertical del punto medio del muelle
    [HideInInspector] public Vector3 dir;    // Direccion del muelle
    [HideInInspector] public Quaternion rot; // Rotación del muelle
    #endregion

    /// <summary>
    /// Método Update.
    /// </summary>
    void Update()
    {
        transform.position = pos;
        transform.localScale = new Vector3(transform.localScale.x, length / defaultSize, transform.localScale.z);
        transform.rotation = rot;
    }
}
