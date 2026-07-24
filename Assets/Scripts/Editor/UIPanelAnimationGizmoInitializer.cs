using Runtime.UI.Core;
using UnityEditor;

namespace Runtime.UI.Editor
{
    [InitializeOnLoad]
    public static class UIPanelAnimationGizmoInitializer
    {
        static UIPanelAnimationGizmoInitializer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            UIPanel selectedPanel = Selection.activeGameObject?.GetComponent<UIPanel>();
            if (selectedPanel == null)
            {
                selectedPanel = Selection.activeGameObject?.GetComponentInParent<UIPanel>();
            }

            if (selectedPanel == null)
            {
                return;
            }

            UIPanelAnimationEditorUtility.DrawPanelGizmos(selectedPanel);
        }
    }
}
