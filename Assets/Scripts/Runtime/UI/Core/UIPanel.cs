using System;
using DG.Tweening;
using Runtime.UI.Animation;
using Runtime.UI.Data;
using Runtime.UI.Interfaces;
using Runtime.UI.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.UI.Core
{
    [HideReferenceObjectPicker]
    public abstract class UIPanel : MonoBehaviour, IUIPanel
    {
        [PropertyOrder(-30)]
        [ToggleLeft, LabelText("Start Hidden")]
        [SerializeField] private bool startHidden = true;

        [TabGroup("Animation", "Panel")]
        [Title("Panel Open")]
        [InlineProperty, HideLabel]
        [SerializeField] private UIPanelAnimationSettings openAnimation = new UIPanelAnimationSettings();

        [TabGroup("Animation", "Panel")]
        [Title("Panel Close")]
        [InlineProperty, HideLabel]
        [SerializeField] private UIPanelAnimationSettings closeAnimation = new UIPanelAnimationSettings();

        [TabGroup("Animation", "Elements")]
        [InlineProperty, HideLabel]
        [SerializeField] private UIPanelChildAnimationSettings childAnimations = new UIPanelChildAnimationSettings();

        [TabGroup("Animation", "Preview")]
        [InfoBox("Preview buttons work in Play Mode. Scene gizmos show capture paths when this panel is selected.", InfoMessageType.Info)]
        [ButtonGroup("Animation/Preview/Row1")]
        [Button("Play Full Open", ButtonSizes.Large), GUIColor(0.45f, 0.85f, 0.55f)]
        [EnableIf(nameof(IsPlaying))]
        private void OdinPreviewOpen() => PreviewOpenAnimation();

        [TabGroup("Animation", "Preview")]
        [ButtonGroup("Animation/Preview/Row1")]
        [Button("Play Full Close", ButtonSizes.Large), GUIColor(0.95f, 0.55f, 0.45f)]
        [EnableIf(nameof(IsPlaying))]
        private void OdinPreviewClose() => PreviewCloseAnimation();

        [TabGroup("Animation", "Preview")]
        [ButtonGroup("Animation/Preview/Row2")]
        [Button("Children Open"), EnableIf(nameof(IsPlaying))]
        private void OdinPreviewChildrenOpen() => PreviewChildOpenAnimations();

        [TabGroup("Animation", "Preview")]
        [ButtonGroup("Animation/Preview/Row2")]
        [Button("Children Close"), EnableIf(nameof(IsPlaying))]
        private void OdinPreviewChildrenClose() => PreviewChildCloseAnimations();

        [TabGroup("Animation", "Preview")]
        [ButtonGroup("Animation/Preview/Row3")]
        [Button("Refresh Registry"), EnableIf(nameof(IsPlaying))]
        private void OdinRefreshRegistry() => RefreshChildAnimationRegistry();

        [TabGroup("Animation", "Preview")]
        [ButtonGroup("Animation/Preview/Row3")]
        [Button("Reset Preview"), EnableIf(nameof(IsPlaying))]
        private void OdinResetPreview() => ResetPreviewState();

        [FoldoutGroup("Events", false)]
        public UnityEvent OnPanelOpened = new UnityEvent();

        [FoldoutGroup("Events")]
        public UnityEvent OnPanelClosed = new UnityEvent();

        protected bool isOpen;
        protected bool isTransitioning;

        public bool IsOpen => isOpen;
        public bool IsTransitioning => isTransitioning;
        public RectTransform PanelTransform => GetComponent<RectTransform>();
        public Type PanelType => GetType();

        protected CanvasGroup canvasGroup;

        private Vector2 defaultAnchoredPosition;
        private Vector3 defaultScale;
        private Sequence activeSequence;
        private UIElementAnimationStateRegistry elementStateRegistry;
        private Action pendingAnimationCallback;

        private static bool IsPlaying => Application.isPlaying;

        public UIPanelChildAnimationSettings ChildAnimations => childAnimations;

        protected virtual void Awake()
        {
            if (GetComponent<RectTransform>() == null)
            {
                Debug.LogError($"[UIPanel] {gameObject.name} must have RectTransform component!");
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            elementStateRegistry = new UIElementAnimationStateRegistry();
            CacheDefaultTransformState();
            RefreshChildAnimationRegistry();

            if (startHidden)
            {
                gameObject.SetActive(false);
                isOpen = false;
            }
            else
            {
                isOpen = gameObject.activeInHierarchy;
            }
        }

        protected virtual void OnDestroy()
        {
            KillActiveAnimation();
        }

        public bool Open()
        {
            if (isOpen || isTransitioning)
            {
                return false;
            }

            isTransitioning = true;
            gameObject.SetActive(true);
            isOpen = true;

            ResetVisualState();
            elementStateRegistry?.RestoreAll();
            HandlePanelOpening();
            PlayConfiguredOpenAnimation(() =>
            {
                isTransitioning = false;
                HandlePanelOpened();
                OnPanelOpened?.Invoke();
            });

            return true;
        }

        public bool Close()
        {
            if (!isOpen || isTransitioning)
            {
                return false;
            }

            isTransitioning = true;
            HandlePanelClosing();

            PlayConfiguredCloseAnimation(() =>
            {
                isOpen = false;
                gameObject.SetActive(false);
                RestoreHiddenState();
                isTransitioning = false;
                HandlePanelClosed();
                OnPanelClosed?.Invoke();
            });

            return true;
        }

        protected virtual void PlayConfiguredOpenAnimation(Action onComplete)
        {
            activeSequence = UIPanelAnimationOrchestrator.BuildOpenSequence(
                openAnimation,
                childAnimations,
                canvasGroup,
                PanelTransform,
                defaultAnchoredPosition,
                defaultScale,
                elementStateRegistry);

            CompleteOrInstant(activeSequence, onComplete);
        }

        protected virtual void PlayConfiguredCloseAnimation(Action onComplete)
        {
            activeSequence = UIPanelAnimationOrchestrator.BuildCloseSequence(
                closeAnimation,
                childAnimations,
                canvasGroup,
                PanelTransform,
                defaultAnchoredPosition,
                defaultScale,
                elementStateRegistry);

            CompleteOrInstant(activeSequence, onComplete);
        }

        internal void ForceClose()
        {
            KillActiveAnimation();
            isTransitioning = false;
            isOpen = false;
            gameObject.SetActive(false);
            RestoreHiddenState();
        }

        public void PreviewOpenAnimation()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            PreparePreviewState();
            activeSequence = UIPanelAnimationOrchestrator.BuildOpenSequence(
                openAnimation,
                childAnimations,
                canvasGroup,
                PanelTransform,
                defaultAnchoredPosition,
                defaultScale,
                elementStateRegistry);
        }

        public void PreviewCloseAnimation()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            PreparePreviewState();
            activeSequence = UIPanelAnimationOrchestrator.BuildCloseSequence(
                closeAnimation,
                childAnimations,
                canvasGroup,
                PanelTransform,
                defaultAnchoredPosition,
                defaultScale,
                elementStateRegistry);
        }

        public void PreviewChildOpenAnimations()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            PreparePreviewState();
            activeSequence = UIPanelAnimationOrchestrator.BuildChildOpenSequence(
                childAnimations,
                elementStateRegistry);
        }

        public void PreviewChildCloseAnimations()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            PreparePreviewState();
            activeSequence = UIPanelAnimationOrchestrator.BuildChildCloseSequence(
                childAnimations,
                elementStateRegistry);
        }

        public void PreviewElementEntry(UIPanelElementAnimationEntry entry, bool isClosing)
        {
            if (!Application.isPlaying || entry == null)
            {
                return;
            }

            PreparePreviewState();
            activeSequence = UIPanelElementAnimationPlayer.PlaySingleEntry(
                entry,
                elementStateRegistry,
                isClosing);
        }

        public void ResetPreviewState()
        {
            KillActiveAnimation();
            ResetVisualState();
            elementStateRegistry?.RestoreAll();
        }

        public void RefreshChildAnimationRegistry()
        {
            elementStateRegistry?.RegisterFromSettings(childAnimations);
        }

        protected UIPanelAnimationSettings GetOpenAnimationSettings() => openAnimation;
        protected UIPanelAnimationSettings GetCloseAnimationSettings() => closeAnimation;
        protected UIPanelChildAnimationSettings GetChildAnimationSettings() => childAnimations;

        protected virtual void HandlePanelOpening() { }
        protected virtual void HandlePanelOpened() { }
        protected virtual void HandlePanelClosing() { }
        protected virtual void HandlePanelClosed() { }

        private void PreparePreviewState()
        {
            KillActiveAnimation();
            RefreshChildAnimationRegistry();
            gameObject.SetActive(true);
            ResetVisualState();
            elementStateRegistry?.RestoreAll();
        }

        private void CacheDefaultTransformState()
        {
            defaultAnchoredPosition = PanelTransform.anchoredPosition;
            defaultScale = PanelTransform.localScale;
        }

        private void ResetVisualState()
        {
            PanelTransform.localScale = defaultScale;
            PanelTransform.anchoredPosition = defaultAnchoredPosition;
            canvasGroup.alpha = 1f;
        }

        private void RestoreHiddenState()
        {
            ResetVisualState();
            elementStateRegistry?.RestoreAll();
        }

        private void CompleteOrInstant(Sequence sequence, Action onComplete)
        {
            pendingAnimationCallback = onComplete;

            void InvokeOnce()
            {
                if (pendingAnimationCallback == null)
                {
                    return;
                }

                Action callback = pendingAnimationCallback;
                pendingAnimationCallback = null;
                callback();
            }

            if (sequence == null)
            {
                InvokeOnce();
                return;
            }

            sequence.OnComplete(InvokeOnce);
            sequence.OnKill(InvokeOnce);
        }

        private void KillActiveAnimation()
        {
            pendingAnimationCallback = null;

            if (activeSequence != null && activeSequence.IsActive())
            {
                activeSequence.Kill();
            }

            activeSequence = null;
            UIAnimationHelper.KillPanelTweens(canvasGroup, PanelTransform);
            elementStateRegistry?.KillAllTweens();
        }
    }
}
