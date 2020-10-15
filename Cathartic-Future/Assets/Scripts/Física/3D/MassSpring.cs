using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase MassSpring que controla y actualiza las físicas de los sólidos deformables
/// </summary>
public class MassSpring : MonoBehaviour
{
    #region Variables Inicializables
    [Header("Files")]
    [Tooltip("Fichero envolvente.ele.txt")]
    [SerializeField] TextAsset eleTXT;
    [Tooltip("Fichero envolvente.node.txt")]
    [SerializeField] TextAsset nodeTXT;

    [Header("Atributes")]
    [Tooltip("Gravedad")]
    [SerializeField] Vector3 g = new Vector3(0, 9.8f, 0);
    [Tooltip("Método de integración elegido para la animación")]
    [SerializeField] Integration integrationMethod;
    [Tooltip("Ejecutar debugger")]
    [SerializeField] Mode executionMode = Mode.Release;
    [Tooltip("Paso de integración (tiempo)")]
    [SerializeField] float h = 0.01f;
    [Tooltip("Densidad del sólido deformable")]
    [Range(0, 10)]
    [SerializeField] float density = 0.2f;

    [Header("Spring")]
    [Tooltip("Constante k de los muelles")]
    [SerializeField] float k = 2000;
    [Tooltip("Constante de amortiguamiento (rotación)")]
    [SerializeField] float dRotation = 0.8f;
    [Tooltip("Constante de amortiguamiento")]
    [SerializeField] float dDeformation = 0.8f;

    [Header("Node")]
    [Tooltip("Constante de amortiguamiento")]
    [SerializeField] float dAbsolute = 0.01f;
    [Header("Wind")]
    [Tooltip("Fuerza del Viento")]
    [Range(0.0F, 100F)]
    [SerializeField] float strength = 60;
    [Tooltip("Componente aleatoria")]
    [Range(0.0F, 15F)]
    [SerializeField] float randomStrength = 10;

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

    [Header("Collisions")]
    [Tooltip("Objetos con los que puede colisionar el sólido (Solo funcional con el método de integración de Euler Simpléctico)")]
    [SerializeField] BoxCollider[] colliders;
    [Tooltip("Bool que determina si una vez la velocidad del nodo sea igual a cero, si se quiere seguir actualizando el objeto")]
    [SerializeField] bool permanentCollision = true;
    [Tooltip("Bool que determina si se desea que el objeto se vea afectado por las colisiones con el jugador")]
    [SerializeField] bool playerCollisions = false;
    [Tooltip("Referencia del personaje del cual se detectarán colisiones")]
    [SerializeField] PlayerBehaviour playerReference;
    #endregion

    #region Variables Privadas
    private Mesh mesh;                                  // Mallado del objeto
    private Vector3[] vertices;                         // Array de las posiciones de los vértices
    private List<Node> nodes;                           // Lista de nodos
    private List<Spring> springs;                       // Lista de muelles
    private GameObject[] fixer;                         // Array de objetos fixer de la escena
    private Vector3[] verticesTetra;                    // Array de posiciones de un tetraedro
    private int[,] indexTetra;                          // Array que contiene el índice de los tetraedros y los índices de sus vértices
    private List<Tetrahedron> tetrahedrons;             // Lista de tetraedros
    private float[,] weights;                           // Array doble que contiene los pesos por vértice
    private Dictionary<int, int> vertsAndTetraRelation; // HashMap que contiene los tetraedros por cada vértice (la relación entre sus índices)
    private FileHandler handleText;                     // Clase que contiene las funciones necesarias para obtener el contenido de los ficheros
    private int numIteraciones = 0;                     // Número de iteraciones
    private Renderer objectRenderer;                    // Renderer del objeto, necesario para optimizar
    private MeshCollider meshCollider;                  // Mesh Collider del objeto (para las colisiones con el jugador)
    #endregion

    #region Otras variables
    [HideInInspector] public Vector3 direction;   // Dirección del viento
    [HideInInspector] public bool paused = false; // Estado del juego
    #endregion

    // Lista enumerada de tipos de integración
    enum Integration
    {
        ExplicitEuler = 0,
        SymplecticEuler = 1,
        Verlet = 2
    }

    // Lista enumerada de los modos de ejecución
    enum Mode
    {
        Debug = 0,
        Release = 1
    }

