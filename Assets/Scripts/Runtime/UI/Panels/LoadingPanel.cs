using System;
using DG.Tweening;
using Runtime.UI.Configuration;
using Runtime.UI.Core;
using Runtime.UI.Enums;
using Runtime.UI.Panels.Loading;
using UnityEngine;

namespace Runtime.UI.Panels
{
    /// <summary>
    /// Reusable Word Wheel loading overlay — app splash and in-game transitions.
    /// </summary>
    [DefaultExecutionOrder(-150)]
    public class LoadingPanel : UIPanel
    {
        public override UIPanelDisplayMode DisplayMode => UIPanelDisplayMode.Overlay;

        [Header("View")]
        [SerializeField] private LoadingPanelView view;

        private LoadingPanelIntroAnimator introAnimator;
        private LoadingTransitionRequest activeRequest;
        private float sessionStartedAt;
        private bool sessionFinishing;

        public bool IsSessionActive => activeRequest != null;

        public LoadingPresentation ActivePresentation =>
            activeRequest?.Presentation ?? LoadingPresentation.Quick;

        public bool UsesQuickTransitionMask =>
            view != null &&
            view.HasQuickTransitionMask &&
            ActivePresentation == LoadingPresentation.Quick;

        protected override void Awake()
        {
            if (view == null)
            {
                view = GetComponent<LoadingPanelView>();
            }

            if (view == null)
            {
                view = gameObject.AddComponent<LoadingPanelView>();
            }

            LoadingPanelViewBuilder.EnsureBuilt(transform, view);
            introAnimator = new LoadingPanelIntroAnimator(view);
            base.Awake();
        }

        protected override void ConfigurePanelAnimations(UIPanelAnimationSetup anim)
        {
            anim.Open.None();
            anim.Close.None();
        }

        /// <summary>Launch splash on app start.</summary>
        public void PlaySplash(Action onComplete)
        {
            RunTransition(LoadingTransitionRequest.Splash(onComplete));
        }

        /// <summary>In-game: logo in → action (e.g. switch panel) → logo out.</summary>
        public void RunTransition(LoadingTransitionRequest request)
        {
            activeRequest = request ?? LoadingTransitionRequest.Transition(null);
            sessionFinishing = false;
            sessionStartedAt = Time.unscaledTime;

            if (IsOpen && gameObject.activeSelf && !IsTransitioning)
            {
                RestartSession();
                return;
            }

            UIManager.Instance?.OpenPanel<LoadingPanel>();
        }

        protected override void PlayConfiguredOpenAnimation(Action onComplete)
        {
            sessionFinishing = false;

            if (activeRequest == null)
            {
                activeRequest = LoadingTransitionRequest.Splash(null);
            }

            introAnimator.PlayIntro(activeRequest.Presentation, () =>
            {
                onComplete?.Invoke();
                BeginSessionHold();
            });
        }

        protected override void PlayConfiguredCloseAnimation(Action onComplete)
        {
            LoadingPresentation presentation = activeRequest?.Presentation ?? LoadingPresentation.Quick;
            introAnimator.PlayOutro(presentation, onComplete);
        }

        protected override void HandlePanelClosed()
        {
            CompleteSession();
        }

        protected override void OnDestroy()
        {
            DOTween.Kill(this);
            introAnimator?.Kill();
            view?.KillAllTweens();
            base.OnDestroy();
        }

        private void RestartSession()
        {
            introAnimator?.Kill();
            view?.RestoreNeutralVisualState();
            isTransitioning = true;
            PlayConfiguredOpenAnimation(() =>
            {
                isOpen = true;
                isTransitioning = false;
                BeginSessionHold();
            });
        }

        private void BeginSessionHold()
        {
            if (sessionFinishing || activeRequest == null)
            {
                return;
            }

            activeRequest.OnIntroComplete?.Invoke();
            activeRequest.MidAction?.Invoke();

            float elapsed = Time.unscaledTime - sessionStartedAt;
            float wait = Mathf.Max(activeRequest.HoldAfterIntro, activeRequest.MinimumVisibleDuration - elapsed);
            DOVirtual.DelayedCall(wait, BeginOutro).SetUpdate(true).SetId(this);
        }

        private void BeginOutro()
        {
            if (sessionFinishing || activeRequest == null)
            {
                return;
            }

            sessionFinishing = true;

            if (!IsOpen || !gameObject.activeSelf)
            {
                CompleteSession();
                return;
            }

            Close();
        }

        private void CompleteSession()
        {
            if (activeRequest == null)
            {
                return;
            }

            Action callback = activeRequest.OnComplete;
            activeRequest = null;
            sessionFinishing = false;
            callback?.Invoke();
        }
    }
}
