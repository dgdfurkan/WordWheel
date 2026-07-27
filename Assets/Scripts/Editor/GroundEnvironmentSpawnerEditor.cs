using UnityEditor;
using UnityEngine;
using WordWheel.Runtime.Controllers;

namespace WordWheel.Editor
{
    [CustomEditor(typeof(GroundEnvironmentSpawner))]
    public class GroundEnvironmentSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GroundEnvironmentSpawner spawner = (GroundEnvironmentSpawner)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Edit Mode Spawner Preview", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.3f, 0.9f, 0.4f);
            if (GUILayout.Button("Generate Preview Spawns", GUILayout.Height(30)))
            {
                EditorApplication.delayCall += () =>
                {
                    if (spawner != null)
                    {
                        spawner.GeneratePreviewSpawns();
                        SceneView.RepaintAll();
                    }
                };
            }

            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("Clear Preview Spawns", GUILayout.Height(25)))
            {
                EditorApplication.delayCall += () =>
                {
                    if (spawner != null)
                    {
                        spawner.ClearPreviewSpawns();
                        SceneView.RepaintAll();
                    }
                };
            }

            GUI.backgroundColor = Color.white;
        }
    }
}