    /// <summary>
    /// Función que se ejecuta antes del refresco del primer fotograma.
    /// </summary>
    private void Awake()
    {
        // Inicializamos la dirección del viento
        direction = new Vector3(x, y, z);
    }

    /// <summary>
    /// Función que se ejecuta en el refresco del primer fotograma.
    /// </summary>
    void Start()
    {
        // Inicializamos el objeto que contiene las funciones necesarias para obtener el contenido de los ficheros
        handleText = new FileHandler(nodeTXT, eleTXT);

        // Leemos y guardamos el contenido del fichero .node
        verticesTetra = handleText.Read_node();

        // Leemos y guardamos el contenido del fichero .ele
        indexTetra = handleText.Read_ele();

        // Buscamos los fixer de la escena
        fixer = GameObject.FindGameObjectsWithTag("Fixer");

        // Buscamos la compomente MeshFilter del objeto y la referenciamos
        // en la variable mesh (de tipo MeshFilter)
        mesh = this.GetComponentInChildren<MeshFilter>().mesh;

        //// Inicializamos la variable vertices con los vértices de la componente mesh
        //// inicializada anteriormente
        vertices = mesh.vertices;

        // Inicializamos la lista de nodos de una longitud determinada
        // por el número de vértices
        nodes = new List<Node>();

        // Inicializamos la lista de muelles
        springs = new List<Spring>();

        // Inicializamos la lista de tetraedros
        tetrahedrons = new List<Tetrahedron>(verticesTetra.Length);

        // Inicializamos la referencia al meshCollider del objeto (si existe)
        meshCollider = GetComponentInChildren<MeshCollider>();

        // Si se desea que al objeto le afecte la colisión con el jugador
        if (playerCollisions)
        {
            if(!playerReference)
                // Inicializamos la referencia al jugador
                playerReference = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour>();
        }
        
        // Creamos tetraedros y los añadimos a la lista de tetraedros
        for (int i = 0; i < indexTetra.GetLength(0); i++)
        {
            tetrahedrons.Add(new Tetrahedron());
        }

        // Volumen auxiliar
        float v = 0;

        // Creamos los nodos recorriendo el número de vértices de los tetraedros
        for (int i = 0; i < verticesTetra.Length; i++)
        {
            v = 0; // Reiniciamos el valor del volumen a 0

            nodes.Add(new Node(verticesTetra[i], 0, dAbsolute, fixer, i)); // Añadimos un nodo en la posición del vértice del 
                                                                           // tetraedro contenido en el fichero

            // Recorremos todos los índices de los tetraedros (12 en este caso)
            for (int j = 0; j < indexTetra.GetLength(0); j++)
            {
                // Calculamos el volumen teniendo en cuenta el vértice de cada tetraedro
                if (i == (indexTetra[j, 0])
                    || i == (indexTetra[j, 1])
                    || i == (indexTetra[j, 2])
                    || i == (indexTetra[j, 3]))
                {
                    // Incrementamos el volumen
                    v += CalculateVolume(verticesTetra[indexTetra[j, 0]], verticesTetra[indexTetra[j, 1]], verticesTetra[indexTetra[j, 2]], verticesTetra[indexTetra[j, 3]]) / 4;

                    // Añadimos el nodo i (creado en el primer for el cual contiene a este segundo
                    // al tetraedro j
                    tetrahedrons[j].Add(nodes[i]);
                }
            }
            // Calculamos la masa del nodo como el volumen del tetraedro multiplicado por la densidad
            nodes[i].mass = v * density;
        }

        // Se inicializan los muelles
        InitSprings();

        #region Inicializar Muelles
        foreach (Spring spring in springs)
        {
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;               // Vector dirección del muelle
            spring.length0 = spring.dir.magnitude;                          // Longitud natural del muelle
            spring.length = spring.length0;                                 // Longitud actual del muelle
            spring.dir = Vector3.Normalize(spring.dir);                     // Vector dirección normalizado
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;        // Posición actual del muelle
            spring.rot = Quaternion.FromToRotation(Vector3.up, spring.dir); // Rotación actual del muelle
        }
        #endregion

        // Inicializamos el array doble de pesos, donde el primer valor es igual a la longitud del array
        // de vértices y el segundo es igual a 4 (cuatro vértices por tetraedro)
        weights = new float[vertices.Length, 4];

        // Inicializamos el Hashmap (o Dictionary aquí en C#) 
        vertsAndTetraRelation = new Dictionary<int, int>();

        #region Inicialización de los Pesos
        // Recorremos el array de vértices
        for (int i = 0; i < vertices.Length; i++)
        {
            // Recorremos la lista de tetraedros por cada vértice
            for (int j = 0; j < tetrahedrons.Count; j++)
            {
                // Comprobamos si el vértice i se encuentra en el tetraedro j
                if (VertexInTeadetron(vertices[i], tetrahedrons[j]))
                {
                    // Calculamos el volumen del tetraedro
                    float volume = CalculateVolume(tetrahedrons[j].nodes[0].pos, tetrahedrons[j].nodes[1].pos,
                                                   tetrahedrons[j].nodes[2].pos, tetrahedrons[j].nodes[3].pos);

                    // Calculamos todos los pesos en el vértice i
                    CalculateAllWeights(i, tetrahedrons[j], volume);

                    // Comprobamos si existe la clave i (no deben existir dos valores con la misma clave)
                    if (vertsAndTetraRelation.ContainsKey(i))
                    {
                        vertsAndTetraRelation.Remove(i); // Borramos la clave junto su valor para poder sobrescribirla
                                                         // en caso de que fuese necesario
                    }
                    vertsAndTetraRelation.Add(i, j); // Añadimos el tetraedro j en la posición del vértice i, es decir, 
                                                     // mediante un diccionario (clave = vertice, valor = vértice del tetraedro)
                }
            }
        }
        #endregion

        // Almacenamos el renderer del objeto en su variable correspondiente
        objectRenderer = GetComponentInChildren<Renderer>();

        // Recalculamos las normales inicialmente
        mesh.RecalculateNormals();

        // Si el modo de ejecución es Debug, si es así, se ejecuta el debug el cual imprime datos en la consola
        if (executionMode == Mode.Debug)
        {
            // Inicializamos el debugger
            InitDebug();
        }
    }

