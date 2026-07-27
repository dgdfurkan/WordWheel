using System;

namespace Runtime.UI.Panels.Loading
{
    public sealed class LoadingTransitionRequest
    {
        public LoadingPresentation Presentation = LoadingPresentation.Quick;
        public float HoldAfterIntro = 0.35f;
        public float MinimumVisibleDuration = 0f;
        public Action MidAction;
        public Action OnIntroComplete;
        public Action OnComplete;

        public static LoadingTransitionRequest Splash(Action onComplete)
        {
            return new LoadingTransitionRequest
            {
                Presentation = LoadingPresentation.Quick,
                HoldAfterIntro = 0.35f,
                OnComplete = onComplete
            };
        }

        public static LoadingTransitionRequest Transition(Action midAction, Action onComplete = null, float hold = 0.15f)
        {
            return new LoadingTransitionRequest
            {
                Presentation = LoadingPresentation.Quick,
                HoldAfterIntro = hold,
                MidAction = midAction,
                OnComplete = onComplete
            };
        }
    }
}
