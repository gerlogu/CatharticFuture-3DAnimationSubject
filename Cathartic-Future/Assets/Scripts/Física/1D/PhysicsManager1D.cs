using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager1D : MonoBehaviour
{

    #region Variables
    [Header("Game Elements")]
    [Tooltip("Lista que contiene todos los nodos")]
    public List<Node1D> listOfNodes;
    [Tooltip("Lista que contiene todos los muelles")]
    public List<Spring1D> listOfSprings;

    [Header("Atributes")]
    [Tooltip("Booleano que indica si la animación está pausada o no")]
    public bool paused = false;
    [Tooltip("Gravedad")]
    [SerializeField] Vector3 g = new Vector3(0, 9.8f, 0);

    // Lista enumerada de tipos de integración
    enum Integration
    {
        ExplicitEuler = 0,
        ImplicitEuler = 1
    }

    [Tooltip("Método de integración elegido para la animación")]
    [SerializeField] Integration integrationMethod;

    [Tooltip("Paso de integración (tiempo)")]
    [SerializeField] float h = 0.01f;

    //private Vector3 force;
    #endregion

    /// <summary>
    /// Método Start
    /// </summary>
    void Start()
    {
        #region Inicializar Nodos
        foreach(Node1D node in listOfNodes)
        {
            node.Start(); // Inicializamos los nodos
        }
        #endregion

        #region Inicializar Muelles
        foreach(Spring1D spring in listOfSprings)
        {
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;               // Vector dirección del muelle
            spring.length0 = spring.dir.magnitude;                          // Longitud natural del muelle
            spring.length = spring.length0;                                 // Longitud actual del muelle
            spring.dir = Vector3.Normalize(spring.dir);                     // Longitud actual del muelle normalizado
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;        // Posición actual del muelle
            spring.rot = Quaternion.FromToRotation(Vector3.up, spring.dir); // Rotación actual del muelle
        }
        
        #endregion
    }

    /// <summary>
    /// Método Update
    /// </summary>
    void Update()
    {
        // Si se presiona la tecla "P", el estado del objeto cambia
        // de pausado a no pausado en función de su estado actual
        if (Input.GetKeyDown(KeyCode.P))
        {
            paused = !paused;
        }
    }

    private void FixedUpdate()
    {
        // Si el estado actual de este objeto es pausado o "paused",
        // no se ejecutan las líneas posteriores (las físicas)
        if (paused)
        {
            return;
        }

        switch (integrationMethod)
        {
            case Integration.ExplicitEuler:
                // Se realizan los cálculos referentes al Euler Explícito
                IntegrateExplicitEuler();
                break;

            case Integration.ImplicitEuler:
                // Se realizan los cálculos referentes al Euler Simpléctico
                IntegrateSymplecticEuler();
                break;

            default:
                Debug.LogError("ERROR, MÉTODO DE INTEGRACIÓN NO VÁLIDO.");
                break;
        }

        // Actualizamos las variables de los muelles
        foreach (Spring1D spring in listOfSprings)
        {
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;               // Vector dirección del muelle
            spring.length = spring.dir.magnitude;                           // Longitud actual del muelle (distancia del nodo A al nodo B)
            spring.dir = Vector3.Normalize(spring.dir);                     // Vector dirección normalizado
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;        // Posición actual del muelle
            spring.rot = Quaternion.FromToRotation(Vector3.up, spring.dir); // Rotación actual del muelle
        }
    }

    /// <summary>
    /// Función que realiza los cálculos para el método del Euler Explicit.
    /// </summary>
    void IntegrateExplicitEuler()
    {
        // Se recorre cada nodo de la lista "listOfNodes"
        foreach (Node1D node in listOfNodes)
        {
            if (!node.fixedNode)
            {
                node.pos += h * node.vel;    // Actualizamos la posición del nodo
                node.force = node.mass * g; // Fuerza de la gravedad
                ApplyDumpingNode(node);      // Aplicamos el amortiguamiento
            }
        }

        // Actualizamos los muelles
        foreach (Spring1D spring in listOfSprings)
        {
            spring.nodeA.force += -spring.k * (spring.length - spring.length0) * spring.dir; // Calculamos la fuerza en el nodo A
            spring.nodeB.force += spring.k * (spring.length - spring.length0) * spring.dir;  // Calculamos la fuerza en el nodo B
            ApplyDumpingSpring(spring); // Aplicamos el amortiguamiento
        }

        // Actualizamos la velocidad de los nodos
        foreach (Node1D node in listOfNodes)
        {
            if (!node.fixedNode)
            {
                node.vel += h * node.force / node.mass; // Actualizamos la velocidad del nodo
            }
        }
    }

    /// <summary>
    /// Función que realiza los cálculos para el método del Euler Simpléctico.
    /// </summary>
    void IntegrateSymplecticEuler()
    {
        // Se recorre cada nodo de la lista "listOfNodes"
        foreach (Node1D node in listOfNodes)
        {
            node.force = node.mass * g; // Actualizamos la fuerza de la gravedad del nodo
            ApplyDumpingNode(node);      // Aplicamos el amortiguamiento del nodo
        }

        // Actualizamos los muelles
        foreach (Spring1D spring in listOfSprings)
        {
            spring.nodeA.force += -spring.k * (spring.length - spring.length0) * spring.dir; // Calculamos la fuerza en el nodo A
            spring.nodeB.force += spring.k * (spring.length - spring.length0) * spring.dir;  // Calculamos la fuerza en el nodo B
            ApplyDumpingSpring(spring); // Aplicamos el amortiguamiento
        }

        // Actualizamos los Nodos
        foreach (Node1D node in listOfNodes)
        {
            if (!node.fixedNode)
            {
                node.vel += h * node.force / node.mass; // Actualizamos la velocidad del nodo
                node.pos += h * node.vel;               // Actualizamos la posición del nodo
            }
        }
    }

    /// <summary>
    /// Función que aplica el amortiguamiento a los nodos.
    /// </summary>
    void ApplyDumpingNode(Node1D node)
    {
        // Se actualiza la fuerza del nodo teniendo en cuenta "dAbsolute"
        node.force += -node.dAbsolute * node.vel;
    }

    /// <summary>
    /// Función que aplica el amortiguamiento a los muelles.
    /// </summary>
    void ApplyDumpingSpring(Spring1D spring)
    {
        // Se actualiza la fuerza del nodo A teniendo en cuenta "dRotation"
        spring.nodeA.force += -spring.dRotation * (spring.nodeA.vel - spring.nodeB.vel);
        // Se actualiza la fuerza del nodo A teniendo en cuenta "dDeformation"
        spring.nodeA.force += -spring.dDeformation * Vector3.Dot(spring.nodeA.vel - spring.nodeB.vel, spring.dir) * spring.dir;
        // Se actualiza la fuerza del nodo B teniendo en cuenta "dRotation"
        spring.nodeB.force += spring.dRotation * (spring.nodeA.vel - spring.nodeB.vel);
        // Se actualiza la fuerza del nodo B teniendo en cuenta "dDeformation"
        spring.nodeB.force += -spring.dDeformation * Vector3.Dot(spring.nodeA.vel - spring.nodeB.vel, spring.dir) * spring.dir;
    }
}