    /// <summary>
    /// Función para calcular el peso de un vértice en función de tres puntos y el volumen.
    /// </summary>
    /// <param name="r1">Primer vértice</param>
    /// <param name="r2">Segundo vértice</param>
    /// <param name="r3">Tercer vértice</param>
    /// <param name="p">Cuarto vértice</param>
    /// <param name="v">Volumen</param>
    /// <returns></returns>
    float CalculateWeight(Vector3 r1, Vector3 r2, Vector3 r3, Vector3 p, float v)
    {
        return CalculateVolume(r1, r2, r3, p) / v; // Se devuelve el volumen calculado
    }

    /// <summary>
    /// Calcula los cuatro pesos de los vértices que componen al tetraedro 
    /// en función del vértice de la posición "index" en el array "vertices".
    /// </summary>
    /// <param name="index">Índice o posición del array doble de los pesos (fila)</param>
    /// <param name="tetraedron">Tetraedro correspondiente</param>
    /// <param name="volume">Volumen</param>
    void CalculateAllWeights(int index, Tetrahedron tetraedron, float volume)
    {
        // Calculamos el peso del primer vértice del tetraedro
        weights[index, 0] = CalculateWeight(tetraedron.nodes[1].pos, tetraedron.nodes[2].pos,
                                            tetraedron.nodes[3].pos, vertices[index],
                                            volume);

        // Calculamos el peso del segundo vértice del tetraedro
        weights[index, 1] = CalculateWeight(tetraedron.nodes[0].pos, tetraedron.nodes[2].pos,
                                            tetraedron.nodes[3].pos, vertices[index],
                                            volume);

        // Calculamos el peso del tercer vértice del tetraedro
        weights[index, 2] = CalculateWeight(tetraedron.nodes[0].pos, tetraedron.nodes[1].pos,
                                            tetraedron.nodes[3].pos, vertices[index],
                                            volume);

        // Calculamos el peso del cuarto vértice del tetraedro
        weights[index, 3] = CalculateWeight(tetraedron.nodes[0].pos, tetraedron.nodes[1].pos,
                                        tetraedron.nodes[2].pos, vertices[index],
                                        volume);
    }

