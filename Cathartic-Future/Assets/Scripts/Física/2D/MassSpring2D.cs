using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase Manager de las físicas de la Tela
/// </summary>
public class MassSpring2D : MonoBehaviour
{
    #region Variables Inicializables
    [Header("Atributes")]
    [Tooltip("Gravedad")]
    [SerializeField] Vector3 g = new Vector3(0, 9.8f, 0);
    [Tooltip("Método de integración elegido para la animación")]
    [SerializeField] Integration integrationMethod;
    [Tooltip("Posición de los vértices para el cálculo de las físicas")]
    [SerializeField] VertexCoords vertexCoords;
    [Tooltip("Ejecutar debugger")]
    [SerializeField] Mode executionMode = Mode.Release;
    [Tooltip("Paso de integración (tiempo)")]
    [SerializeField] float h = 0.01f;

    [Header("Spring")]
    [Tooltip("Constante k de los muelles de Tracción")]
    [SerializeField] float k_T = 2000;
    [Tooltip("Constante k de los muelles de Flexión")]
    [SerializeField] float k_F = 800;
    [Tooltip("Constante de amortiguamiento (rotación)")]
    [SerializeField] float dRotation = 0.8f;
    [Tooltip("Constante de amortiguamiento")]
    [SerializeField] float dDeformation = 0.8f;

    [Header("Node")]
    [Tooltip("Masa de los nodos")]
    [Range(0.01F, 2.5F)]
    [SerializeField] float mass = 0.95f;
    [Tooltip("Constante de amortiguamiento")]
    [SerializeField] float dAbsolute = 0.01f;

    [Header("Wind")]
    [Tooltip("Fuerza del Viento")]
    [Range(0.0F,200F)]
    public float strength = 60;
    [Tooltip("Componente aleatoria")]
    [Range(0.0F, 1000F)]
    public float randomStrength = 10;

    [Header("Wind Direction")]
    [Tooltip("Dirección del Viento en el eje X")]
    [Range(-1f, 1f)]
    [SerializeField] float x = -0.5f;
    [Tooltip("Dirección del Viento en el eje Y")]
    [Range(-1f, 1f)]
    [SerializeField] float y = 0;
    [Tooltip("Dirección del Viento en el eje Z")]
    [Range(-1f, 1f)]
    [SerializeField] float z = 0;
    #endregion

    #region Variables Privadas
    private Mesh mesh;                // Mallado de la bandera
    private Vector3[] vertices;       // Posición de los vértices de la malla
    private Vector3[] globalVerts;    // Posición global (de la escena
    private List<Node2D> nodes;       // Lista de nodos de la malla
    private List<Spring2D> springsT;  // Lista de muelles de Tracción
    private List<Spring2D> springsF;  // Lista de muelles de Flexión
    private int[] triangles;          // Índice de los vértices
    private GameObject[] fixer;       // Array de Fixer
    private List<Edge2D> edges;       // Lista de aristas
    private int numIteraciones = 0;   // Número de iteraciones
    private Renderer objectRenderer;  // Renderer del objeto, necesario para optimizar
    #endregion

    #region Otras variables
    [HideInInspector] public Vector3 direction;  // Dirección del viento
    [HideInInspector] public bool paused = true; // Determina si la animación está transcurriendo o no
    #endregion

    // Lista enumerada de tipos de integración
    enum Integration
    {
        ExplicitEuler = 0,
        SymplecticEuler = 1,
        Verlet = 2
    }

    // Lista enumerada de tipos de integración
    enum VertexCoords
    {
        Global = 0,
        Local = 1
    }

    // Lista enumerada de los modos de ejecución
    enum Mode
    {
        Debug = 0,
        Release = 1
    }

    /// <summary>
    /// Función que se ejecuta antes del Start
    /// </summary>
    void Awake()
    {
        // Inicializamos la dirección del viento
        direction = new Vector3(x, y, z);
    }

