using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MassSpring2D))]
public class MassSpring2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MassSpring2D massSpring = (MassSpring2D)target;
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
        // GUILayout.Space(1.76f);
        if (GUILayout.Button("Configuration 1", GUILayout.Height(30)))
        {
            massSpring.ResetOriginalValues(/*g*/ new Vector3(0, -9.8f, 0), /*h*/ 0.01f, /*k_T*/ 2000,
                                           /*k_F*/ 850, /*dRotation*/ 0.8f, /*dDeformation*/ 0.8f, /*mass*/ 0.95f,
                                           /*dAbsolute*/ 0.01f, /*strength*/ 60,/*randomStrength*/ 8f,
                                           /*windDir*/ new Vector3(-0.5f, 0.11f, 0.11f), /*Integration Method*/ 2);
        }

        if (GUILayout.Button("Configuration 2", GUILayout.Height(30)))
        {
            massSpring.ResetOriginalValues(/*g*/ new Vector3(0, -9.8f, 0), /*h*/ 0.01f, /*k_T*/ 4000,
                                           /*k_F*/ 1600, /*dRotation*/ 0.8f, /*dDeformation*/ 0.8f, /*mass*/ 1.96f,
                                           /*dAbsolute*/ 0.01f, /*strength*/ 112,/*randomStrength*/ 7.7f,
                                           /*windDir*/ new Vector3(-0.33f, 0, 0), /*Integration Method*/ 2);
        }

        if (GUILayout.Button("Configuration 3", GUILayout.Height(30)))
        {
            massSpring.ResetOriginalValues(/*g*/ new Vector3(0, -9.8f, 0), /*h*/ 0.01f, /*k_T*/ 2000,
                                           /*k_F*/ 1200, /*dRotation*/ 0.8f, /*dDeformation*/ 0.8f, /*mass*/ 1.96f,
                                           /*dAbsolute*/ 0.01f, /*strength*/ 112,/*randomStrength*/ 3.6f,
                                           /*windDir*/ new Vector3(-0.42f, 0, 0), /*Integration Method*/ 2);
        }

        if (GUILayout.Button("Configuration 4", GUILayout.Height(30)))
        {
            massSpring.ResetOriginalValues(/*g*/ new Vector3(0, -9.8f, 0), /*h*/ 0.01f, /*k_T*/ 2000,
                                           /*k_F*/ 800, /*dRotation*/ 0.8f, /*dDeformation*/ 0.8f, /*mass*/ 1.96f,
                                           /*dAbsolute*/ 0.01f, /*strength*/ 112,/*randomStrength*/ 7.8f,
                                           /*windDir*/ new Vector3(-0.33f, 0, 0), /*Integration Method*/ 2);
        }
        if (GUILayout.Button("Configuration 5", GUILayout.Height(30)))
        {
            massSpring.ResetOriginalValues(/*g*/ new Vector3(0, -9.8f, 0), /*h*/ 0.011f, /*k_T*/ 50,
                                           /*k_F*/ 50, /*dRotation*/ 0.9f, /*dDeformation*/ 0.9f, /*mass*/ 0.95f,
                                           /*dAbsolute*/ 0.01f, /*strength*/ 60,/*randomStrength*/ 5f,
                                           /*windDir*/ new Vector3(-0.02f, 0.155F, 0), /*Integration Method*/ 1);
        }

        if (GUILayout.Button("Configuration 6", GUILayout.Height(30)))
        {
            massSpring.ResetOriginalValues(/*g*/ new Vector3(0, -9.8f, 0), /*h*/ 0.01f, /*k_T*/ 2000,
                                           /*k_F*/ 850, /*dRotation*/ 0.8f, /*dDeformation*/ 0.8f, /*mass*/ 0.95f,
                                           /*dAbsolute*/ 0.01f, /*strength*/ 54,/*randomStrength*/ 6.5f,
                                           /*windDir*/ new Vector3(-0.5f, 0.11f, 0.11f), /*Integration Method*/ 2);
        }
        EditorGUILayout.HelpBox("Hay cambios que requieren de reiniciar la ejecución (como alterar el valor de la masa, cambiar el método de integración, etc.).", MessageType.Warning);
    }
}
