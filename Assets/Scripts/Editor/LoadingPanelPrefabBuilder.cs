using Runtime.UI.Panels;
using Runtime.UI.Panels.Loading;
using UnityEditor;
using UnityEngine;

namespace Runtime.UI.Editor
{
    public static class LoadingPanelPrefabBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/UI/LoadingPanel.prefab";

        [InitializeOnLoadMethod]
        private static void AutoBuildOnCompile()
        {
            EditorApplication.delayCall += TryAutoBuildPrefab;
        }

        private static void TryAutoBuildPrefab()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefabAsset == null)
            {
                return;
            }

            LoadingPanelView view = prefabAsset.GetComponent<LoadingPanelView>();
            if (view != null)
            {
                view.CaptureReferences();
                if (view.IsBuilt && !view.HasLegacyTextLayout())
                {
                    return;
                }
            }

            BuildPrefabAsset();
        }

        [MenuItem("WordWheel/UI/Rebuild Loading Panel Prefab (Image Pieces)")]
        public static void RebuildPrefabAsset()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            if (root == null)
            {
                Debug.LogError($"[LoadingPanelPrefabBuilder] Prefab not found at {PrefabPath}");
                return;
            }

            LoadingPanelView view = root.GetComponent<LoadingPanelView>();
            if (view != null)
            {
                Transform logoAssembly = root.transform.Find("LogoAssembly");
                DestroyGroup(logoAssembly, "WordGroup");
                DestroyGroup(logoAssembly, "WheelLabelGroup");
            }

            BuildOnRoot(root);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LoadingPanelPrefabBuilder] LoadingPanel prefab rebuilt with image pieces.");
        }

        private static void DestroyGroup(Transform parent, string groupName)
        {
            if (parent == null)
            {
                return;
            }

            Transform group = parent.Find(groupName);
            if (group != null)
            {
                Object.DestroyImmediate(group.gameObject);
            }
        }

        [MenuItem("WordWheel/UI/Build Loading Panel Hierarchy")]
        public static void BuildSelected()
        {
            foreach (Object selected in Selection.objects)
            {
                if (selected is GameObject gameObject)
                {
                    BuildOnRoot(gameObject);
                }
            }
        }

        [MenuItem("WordWheel/UI/Build Loading Panel Prefab Asset")]
        public static void BuildPrefabAsset()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            if (root == null)
            {
                Debug.LogError($"[LoadingPanelPrefabBuilder] Prefab not found at {PrefabPath}");
                return;
            }

            BuildOnRoot(root);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LoadingPanelPrefabBuilder] LoadingPanel prefab built and saved.");
        }

        private static void BuildOnRoot(GameObject root)
        {
            RectTransform rect = root.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localScale = Vector3.one;
            }

            if (root.GetComponent<CanvasGroup>() == null)
            {
                root.AddComponent<CanvasGroup>();
            }

            LoadingPanelView view = root.GetComponent<LoadingPanelView>();
            if (view == null)
            {
                view = root.AddComponent<LoadingPanelView>();
            }

            if (root.GetComponent<LoadingPanel>() == null)
            {
                root.AddComponent<LoadingPanel>();
            }

            LoadingPanelViewBuilder.EnsureBuilt(root.transform, view);
            EditorUtility.SetDirty(root);
        }
    }
}
