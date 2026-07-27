#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Runtime.UI.Editor
{
    /// <summary>
    /// Clears Inspector selection when exiting Play Mode so destroyed runtime UI
    /// objects (e.g. OverlayScrim) do not spam SerializedObject errors.
    /// </summary>
    [InitializeOnLoad]
    public static class UIRuntimeSelectionGuard
    {
        static UIRuntimeSelectionGuard()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }

            Object active = Selection.activeObject;
            if (active == null)
            {
                return;
            }

            if (active is GameObject || active is Component)
            {
                Selection.activeObject = null;
            }
        }
    }
}
#endif
