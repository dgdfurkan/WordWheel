using System;
using DG.Tweening;
using Runtime.UI.Core;
using Runtime.UI.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.UI.Data
{
    [Serializable]
    public class UIPanelElementAnimationEntry
    {
        [HorizontalGroup("Header", Width = 0.55f)]
        [LabelText("Label")]
        [SerializeField] private string label = "Element";

        [HorizontalGroup("Header", Width = 0.45f)]
        [LabelText("Target")]
        [Required]
        [AssetsOnly]
        [SerializeField] private RectTransform target;

        [LabelText("Entrance Style")]
        [EnumPaging]
        [SerializeField] private UIPanelElementAnimationType openType = UIPanelElementAnimationType.PopIn;

        [ShowIf(nameof(UsesSceneCapture))]
        [FoldoutGroup("SceneCapture")]
        [InfoBox(
            "Step 1: Place element at final position -> Capture End\n" +
            "Step 2: Move element to start position -> Capture Start\n" +
            "Step 3: Use Preview buttons or Play Open/Close",
            InfoMessageType.None)]
        [ShowInInspector, ReadOnly, HideLabel]
        [ProgressBar(0, 1, 0.95f, 0.45f, 0.2f)]
        private float CaptureSetupProgress => !hasSceneCaptureTo ? 0.15f : !hasSceneCaptureFrom ? 0.55f : 1f;

        [ShowIf(nameof(UsesSceneCapture))]
        [FoldoutGroup("SceneCapture")]
        [ShowInInspector, ReadOnly, LabelText("Path")]
        private string CapturePathSummary => HasValidSceneCapture
            ? $"Start {FormatVector(sceneCaptureFrom)}  ->  End {FormatVector(sceneCaptureTo)}  ({CaptureDistance:0}px)"
            : hasSceneCaptureTo
                ? $"End saved {FormatVector(sceneCaptureTo)} | waiting for Start"
                : "Capture End first, then Start";

        [ShowIf(nameof(UsesSceneCapture))]
        [FoldoutGroup("SceneCapture")]
        [HorizontalGroup("SceneCapture/Record")]
        [Button("Capture End (Final Layout)", ButtonSizes.Medium), GUIColor(0.35f, 0.95f, 0.55f)]
        [EnableIf("@target != null")]
        private void CaptureSceneTo()
        {
            sceneCaptureTo = target.anchoredPosition;
            hasSceneCaptureTo = true;
            MarkOwnerDirty();
        }

        [ShowIf(nameof(UsesSceneCapture))]
        [FoldoutGroup("SceneCapture")]
        [HorizontalGroup("SceneCapture/Record")]
        [Button("Capture Start", ButtonSizes.Medium), GUIColor(0.35f, 0.75f, 1f)]
        [EnableIf("@target != null && hasSceneCaptureTo")]
        private void CaptureSceneFrom()
        {
            sceneCaptureFrom = target.anchoredPosition;
            hasSceneCaptureFrom = true;
            MarkOwnerDirty();
        }

        [ShowIf(nameof(UsesSceneCapture))]
        [FoldoutGroup("SceneCapture")]
        [HorizontalGroup("SceneCapture/Preview")]
        [Button("Go To Start"), EnableIf("@HasSceneCaptureFrom && target != null")]
        private void PreviewSceneFrom() => target.anchoredPosition = sceneCaptureFrom;

        [ShowIf(nameof(UsesSceneCapture))]
        [FoldoutGroup("SceneCapture")]
        [HorizontalGroup("SceneCapture/Preview")]
        [Button("Go To End"), EnableIf("@HasSceneCaptureTo && target != null")]
        private void PreviewSceneTo() => target.anchoredPosition = sceneCaptureTo;

        [ShowIf(nameof(UsesSceneCapture))]
        [FoldoutGroup("SceneCapture")]
        [HorizontalGroup("SceneCapture/Play")]
        [Button("Play Open", ButtonSizes.Medium), GUIColor(0.45f, 0.9f, 0.5f)]
        [EnableIf("@HasValidSceneCapture && target != null && UnityEngine.Application.isPlaying")]
        private void PreviewCapturedOpen()
        {
            target.GetComponentInParent<UIPanel>()?.PreviewElementEntry(this, isClosing: false);
        }

        [ShowIf(nameof(UsesSceneCapture))]
        [FoldoutGroup("SceneCapture")]
        [HorizontalGroup("SceneCapture/Play")]
        [Button("Play Close", ButtonSizes.Medium), GUIColor(0.95f, 0.55f, 0.4f)]
        [EnableIf("@HasValidSceneCapture && target != null && UnityEngine.Application.isPlaying")]
        private void PreviewCapturedClose()
        {
            target.GetComponentInParent<UIPanel>()?.PreviewElementEntry(this, isClosing: true);
        }

        [SerializeField, HideInInspector] private Vector2 sceneCaptureFrom;
        [SerializeField, HideInInspector] private Vector2 sceneCaptureTo;
        [SerializeField, HideInInspector] private bool hasSceneCaptureFrom;
        [SerializeField, HideInInspector] private bool hasSceneCaptureTo;

        [FoldoutGroup("Timing", false)]
        [LabelText("Delay")]
        [SerializeField] private float delay;

        [FoldoutGroup("Timing")]
        [LabelText("Duration")]
        [SerializeField] private float duration = 0.45f;

        [FoldoutGroup("Timing")]
        [SerializeField] private Ease ease = Ease.OutCubic;

        [FoldoutGroup("Timing")]
        [LabelText("Unscaled Time")]
        [SerializeField] private bool useUnscaledTime;

        [FoldoutGroup("Values", false)]
        [ShowIf(nameof(UsesSlideValues))]
        [LabelText("Slide Distance")]
        [SerializeField] private float slideDistance = 120f;

        [FoldoutGroup("Values")]
        [ShowIf(nameof(UsesSlideValues))]
        [SerializeField] private bool useScreenRelativeSlide;

        [FoldoutGroup("Values")]
        [ShowIf(nameof(UsesCustomMove))]
        [LabelText("Move Offset")]
        [SerializeField] private Vector2 customMoveOffset = new Vector2(0f, -80f);

        [FoldoutGroup("Values")]
        [ShowIf(nameof(UsesScaleValues))]
        [SerializeField] private float scaleFrom;

        [FoldoutGroup("Values")]
        [ShowIf(nameof(UsesScaleValues))]
        [SerializeField] private float scaleTo = 1f;

        [FoldoutGroup("Values")]
        [ShowIf(nameof(UsesFadeValues))]
        [SerializeField] private float fadeFrom;

        [FoldoutGroup("Values")]
        [ShowIf(nameof(UsesFadeValues))]
        [SerializeField] private float fadeTo = 1f;

        [FoldoutGroup("Values")]
        [ShowIf("@openType == UIPanelElementAnimationType.RotateIn")]
        [SerializeField] private float rotationFrom;

        [FoldoutGroup("Values")]
        [ShowIf("@openType == UIPanelElementAnimationType.RotateIn")]
        [SerializeField] private float rotationTo;

        [FoldoutGroup("Close", false)]
        [LabelText("Mirror Open On Close")]
        [SerializeField] private bool mirrorOpenOnClose = true;

        [FoldoutGroup("Close")]
        [ShowIf("@!mirrorOpenOnClose")]
        [SerializeField] private UIPanelElementAnimationType closeType = UIPanelElementAnimationType.Fade;

        [FoldoutGroup("Close")]
        [ShowIf("@!mirrorOpenOnClose")]
        [SerializeField] private float closeDuration = 0.3f;

        [FoldoutGroup("Close")]
        [ShowIf("@!mirrorOpenOnClose")]
        [SerializeField] private Ease closeEase = Ease.InCubic;

        public string Label => label;
        public RectTransform Target => target;
        public UIPanelElementAnimationType OpenType => openType;
        public float Delay => Mathf.Max(0f, delay);
        public float Duration => Mathf.Max(0f, duration);
        public Ease Ease => ease;
        public bool UseUnscaledTime => useUnscaledTime;
        public float SlideDistance => slideDistance;
        public bool UseScreenRelativeSlide => useScreenRelativeSlide;
        public Vector2 CustomMoveOffset => customMoveOffset;
        public float ScaleFrom => scaleFrom;
        public float ScaleTo => scaleTo;
        public float FadeFrom => fadeFrom;
        public float FadeTo => fadeTo;
        public float RotationFrom => rotationFrom;
        public float RotationTo => rotationTo;
        public bool MirrorOpenOnClose => mirrorOpenOnClose;
        public UIPanelElementAnimationType CloseType => closeType;
        public float CloseDuration => Mathf.Max(0f, closeDuration);
        public Ease CloseEase => closeEase;
        public Vector2 SceneCaptureFrom => sceneCaptureFrom;
        public Vector2 SceneCaptureTo => sceneCaptureTo;
        public bool HasSceneCaptureFrom => hasSceneCaptureFrom;
        public bool HasSceneCaptureTo => hasSceneCaptureTo;
        public bool UsesSceneCapture =>
            openType is UIPanelElementAnimationType.SceneCapture
                or UIPanelElementAnimationType.SceneCaptureFade;

        public bool HasValidSceneCapture =>
            UsesSceneCapture && hasSceneCaptureFrom && hasSceneCaptureTo;

        public float CaptureDistance =>
            HasValidSceneCapture ? Vector2.Distance(sceneCaptureFrom, sceneCaptureTo) : 0f;

        public bool IsValid =>
            target != null
            && openType != UIPanelElementAnimationType.None
            && (!UsesSceneCapture || HasValidSceneCapture);

        public static UIPanelElementAnimationEntry CreateDefault(
            RectTransform rectTransform,
            float entryDelay,
            UIPanelElementAnimationType type)
        {
            return new UIPanelElementAnimationEntry
            {
                label = rectTransform.name,
                target = rectTransform,
                openType = type,
                delay = entryDelay
            };
        }

        public void ApplyScriptConfiguration(
            RectTransform targetTransform,
            string entryLabel,
            UIPanelElementAnimationType openAnimationType,
            float entryDelay = 0f,
            float entryDuration = 0.45f,
            Ease entryEase = Ease.OutCubic,
            float slideDist = 120f,
            bool screenRelativeSlide = false,
            Vector2 moveOffset = default,
            float fromScale = 0f,
            float toScale = 1f,
            float fromFade = 0f,
            float toFade = 1f,
            float fromRotation = 0f,
            float toRotation = 0f,
            bool mirrorClose = true,
            UIPanelElementAnimationType customCloseType = UIPanelElementAnimationType.Fade,
            float customCloseDuration = 0.3f,
            Ease customCloseEase = Ease.InCubic,
            bool unscaledTime = false,
            Vector2? captureFrom = null,
            Vector2? captureTo = null)
        {
            label = string.IsNullOrWhiteSpace(entryLabel)
                ? targetTransform != null ? targetTransform.name : "Element"
                : entryLabel;
            target = targetTransform;
            openType = openAnimationType;
            delay = entryDelay;
            duration = entryDuration;
            ease = entryEase;
            slideDistance = slideDist;
            useScreenRelativeSlide = screenRelativeSlide;
            customMoveOffset = moveOffset;
            scaleFrom = fromScale;
            scaleTo = toScale;
            fadeFrom = fromFade;
            fadeTo = toFade;
            rotationFrom = fromRotation;
            rotationTo = toRotation;
            mirrorOpenOnClose = mirrorClose;
            closeType = customCloseType;
            closeDuration = customCloseDuration;
            closeEase = customCloseEase;
            useUnscaledTime = unscaledTime;

            if (captureFrom.HasValue)
            {
                sceneCaptureFrom = captureFrom.Value;
                hasSceneCaptureFrom = true;
            }

            if (captureTo.HasValue)
            {
                sceneCaptureTo = captureTo.Value;
                hasSceneCaptureTo = true;
            }
        }

        public string GetListLabel()
        {
            if (target == null)
            {
                return label;
            }

            string status = UsesSceneCapture && !HasValidSceneCapture ? " [incomplete]" : string.Empty;
            return target.name + status;
        }

        private bool UsesSlideValues =>
            openType is UIPanelElementAnimationType.SlideFromLeft
                or UIPanelElementAnimationType.SlideFromRight
                or UIPanelElementAnimationType.SlideFromTop
                or UIPanelElementAnimationType.SlideFromBottom
                or UIPanelElementAnimationType.DropBounce
                or UIPanelElementAnimationType.FloatUp;

        private bool UsesCustomMove =>
            openType is UIPanelElementAnimationType.MoveCustom
                or UIPanelElementAnimationType.FadeAndSlide
                or UIPanelElementAnimationType.DriftIn;

        private bool UsesScaleValues =>
            openType is UIPanelElementAnimationType.Scale
                or UIPanelElementAnimationType.PopIn
                or UIPanelElementAnimationType.ElasticScale
                or UIPanelElementAnimationType.PunchScale
                or UIPanelElementAnimationType.FadeAndScale;

        private bool UsesFadeValues =>
            openType is UIPanelElementAnimationType.Fade
                or UIPanelElementAnimationType.FadeAndScale
                or UIPanelElementAnimationType.FadeAndSlide
                or UIPanelElementAnimationType.DriftIn
                or UIPanelElementAnimationType.FloatUp
                or UIPanelElementAnimationType.DropBounce
                or UIPanelElementAnimationType.SceneCaptureFade;

        private static string FormatVector(Vector2 value) => $"({value.x:0}, {value.y:0})";

        private void MarkOwnerDirty()
        {
#if UNITY_EDITOR
            if (target == null)
            {
                return;
            }

            UIPanel panel = target.GetComponentInParent<UIPanel>();
            if (panel != null)
            {
                UnityEditor.EditorUtility.SetDirty(panel);
            }
#endif
        }
    }
}
