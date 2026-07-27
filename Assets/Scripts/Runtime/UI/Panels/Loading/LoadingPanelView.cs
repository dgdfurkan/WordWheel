using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runtime.UI.Panels.Loading
{
    /// <summary>
    /// Holds references for the Word Wheel loading logo assembly.
    /// Wheel: WheelHalf_0/1 (or legacy WheelSegment_*).
    /// Word/Wheel labels are image pieces from atlas — Word_0..3, Wheel_0..4.
    /// Sparkle counts are dynamic — add SparkLeft_2, SparkRight_3 etc. freely.
    /// </summary>
    public class LoadingPanelView : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private CanvasGroup background;
        [SerializeField] private RectTransform logoAssembly;
        [SerializeField] private RectTransform wheelRoot;

        [Header("Wheel Pieces")]
        [SerializeField] private RectTransform[] wheelPieces = new RectTransform[2];

        [Header("Logo Images (Word / Wheel)")]
        [SerializeField] private RectTransform[] wordLetters = new RectTransform[4];
        [SerializeField] private RectTransform[] wheelLetters = new RectTransform[5];

        [Header("Sparkles")]
        [SerializeField] private RectTransform[] leftSparkles = new RectTransform[0];
        [SerializeField] private RectTransform[] rightSparkles = new RectTransform[0];

        [Header("Layout")]
        [SerializeField] private LoadingPanelLayoutSnapshot savedLayout;
        [SerializeField] private bool applySavedLayoutOnBuild = true;

        [Header("Quick Transition")]
        [Tooltip("Play / in-game loading: round mask grows from center. Assign your circle sprite here.")]
        [SerializeField] private RectTransform quickTransitionMask;
        [SerializeField] private CanvasGroup quickTransitionMaskGroup;

        public CanvasGroup Background => background;
        public RectTransform LogoAssembly => logoAssembly;
        public RectTransform WheelRoot => wheelRoot;
        public RectTransform[] WordLetters => wordLetters;
        public RectTransform[] WheelLetters => wheelLetters;
        public RectTransform[] LeftSparkles => leftSparkles;
        public RectTransform[] RightSparkles => rightSparkles;
        public LoadingPanelLayoutSnapshot SavedLayout => savedLayout;
        public RectTransform QuickTransitionMask => quickTransitionMask;
        public CanvasGroup QuickTransitionMaskGroup => quickTransitionMaskGroup;
        public bool HasQuickTransitionMask => quickTransitionMask != null;

        public bool IsBuilt => logoAssembly != null && wheelRoot != null;

        public bool HasLegacyTextLayout()
        {
            return HasTextComponent(wordLetters) || HasTextComponent(wheelLetters);
        }

        public void SetSavedLayout(LoadingPanelLayoutSnapshot snapshot)
        {
            savedLayout = snapshot;
        }

        public void ApplySavedLayout()
        {
            if (savedLayout == null)
            {
                savedLayout = LoadingPanelLayoutUtility.LoadFromJson();
            }

            if (savedLayout != null)
            {
                LoadingPanelLayoutUtility.Apply(this, savedLayout);
            }
        }

        public LoadingPanelLayoutSnapshot CaptureCurrentLayout()
        {
            CaptureReferences();
            return LoadingPanelLayoutUtility.Capture(this);
        }

        public void CaptureAndSaveLayout()
        {
            LoadingPanelLayoutSnapshot snapshot = CaptureCurrentLayout();
            savedLayout = snapshot;
            LoadingPanelLayoutUtility.SaveToJson(snapshot);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
            Debug.Log(
                $"[LoadingPanelLayout] Captured {snapshot.elements.Length} elements → {LoadingPanelLayoutUtility.DefaultJsonPath}");
#else
            Debug.Log($"[LoadingPanelLayout] Captured {snapshot.elements.Length} layout elements.");
#endif
        }

        /// <summary>Stops tweens and returns logo pieces to layout rest pose before replaying intro.</summary>
        public void RestoreNeutralVisualState()
        {
            ConsolidateVisualLayers();
            CaptureReferences();
            ApplySavedLayout();

            KillAllTweens();
            ResetRectTransform(logoAssembly);
            ResetRectTransform(wheelRoot);
            ResetRectTransforms(GetActiveWheelPieces());
            ResetRectTransforms(wordLetters);
            ResetRectTransforms(wheelLetters);
            ResetRectTransforms(leftSparkles);
            ResetRectTransforms(rightSparkles);

            if (background != null)
            {
                background.DOKill(true);
                background.alpha = 0f;
                background.gameObject.SetActive(false);
            }

            ResetQuickTransitionMask();
        }

        public void SetQuickMaskForPresentation(LoadingPresentation presentation)
        {
            ConsolidateVisualLayers();
            CaptureQuickMaskReferences();

            if (quickTransitionMask == null)
            {
                return;
            }

            bool showMask = presentation == LoadingPresentation.Quick;
            quickTransitionMask.gameObject.SetActive(showMask);
            if (!showMask)
            {
                return;
            }

            quickTransitionMask.DOKill(true);
            quickTransitionMask.localScale = Vector3.zero;

            if (quickTransitionMaskGroup != null)
            {
                quickTransitionMaskGroup.DOKill(true);
                quickTransitionMaskGroup.alpha = 0f;
            }
        }

        public float GetQuickMaskCoverScale()
        {
            if (quickTransitionMask == null)
            {
                return 1f;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : transform as RectTransform;
            Vector2 canvasSize = canvasRect != null ? canvasRect.rect.size : new Vector2(1080f, 1920f);
            float maskWidth = Mathf.Max(quickTransitionMask.rect.width, 1f);
            float maskHeight = Mathf.Max(quickTransitionMask.rect.height, 1f);
            float coverScaleX = canvasSize.x / maskWidth;
            float coverScaleY = canvasSize.y / maskHeight;

            return Mathf.Max(coverScaleX, coverScaleY) * 1.35f;
        }

        private void ResetQuickTransitionMask()
        {
            CaptureQuickMaskReferences();
            if (quickTransitionMask == null)
            {
                return;
            }

            quickTransitionMask.DOKill(true);
            quickTransitionMask.localScale = Vector3.zero;
            quickTransitionMask.gameObject.SetActive(false);

            if (quickTransitionMaskGroup != null)
            {
                quickTransitionMaskGroup.DOKill(true);
                quickTransitionMaskGroup.alpha = 0f;
            }
        }

        private void CaptureQuickMaskReferences()
        {
            if (quickTransitionMask == null)
            {
                quickTransitionMask = transform.Find("QuickTransitionMask") as RectTransform;
            }

            if (quickTransitionMask != null && quickTransitionMaskGroup == null)
            {
                quickTransitionMaskGroup = quickTransitionMask.GetComponent<CanvasGroup>();
            }
        }

        public void ConsolidateVisualLayers()
        {
            CaptureReferences();
            CaptureQuickMaskReferences();

            DeactivateDuplicateChildren(transform, "Background", background != null ? background.transform : null);
            DeactivateDuplicateChildren(transform, "QuickTransitionMask", quickTransitionMask);
            ApplyLayerOrder();
        }

        public void ApplyLayerOrder()
        {
            CaptureReferences();
            CaptureQuickMaskReferences();

            if (background != null)
            {
                background.transform.SetAsFirstSibling();
            }

            if (quickTransitionMask != null)
            {
                int maskIndex = background != null ? 1 : 0;
                quickTransitionMask.SetSiblingIndex(maskIndex);
            }

            if (logoAssembly != null)
            {
                logoAssembly.SetAsLastSibling();
            }
        }

        public void EnsureSplashBackgroundLayout()
        {
            if (background == null)
            {
                return;
            }

            RectTransform rect = background.transform as RectTransform;
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            Image image = background.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
                image.color = Color.black;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                image.raycastTarget = true;
            }
        }

        private static void DeactivateDuplicateChildren(Transform root, string childName, Transform keep)
        {
            if (root == null)
            {
                return;
            }

            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (!string.Equals(child.name, childName))
                {
                    continue;
                }

                if (keep != null && child == keep)
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        public void SetBackgroundForPresentation(LoadingPresentation presentation)
        {
            ConsolidateVisualLayers();
            CaptureReferences();
            if (background == null)
            {
                return;
            }

            bool showBackground = presentation == LoadingPresentation.Full;
            background.gameObject.SetActive(showBackground);
            if (showBackground)
            {
                EnsureSplashBackgroundLayout();
                background.DOKill(true);
                background.alpha = 0f;
            }
        }

        public void KillAllTweens()
        {
            KillRectTweens(logoAssembly);
            KillRectTweens(wheelRoot);
            KillRectTweens(GetActiveWheelPieces());
            KillRectTweens(wordLetters);
            KillRectTweens(wheelLetters);
            KillRectTweens(leftSparkles);
            KillRectTweens(rightSparkles);

            if (background != null)
            {
                background.DOKill(true);
            }

            CaptureQuickMaskReferences();
            if (quickTransitionMask != null)
            {
                quickTransitionMask.DOKill(true);
            }

            if (quickTransitionMaskGroup != null)
            {
                quickTransitionMaskGroup.DOKill(true);
            }
        }

        private static void ResetRectTransform(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.localScale = Vector3.one;
            rect.localEulerAngles = Vector3.zero;
            ResetCanvasGroup(rect);
        }

        private static void ResetRectTransforms(IReadOnlyList<RectTransform> rects)
        {
            if (rects == null)
            {
                return;
            }

            for (int index = 0; index < rects.Count; index++)
            {
                ResetRectTransform(rects[index]);
            }
        }

        private static void ResetRectTransforms(RectTransform[] rects)
        {
            if (rects == null)
            {
                return;
            }

            for (int index = 0; index < rects.Length; index++)
            {
                ResetRectTransform(rects[index]);
            }
        }

        private static void ResetCanvasGroup(RectTransform rect)
        {
            CanvasGroup group = rect.GetComponent<CanvasGroup>();
            if (group == null)
            {
                return;
            }

            group.alpha = 1f;
        }

        private static void KillRectTweens(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.DOKill(true);
            CanvasGroup group = rect.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.DOKill(true);
            }
        }

        private static void KillRectTweens(IReadOnlyList<RectTransform> rects)
        {
            if (rects == null)
            {
                return;
            }

            for (int index = 0; index < rects.Count; index++)
            {
                KillRectTweens(rects[index]);
            }
        }

        private static void KillRectTweens(RectTransform[] rects)
        {
            if (rects == null)
            {
                return;
            }

            for (int index = 0; index < rects.Length; index++)
            {
                KillRectTweens(rects[index]);
            }
        }

        private static bool HasTextComponent(RectTransform[] pieces)
        {
            if (pieces == null)
            {
                return false;
            }

            for (int index = 0; index < pieces.Length; index++)
            {
                RectTransform piece = pieces[index];
                if (piece != null && piece.GetComponent("TextMeshProUGUI") != null)
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            EditorApplication.delayCall += TryBuildInEditor;
        }

        private void OnValidate()
        {
            EditorApplication.delayCall += TryBuildInEditor;
        }

        private void TryBuildInEditor()
        {
            if (this == null || Application.isPlaying)
            {
                return;
            }

            CaptureReferences();
            LoadingPanelViewBuilder.EnsureBuilt(transform, this);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(gameObject);
        }
#endif

        public IReadOnlyList<RectTransform> GetActiveWheelPieces()
        {
            List<RectTransform> activePieces = new List<RectTransform>();
            if (wheelPieces == null)
            {
                return activePieces;
            }

            for (int index = 0; index < wheelPieces.Length; index++)
            {
                if (wheelPieces[index] != null)
                {
                    activePieces.Add(wheelPieces[index]);
                }
            }

            return activePieces;
        }

        public void CaptureReferences()
        {
            Transform root = transform;

            if (background == null)
            {
                background = root.Find("Background")?.GetComponent<CanvasGroup>();
            }

            CaptureQuickMaskReferences();

            Transform assembly = root.Find("LogoAssembly");
            if (assembly == null)
            {
                return;
            }

            logoAssembly = assembly as RectTransform;
            wheelRoot = assembly.Find("WheelRoot") as RectTransform;
            wheelPieces = CaptureWheelPieces(wheelRoot);

            wordLetters = CaptureNamedPieces(assembly.Find("WordGroup"), "Word_", 16);
            wheelLetters = CaptureNamedPieces(assembly.Find("WheelLabelGroup"), "Wheel_", 16);
            leftSparkles = CaptureNamedPieces(assembly.Find("SparkLeftGroup"), "SparkLeft_", 32);
            rightSparkles = CaptureNamedPieces(assembly.Find("SparkRightGroup"), "SparkRight_", 32);
        }

        internal void AssignBuiltReferences(
            CanvasGroup builtBackground,
            RectTransform builtQuickTransitionMask,
            RectTransform builtLogoAssembly,
            RectTransform builtWheelRoot,
            RectTransform[] builtWheelPieces,
            RectTransform[] builtWordLetters,
            RectTransform[] builtWheelLetters,
            RectTransform[] builtLeftSparkles,
            RectTransform[] builtRightSparkles)
        {
            background = builtBackground;
            quickTransitionMask = builtQuickTransitionMask;
            CaptureQuickMaskReferences();
            logoAssembly = builtLogoAssembly;
            wheelRoot = builtWheelRoot;
            wheelPieces = builtWheelPieces;
            wordLetters = builtWordLetters;
            wheelLetters = builtWheelLetters;
            leftSparkles = builtLeftSparkles;
            rightSparkles = builtRightSparkles;
        }

        internal void TryApplySavedLayoutAfterBuild()
        {
            if (!applySavedLayoutOnBuild)
            {
                return;
            }

            ApplySavedLayout();
        }

        private static RectTransform[] CaptureWheelPieces(Transform wheelRootTransform)
        {
            if (wheelRootTransform == null)
            {
                return new RectTransform[0];
            }

            RectTransform[] halves = CaptureNamedPieces(wheelRootTransform, "WheelHalf_", 2);
            if (halves.Length > 0)
            {
                return halves;
            }

            return CaptureNamedPieces(wheelRootTransform, "WheelSegment_", 8);
        }

        private static RectTransform[] CaptureNamedPieces(Transform parent, string prefix, int maxCount)
        {
            List<RectTransform> pieces = new List<RectTransform>();
            if (parent == null)
            {
                return pieces.ToArray();
            }

            List<(int index, RectTransform rect)> indexed = new List<(int, RectTransform)>();
            for (int childIndex = 0; childIndex < parent.childCount; childIndex++)
            {
                Transform child = parent.GetChild(childIndex);
                if (child is not RectTransform rect || !child.name.StartsWith(prefix))
                {
                    continue;
                }

                string suffix = child.name.Substring(prefix.Length);
                if (!int.TryParse(suffix, out int pieceIndex))
                {
                    continue;
                }

                indexed.Add((pieceIndex, rect));
            }

            indexed.Sort((left, right) => left.index.CompareTo(right.index));
            for (int index = 0; index < indexed.Count && index < maxCount; index++)
            {
                pieces.Add(indexed[index].rect);
            }

            return pieces.ToArray();
        }
    }
}
