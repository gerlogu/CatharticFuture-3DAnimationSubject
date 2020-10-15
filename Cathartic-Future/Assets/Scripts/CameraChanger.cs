using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Controlador de la cámara.
/// </summary>
public class CameraChanger : MonoBehaviour
{
    #region Variables Inicializables
    [Tooltip("Referencia a la cámara.")]
    [SerializeField] CinemachineFreeLook freelook;
    [Tooltip("Radios originales")]
    [SerializeField] float[] preRadius;
    [Tooltip("Alturas originales")]
    [SerializeField] float[] preHeights;
    [Tooltip("Nuevo radio")]
    [SerializeField] float newRadius = 1.8f;
    [Tooltip("Posición en el eje Y")]
    [SerializeField] float screenY = 0.61f;
    #endregion

    #region Variables Privadas
    private CinemachineFreeLook.Orbit[] originalOrbits; // Órbitas de la cámara
    private float[] radius;                             // Radios
    private float[] heights;                            // Alturas
    private bool isIn;                                  // Determina si el jugador se encuentra en el área
    #endregion

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        this.GetComponent<MeshRenderer>().enabled = false;
        originalOrbits = new CinemachineFreeLook.Orbit[freelook.m_Orbits.Length];

        radius = new float[3];
        heights = new float[3];
        for (int i = 0; i < freelook.m_Orbits.Length; i++)
        {
            originalOrbits[i].m_Height = freelook.m_Orbits[i].m_Height;
            originalOrbits[i].m_Radius = freelook.m_Orbits[i].m_Radius;
            
        }
        radius[0] = preRadius[0] / newRadius;
        radius[1] = preRadius[1] / newRadius;
        radius[2] = preRadius[2] / newRadius;

        heights[0] = preHeights[0] / 0.5f;
        heights[1] = preHeights[1] / 0.5f;
        heights[2] = preHeights[2] / 0.5f;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (isIn)
        {
            for (int i = 0; i < freelook.m_Orbits.Length; i++)
            {
                freelook.m_Orbits[i].m_Radius = Mathf.Lerp(freelook.m_Orbits[i].m_Radius, radius[i], 0.01f);
                freelook.m_Orbits[i].m_Height = Mathf.Lerp(freelook.m_Orbits[i].m_Height, heights[i], 0.01f);
            }
        }
        else
        {
            for (int i = 0; i < freelook.m_Orbits.Length; i++)
            {
                freelook.m_Orbits[i].m_Radius = Mathf.Lerp(freelook.m_Orbits[i].m_Radius, preRadius[i], 0.01f);
                freelook.m_Orbits[i].m_Height = Mathf.Lerp(freelook.m_Orbits[i].m_Height, preHeights[i], 0.01f);
            }
        }
    }

    /// <summary>
    /// El jugador se encuentra en el área.
    /// </summary>
    /// <param name="other">Objeto que entra en el área</param>
    private void OnTriggerEnter(Collider other)
    {
        // Si el tag del objeto es "Player", es decir, el jugador
        if (other.CompareTag("Player"))
        {
            isIn = true; // El jugador se encuentra en el área
        }
    }

    /// <summary>
    /// El jugador sale del area.
    /// </summary>
    /// <param name="other">Objeto que sale del área</param>
    private void OnTriggerExit(Collider other)
    {
        // Si el tag del objeto es "Player", es decir, el jugador
        if (other.CompareTag("Player"))
        {
            isIn = false; // El jugador no se encuentra en el área
        }
    }
}
