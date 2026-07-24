using System;
using DG.Tweening;
using Runtime.UI.Data;
using Runtime.UI.Enums;
using UnityEngine;

namespace Runtime.UI.Animation
{
    /// <summary>
    /// Builds and plays per-element DOTween sequences for panel child animations.
    /// </summary>
    public static class UIPanelElementAnimationPlayer
    {
        public static Sequence PlayOpenGroups(
            UIPanelChildAnimationSettings settings,
            UIElementAnimationStateRegistry registry)
        {
            return PlayGroups(settings, registry, isClosing: false);
        }

        public static Sequence PlayCloseGroups(
            UIPanelChildAnimationSettings settings,
            UIElementAnimationStateRegistry registry)
        {
            return PlayGroups(settings, registry, isClosing: true);
        }

        public static Sequence PlaySingleEntry(
            UIPanelElementAnimationEntry entry,
            UIElementAnimationStateRegistry registry,
            bool isClosing)
        {
            if (entry == null || !entry.IsValid || registry == null)
            {
                return null;
            }

            if (!registry.TryGetSnapshot(entry.Target, out UIElementTransformSnapshot snapshot))
            {
                registry.RegisterTarget(entry.Target);
                registry.TryGetSnapshot(entry.Target, out snapshot);
            }

            if (snapshot == null)
            {
                return null;
            }

            return BuildEntrySequence(entry, snapshot, isClosing);
        }

        public static float EstimateOpenDuration(UIPanelChildAnimationSettings settings)
        {
            return EstimateDuration(settings, isClosing: false);
        }

        public static float EstimateCloseDuration(UIPanelChildAnimationSettings settings)
        {
            return EstimateDuration(settings, isClosing: true);
        }

        private static Sequence PlayGroups(
            UIPanelChildAnimationSettings settings,
            UIElementAnimationStateRegistry registry,
            bool isClosing)
        {
            if (settings == null || !settings.Enabled || registry == null)
            {
                return null;
            }

            Sequence master = DOTween.Sequence();

            foreach (UIPanelElementAnimationGroup group in settings.Groups)
            {
                if (group == null || !group.Enabled)
                {
                    continue;
                }

                if (isClosing && !group.PlayOnClose)
                {
                    continue;
                }

                if (!isClosing && !group.PlayOnOpen)
                {
                    continue;
                }

                Sequence groupSequence = BuildGroupSequence(group, registry, isClosing);
                if (groupSequence == null)
                {
                    continue;
                }

                master.Join(groupSequence);
            }

            return master.Duration() > 0f ? master : null;
        }

        private static Sequence BuildGroupSequence(
            UIPanelElementAnimationGroup group,
            UIElementAnimationStateRegistry registry,
            bool isClosing)
        {
            UIPanelElementAnimationEntry[] entries = group.Elements;
            if (entries == null || entries.Length == 0)
            {
                return null;
            }

            Sequence groupSequence = DOTween.Sequence();
            bool hasTween = false;

            for (int index = 0; index < entries.Length; index++)
            {
                int resolvedIndex = isClosing && group.ReverseStaggerOnClose
                    ? entries.Length - 1 - index
                    : index;

                UIPanelElementAnimationEntry entry = entries[resolvedIndex];
                if (entry == null || !entry.IsValid)
                {
                    continue;
                }

                if (!registry.TryGetSnapshot(entry.Target, out UIElementTransformSnapshot snapshot))
                {
                    registry.RegisterTarget(entry.Target);
                    registry.TryGetSnapshot(entry.Target, out snapshot);
                }

                if (snapshot == null)
                {
                    continue;
                }

                float startAt = group.GroupStartDelay + index * group.StaggerInterval + entry.Delay;
                Sequence entrySequence = BuildEntrySequence(entry, snapshot, isClosing);
                if (entrySequence == null)
                {
                    continue;
                }

                groupSequence.Insert(startAt, entrySequence);
                hasTween = true;
            }

            return hasTween ? groupSequence : null;
        }