    /// <summary>
    /// Función que se ejecuta en el refresco del primer fotograma.
    /// </summary>
    void Start()
    {
        numIteraciones = 0;

        // Buscamos los objetos fixer de la escena
        fixer = GameObject.FindGameObjectsWithTag("Fixer");

        // Buscamos la compomente MeshFilter del objeto y la referenciamos
        // en la variable mesh (de tipo MeshFilter)
        mesh = this.GetComponent<MeshFilter>().mesh;

        // Inicializamos el array de vértices en coordenadas globales
        globalVerts = new Vector3[mesh.vertices.Length];

        // Inicializamos la variable vertices con los vértices de la componente mesh
        // inicializada anteriormente
        vertices = mesh.vertices;

        // Inicializamos la lista de Edges
        edges = new List<Edge2D>();

        // Inicializamos la lista de nodos de una longitud determinada
        // por el número de vértices
        nodes = new List<Node2D>(vertices.Length);

        // Inicializamos la lista de muelles de tracción
        springsT = new List<Spring2D>();

        // Inicializamos la lista de muelles de flexión
        springsF = new List<Spring2D>();

        // Creamos los nodos que componen la tela (o el plano),
        // para ello realizamos un blucle que recorre los vértices
        // que conocemos
        for (int i = 0; i < vertices.Length; i++)
        {
            switch (vertexCoords)
            {
                case VertexCoords.Global:
                    // Inicializamos los vértices en posiciones globales
                    globalVerts[i] = this.transform.TransformPoint(vertices[i]);

                    // Creamos y añadimos un nodo a la lista de nodos con la posición
                    // del vértice actual y la masa junto a la constante de rigidez
                    nodes.Add(new Node2D(globalVerts[i], mass, dAbsolute, fixer, 1));
                    break;
                case VertexCoords.Local:
                    // Creamos y añadimos un nodo a la lista de nodos con la posición
                    // del vértice actual y la masa junto a la constante de rigidez
                    nodes.Add(new Node2D(vertices[i], mass, dAbsolute, fixer, 2));

                    // Como las coordenadas de los vértices ahora son locales y debemos inicializar
                    // los fixer, llamamos al método InitFixerState introduciendo las coordenadas
                    // globales del vértice
                    nodes[i].InitFixerState(this.transform.TransformPoint(vertices[i]), fixer);
                    break;
                default:
                    Debug.LogError("Vertex Position not supported.");
                    break;
            }   
        }

        // Inicializamos los triángulos a los pertenecientes por la
        // componente mesh
        triangles = mesh.triangles;

        // Añadimos los edges recorriendo la lista de triángulos de tres en tres
        for(int i = 0; i < triangles.Length; i+= 3)
        {
            int A = triangles[i];         // Vértices 1
            int B = triangles[i + 1];     // Vértices 2
            int C = triangles[i + 2];     // Vértices 3

            edges.Add(new Edge2D(A, B, C)); // Arista 1
            edges.Add(new Edge2D(B, C, A)); // Arista 2
            edges.Add(new Edge2D(C, A, B)); // Arista 3
        }

        // Ordenamos la lista de aristas
        edges.Sort((a,b) =>
        {
            int comparison = a.vertexA.CompareTo(b.vertexA);
            if (comparison == 0)
            {
                comparison = a.vertexB.CompareTo(b.vertexB);
            }
            return comparison;
        });

        // Se crean los muelles de Tracción y se añaden a su respectiva lista
        CreateSpringsT();

        // Se crean los muelles de Flexión y se añaden a su respectiva lista
        CreateSpringsF();

        #region Inicializar Muelles de Tracción
        foreach (Spring2D spring in springsT)
        {
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;               // Vector dirección del muelle
            spring.length0 = spring.dir.magnitude;                          // Longitud natural del muelle
            spring.length = spring.length0;                                 // Longitud actual del muelle
            spring.dir = Vector3.Normalize(spring.dir);                     // Longitud actual del muelle normalizado
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;        // Posición actual del muelle
            spring.rot = Quaternion.FromToRotation(Vector3.up, spring.dir); // Rotación actual del muelle
        }
        #endregion

        #region Inicializar Muelles de Flexión
        foreach (Spring2D spring in springsF)
        {
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;               // Vector dirección del muelle
            spring.length0 = spring.dir.magnitude;                          // Longitud natural del muelle
            spring.length = spring.length0;                                 // Longitud actual del muelle
            spring.dir = Vector3.Normalize(spring.dir);                     // Longitud actual del muelle normalizado
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;        // Posición actual del muelle
            spring.rot = Quaternion.FromToRotation(Vector3.up, spring.dir); // Rotación actual del muelle
        }
        #endregion

        // Almacenamos el renderer del objeto en su variable correspondiente
        objectRenderer = GetComponent<Renderer>();

        if (executionMode == Mode.Debug)
        {
            // Mostramos el número de nodos y muelles por consola para debuggear
            InitDebug();
        }
    }

