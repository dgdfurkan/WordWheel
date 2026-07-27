using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Runtime.UI.Panels.Loading
{
    public static class LoadingPanelLayoutUtility
    {
        public const string DefaultJsonPath = "Assets/Prefabs/UI/LoadingPanel.layout.json";

        public static LoadingPanelLayoutSnapshot Capture(LoadingPanelView view)
        {
            if (view == null || view.LogoAssembly == null)
            {
                return new LoadingPanelLayoutSnapshot();
            }

            List<LoadingPanelElementLayout> elements = new List<LoadingPanelElementLayout>();
            CaptureTransform(view.LogoAssembly, string.Empty, elements);

            return new LoadingPanelLayoutSnapshot
            {
                capturedAt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                logoAssembly = CreateEntry(view.LogoAssembly, string.Empty),
                elements = elements.ToArray()
            };
        }

        public static void Apply(LoadingPanelView view, LoadingPanelLayoutSnapshot snapshot, bool includeInactive = true)
        {
            if (view == null || snapshot == null || view.LogoAssembly == null)
            {
                return;
            }

            if (snapshot.logoAssembly != null)
            {
                ApplyEntry(view.LogoAssembly, snapshot.logoAssembly);
            }

            if (snapshot.elements == null)
            {
                return;
            }

            for (int index = 0; index < snapshot.elements.Length; index++)
            {
                LoadingPanelElementLayout entry = snapshot.elements[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.path))
                {
                    continue;
                }

                Transform target = view.LogoAssembly.Find(entry.path);
                if (target is not RectTransform rect)
                {
                    Debug.LogWarning($"[LoadingPanelLayout] Missing path: {entry.path}");
                    continue;
                }

                if (!includeInactive && !rect.gameObject.activeSelf)
                {
                    continue;
                }

                ApplyEntry(rect, entry);
            }

            view.CaptureReferences();
        }

        public static LoadingPanelLayoutSnapshot LoadFromJson(string jsonPath = DefaultJsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            return JsonUtility.FromJson<LoadingPanelLayoutSnapshot>(json);
        }

        public static void SaveToJson(LoadingPanelLayoutSnapshot snapshot, string jsonPath = DefaultJsonPath)
        {
            if (snapshot == null)
            {
                return;
            }

            string directory = Path.GetDirectoryName(jsonPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(snapshot, true);
            File.WriteAllText(jsonPath, json);
        }

        private static void CaptureTransform(RectTransform root, string relativePath, List<LoadingPanelElementLayout> output)
        {
            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (child is not RectTransform childRect)
                {
                    continue;
                }

                string childPath = string.IsNullOrEmpty(relativePath)
                    ? child.name
                    : $"{relativePath}/{child.name}";

                output.Add(CreateEntry(childRect, childPath));
                CaptureTransform(childRect, childPath, output);
            }
        }

        private static LoadingPanelElementLayout CreateEntry(RectTransform rect, string path)
        {
            return new LoadingPanelElementLayout
            {
                path = path,
                active = rect.gameObject.activeSelf,
                anchoredPosition = rect.anchoredPosition,
                sizeDelta = rect.sizeDelta,
                anchorMin = rect.anchorMin,
                anchorMax = rect.anchorMax,
                pivot = rect.pivot,
                localScale = rect.localScale,
                localEulerAngles = rect.localEulerAngles
            };
        }

        private static void ApplyEntry(RectTransform rect, LoadingPanelElementLayout entry)
        {
            rect.gameObject.SetActive(entry.active);
            rect.anchorMin = entry.anchorMin;
            rect.anchorMax = entry.anchorMax;
            rect.pivot = entry.pivot;
            rect.sizeDelta = entry.sizeDelta;
            rect.anchoredPosition = entry.anchoredPosition;
            rect.localScale = entry.localScale;
            rect.localEulerAngles = entry.localEulerAngles;
        }
    }
}
