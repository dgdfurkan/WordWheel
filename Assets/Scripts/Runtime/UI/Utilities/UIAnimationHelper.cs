using DG.Tweening;
using Runtime.UI.Data;
using Runtime.UI.Enums;
using UnityEngine;

namespace Runtime.UI.Utilities
{
    /// <summary>
    /// Helper class for smooth UI animations using DOTween.
    /// Animation values are driven by UIPanelAnimationSettings from the Inspector.
    /// </summary>
    public static class UIAnimationHelper
    {
        public static void KillPanelTweens(CanvasGroup canvasGroup, RectTransform rectTransform)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
            }

            if (rectTransform != null)
            {
                rectTransform.DOKill();
            }
        }

        public static Sequence PlayOpenAnimation(
            UIPanelAnimationSettings settings,
            CanvasGroup canvasGroup,
            RectTransform rectTransform,
            Vector2 defaultAnchoredPosition,
            Vector3 defaultScale)
        {
            return PlayAnimation(settings, canvasGroup, rectTransform, defaultAnchoredPosition, defaultScale, isClosing: false);
        }

        public static Sequence PlayCloseAnimation(
            UIPanelAnimationSettings settings,
            CanvasGroup canvasGroup,
            RectTransform rectTransform,
            Vector2 defaultAnchoredPosition,
            Vector3 defaultScale)
        {
            return PlayAnimation(settings, canvasGroup, rectTransform, defaultAnchoredPosition, defaultScale, isClosing: true);
        }

        public static float GetAnimationDuration(UIPanelAnimationSettings settings)
        {
            if (settings == null || settings.AnimationType == UIPanelAnimationType.None)
            {
                return 0f;
            }

            return settings.Duration;
        }

        private static Sequence PlayAnimation(
            UIPanelAnimationSettings settings,
            CanvasGroup canvasGroup,
            RectTransform rectTransform,
            Vector2 defaultAnchoredPosition,
            Vector3 defaultScale,
            bool isClosing)
        {
            if (settings == null || settings.AnimationType == UIPanelAnimationType.None)
            {
                return null;
            }

            if (canvasGroup == null || rectTransform == null)
            {
                return null;
            }

            KillPanelTweens(canvasGroup, rectTransform);

            float duration = settings.Duration;
            Ease ease = settings.Ease;
            float slideDistance = GetSlideDistance(settings);

            Sequence sequence = DOTween.Sequence();
            ApplyUpdateMode(sequence, settings.UseUnscaledTime);
            LinkSequenceToTarget(sequence, canvasGroup);

            switch (settings.AnimationType)
            {
                case UIPanelAnimationType.Fade:
                    PrepareFade(canvasGroup, rectTransform, defaultAnchoredPosition, defaultScale, isClosing);
                    sequence.Append(canvasGroup.DOFade(isClosing ? 0f : 1f, duration).SetEase(ease));
                    break;

                case UIPanelAnimationType.Pop:
                    PreparePop(canvasGroup, rectTransform, defaultAnchoredPosition, defaultScale, isClosing);
                    sequence.Append(rectTransform
                        .DOScale(isClosing ? Vector3.zero : defaultScale, duration)
                        .SetEase(ease));
                    break;

                case UIPanelAnimationType.PopAndFade:
                    PreparePopAndFade(canvasGroup, rectTransform, defaultAnchoredPosition, defaultScale, isClosing);
                    sequence.Append(canvasGroup.DOFade(isClosing ? 0f : 1f, duration).SetEase(ease));
                    sequence.Join(rectTransform
                        .DOScale(isClosing ? Vector3.zero : defaultScale, duration)
                        .SetEase(ease));
                    break;

                case UIPanelAnimationType.SlideFromLeft:
                    PrepareSlide(canvasGroup, rectTransform, defaultAnchoredPosition, defaultScale);
                    if (isClosing)
                    {
                        sequence.Append(rectTransform
                            .DOAnchorPos(new Vector2(defaultAnchoredPosition.x + slideDistance, defaultAnchoredPosition.y), duration)
                            .SetEase(ease));
                    }
                    else
                    {
                        rectTransform.anchoredPosition = new Vector2(defaultAnchoredPosition.x - slideDistance, defaultAnchoredPosition.y);
                        sequence.Append(rectTransform.DOAnchorPos(defaultAnchoredPosition, duration).SetEase(ease));
                    }
                    break;

                case UIPanelAnimationType.SlideToRight:
                    PrepareSlide(canvasGroup, rectTransform, defaultAnchoredPosition, defaultScale);
                    if (isClosing)
                    {
                        sequence.Append(rectTransform
                            .DOAnchorPos(new Vector2(defaultAnchoredPosition.x + slideDistance, defaultAnchoredPosition.y), duration)
                            .SetEase(ease));
                    }
                    else
                    {
                        rectTransform.anchoredPosition = new Vector2(defaultAnchoredPosition.x - slideDistance, defaultAnchoredPosition.y);
                        sequence.Append(rectTransform.DOAnchorPos(defaultAnchoredPosition, duration).SetEase(ease));
                    }
                    break;

                case UIPanelAnimationType.Entrance:
                    PrepareEntrance(canvasGroup, rectTransform, defaultAnchoredPosition, defaultScale, isClosing, slideDistance);
                    if (isClosing)
                    {
                        sequence.Append(canvasGroup.DOFade(0f, duration * 0.6f).SetEase(Ease.InQuad));
                        sequence.Join(rectTransform
                            .DOAnchorPos(new Vector2(defaultAnchoredPosition.x - slideDistance * 0.5f, defaultAnchoredPosition.y), duration)
                            .SetEase(ease));
                    }
                    else
                    {
                        sequence.Append(canvasGroup.DOFade(1f, duration * 0.7f).SetEase(Ease.OutQuad));
                        sequence.Join(rectTransform.DOAnchorPos(defaultAnchoredPosition, duration).SetEase(ease));
                    }
                    break;

                case UIPanelAnimationType.Exit:
                    PrepareEntrance(canvasGroup, rectTransform, defaultAnchoredPosition, defaultScale, isClosing, slideDistance);
                    if (isClosing)
                    {
                        sequence.Append(canvasGroup.DOFade(0f, duration * 0.6f).SetEase(Ease.InQuad));
                        sequence.Join(rectTransform
                            .DOAnchorPos(new Vector2(defaultAnchoredPosition.x - slideDistance * 0.5f, defaultAnchoredPosition.y), duration)
                            .SetEase(ease));
                    }
                    else
                    {
                        sequence.Append(canvasGroup.DOFade(1f, duration * 0.7f).SetEase(Ease.OutQuad));
                        sequence.Join(rectTransform.DOAnchorPos(defaultAnchoredPosition, duration).SetEase(ease));
                    }
                    break;
            }

            return sequence;
        }

        private static float GetSlideDistance(UIPanelAnimationSettings settings)
        {
            if (settings.UseScreenWidthForSlide)
            {
                return Screen.width;
            }

            return settings.SlideOffset;
        }

        private static void ApplyUpdateMode(Tween tween, bool useUnscaledTime)
        {
            tween.SetUpdate(useUnscaledTime);
        }

        private static void LinkSequenceToTarget(Sequence sequence, CanvasGroup canvasGroup)
        {
            if (sequence == null || canvasGroup == null)
            {
                return;
            }

            sequence.SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDestroy);
        }

        private static void PrepareFade(
            CanvasGroup canvasGroup,
            RectTransform rectTransform,
            Vector2 defaultAnchoredPosition,
            Vector3 defaultScale,
            bool isClosing)
        {
            rectTransform.localScale = defaultScale;
            rectTransform.anchoredPosition = defaultAnchoredPosition;
            canvasGroup.alpha = isClosing ? 1f : 0f;
        }

        private static void PreparePop(
            CanvasGroup canvasGroup,
            RectTransform rectTransform,
            Vector2 defaultAnchoredPosition,
            Vector3 defaultScale,
            bool isClosing)
        {
            rectTransform.anchoredPosition = defaultAnchoredPosition;
            canvasGroup.alpha = 1f;
            rectTransform.localScale = isClosing ? defaultScale : Vector3.zero;
        }

        private static void PreparePopAndFade(
            CanvasGroup canvasGroup,
            RectTransform rectTransform,
            Vector2 defaultAnchoredPosition,
            Vector3 defaultScale,
            bool isClosing)
        {
            rectTransform.anchoredPosition = defaultAnchoredPosition;
            canvasGroup.alpha = isClosing ? 1f : 0f;
            rectTransform.localScale = isClosing ? defaultScale : Vector3.zero;
        }

        private static void PrepareSlide(
            CanvasGroup canvasGroup,
            RectTransform rectTransform,
            Vector2 defaultAnchoredPosition,
            Vector3 defaultScale)
        {
            rectTransform.localScale = defaultScale;
            canvasGroup.alpha = 1f;
            rectTransform.anchoredPosition = defaultAnchoredPosition;
        }

        private static void PrepareEntrance(
            CanvasGroup canvasGroup,
            RectTransform rectTransform,
            Vector2 defaultAnchoredPosition,
            Vector3 defaultScale,
            bool isClosing,
            float slideDistance)
        {
            rectTransform.localScale = defaultScale;
            canvasGroup.alpha = isClosing ? 1f : 0f;
            rectTransform.anchoredPosition = isClosing
                ? defaultAnchoredPosition
                : new Vector2(defaultAnchoredPosition.x + slideDistance * 0.5f, defaultAnchoredPosition.y);
        }

        /// <summary>
        /// Bounce scale effect for button feedback.
        /// </summary>
        public static Sequence BounceScale(RectTransform rectTransform, float scale = 1.1f, float duration = 0.3f, bool useUnscaledTime = false)
        {
            if (rectTransform == null)
            {
                return null;
            }

            Vector3 originalScale = rectTransform.localScale;
            Sequence sequence = DOTween.Sequence();
            ApplyUpdateMode(sequence, useUnscaledTime);
            sequence
                .Append(rectTransform.DOScale(originalScale * scale, duration * 0.5f).SetEase(Ease.OutCubic))
                .Append(rectTransform.DOScale(originalScale, duration * 0.5f).SetEase(Ease.InCubic));

            return sequence;
        }

        /// <summary>
        /// Shake animation for errors or warnings.
        /// </summary>
        public static Sequence Shake(RectTransform rectTransform, float strength = 10f, int vibrato = 10, float duration = 0.5f, bool useUnscaledTime = false)
        {
            if (rectTransform == null)
            {
                return null;
            }

            Vector2 originalPos = rectTransform.anchoredPosition;
            Sequence sequence = DOTween.Sequence();
            ApplyUpdateMode(sequence, useUnscaledTime);
            sequence
                .Append(rectTransform.DOShakeAnchorPos(duration, strength, vibrato))
                .OnComplete(() => rectTransform.anchoredPosition = originalPos);

            return sequence;
        }

        /// <summary>
        /// Pulse animation for emphasis.
        /// </summary>
        public static Tween Pulse(RectTransform rectTransform, float scale = 1.05f, float duration = 0.5f, int loops = -1, bool useUnscaledTime = false)
        {
            if (rectTransform == null)
            {
                return null;
            }

            Vector3 originalScale = rectTransform.localScale;
            return rectTransform
                .DOScale(originalScale * scale, duration * 0.5f)
                .SetEase(Ease.InOutQuad)
                .SetLoops(loops, LoopType.Yoyo)
                .SetUpdate(useUnscaledTime);
        }
    }
}
