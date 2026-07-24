using Runtime.UI.Core;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Runtime.UI.Editor
{
    [CustomEditor(typeof(UIPanel), true)]
    public class UIPanelOdinEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            UIPanel panel = (UIPanel)target;

            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Animation Studio", GUILayout.Height(28f)))
            {
                UIPanelAnimationStudioWindow.OpenWithPanel(panel);
            }

            if (GUILayout.Button("Add Selected To Group", GUILayout.Height(28f)))
            {
                UIPanelAnimationEditorUtility.AddSelectionToNewGroup(panel);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(6f);

            base.OnInspectorGUI();
        }
    }
}