    /// <summary>
    /// Analiza un plano formado por r1, r2 y r3, te saca la normal mediante el uso de r4
    /// y comprueba mediante el signo, si el punto p se encuentra en el mismo lado.
    /// </summary>
    /// <param name="r1">Primer vértice del tetraedro</param>
    /// <param name="r2">Segundo vértice del tetraedro</param>
    /// <param name="r3">Tercer vértice del tetraedro</param>
    /// <param name="r4">Cuarto vértice del tetraedro</param>
    /// <param name="p">Punto del que se desea comprobar en qué lado se encuentra</param>
    /// <returns></returns>
    bool IsSameSide(Vector3 r1, Vector3 r2, Vector3 r3, Vector3 r4, Vector3 p)
    {
        Vector3 normal = Vector3.Cross(r2 - r1, r3 - r1);            // Normal entre los puntos del tetraedro
        float dotV4 = Vector3.Dot(normal, r4 - r1);                  // 
        float dotP = Vector3.Dot(normal, p - r1);                    // 
        return (Mathf.Sign(dotV4) == Mathf.Sign(dotP)) || dotP == 0; // Comprobamos mediante el signo, si el punto p 
                                                                     // se encuentra en el mismo lado
    }

    /// <summary>
    /// Comprueba si un punto se encuentra contenido en un tetraedro.
    /// </summary>
    /// <param name="p">Punto del cual se desea saber si se encuentra contenido en el tetraedro</param>
    /// <param name="tetra">Tetraedro que debe contener al punto</param>
    /// <returns></returns>
    bool VertexInTeadetron(Vector3 p, Tetrahedron tetra)
    {
        Vector3 r1 = tetra.nodes[0].pos; // Posición del vértice 1 del tetraedro
        Vector3 r2 = tetra.nodes[1].pos; // Posición del vértice 2 del tetraedro
        Vector3 r3 = tetra.nodes[2].pos; // Posición del vértice 3 del tetraedro
        Vector3 r4 = tetra.nodes[3].pos; // Posición del vértice 4 del tetraedro

        // Comprobamos si el vértice se encuentra contenido en el tetraedro
        // analizando su relación con los puntos del tetraedro en los
        // distintos órdenes posibles
        if (IsSameSide(r1, r2, r3, r4, p) && IsSameSide(r2, r3, r4, r1, p)
            && IsSameSide(r3, r4, r1, r2, p) && IsSameSide(r4, r1, r2, r3, p))
        {
            return true; // Si la condición anterior se cumple, el vértice se encuentra contenido
                         // en el tetraedro
        }
        return false; // Si la condición anterior NO se cumple, el vértice NO se encuentra contenido
                      // en el tetraedro
    }

    /// <summary>
    /// Calcular el volumen entre cuatro puntos.
    /// </summary>
    /// <param name="r1">Primer vértice del tetraedro</param>
    /// <param name="r2">Segundo vértice del tetraedro</param>
    /// <param name="r3">Tercer vértice del tetraedro</param>
    /// <param name="r4">Cuarto vértice del tetraedro</param>
    /// <returns></returns>
    float CalculateVolume(Vector3 r1, Vector3 r2, Vector3 r3, Vector3 r4)
    {
        // Calculamos el volumen en función de los cuatro puntos
        float v = Mathf.Abs(Vector3.Dot((r2 - r1), Vector3.Cross(r3 - r1, r4 - r1))) / 6;
        return v; // Devolvemos el volumen calculado
    }

    /// <summary>
    /// Función que inicializa los muelles de tracción.
    /// </summary>
    void InitSprings()
    {
        // Recorremos los índices de los tetraedros
        for (int i = 0; i < indexTetra.GetLength(0); i++)
        {
            // Comprobamos si el muelle que se crearía entre los distintos nodos está repetido (para 
            // no añadirlo de nuevo y evitar así crear muelles de más)
            // En caso de no estar repetido, se crea un muelle cuyos nodos son los de los
            // índices del tetraedro i (Combinaciones: 0 y 1, 0 y 2, 0 y 3, 1 y 2, 1 y 3 ó 2 y 3; los cuatro
            // vértices del tetraedro)
            if (!CheckRepeatedSpring(nodes[indexTetra[i, 0]], nodes[indexTetra[i, 1]], springs))
            {
                springs.Add(new Spring(nodes[indexTetra[i, 0]], nodes[indexTetra[i, 1]], dDeformation, dRotation, k));
            }
            if (!CheckRepeatedSpring(nodes[indexTetra[i, 0]], nodes[indexTetra[i, 2]], springs))
            {
                springs.Add(new Spring(nodes[indexTetra[i, 0]], nodes[indexTetra[i, 2]], dDeformation, dRotation, k));
            }
            if (!CheckRepeatedSpring(nodes[indexTetra[i, 0]], nodes[indexTetra[i, 3]], springs))
            {
                springs.Add(new Spring(nodes[indexTetra[i, 0]], nodes[indexTetra[i, 3]], dDeformation, dRotation, k));
            }
            if (!CheckRepeatedSpring(nodes[indexTetra[i, 1]], nodes[indexTetra[i, 2]], springs))
            {
                springs.Add(new Spring(nodes[indexTetra[i, 1]], nodes[indexTetra[i, 2]], dDeformation, dRotation, k));
            }
            if (!CheckRepeatedSpring(nodes[indexTetra[i, 1]], nodes[indexTetra[i, 3]], springs))
            {
                springs.Add(new Spring(nodes[indexTetra[i, 1]], nodes[indexTetra[i, 3]], dDeformation, dRotation, k));
            }
            if (!CheckRepeatedSpring(nodes[indexTetra[i, 2]], nodes[indexTetra[i, 3]], springs))
            {
                springs.Add(new Spring(nodes[indexTetra[i, 2]], nodes[indexTetra[i, 3]], dDeformation, dRotation, k));
            }
        }
    }

