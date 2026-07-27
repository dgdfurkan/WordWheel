using System;
using DG.Tweening;
using Runtime.UI.Animation;
using Runtime.UI.Configuration;
using Runtime.UI.Data;
using Runtime.UI.Enums;
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

        [TabGroup("Animation", "Script")]
        [InfoBox(
            "Override ConfigurePanelAnimations() in your panel script to define animations in code. " +
            "Settings sync to the Inspector tabs above on Awake and in Edit Mode.",
            InfoMessageType.Info)]
        [ShowInInspector, ReadOnly, LabelText("Source")]
        private string ScriptAnimationSource => GetType().Name + ".ConfigurePanelAnimations()";

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
        public virtual UIPanelDisplayMode DisplayMode => UIPanelDisplayMode.Exclusive;

        protected CanvasGroup canvasGroup;

        private Vector2 defaultAnchoredPosition;
        private Vector3 defaultScale;
        private Sequence activeSequence;
        private UIElementAnimationStateRegistry elementStateRegistry;
        private Action pendingAnimationCallback;

        private bool visualDefaultsCached;

        private static bool IsPlaying => Application.isPlaying;

        internal void SyncRuntimeState()
        {
            EnsureInitialized();

            if (!gameObject.activeInHierarchy)
            {
                isTransitioning = false;
                isOpen = false;
            }
        }

        internal bool IsDisplayed => gameObject.activeSelf && isOpen && !isTransitioning;

        internal void HealOpenState()
        {
            SyncRuntimeState();

            if (!gameObject.activeSelf && (isOpen || isTransitioning))
            {
                ForceSyncClosedState();
            }
        }

        /// <summary>
        /// Applies initial hidden state once at bootstrap.
        /// Must NOT run inside Awake — first SetActive(true) triggers Awake and would fight Open().
        /// </summary>
        internal void BootstrapStartHiddenState()
        {
            EnsureInitialized();

            if (!startHidden)
            {
                isOpen = gameObject.activeInHierarchy;
                return;
            }

            isOpen = false;
            isTransitioning = false;

            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        internal bool CanStartOpen()
        {
            return !isTransitioning;
        }

        internal void BringToFront()
        {
            transform.SetAsLastSibling();
        }

        internal void EnsureInitialized()
        {
            if (GetComponent<RectTransform>() == null)
            {
                Debug.LogError($"[UIPanel] {gameObject.name} must have RectTransform component!");
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (elementStateRegistry == null)
            {
                elementStateRegistry = new UIElementAnimationStateRegistry();
                RefreshChildAnimationRegistry();
            }

            if (!visualDefaultsCached && PanelTransform != null)
            {
                CacheDefaultTransformState();
                visualDefaultsCached = true;
            }
        }

        public UIPanelChildAnimationSettings ChildAnimations => childAnimations;
        public UIPanelAnimationSettings OpenAnimationSettings => openAnimation;
        public UIPanelAnimationSettings CloseAnimationSettings => closeAnimation;

        protected virtual void ConfigurePanelAnimations(UIPanelAnimationSetup setup) { }

        protected virtual void Awake()
        {
            EnsureInitialized();
            ApplyScriptAnimationConfiguration();

            if (startHidden)
            {
                isOpen = false;
            }
            else if (!isOpen)
            {
                isOpen = gameObject.activeInHierarchy;
            }
        }

        protected virtual void OnDestroy()
        {
            KillActiveAnimation();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            ApplyScriptAnimationConfiguration();
        }
#endif

        protected void ApplyScriptAnimationConfiguration()
        {
            UIPanelAnimationSetup setup = new UIPanelAnimationSetup(openAnimation, closeAnimation, childAnimations);
            ConfigurePanelAnimations(setup);

            if (!setup.HasChanges)
            {
                return;
            }

            setup.Commit();
            RefreshChildAnimationRegistry();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        public bool Open()
        {
            EnsureInitialized();
            HealOpenState();

            gameObject.SetActive(true);

            // First activation runs Awake synchronously; never allow startHidden to deactivate again here.
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            BringToFront();

            if (DisplayMode == UIPanelDisplayMode.Overlay)
            {
                UIManager.Instance?.RefreshOverlayScrim();
            }

            if (IsDisplayed)
            {
                return true;
            }

            if (isTransitioning)
            {
                KillActiveAnimation();
            }

            isTransitioning = true;

            if (DisplayMode == UIPanelDisplayMode.Overlay)
            {
                isOpen = true;
            }
            else
            {
                isOpen = false;
            }

            ResetVisualState();
            elementStateRegistry?.RestoreAll();
            HandlePanelOpening();
            PlayConfiguredOpenAnimation(() =>
            {
                isOpen = true;
                isTransitioning = false;
                FinalizeOpenVisuals();
                HandlePanelOpened();
                OnPanelOpened?.Invoke();
                UIManager.Instance?.RefreshOverlayScrim();
            });

            return true;
        }

        public bool Close()
        {
            EnsureInitialized();

            if (isTransitioning)
            {
                KillActiveAnimation();
                isTransitioning = false;

                if (!gameObject.activeSelf)
                {
                    return false;
                }

                if (!isOpen)
                {
                    isOpen = true;
                }
            }
            else if (!gameObject.activeSelf && !isOpen)
            {
                return false;
            }
            else if (gameObject.activeSelf && !isOpen)
            {
                isOpen = true;
            }

            if (!isOpen)
            {
                return false;
            }

            KillActiveAnimation();
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
                UIManager.Instance?.RefreshOverlayScrim();
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
            if (!this || !gameObject)
            {
                return;
            }

            EnsureInitialized();

            bool wasVisible = isOpen || isTransitioning || gameObject.activeSelf;
            KillActiveAnimation();
            isTransitioning = false;
            isOpen = false;

            if (gameObject.activeSelf)
            {
                RestoreHiddenState();
            }

            gameObject.SetActive(false);

            if (wasVisible)
            {
                HandlePanelClosed();
                OnPanelClosed?.Invoke();
                UIManager.Instance?.RefreshOverlayScrim();
            }
        }

        internal void ForceSyncClosedState()
        {
            ForceClose();
        }

        internal void PrepareForImmediateOpen()
        {
            EnsureInitialized();
            KillActiveAnimation();
            isTransitioning = false;
            isOpen = false;
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
            EnsureInitialized();

            if (PanelTransform == null || canvasGroup == null)
            {
                return;
            }

            PanelTransform.localScale = defaultScale;
            PanelTransform.anchoredPosition = defaultAnchoredPosition;
            canvasGroup.alpha = 1f;
        }

        private void RestoreHiddenState()
        {
            ResetVisualState();
            elementStateRegistry?.RestoreAll();
        }

        private void FinalizeOpenVisuals()
        {
            EnsureInitialized();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            if (PanelTransform != null)
            {
                PanelTransform.localScale = defaultScale;
                PanelTransform.anchoredPosition = defaultAnchoredPosition;
            }
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

            sequence.Play();
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

            if (canvasGroup != null || PanelTransform != null)
            {
                UIAnimationHelper.KillPanelTweens(canvasGroup, PanelTransform);
            }

            elementStateRegistry?.KillAllTweens();
        }
    }
}
