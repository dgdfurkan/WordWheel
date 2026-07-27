using Runtime.UI.Panels.Loading;
using UnityEditor;
using UnityEngine;

namespace Runtime.UI.Editor
{
    public static class LoadingPanelLayoutEditorUtility
    {
        [MenuItem("WordWheel/UI/Loading Panel/Apply Layout JSON To Prefab")]
        public static void ApplyLayoutJsonToPrefab()
        {
            LoadingPanelLayoutSnapshot snapshot = LoadingPanelLayoutUtility.LoadFromJson();
            if (snapshot == null)
            {
                EditorUtility.DisplayDialog(
                    "Loading Panel Layout",
                    $"Layout file not found:\n{LoadingPanelLayoutUtility.DefaultJsonPath}\n\nCapture in Play Mode with F9 first.",
                    "OK");
                return;
            }

            const string prefabPath = "Assets/Prefabs/UI/LoadingPanel.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            LoadingPanelView view = root.GetComponent<LoadingPanelView>();
            if (view == null)
            {
                PrefabUtility.UnloadPrefabContents(root);
                EditorUtility.DisplayDialog("Loading Panel Layout", "LoadingPanelView not found on prefab.", "OK");
                return;
            }

            view.CaptureReferences();
            LoadingPanelLayoutUtility.Apply(view, snapshot);
            view.SetSavedLayout(snapshot);

            EditorUtility.SetDirty(view);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[LoadingPanelLayout] Applied layout JSON to LoadingPanel prefab.");
        }

        [MenuItem("WordWheel/UI/Loading Panel/Open Layout JSON")]
        public static void OpenLayoutJson()
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(LoadingPanelLayoutUtility.DefaultJsonPath);
            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset);
                return;
            }

            EditorUtility.DisplayDialog(
                "Loading Panel Layout",
                $"No layout file yet at:\n{LoadingPanelLayoutUtility.DefaultJsonPath}",
                "OK");
        }
    }
}