    /// <summary>
    /// Función que se ejecuta una vez por fotograma
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

    /// <summary>
    /// Función que se ejecuta en bucle (para físicas)
    /// </summary>
    private void FixedUpdate()
    {
        // Si el estado actual de este objeto es pausado o "paused",
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
                // Se realizan los cálculos referentes al Euler Simpléctico
                IntegrateVerlet();
                break;

            default:
                // En caso de no tener elegida ninguna de las opciones programas
                // se consideraría una opción errónea
                Debug.LogError("ERROR, MÉTODO DE INTEGRACIÓN NO VÁLIDO.");
                break;
        }

        // Actualizamos las variables de los muelles
        foreach (Spring spring in springs)
        {
            spring.dir = spring.nodeA.pos - spring.nodeB.pos;               // Vector dirección del muelle
            spring.length = spring.dir.magnitude;                           // Longitud natural del muelle
            spring.dir = Vector3.Normalize(spring.dir);                     // Longitud actual del muelle normalizado
            spring.pos = (spring.nodeA.pos + spring.nodeB.pos) / 2f;        // Posición actual del muelle
            spring.rot = Quaternion.FromToRotation(Vector3.up, spring.dir); // Rotación actual del muelle
        }

        // Recorremos los vértices para actualizarlos
        for (int i = 0; i < vertices.Length; i++)
        {
            // Actualizamos los vértices en función de los pesos de los cuatro vértices del tetraedro
            // Multiplicamos el peso 0 por el nodo 0 e invertimos el punto a coordenadas locales del objeto
            vertices[i] = transform.InverseTransformPoint(weights[i, 0] * tetrahedrons[vertsAndTetraRelation[i]].nodes[0].pos);
            // Multiplicamos el peso 1 por el nodo 1 e invertimos el punto a coordenadas locales del objeto
            vertices[i] += transform.InverseTransformPoint(weights[i, 1] * tetrahedrons[vertsAndTetraRelation[i]].nodes[1].pos);
            // Multiplicamos el peso 2 por el nodo 2 e invertimos el punto a coordenadas locales del objeto
            vertices[i] += transform.InverseTransformPoint(weights[i, 2] * tetrahedrons[vertsAndTetraRelation[i]].nodes[2].pos);
            // Multiplicamos el peso 3 por el nodo 3 e invertimos el punto a coordenadas locales del objeto
            vertices[i] += transform.InverseTransformPoint(weights[i, 3] * tetrahedrons[vertsAndTetraRelation[i]].nodes[3].pos);
        }

        // Actualizamos los vértices de la malla igualandolos al array de vértices
        mesh.vertices = vertices;

        // Después de modificar los vértices llamamos a esta función para asegurarnos que el volumen es correcto
        mesh.RecalculateBounds();


        mesh.RecalculateNormals();