        private static Sequence BuildEntrySequence(
            UIPanelElementAnimationEntry entry,
            UIElementTransformSnapshot snapshot,
            bool isClosing)
        {
            snapshot.KillTweens();

            UIPanelElementAnimationType animationType = isClosing
                ? (entry.MirrorOpenOnClose ? entry.OpenType : entry.CloseType)
                : entry.OpenType;

            if (animationType == UIPanelElementAnimationType.None)
            {
                return null;
            }

            float duration = isClosing
                ? (entry.MirrorOpenOnClose ? entry.Duration : entry.CloseDuration)
                : entry.Duration;

            Ease ease = isClosing
                ? (entry.MirrorOpenOnClose ? GetMirrorEase(entry.Ease) : entry.CloseEase)
                : entry.Ease;

            PrepareEntryState(entry, snapshot, animationType, isClosing);

            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(entry.UseUnscaledTime);
            AppendAnimationTweens(sequence, entry, snapshot, animationType, duration, ease, isClosing);

            return sequence.Duration() > 0f ? sequence : null;
        }

        private static void PrepareEntryState(
            UIPanelElementAnimationEntry entry,
            UIElementTransformSnapshot snapshot,
            UIPanelElementAnimationType animationType,
            bool isClosing)
        {
            if (isClosing)
            {
                if (IsSceneCaptureType(animationType))
                {
                    RectTransform captureTarget = snapshot.Target;
                    captureTarget.anchoredPosition = entry.SceneCaptureTo;

                    if (animationType == UIPanelElementAnimationType.SceneCaptureFade)
                    {
                        SetAlpha(snapshot, entry.FadeTo);
                    }
                }

                return;
            }

            snapshot.Restore();

            if (IsSceneCaptureType(animationType))
            {
                RectTransform captureTarget = snapshot.Target;
                captureTarget.anchoredPosition = entry.SceneCaptureFrom;

                if (animationType == UIPanelElementAnimationType.SceneCaptureFade)
                {
                    SetAlpha(snapshot, entry.FadeFrom);
                }

                return;
            }

            RectTransform target = snapshot.Target;
            Vector3 defaultScale = snapshot.LocalScale;
            float defaultScaleMultiplier = defaultScale.x;

            switch (animationType)
            {
                case UIPanelElementAnimationType.Fade:
                case UIPanelElementAnimationType.FadeAndScale:
                case UIPanelElementAnimationType.FadeAndSlide:
                case UIPanelElementAnimationType.DriftIn:
                case UIPanelElementAnimationType.FloatUp:
                    SetAlpha(snapshot, entry.FadeFrom);
                    break;
            }

            switch (animationType)
            {
                case UIPanelElementAnimationType.Scale:
                case UIPanelElementAnimationType.PopIn:
                case UIPanelElementAnimationType.ElasticScale:
                case UIPanelElementAnimationType.PunchScale:
                case UIPanelElementAnimationType.FadeAndScale:
                    target.localScale = defaultScale * entry.ScaleFrom;
                    break;
            }

            switch (animationType)
            {
                case UIPanelElementAnimationType.SlideFromLeft:
                    target.anchoredPosition = snapshot.AnchoredPosition + Vector2.left * ResolveSlideDistance(entry);
                    break;
                case UIPanelElementAnimationType.SlideFromRight:
                    target.anchoredPosition = snapshot.AnchoredPosition + Vector2.right * ResolveSlideDistance(entry);
                    break;
                case UIPanelElementAnimationType.SlideFromTop:
                    target.anchoredPosition = snapshot.AnchoredPosition + Vector2.up * ResolveSlideDistance(entry);
                    break;
                case UIPanelElementAnimationType.SlideFromBottom:
                case UIPanelElementAnimationType.DropBounce:
                case UIPanelElementAnimationType.FloatUp:
                    target.anchoredPosition = snapshot.AnchoredPosition + Vector2.down * ResolveSlideDistance(entry);
                    break;
                case UIPanelElementAnimationType.MoveCustom:
                    target.anchoredPosition = snapshot.AnchoredPosition + entry.CustomMoveOffset;
                    break;
                case UIPanelElementAnimationType.FadeAndSlide:
                case UIPanelElementAnimationType.DriftIn:
                    target.anchoredPosition = snapshot.AnchoredPosition + entry.CustomMoveOffset;
                    break;
            }

            switch (animationType)
            {
                case UIPanelElementAnimationType.RotateIn:
                    target.localEulerAngles = new Vector3(0f, 0f, entry.RotationFrom);
                    break;
            }

            if (animationType == UIPanelElementAnimationType.PunchScale)
            {
                target.localScale = defaultScale * Mathf.Max(entry.ScaleTo, defaultScaleMultiplier);
            }
        }