    /// <summary>
    /// Función que inicializa los muelles de tracción.
    /// </summary>
    void CreateSpringsT()
    {
        // Recorremos todos los triángulos de la malla
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Si el muelle no se estuviese repitiendo, entonces se añade un muelle creado a raiz
            // de la posición i e i+1 del array de triángulos
            if (!CheckRepeatedSpring(nodes[triangles[i]], nodes[triangles[i + 1]], springsT))
            {
                springsT.Add(new Spring2D(nodes[triangles[i]], nodes[triangles[i + 1]], dDeformation, dRotation, k_T));
            }

            // Si el muelle no se estuviese repitiendo, entonces se añade un muelle creado a raiz
            // de la posición i e i+2 del array de triángulos
            if (!CheckRepeatedSpring(nodes[triangles[i]], nodes[triangles[i + 2]], springsT))
            {
                springsT.Add(new Spring2D(nodes[triangles[i]], nodes[triangles[i + 2]], dDeformation, dRotation, k_T));
            }

            // Si el muelle no se estuviese repitiendo, entonces se añade un muelle creado a raiz
            // de la posición i+1 e i+2 del array de triángulos
            if (!CheckRepeatedSpring(nodes[triangles[i + 1]], nodes[triangles[i + 2]], springsT))
            {
                springsT.Add(new Spring2D(nodes[triangles[i + 1]], nodes[triangles[i + 2]], dDeformation, dRotation, k_T));
            }
        }
    }

    /// <summary>
    /// Función que inicializa los muelles de flexión.
    /// </summary>
    void CreateSpringsF()
    {
        // Recorremos la lista de aristas
        for (int i = 0; i < edges.Count-1; i++)
        {
            // Si el vértice A coincide con el A de la siguiente arista
            // e igualmente coincide el vértice B con el B de la siguiente arista
            // entonces debe existir un muelle desde el vértice C de la primera arista
            // hasta el vértice C de la arista siguiente
            if ((edges[i].vertexA == edges[i+1].vertexA) 
               && (edges[i].vertexB == edges[i+1].vertexB)) 
            {
                springsF.Add(new Spring2D(nodes[edges[i].vertexC], nodes[edges[i+1].vertexC], dDeformation, dRotation, k_F));
            }
        }
    }

    /// <summary>
    /// Función que se ejecuta una vez por fotograma
    /// </summary>
    void Update()
    {

        #region Controles
        // Si se presiona la tecla "P", el estado del objeto cambia
        // de pausado a no pausado en función de su estado actual
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (paused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }
        #endregion
    }

    /// <summary>
    /// Función que se ejecuta en bucle (para físicas)
    /// </summary>
    private void FixedUpdate()
    {
        // Si el estado actual de este objeto es pausado o "paused"
        // o no se encuentra en el campo de visión del jugador,
        // no se ejecutan las líneas posteriores (las físicas)
        if (paused || !objectRenderer.isVisible)
        {
            return;
        }

        // En función del método de integración elegido, se ejecuta una función u otra
        switch (integrationMethod)
        {
            case Integration.ExplicitEuler:
                // Se realizan los cálculos referentes al Euler Explícito
                IntegrateExplicitEuler();
                break;

            case Integration.SymplecticEuler:
                // Se realizan los cálculos referentes al Euler Simpléctico
                IntegrateSymplecticEuler();
                break;
            case Integration.Verlet:
                // Se realizan los cálculos referentes al método de Verlet
                IntegrateVerlet();
                break;
            default:
                // En caso de no tener elegida ninguna de las opciones programas
                // se consideraría una opción errónea
                Debug.LogError("ERROR, MÉTODO DE INTEGRACIÓN NO VÁLIDO.");
                break;
        }

        // Actualizamos las variables de los muelles de TRACCIÓN
        foreach (Spring2D spring in springsT)
        {
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;               // Vector dirección del muelle
            spring.length = spring.dir.magnitude;                           // Longitud actual del muelle (distancia del nodo A al nodo B)
            spring.dir = Vector3.Normalize(spring.dir);                     // Vector dirección normalizado
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;        // Posición actual del muelle
            spring.rot = Quaternion.FromToRotation(Vector3.up, spring.dir); // Rotación actual del muelle
        }

        // Actualizamos las variables de los muelles de FLEXIÓN
        foreach (Spring2D spring in springsF)
        {
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;               // Vector dirección del muelle
            spring.length = spring.dir.magnitude;                           // Longitud actual del muelle (distancia del nodo A al nodo B)
            spring.dir = Vector3.Normalize(spring.dir);                     // Vector dirección normalizado
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;        // Posición actual del muelle
            spring.rot = Quaternion.FromToRotation(Vector3.up, spring.dir); // Rotación actual del muelle
        }

        // Actualizamos los vértices de la malla
        for(int i = 0; i < vertices.Length; i++)
        {
            switch (vertexCoords)
            {
                case VertexCoords.Global:
                    vertices[i] = transform.InverseTransformPoint(nodes[i].pos); // Igualamos los vértices de la bandera a la posición 
                                                                                 // de los nodos transformados a coordenadas
                                                                                 // globales
                    break;
                case VertexCoords.Local:
                    vertices[i] = nodes[i].pos; // Igualamos los vértices de la bandera a la posición 
                                                // de los nodos transformados a coordenadas
                                                // globales
                    break;
                default:
                    Debug.LogError("Vertex Position not supported.");
                    break;
            }
        }
        mesh.vertices = vertices;  // Actualizamos los vertices de la malla al array auxiliar de vértices
        mesh.RecalculateBounds();  // Después de modificar los vértices llamamos a esta función para asegurarnos que el volumen es correcto
        mesh.RecalculateNormals(); // Recalculamos las normales por si hubiese algún error con las mismas
    }

    /// <summary>
    /// Función que muestra el número de nodos, muelles de tracción y muelles de flexión 
    /// encontrados y generados para la malla.
    /// </summary>
    void InitDebug()
    {
        Debug.Log("Número de <color=orange>nodos</color>: " + nodes.Count);                  // Muestra en la consola el número de nodos
        Debug.Log("Número de <color=orange>muelles de tracción</color>: " + springsT.Count); // Muestra en la consola el número de muelles de Tracción
        Debug.Log("Número de <color=orange>muelles de flexión</color>: " + springsF.Count);  // Muestra en la consola el número de muelles de Flexión
    }

    /// <summary>
    /// Función que realiza los cálculos para el método del Euler Explicit.
    /// </summary>
    void IntegrateExplicitEuler()
    {
        // Se recorre cada nodo de la lista "nodes"
        foreach (Node2D node in nodes)
        {
            Vector2 aDir = Vector2.Perpendicular(new Vector2(direction.x, direction.y)); // Calculamos la perpendicular al viento
            float aS = Random.Range(-randomStrength, randomStrength) * strength;         // Multiplicamos la fuerza del viento por la componente aleatoria
            Vector3 aDir3D = new Vector3(aDir.x, aDir.y, 0);                             // Transformamos la perpendicular a un Vector de 3 dimensiones

            if (!node.fixedNode)
            {
                node.pos += h * node.vel;           // Actualizamos la posición del nodo
                node.force = node.mass * g;         // Fuerza de la gravedad
                node.force += direction * strength; // Fuerza del viento
                node.force += aDir3D * aS;          // Fuerza aleatoria
                ApplyDumpingNode(node);             // Aplicamos el amortiguamiento
            }
        }

        // Actualizamos los muelles de Tracción
        foreach (Spring2D spring in springsT)
        {
            spring.nodeA.force += -spring.k * (spring.length - spring.length0) * spring.dir; // Calculamos la fuerza en el nodo A
            spring.nodeB.force += spring.k * (spring.length - spring.length0) * spring.dir;  // Calculamos la fuerza en el nodo B
            ApplyDumpingSpring(spring); // Aplicamos el amortiguamiento
        }

        // Actualizamos los muelles de Flexión
        foreach (Spring2D spring in springsF)
        {
            spring.nodeA.force += -spring.k * (spring.length - spring.length0) * spring.dir; // Calculamos la fuerza en el nodo A
            spring.nodeB.force += spring.k * (spring.length - spring.length0) * spring.dir;  // Calculamos la fuerza en el nodo B
            ApplyDumpingSpring(spring); // Aplicamos el amortiguamiento
        }

        // Actualizamos la velocidad de los nodos
        foreach (Node2D node in nodes)
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
        // Se recorre cada nodo de la lista "nodes"
        foreach (Node2D node in nodes)
        {
            Vector2 aDir = Vector2.Perpendicular(new Vector2(direction.x, direction.y)); // Calculamos la perpendicular al viento
            float aS = Random.Range(-randomStrength, randomStrength) * strength;         // Multiplicamos la fuerza del viento por la componente aleatoria
            Vector3 aDir3D = new Vector3(aDir.x, aDir.y, 0);                             // Transformamos la perpendicular a un Vector de 3 dimensiones

            node.force = node.mass * g;         // Fuerza de la gravedad
            node.force += direction * strength; // Fuerza del viento
            node.force += aDir3D * aS;          // Fuerza aleatoria (o de la componente aleatoria)
            ApplyDumpingNode(node);             // Aplicamos el amortiguamiento
        }

        // Actualizamos los muelles de Tracción
        foreach (Spring2D spring in springsT)
        {
            spring.nodeA.force += -spring.k * (spring.length - spring.length0) * spring.dir; // Calculamos la fuerza en el nodo A
            spring.nodeB.force += spring.k * (spring.length - spring.length0) * spring.dir;  // Calculamos la fuerza en el nodo B
            ApplyDumpingSpring(spring); // Aplicamos el amortiguamiento
        }

        // Actualizamos los muelles de Flexión
        foreach (Spring2D spring in springsF)
        {
            spring.nodeA.force += -spring.k * (spring.length - spring.length0) * spring.dir; // Calculamos la fuerza en el nodo A
            spring.nodeB.force += spring.k * (spring.length - spring.length0) * spring.dir;  // Calculamos la fuerza en el nodo B
            ApplyDumpingSpring(spring); // Aplicamos el amortiguamiento
        }

        // Actualizamos los Nodos
        foreach (Node2D node in nodes)
        {
            if (!node.fixedNode)
            {
                node.vel += h * node.force / node.mass; // Actualizamos la velocidad del nodo
                node.pos += h * node.vel;               // Actualizamos la posición del nodo
            }
        }
    }

    /// <summary>
    /// Función que realiza los cálculos para el método de Verlet.
    /// </summary>
    void IntegrateVerlet()
    {
        Vector3 auxPos = new Vector3(0,0,0);
        // Se recorre cada nodo de la lista "nodes"
        foreach (Node2D node in nodes)
        {
            Vector2 aDir = Vector2.Perpendicular(new Vector2(direction.x, direction.y)); // Calculamos la perpendicular al viento
            float aS = Random.Range(-randomStrength, randomStrength) * strength;         // Multiplicamos la fuerza del viento por la componente aleatoria
            Vector3 aDir3D = new Vector3(aDir.x, aDir.y, 0);                             // Transformamos la perpendicular a un Vector de 3 dimensiones

            
            node.force = node.mass * g;         // Fuerza de la gravedad
            node.force += direction * strength; // Fuerza del viento
            node.force += aDir3D * aS;          // Fuerza aleatoria (o de la componente aleatoria)
            ApplyDumpingNode(node);             // Aplicamos el amortiguamiento
        }

        // Actualizamos los muelles de Tracción
        foreach (Spring2D spring in springsT)
        {
            spring.nodeA.force += -spring.k * (spring.length - spring.length0) * spring.dir; // Calculamos la fuerza en el nodo A
            spring.nodeB.force += spring.k * (spring.length - spring.length0) * spring.dir;  // Calculamos la fuerza en el nodo B
            ApplyDumpingSpring(spring); // Aplicamos el amortiguamiento
        }

        // Actualizamos los muelles de Flexión
        foreach (Spring2D spring in springsF)
        {
            spring.nodeA.force += -spring.k * (spring.length - spring.length0) * spring.dir; // Calculamos la fuerza en el nodo A
            spring.nodeB.force += spring.k * (spring.length - spring.length0) * spring.dir;  // Calculamos la fuerza en el nodo B
            ApplyDumpingSpring(spring); // Aplicamos el amortiguamiento
        }

        // Actualizamos los Nodos
        foreach (Node2D node in nodes)
        {
            if (!node.fixedNode)
            {
                auxPos = node.pos;       // Guardamos la posición actual en una variable auxiliar (Rn-1)
                if (numIteraciones == 0) // Primera iteración del método
                {
                    //node.posAnterior = node.pos; // Actualizamos la posición anterior
                    // Primera iteración del método de Verlet
                    node.pos = node.pos + h * node.vel + (1 / 2) * Mathf.Pow(h, 2) * (node.force / node.mass); 
                    numIteraciones++; // El número de iteraciones ahora es mayor a 0, por lo que este if
                                      // nunca volverá a ejecutarse
                }
                else
                {
                    // Calculamos la nueva posición del nodo
                    node.pos = 2 * node.pos - node.posAnterior + Mathf.Pow(h, 2) * (node.force / node.mass); 
                }
                node.posAnterior = auxPos; // Almacenamos en posAnterior la posición anterior del nodo
                node.vel = (node.pos - node.posAnterior) / h; // Calculamos la velocidad del nodo
                
            }
        }
    }

    /// <summary>
    /// Función que aplica el amortiguamiento a los nodos.
    /// </summary>
    void ApplyDumpingNode(Node2D node)
    {
        // Se actualiza la fuerza del nodo teniendo en cuenta "dAbsolute"
        node.force += -node.dAbsolute * node.vel;
    }

    /// <summary>
    /// Función que aplica el amortiguamiento a los muelles.
    /// </summary>
    void ApplyDumpingSpring(Spring2D spring)
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

    /// <summary>
    /// Método que comprueba si un muelle ESTUVIESE repetido.
    /// Devuelve "true" en case de que sea así.
    /// </summary>
    bool CheckRepeatedSpring(Node2D nodeA, Node2D nodeB, List<Spring2D> springs)
    {
        bool isRepeated = false;

        // Se recorre cada muelle
        foreach (Spring2D spring in springs)
        {
            // Se comprueba si los nodos enlazados al muelle coinciden 
            // con los muelles que se pasan como parámetros,
            // en caso de que así sea, el nuevo muelle se consideraría
            // uno repetido y se descartaría (de devuelve "true")
            if ((spring.nodeA.Equals(nodeA) && spring.nodeB.Equals(nodeB)) 
                || (spring.nodeB.Equals(nodeA) && spring.nodeA.Equals(nodeB)))
            {
                isRepeated = true;
            }
        }

        // Si se comprueba que el muelle no estaría repetido se devuelve "false"
        return isRepeated;
    }

    #region Métodos del Editor
    /// <summary>
    /// Pausa las físicas actualizando el booleano "paused" a true.
    /// </summary>
    public void Pause()
    {
        if (!paused && executionMode.Equals(Mode.Debug))
        {
            Debug.Log("Animación <color=red>Pausada</color>");
        }
        paused = true;
    }

    /// <summary>
    /// Reanuda las físicas actualizando el booleano "paused" a false.
    /// </summary>
    public void Unpause()
    {
        if (paused && executionMode.Equals(Mode.Debug))
        {
            Debug.Log("Animación <color=green>Reanudada</color>");
        }
        paused = false;
    }

    /// <summary>
    /// Reinicia los atributos de la clase a unos valores por defecto. (Utilizada en MassSpringEditor.cs)
    /// </summary>
    public void ResetOriginalValues(Vector3 g, float h, float k_T, float k_F, 
                                    float dRotation, float dDeformation, float mass,
                                    float dAbsolute, float strength, float randomStrength,
                                    Vector3 windDir, int method)
    {
        this.g              = g;
        this.h              = h;
        this.k_T            = k_T;
        this.k_F            = k_F;
        this.dRotation      = dRotation;
        this.dDeformation   = dDeformation;
        this.mass           = mass;
        this.dAbsolute      = dAbsolute;
        this.strength       = strength;
        this.randomStrength = randomStrength;
        this.x              = windDir.x;
        this.y              = windDir.y;
        this.z              = windDir.z;
        if(method == 1)
        {
            this.integrationMethod = Integration.ExplicitEuler;
        }
        else
        {
            this.integrationMethod = Integration.SymplecticEuler;
        }
        
    }
    #endregion

    /// <summary>
    /// Función para dibujar los muelles y mostrarlos por pantalla.
    /// </summary>
    public void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Dibujamos los muelles de Tracción
        Gizmos.color = Color.red;
        foreach (Spring2D spring in springsT)
        {
            Gizmos.DrawLine(spring.nodeA.pos, spring.nodeB.pos); // Dibujamos la línea que une los nodos A y B
        }

        // Dibujamos los muelles de Flexión
        Gizmos.color = Color.blue;
        foreach (Spring2D spring in springsF)
        {
            Vector3 aux0 = spring.nodeA.pos; // Definimos el punto 1
            Vector3 aux1 = spring.nodeB.pos; // Definimos el punto 2
            Gizmos.DrawLine(aux0, aux1);     // Dibujamos la línea que une ambos puntos
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, direction*8);
    }
}