        if (meshCollider)
        {
            meshCollider.sharedMesh = mesh;
        }
    }

    /// <summary>
    /// Función que muestra el número de nodos, muelles de tracción y muelles de flexión 
    /// encontrados y generados para la malla.
    /// </summary>
    void InitDebug()
    {
        for (int i = 0; i < tetrahedrons.Count; i++)
        {
            Debug.Log("Tetraedro " + (i + 1) + ": (" + tetrahedrons[i].ToString() + ")");
        }
        Debug.Log("Número de <color=orange>nodos</color>: <color=cyan>" + nodes.Count + "</color>");
        Debug.Log("Número de <color=orange>muelles</color>: <color=cyan>" + springs.Count + "</color>");
        Debug.Log("Número de <color=orange>tetraedros</color>: <color=cyan>" + tetrahedrons.Count + "</color>");
    }

    /// <summary>
    /// Función que realiza los cálculos para el método del Euler Explicito.
    /// </summary>
    void IntegrateExplicitEuler()
    {
        // Se recorre cada nodo de la lista "nodes"
        foreach (Node node in nodes)
        {
            Vector2 aDir = Vector2.Perpendicular(new Vector2(direction.x, direction.y)); // Calculamos la perpendicular al viento
            float aS = Random.Range(-randomStrength, randomStrength) * strength;         // Multiplicamos la fuerza del viento por la componente aleatoria
            Vector3 aDir3D = new Vector3(aDir.x, aDir.y, 0);                             // Transformamos la perpendicular a un Vector de 3 dimensiones

            if (!node.fixedNode)
            {
                node.pos += h * node.vel;           // Actualizamos la posición del nodo multiplicando el paso por
                                                    // la velocidad del nodo
                node.force = node.mass * g;         // Añadimos la fuerza de la gravedad
                node.force += direction * strength; // Fuerza del viento
                node.force += aDir3D * aS;          // Fuerza aleatoria
                ApplyDumpingNode(node);             // Aplicamos la fuerza de amortiguamiento al nodo
            }
        }

        float v; // Inicializamos una variable auxiliar para el volumen

        // Actualizamos los muelles
        foreach (Spring spring in springs)
        {
            v = 0; // Reiniciamos el valor del volumen (para el muelle siguiente)
            // Recorremos los tetraedros
            for (int j = 0; j < indexTetra.GetLength(0); j++)
            {
                // Comprobamos si los nodos A y B del muelle se pertenecen ambos al tetraedro j
                if (tetrahedrons[j].CheckNode(spring.nodeA) && tetrahedrons[j].CheckNode(spring.nodeB))
                {
                    // Incrementamos el volumen en función de los tetraedros
                    v += CalculateVolume(verticesTetra[indexTetra[j, 0]],
                                         verticesTetra[indexTetra[j, 1]],
                                         verticesTetra[indexTetra[j, 2]],
                                         verticesTetra[indexTetra[j, 3]]) / 6;
                }
            }
            // Fuerza elástica del nodo A
            spring.nodeA.force -= CalculateElasticForce(spring, v);

            // Fuerza elástica del nodo B
            spring.nodeB.force += CalculateElasticForce(spring, v);

            // Aplicamos el amortiguamiento al muelle
            ApplyDumpingSpring(spring);
        }

        // Actualizamos los Nodos
        foreach (Node node in nodes)
        {
            // Comprobamos si el nodo es fijo. Si no lo es, 
            // actualizamos la velocidad y la posición
            if (!node.fixedNode)
            {
                node.vel += h * node.force / node.mass; // Actualizamos la velocidad del nodo multiplicando el paso 
                                                        // por la fuerza del nodo entre su masa
            }
        }
    }

    /// <summary>
    /// Función que realiza los cálculos para el método del Euler Simpléctico.
    /// </summary>
    void IntegrateSymplecticEuler()
    {
        // Se recorre cada nodo de la lista "nodes"
        foreach (Node node in nodes)
        {
            Vector2 aDir = Vector2.Perpendicular(new Vector2(direction.x, direction.y)); // Calculamos la perpendicular al viento
            float aS = Random.Range(-randomStrength, randomStrength) * strength;         // Multiplicamos la fuerza del viento por la componente aleatoria
            Vector3 aDir3D = new Vector3(aDir.x, aDir.y, 0);                             // Transformamos la perpendicular a un Vector de 3 dimensiones

            // Se comprueba si el nodo está colisionando con el jugador
            if (playerCollisions && node.CheckCollisionPlayer(playerReference.collider, transform.InverseTransformPoint(node.pos)))
            {
                // Se suma la masa del personaje al nodo (ahora pesa más el nodo) y se calcula
                // la fuerza de la gravedad
                node.force = (node.mass+playerReference.weight) * g;
            }
            else
            {
                // Se calcula la fuerza de la gravedad
                node.force = node.mass * g;
            }

                
            node.force += direction * strength; // Fuerza del viento
            node.force += aDir3D * aS;          // Fuerza de la componente aleatoria del viento
        }

        float v; // Inicializamos una variable auxiliar para el volumen

        // Actualizamos los muelles
        foreach (Spring spring in springs)
        {
            v = 0; // Reiniciamos el valor del volumen (para el muelle siguiente)
            // Recorremos los tetraedros
            for (int j = 0; j < indexTetra.GetLength(0); j++)
            {
                // Comprobamos si los nodos A y B del muelle se pertenecen ambos al tetraedro j
                if (tetrahedrons[j].CheckNode(spring.nodeA) && tetrahedrons[j].CheckNode(spring.nodeB))
                {
                    // Incrementamos el volumen en función de los tetraedros
                    v += CalculateVolume(verticesTetra[indexTetra[j, 0]],
                                         verticesTetra[indexTetra[j, 1]],
                                         verticesTetra[indexTetra[j, 2]],
                                         verticesTetra[indexTetra[j, 3]]) / 6;
                }
            }

            // Fuerza elástica del nodo A
            spring.nodeA.force -= CalculateElasticForce(spring, v);

            // Fuerza elástica del nodo B
            spring.nodeB.force += CalculateElasticForce(spring, v);

            // Aplicamos el amortiguamiento al muelle
            ApplyDumpingSpring(spring);
        }

        // Actualizamos los Nodos
        foreach (Node node in nodes)
        {
            // Comprobamos si el nodo es fijo. Si no lo es, 
            // actualizamos la velocidad y la posición
            if (!node.fixedNode)
            {
                node.vel += h * node.force / node.mass; // Actualizamos la velocidad del nodo multiplicando el paso 
                                                        // por la fuerza del nodo entre su masa

                foreach (BoxCollider col in colliders)
                {
                    if (col)
                    {
                        node.CheckCollision(col, transform.InverseTransformPoint(node.pos), permanentCollision);
                    }
                }
                node.pos += h * node.vel;        // Actualizamos la posición del nodo multiplicando el paso por
                                                 // la velocidad del nodo
            }
        }
        numIteraciones++;
    }

    /// <summary>
    /// Función que realiza los cálculos para el método de Verlet.
    /// </summary>
    void IntegrateVerlet()
    {
        Vector3 auxPos = new Vector3(0, 0, 0);
        // Se recorre cada nodo de la lista "nodes"
        foreach (Node node in nodes)
        {
            Vector2 aDir = Vector2.Perpendicular(new Vector2(direction.x, direction.y)); // Calculamos la perpendicular al viento
            float aS = Random.Range(-randomStrength, randomStrength) * strength;         // Multiplicamos la fuerza del viento por la componente aleatoria
            Vector3 aDir3D = new Vector3(aDir.x, aDir.y, 0);                             // Transformamos la perpendicular a un Vector de 3 dimensiones


            node.force = node.mass * g;         // Fuerza de la gravedad
            node.force += direction * strength; // Fuerza del viento
            node.force += aDir3D * aS;          // Fuerza aleatoria (o de la componente aleatoria)
            ApplyDumpingNode(node);             // Aplicamos el amortiguamiento
        }

        float v; // Inicializamos una variable auxiliar para el volumen

        // Actualizamos los muelles
        foreach (Spring spring in springs)
        {
            v = 0; // Reiniciamos el valor del volumen (para el muelle siguiente)
            // Recorremos los tetraedros
            for (int j = 0; j < indexTetra.GetLength(0); j++)
            {
                // Comprobamos si los nodos A y B del muelle se pertenecen ambos al tetraedro j
                if (tetrahedrons[j].CheckNode(spring.nodeA) && tetrahedrons[j].CheckNode(spring.nodeB))
                {
                    // Incrementamos el volumen en función de los tetraedros
                    v += CalculateVolume(verticesTetra[indexTetra[j, 0]],
                                         verticesTetra[indexTetra[j, 1]],
                                         verticesTetra[indexTetra[j, 2]],
                                         verticesTetra[indexTetra[j, 3]]) / 6;
                }
            }

            // Fuerza elástica del nodo A
            spring.nodeA.force -= CalculateElasticForce(spring, v);

            // Fuerza elástica del nodo B
            spring.nodeB.force += CalculateElasticForce(spring, v);

            // Aplicamos el amortiguamiento al muelle
            ApplyDumpingSpring(spring);
        }

        // Actualizamos los Nodos
        foreach (Node node in nodes)
        {
            if (!node.fixedNode)
            {
                auxPos = node.pos;  // Guardamos la posición actual en una variable auxiliar (Rn-1)
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
    /// Calcula la fuerza elástica de un muelle en función del volumen.
    /// </summary>
    /// <param name="spring">Muelle del que se desea calcular la fuerza elástica</param>
    /// <param name="v">Volumen del tetraedro</param>
    /// <returns></returns>
    Vector3 CalculateElasticForce(Spring spring, float v)
    {
        return (v / Mathf.Pow(spring.length0, 2)) * spring.k * (spring.length - spring.length0) * ((spring.nodeA.pos - spring.nodeB.pos) / spring.length0);
    }

    /// <summary>
    /// Función que aplica el amortiguamiento a los nodos.
    /// </summary>
    /// <param name="node">Nodo al que se le quiere aplicar el amortiguamiento</param>
    void ApplyDumpingNode(Node node)
    {
        // Se actualiza la fuerza del nodo teniendo en cuenta "dAbsolute"
        node.force += -node.dAbsolute * node.vel;
    }

    /// <summary>
    /// Función que aplica el amortiguamiento a los muelles.
    /// </summary>
    /// <param name="spring">Muelle al que se le quiere aplicar el amortiguamiento</param>
    void ApplyDumpingSpring(Spring spring)
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
    /// <param name="nodeA">Nodo A del muelle</param>
    /// <param name="nodeB">Nodo B del muelle</param>
    /// <param name="springs">Lista de todos los muelles</param>
    /// <returns></returns>
    bool CheckRepeatedSpring(Node nodeA, Node nodeB, List<Spring> springs)
    {
        bool isRepeated = false;

        // Se recorre cada muelle
        foreach (Spring spring in springs)
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

    /// <summary>
    /// Anula todos los fixers que sostienen al objeto
    /// </summary>
    public void ResetFixed()
    {
        // Se recorre la lista de nodos, haciendo que ninguno sea fijo
        foreach (Node node in nodes)
        {
            node.fixedNode = false;
        }
    }

    #region Métodos del Editor
    /// <summary>
    /// Pausa las físicas actualizando el booleano "paused" a true.
    /// </summary>
    public void Pause()
    {
        paused = true;
    }

    /// <summary>
    /// Reanuda las físicas actualizando el booleano "paused" a false.
    /// </summary>
    public void Unpause()
    {
        paused = false;
    }

    /// <summary>
    /// Resetea los atributos de la clase a unos valores por defecto.
    /// </summary>
    /// <param name="g">Gravedad</param>
    /// <param name="h">Paso</param>
    /// <param name="densidad">Densidad</param>
    /// <param name="k">Constante de rigidez</param>
    /// <param name="dRotation">Amortiguamiento de los muelles (rotación)</param>
    /// <param name="dDeformation">Amortiguamiento de los muelles</param>
    /// <param name="dAbsolute">Amortiguamiento de los nodos</param>
    /// <param name="index">Índice que determina el método de integración deseado</param>
    public void ResetOriginalValues(Vector3 g, float h, float densidad, float k, float dRotation, float dDeformation, float dAbsolute, int index)
    {
        this.g = g;
        this.h = h;
        this.density = densidad;
        this.k = k;
        this.dRotation = dRotation;
        this.dDeformation = dDeformation;
        this.dAbsolute = dAbsolute;
        switch (index)
        {
            case 1:
                integrationMethod = Integration.ExplicitEuler;   // El valor 1 equivale al Euler Explícito
                break;
            case 2:
                integrationMethod = Integration.SymplecticEuler; // El valor 2 equivale al Euler Simpléctico
                break;
            default:
                Debug.Log("Método de integración no válido.");   // Cualquier otro valor no está aceptado
                break;
        }
    }
    #endregion

    /// <summary>
    /// Función para dibujar los muelles y mostrarlos por pantalla.
    /// </summary>
    public void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(0.8f, 0.2f, 0, 1);
        foreach (Spring spring in springs)
        {
            Gizmos.DrawLine(spring.nodeA.pos, spring.nodeB.pos);
        }

        foreach (Node n in nodes)
        {
            Gizmos.DrawSphere(n.pos, 0.1f);
        }
    }
}