        private static void AppendAnimationTweens(
            Sequence sequence,
            UIPanelElementAnimationEntry entry,
            UIElementTransformSnapshot snapshot,
            UIPanelElementAnimationType animationType,
            float duration,
            Ease ease,
            bool isClosing)
        {
            RectTransform target = snapshot.Target;
            Vector3 defaultScale = snapshot.LocalScale;
            float scaleTarget = isClosing ? entry.ScaleFrom : entry.ScaleTo;
            float fadeTarget = isClosing ? entry.FadeFrom : entry.FadeTo;
            float rotationTarget = isClosing ? entry.RotationFrom : entry.RotationTo;
            Vector2 moveTarget = isClosing
                ? snapshot.AnchoredPosition + GetCloseMoveOffset(entry, animationType)
                : snapshot.AnchoredPosition;

            switch (animationType)
            {
                case UIPanelElementAnimationType.Fade:
                    sequence.Append(CreateFadeTween(snapshot, fadeTarget, duration, ease));
                    break;

                case UIPanelElementAnimationType.Scale:
                    sequence.Append(target.DOScale(defaultScale * scaleTarget, duration).SetEase(ease));
                    break;

                case UIPanelElementAnimationType.PopIn:
                    sequence.Append(target.DOScale(defaultScale * scaleTarget, duration).SetEase(Ease.OutBack));
                    break;

                case UIPanelElementAnimationType.ElasticScale:
                    sequence.Append(target.DOScale(defaultScale * scaleTarget, duration).SetEase(Ease.OutElastic));
                    break;

                case UIPanelElementAnimationType.PunchScale:
                    if (isClosing)
                    {
                        sequence.Append(target.DOScale(defaultScale * entry.ScaleFrom, duration).SetEase(ease));
                    }
                    else
                    {
                        sequence.Append(target.DOPunchScale(Vector3.one * 0.15f, duration, 8, 0.8f));
                    }
                    break;

                case UIPanelElementAnimationType.SlideFromLeft:
                case UIPanelElementAnimationType.SlideFromRight:
                case UIPanelElementAnimationType.SlideFromTop:
                case UIPanelElementAnimationType.SlideFromBottom:
                case UIPanelElementAnimationType.MoveCustom:
                    sequence.Append(target.DOAnchorPos(moveTarget, duration).SetEase(ease));
                    break;

                case UIPanelElementAnimationType.FadeAndScale:
                    sequence.Append(CreateFadeTween(snapshot, fadeTarget, duration, ease));
                    sequence.Join(target.DOScale(defaultScale * scaleTarget, duration).SetEase(ease));
                    break;

                case UIPanelElementAnimationType.FadeAndSlide:
                    sequence.Append(CreateFadeTween(snapshot, fadeTarget, duration, ease));
                    sequence.Join(target.DOAnchorPos(moveTarget, duration).SetEase(ease));
                    break;

                case UIPanelElementAnimationType.RotateIn:
                    sequence.Append(target.DOLocalRotate(new Vector3(0f, 0f, rotationTarget), duration).SetEase(ease));
                    break;

                case UIPanelElementAnimationType.DriftIn:
                    sequence.Append(CreateFadeTween(snapshot, fadeTarget, duration * 0.8f, Ease.OutQuad));
                    sequence.Join(target.DOAnchorPos(moveTarget, duration).SetEase(Ease.OutQuart));
                    if (!isClosing)
                    {
                        sequence.Append(target.DOAnchorPosY(snapshot.AnchoredPosition.y - 6f, duration * 0.35f).SetEase(Ease.InOutSine));
                        sequence.Append(target.DOAnchorPosY(snapshot.AnchoredPosition.y, duration * 0.35f).SetEase(Ease.InOutSine));
                    }
                    break;

                case UIPanelElementAnimationType.DropBounce:
                    sequence.Append(target.DOAnchorPos(snapshot.AnchoredPosition, duration).SetEase(Ease.OutBounce));
                    if (!isClosing)
                    {
                        sequence.Join(CreateFadeTween(snapshot, fadeTarget, duration * 0.6f, Ease.OutQuad));
                    }
                    break;

                case UIPanelElementAnimationType.FloatUp:
                    sequence.Append(CreateFadeTween(snapshot, fadeTarget, duration * 0.75f, Ease.OutQuad));
                    sequence.Join(target.DOAnchorPos(snapshot.AnchoredPosition, duration).SetEase(Ease.OutQuart));
                    break;

                case UIPanelElementAnimationType.SceneCapture:
                    sequence.Append(target.DOAnchorPos(
                        isClosing ? entry.SceneCaptureFrom : entry.SceneCaptureTo,
                        duration).SetEase(ease));
                    break;

                case UIPanelElementAnimationType.SceneCaptureFade:
                    sequence.Append(target.DOAnchorPos(
                        isClosing ? entry.SceneCaptureFrom : entry.SceneCaptureTo,
                        duration).SetEase(ease));
                    sequence.Join(CreateFadeTween(snapshot, fadeTarget, duration, ease));
                    break;
            }
        }

