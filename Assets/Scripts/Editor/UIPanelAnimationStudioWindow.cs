using Runtime.UI.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Runtime.UI.Editor
{
    public class UIPanelAnimationStudioWindow : OdinEditorWindow
    {
        [MenuItem("Tools/UI/Animation Studio")]
        private static void OpenEmpty()
        {
            GetWindow<UIPanelAnimationStudioWindow>("UI Animation Studio").Show();
        }

        public static void OpenWithPanel(UIPanel panel)
        {
            UIPanelAnimationStudioWindow window = GetWindow<UIPanelAnimationStudioWindow>("UI Animation Studio");
            window.TargetPanel = panel;
            window.Show();
            window.Focus();
        }

        [Title("Target Panel")]
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        [SerializeField] private UIPanel targetPanel;

        public UIPanel TargetPanel
        {
            get => targetPanel;
            set => targetPanel = value;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        protected override void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            base.OnDisable();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (targetPanel == null)
            {
                return;
            }

            UIPanelAnimationEditorUtility.DrawPanelGizmos(targetPanel);
        }

        [Button("Frame Panel In Scene", ButtonSizes.Medium)]
        [EnableIf(nameof(HasTargetPanel))]
        private void FramePanel()
        {
            if (targetPanel == null)
            {
                return;
            }

            Selection.activeGameObject = targetPanel.gameObject;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        [Button("Add Selected UI To New Group", ButtonSizes.Medium), GUIColor(0.45f, 0.85f, 1f)]
        [EnableIf(nameof(HasTargetPanel))]
        private void AddSelection()
        {
            UIPanelAnimationEditorUtility.AddSelectionToNewGroup(targetPanel);
        }

        private bool HasTargetPanel => targetPanel != null;
    }
}
