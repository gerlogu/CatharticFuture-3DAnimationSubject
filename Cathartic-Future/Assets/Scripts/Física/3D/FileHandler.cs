using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileHandler
{
    TextAsset nodeTXT; // Fichero .node.txt
    TextAsset eleTXT;  // Fichero .ele.txt

    /// <summary>
    /// Constructor del controlador de archivos (File Handler)
    /// </summary>
    public FileHandler(TextAsset nodeTXT, TextAsset eleTXT)
    {
        this.nodeTXT = nodeTXT;
        this.eleTXT = eleTXT;
    }

    #region Ficheros
    /// <summary>
    /// Función que lee el fichero .ele
    /// </summary>
    public int[,] Read_ele()
    {
        string[] strings = eleTXT.ToString().Split('\n');     // Array auxiliar para contener las líneas (cadenas de caracteres) 
                                                              // del documento al completo

        string[] auxStrings;                                  // Array auxiliar para contener las palabras (valores) de una línea

        int cont = 0;                                         // Contador auxiliar

        int auxInt = 0;                                       // Auxiliar al que le restaremos 1 para que los índices comiencen por 0 en lugar de por 1
                                                              // y simplificar así los accesos

        // Inicializamos el array auxiliar de índices de los vértices de los tetraedros
        // Las filas del array es igual al número de índices contenido en el fichero mientras que,
        // por otro lado, el número de columnas es igual 4 (cuatro vértices por tetraedro)
        int[,] auxIndexTetra = new int[strings.Length - 2, 4];

        // Recorremos las líneas con contenido de interés (excluimos la primera y última línea)
        for (int i = 1; i < strings.Length - 1; i++)
        {
            auxStrings = strings[i].Split(' ');      // Almacenamos en el Array todas las palabras (valores) de la línea

            int.TryParse(auxStrings[1], out auxInt); // Convertimos el String en un Int y lo almacenamos en auxInt

            auxIndexTetra[cont, 0] = auxInt - 1;     // Le restamos uno al auxiliar (para que el índice primero sea 0)
                                                     // y añadimos su valor a la posición 0 del tetraedro "cont"

            int.TryParse(auxStrings[2], out auxInt); // Convertimos el String en un Int y lo almacenamos en auxInt

            auxIndexTetra[cont, 1] = auxInt - 1;     // Le restamos uno al auxiliar (para que el índice primero sea 0)
                                                     // y añadimos su valor a la posición 1 del tetraedro "cont"

            int.TryParse(auxStrings[3], out auxInt); // Convertimos el String en un Int y lo almacenamos en auxInt

            auxIndexTetra[cont, 2] = auxInt - 1;     // Le restamos uno al auxiliar (para que el índice primero sea 0)
                                                     // y añadimos su valor a la posición 2 del tetraedro "cont"

            int.TryParse(auxStrings[4], out auxInt); // Convertimos el String en un Int y lo almacenamos en auxInt

            auxIndexTetra[cont, 3] = auxInt - 1;     // Le restamos uno al auxiliar (para que el índice primero sea 0)
                                                     // y añadimos su valor a la posición 3 del tetraedro "cont"

            cont++;                                  // Incrementamos el contador en 1
        }
        return auxIndexTetra; // Devolvemos el array resultante
    }

    /// <summary>
    /// Función que lee el fichero .node
    /// </summary>
    public Vector3[] Read_node()
    {
        string[] strings = nodeTXT.ToString().Split('\n'); // Array auxiliar para contener las líneas (cadenas de caracteres) 
                                                           // del documento al completo

        string[] auxStrings;                               // Array auxiliar para contener las palabras (valores) de una línea

        // Inicializamos el array auxiliar de las posiciones de los vértices de los tetraedros
        // cuya longitud es igual al número de lineas del fichero menos la primera y última
        // línea, las cuales no contienen datos de utilidad
        Vector3[] auxVerticesTetra = new Vector3[strings.Length - 2];

        // Recorremos las líneas con contenido de interés (excluimos la primera y última línea)
        for (int i = 1; i < strings.Length - 1; i++)
        {
            float aux = 0;
            auxStrings = strings[i].Split(' '); // Almacenamos en el Array todas las palabras (valores) de la línea
            // Usamos los valores de los índices 1, 2 y 3 (no utilizamos el 0 ya que queremos evitar dicho valor, no es necesario)
            float.TryParse(auxStrings[1], out aux); // El valor 1 equivale a la posición en la coordenada x
            auxVerticesTetra[i - 1].x = -aux;
            float.TryParse(auxStrings[2], out aux); // El valor 2 equivale a la posición en la coordenada z (¡Diferencia de ejes!)
            auxVerticesTetra[i - 1].z = -aux;
            float.TryParse(auxStrings[3], out auxVerticesTetra[i - 1].y); // El valor 3 equivale a la posición en la coordenada y (¡Diferencia de ejes!)                                    
        }
        return auxVerticesTetra; // Devolvemos el array resultante
    }
    #endregion
}