        private static bool IsSceneCaptureType(UIPanelElementAnimationType animationType)
        {
            return animationType is UIPanelElementAnimationType.SceneCapture
                or UIPanelElementAnimationType.SceneCaptureFade;
        }

        private static Tween CreateFadeTween(
            UIElementTransformSnapshot snapshot,
            float targetAlpha,
            float duration,
            Ease ease)
        {
            if (snapshot?.Target == null)
            {
                return null;
            }

            CanvasGroup canvasGroup = snapshot.GetOrCreateCanvasGroup();
            if (canvasGroup == null)
            {
                return null;
            }

            return canvasGroup.DOFade(targetAlpha, duration).SetEase(ease);
        }

        private static void SetAlpha(UIElementTransformSnapshot snapshot, float alpha)
        {
            if (snapshot?.Target == null)
            {
                return;
            }

            CanvasGroup canvasGroup = snapshot.GetOrCreateCanvasGroup();
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = alpha;
        }

        private static float ResolveSlideDistance(UIPanelElementAnimationEntry entry)
        {
            if (entry.UseScreenRelativeSlide)
            {
                return Screen.width * 0.25f;
            }

            return entry.SlideDistance;
        }

        private static Vector2 GetCloseMoveOffset(UIPanelElementAnimationEntry entry, UIPanelElementAnimationType animationType)
        {
            switch (animationType)
            {
                case UIPanelElementAnimationType.SlideFromLeft:
                    return Vector2.left * ResolveSlideDistance(entry);
                case UIPanelElementAnimationType.SlideFromRight:
                    return Vector2.right * ResolveSlideDistance(entry);
                case UIPanelElementAnimationType.SlideFromTop:
                    return Vector2.up * ResolveSlideDistance(entry);
                case UIPanelElementAnimationType.SlideFromBottom:
                case UIPanelElementAnimationType.DropBounce:
                case UIPanelElementAnimationType.FloatUp:
                    return Vector2.down * ResolveSlideDistance(entry);
                case UIPanelElementAnimationType.MoveCustom:
                case UIPanelElementAnimationType.FadeAndSlide:
                case UIPanelElementAnimationType.DriftIn:
                    return entry.CustomMoveOffset;
                default:
                    return Vector2.zero;
            }
        }

        private static Ease GetMirrorEase(Ease openEase)
        {
            switch (openEase)
            {
                case Ease.OutBack:
                    return Ease.InBack;
                case Ease.OutElastic:
                    return Ease.InElastic;
                case Ease.OutBounce:
                    return Ease.InBounce;
                case Ease.OutCubic:
                    return Ease.InCubic;
                case Ease.OutQuad:
                    return Ease.InQuad;
                case Ease.OutQuart:
                    return Ease.InQuart;
                default:
                    return Ease.InCubic;
            }
        }

        private static float EstimateDuration(UIPanelChildAnimationSettings settings, bool isClosing)
        {
            if (settings == null || !settings.Enabled)
            {
                return 0f;
            }

            float maxDuration = 0f;

            foreach (UIPanelElementAnimationGroup group in settings.Groups)
            {
                if (group == null || !group.Enabled)
                {
                    continue;
                }

                if (isClosing && !group.PlayOnClose)
                {
                    continue;
                }

                if (!isClosing && !group.PlayOnOpen)
                {
                    continue;
                }

                UIPanelElementAnimationEntry[] entries = group.Elements;
                for (int index = 0; index < entries.Length; index++)
                {
                    UIPanelElementAnimationEntry entry = entries[index];
                    if (entry == null || !entry.IsValid)
                    {
                        continue;
                    }

                    float entryDuration = isClosing
                        ? (entry.MirrorOpenOnClose ? entry.Duration : entry.CloseDuration)
                        : entry.Duration;

                    if (entry.OpenType == UIPanelElementAnimationType.DriftIn && !isClosing)
                    {
                        entryDuration += entry.Duration * 0.7f;
                    }

                    float total = group.GroupStartDelay + index * group.StaggerInterval + entry.Delay + entryDuration;
                    maxDuration = Mathf.Max(maxDuration, total);
                }
            }

            return maxDuration;
        }
    }
}
