using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MassSpring))]
public class MassSpringEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MassSpring massSpring = (MassSpring)target;
        GUILayout.Space(10); // Espacio de línea
        EditorGUILayout.LabelField("Other Settings", EditorStyles.boldLabel); // Título de la sección
        GUILayout.Space(2.75f);
        EditorGUILayout.LabelField("State"); // Nombre de la subsección 1 (el estado de las físicas, pausadas o en ejecución)
        GUILayout.Space(-7);
        EditorGUILayout.LabelField("--------------"); // Subrayado básico
        GUILayout.Space(2.75f);

        GUILayout.BeginHorizontal(); // Botones para pausar y reanudar la ejecución de la animación
        if (GUILayout.Button("Play", GUILayout.Height(35)))
        {

            massSpring.Unpause();

        }

        if (GUILayout.Button("Pause", GUILayout.Height(35)))
        {

            massSpring.Pause();

        }
        GUILayout.EndHorizontal();
        GUILayout.Space(7.5f);
        if (massSpring.paused)
        {
            EditorGUILayout.LabelField("Current State: -= Paused =-");
        }
        else
        {
            EditorGUILayout.LabelField("Current State: -= Playing =-");
        }
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Reset Values"); // Nombre de la subsección 2 (Reseteo de valores, 
                                                    // se pueden elegir 4 configuraciones predeterminadas, 
                                                    // a gusto del desarrollador)
        GUILayout.Space(-7);
        EditorGUILayout.LabelField("--------------"); // Subrayado básico
        GUILayout.Space(2.75f);
        if (GUILayout.Button("Configuration 1", GUILayout.Height(30)))
        {
            massSpring.ResetOriginalValues(/*g*/ new Vector3(0, -9.8f, 0), /*h*/ 0.017f,  /*densidad*/ 0.05f,
                                           /*k*/ 260, /*dRotation*/ 0.1f, /*dDeformation*/ 0.1f, 
                                           /*dAbsolute*/ 0.01f, /*Integration Method*/ 2);
        }
        if (GUILayout.Button("Configuration 2", GUILayout.Height(30)))
        {
            massSpring.ResetOriginalValues(/*g*/ new Vector3(0, -9.8f, 0), /*h*/ 0.017f,  /*densidad*/ 0.05f,
                                           /*k*/ 500, /*dRotation*/ 1f, /*dDeformation*/ 1f,
                                           /*dAbsolute*/ 0.01f, /*Integration Method*/ 2);
        }
        if (GUILayout.Button("Configuration 3", GUILayout.Height(30)))
        {
            massSpring.ResetOriginalValues(/*g*/ new Vector3(0, -9.8f, 0), /*h*/ 0.017f,  /*densidad*/ 0.1f,
                                           /*k*/ 510, /*dRotation*/ 0.9f, /*dDeformation*/ 0.9f,
                                           /*dAbsolute*/ 0.01f, /*Integration Method*/ 2);
        }
        EditorGUILayout.HelpBox("Hay cambios que requieren de reiniciar la ejecución (como alterar el valor de la densidad, cambiar el método de integración, etc.).", MessageType.Warning);
    }
}
