using DG.Tweening;
using Runtime.UI.Animation;
using Runtime.UI.Data;
using Runtime.UI.Enums;
using Runtime.UI.Utilities;
using UnityEngine;

namespace Runtime.UI.Animation
{
    /// <summary>
    /// Combines panel-level and child-level animation sequences based on timing settings.
    /// On close, child exit animations overlap with the panel fade for a smoother handoff.
    /// </summary>
    public static class UIPanelAnimationOrchestrator
    {
        public static Sequence BuildOpenSequence(
            UIPanelAnimationSettings panelSettings,
            UIPanelChildAnimationSettings childSettings,
            CanvasGroup canvasGroup,
            RectTransform panelTransform,
            Vector2 defaultAnchoredPosition,
            Vector3 defaultScale,
            UIElementAnimationStateRegistry registry)
        {
            bool suppressPanelFade = ShouldSuppressPanelFadeOnOpen(panelSettings, childSettings);

            if (suppressPanelFade && canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            Sequence panelSequence = suppressPanelFade
                ? null
                : UIAnimationHelper.PlayOpenAnimation(
                    panelSettings,
                    canvasGroup,
                    panelTransform,
                    defaultAnchoredPosition,
                    defaultScale);

            Sequence childSequence = UIPanelElementAnimationPlayer.PlayOpenGroups(childSettings, registry);
            return CombineOpenSequences(panelSequence, childSequence, childSettings);
        }

        public static Sequence BuildCloseSequence(
            UIPanelAnimationSettings panelSettings,
            UIPanelChildAnimationSettings childSettings,
            CanvasGroup canvasGroup,
            RectTransform panelTransform,
            Vector2 defaultAnchoredPosition,
            Vector3 defaultScale,
            UIElementAnimationStateRegistry registry)
        {
            Sequence panelSequence = UIAnimationHelper.PlayCloseAnimation(
                panelSettings,
                canvasGroup,
                panelTransform,
                defaultAnchoredPosition,
                defaultScale);

            Sequence childSequence = UIPanelElementAnimationPlayer.PlayCloseGroups(childSettings, registry);
            return CombineCloseSequences(panelSequence, childSequence, childSettings);
        }

        public static Sequence BuildChildOpenSequence(
            UIPanelChildAnimationSettings childSettings,
            UIElementAnimationStateRegistry registry)
        {
            return UIPanelElementAnimationPlayer.PlayOpenGroups(childSettings, registry);
        }

        public static Sequence BuildChildCloseSequence(
            UIPanelChildAnimationSettings childSettings,
            UIElementAnimationStateRegistry registry)
        {
            return UIPanelElementAnimationPlayer.PlayCloseGroups(childSettings, registry);
        }

        private static Sequence CombineOpenSequences(
            Sequence panelSequence,
            Sequence childSequence,
            UIPanelChildAnimationSettings childSettings)
        {
            bool hasPanel = HasDuration(panelSequence);
            bool hasChild = HasChildSequence(childSettings, childSequence);

            if (!hasPanel && !hasChild)
            {
                return null;
            }

            if (!hasChild)
            {
                return panelSequence;
            }

            if (!hasPanel)
            {
                return childSequence;
            }

            Sequence master = DOTween.Sequence();

            switch (childSettings.Timing)
            {
                case UIPanelChildAnimationTiming.ParallelWithPanel:
                    master.Append(panelSequence);
                    master.Join(childSequence);
                    break;

                case UIPanelChildAnimationTiming.AfterPanelStarts:
                    master.Append(panelSequence);
                    master.Join(childSequence.SetDelay(childSettings.PanelStartOffset));
                    break;

                case UIPanelChildAnimationTiming.AfterPanelCompletes:
                    master.Append(panelSequence);
                    master.Append(childSequence);
                    break;

                case UIPanelChildAnimationTiming.BeforePanel:
                    master.Append(childSequence);
                    master.Append(panelSequence);
                    break;
            }

            return master;
        }

        private static Sequence CombineCloseSequences(
            Sequence panelSequence,
            Sequence childSequence,
            UIPanelChildAnimationSettings childSettings)
        {
            bool hasPanel = HasDuration(panelSequence);
            bool hasChild = HasChildSequence(childSettings, childSequence);

            if (!hasPanel && !hasChild)
            {
                return null;
            }

            if (!hasChild)
            {
                return panelSequence;
            }

            if (!hasPanel)
            {
                return childSequence;
            }

            float childDuration = childSequence.Duration();
            float panelDuration = panelSequence.Duration();
            float overlap = Mathf.Clamp(panelDuration * 0.8f, 0.12f, childDuration * 0.45f);
            float panelInsertTime = Mathf.Max(0f, childDuration - overlap);

            Sequence master = DOTween.Sequence();
            master.Append(childSequence);
            master.Insert(panelInsertTime, panelSequence);
            return master;
        }

        private static bool HasChildSequence(UIPanelChildAnimationSettings childSettings, Sequence childSequence)
        {
            return childSettings != null
                && childSettings.Enabled
                && childSequence != null
                && childSequence.Duration() > 0f;
        }

        private static bool HasDuration(Sequence sequence)
        {
            return sequence != null && sequence.Duration() > 0f;
        }

        private static bool ShouldSuppressPanelFadeOnOpen(
            UIPanelAnimationSettings panelSettings,
            UIPanelChildAnimationSettings childSettings)
        {
            if (panelSettings == null
                || panelSettings.AnimationType != Enums.UIPanelAnimationType.Fade
                || childSettings == null
                || !childSettings.Enabled)
            {
                return false;
            }

            foreach (UIPanelElementAnimationGroup group in childSettings.Groups)
            {
                if (group != null && group.Enabled && group.PlayOnOpen)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
