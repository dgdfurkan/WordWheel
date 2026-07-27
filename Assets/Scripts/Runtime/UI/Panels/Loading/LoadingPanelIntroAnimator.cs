using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Runtime.UI.Panels.Loading
{
    /// <summary>
    /// Cinematic intro/outro for the Word Wheel loading logo.
    /// Full: halves snap → pulse → WORD images → sparkles → WHEEL images → sparkles → breathe.
    /// Quick: wheel halves → pulse → WORD/WHEEL letters → short breathe (in-game, no sparkles).
    /// </summary>
    public sealed class LoadingPanelIntroAnimator
    {
        private const float HalfStagger = 0.12f;
        private const float SegmentStagger = 0.07f;
        private const float LetterStagger = 0.07f;
        private const float QuickMaskIntroDuration = 0.42f;
        private const float QuickMaskOutroDuration = 0.32f;

        private readonly LoadingPanelView view;
        private readonly Dictionary<RectTransform, Vector2> letterDefaults = new Dictionary<RectTransform, Vector2>();
        private readonly Dictionary<RectTransform, Vector2> wheelDefaults = new Dictionary<RectTransform, Vector2>();

        private Sequence activeSequence;

        public LoadingPanelIntroAnimator(LoadingPanelView view)
        {
            this.view = view;
        }

        public void Kill()
        {
            activeSequence?.Kill();
            activeSequence = null;
            view?.KillAllTweens();
        }

        public Sequence PlayIntro(LoadingPresentation presentation, Action onComplete)
        {
            Kill();
            RefreshRestCache();
            PrepareIntroState(presentation);

            Sequence sequence = DOTween.Sequence().SetUpdate(true);
            float timeline = 0f;

            if (presentation == LoadingPresentation.Full && view.Background != null)
            {
                sequence.Insert(timeline, view.Background.DOFade(1f, 0.22f).SetEase(Ease.OutQuad));
            }
            else if (presentation == LoadingPresentation.Quick && view.HasQuickTransitionMask)
            {
                AppendQuickMaskIntro(sequence, ref timeline);
            }

            timeline += presentation == LoadingPresentation.Quick ? 0.04f : 0.08f;
            AppendWheelIntro(sequence, ref timeline);
            AppendRingPulse(sequence, ref timeline);

            if (presentation == LoadingPresentation.Full)
            {
                AppendLetterSlams(sequence, view.WordLetters, ref timeline);
                AppendSparkBurst(sequence, view.LeftSparkles, view.RightSparkles, ref timeline);
                AppendLetterSlams(sequence, view.WheelLetters, ref timeline);
                AppendSparkBurst(sequence, view.LeftSparkles, view.RightSparkles, ref timeline);
                AppendFinalBreath(sequence, ref timeline);
            }
            else
            {
                AppendLetterSlams(sequence, view.WordLetters, ref timeline, letterStagger: 0.05f);
                AppendLetterSlams(sequence, view.WheelLetters, ref timeline, letterStagger: 0.05f);
                AppendFinalBreath(sequence, ref timeline, scale: 1.03f, duration: 0.1f);
            }

            timeline += 0.05f;
            sequence.InsertCallback(timeline, () => onComplete?.Invoke());

            activeSequence = sequence;
            sequence.Play();
            return sequence;
        }

        public Sequence PlayOutro(LoadingPresentation presentation, Action onComplete)
        {
            Kill();

            Sequence sequence = DOTween.Sequence().SetUpdate(true);
            float timeline = 0f;
            bool quick = presentation == LoadingPresentation.Quick;

            if (view.LogoAssembly != null)
            {
                sequence.Insert(
                    timeline,
                    view.LogoAssembly.DOScale(quick ? 0.97f : 0.94f, quick ? 0.08f : 0.14f).SetEase(Ease.InBack));
            }

            timeline += quick ? 0.04f : 0.08f;

            AppendLetterOutro(sequence, view.WheelLetters, ref timeline, quick);
            AppendLetterOutro(sequence, view.WordLetters, ref timeline, quick);

            AppendWheelOutro(sequence, ref timeline, quick);

            if (view.LogoAssembly != null)
            {
                sequence.Insert(timeline, view.LogoAssembly.DOScale(0.75f, quick ? 0.12f : 0.18f).SetEase(Ease.InQuad));
            }

            if (presentation == LoadingPresentation.Full && view.Background != null)
            {
                sequence.Insert(
                    timeline + 0.04f,
                    view.Background.DOFade(0f, 0.22f).SetEase(Ease.InQuad));
            }
            else if (quick && view.HasQuickTransitionMask)
            {
                AppendQuickMaskOutro(sequence, ref timeline);
            }

            timeline += quick
                ? (view.HasQuickTransitionMask ? 0.04f : 0.18f)
                : 0.28f;
            sequence.InsertCallback(timeline, () =>
            {
                view.RestoreNeutralVisualState();
                onComplete?.Invoke();
            });

            activeSequence = sequence;
            sequence.Play();
            return sequence;
        }

        private void RefreshRestCache()
        {
            wheelDefaults.Clear();
            letterDefaults.Clear();

            view.RestoreNeutralVisualState();

            IReadOnlyList<RectTransform> wheelPieces = view.GetActiveWheelPieces();
            for (int index = 0; index < wheelPieces.Count; index++)
            {
                RectTransform piece = wheelPieces[index];
                if (piece != null)
                {
                    wheelDefaults[piece] = piece.anchoredPosition;
                }
            }

            CacheLetterDefaults(view.WordLetters);
            CacheLetterDefaults(view.WheelLetters);
        }

        private void CacheLetterDefaults(RectTransform[] letters)
        {
            if (letters == null)
            {
                return;
            }

            for (int index = 0; index < letters.Length; index++)
            {
                RectTransform letter = letters[index];
                if (letter != null)
                {
                    letterDefaults[letter] = letter.anchoredPosition;
                }
            }
        }

        private void PrepareIntroState(LoadingPresentation presentation)
        {
            view.SetBackgroundForPresentation(presentation);
            view.SetQuickMaskForPresentation(presentation);

            if (view.LogoAssembly != null)
            {
                view.LogoAssembly.localScale = Vector3.one;
            }

            if (view.WheelRoot != null)
            {
                view.WheelRoot.localScale = Vector3.one;
            }

            PrepareWheelHidden();
            PrepareLettersHidden(view.WordLetters, presentation);
            PrepareLettersHidden(view.WheelLetters, presentation);
            PrepareSparklesHidden(view.LeftSparkles, presentation);
            PrepareSparklesHidden(view.RightSparkles, presentation);
        }

        private void PrepareWheelHidden()
        {
            IReadOnlyList<RectTransform> pieces = view.GetActiveWheelPieces();
            bool isHalfLayout = pieces.Count <= 2;

            for (int index = 0; index < pieces.Count; index++)
            {
                RectTransform piece = pieces[index];
                if (piece == null)
                {
                    continue;
                }

                Vector2 restPosition = wheelDefaults.TryGetValue(piece, out Vector2 cached)
                    ? cached
                    : piece.anchoredPosition;

                CanvasGroup group = GetOrAddCanvasGroup(piece);
                piece.localScale = Vector3.zero;
                group.alpha = 0f;

                if (isHalfLayout)
                {
                    bool fromLeft = index == 0;
                    float offset = fromLeft ? -360f : 360f;
                    piece.anchoredPosition = restPosition + new Vector2(offset, 0f);
                }
                else
                {
                    piece.anchoredPosition = restPosition;
                }
            }
        }

        private void PrepareLettersHidden(RectTransform[] letters, LoadingPresentation presentation)
        {
            if (letters == null)
            {
                return;
            }

            for (int index = 0; index < letters.Length; index++)
            {
                RectTransform letter = letters[index];
                if (letter == null)
                {
                    continue;
                }

                CanvasGroup group = GetOrAddCanvasGroup(letter);
                Vector2 restPosition = letterDefaults.TryGetValue(letter, out Vector2 cached)
                    ? cached
                    : letter.anchoredPosition;

                float lift = presentation == LoadingPresentation.Full ? 70f : 48f;
                float startScale = presentation == LoadingPresentation.Full ? 1.8f : 1.35f;

                letter.anchoredPosition = restPosition + new Vector2(0f, lift);
                letter.localScale = Vector3.one * startScale;
                letter.localEulerAngles = Vector3.zero;
                group.alpha = 0f;
            }
        }

        private static void PrepareSparklesHidden(RectTransform[] sparkles, LoadingPresentation presentation)
        {
            if (sparkles == null)
            {
                return;
            }

            for (int index = 0; index < sparkles.Length; index++)
            {
                RectTransform sparkle = sparkles[index];
                if (sparkle == null)
                {
                    continue;
                }

                CanvasGroup group = GetOrAddCanvasGroup(sparkle);
                sparkle.localScale = Vector3.zero;
                sparkle.localEulerAngles = Vector3.zero;
                group.alpha = 0f;
            }
        }

        private void AppendWheelIntro(Sequence sequence, ref float timeline)
        {
            IReadOnlyList<RectTransform> pieces = view.GetActiveWheelPieces();
            if (pieces.Count == 0)
            {
                return;
            }

            bool isHalfLayout = pieces.Count <= 2;
            float stagger = isHalfLayout ? HalfStagger : SegmentStagger;

            for (int index = 0; index < pieces.Count; index++)
            {
                RectTransform piece = pieces[index];
                if (piece == null)
                {
                    continue;
                }

                Vector2 target = wheelDefaults.TryGetValue(piece, out Vector2 cached)
                    ? cached
                    : piece.anchoredPosition;

                sequence.Insert(
                    timeline + index * stagger,
                    isHalfLayout
                        ? CreateHalfSnapIn(piece, target)
                        : CreateSegmentSnapIn(piece));
            }

            timeline += pieces.Count * stagger + 0.08f;
        }

        private static Tween CreateHalfSnapIn(RectTransform half, Vector2 targetPosition)
        {
            CanvasGroup group = GetOrAddCanvasGroup(half);

            Sequence snap = DOTween.Sequence().SetUpdate(true);
            snap.Append(group.DOFade(1f, 0.06f));
            snap.Join(half.DOAnchorPos(targetPosition, 0.18f).SetEase(Ease.OutBack));
            snap.Join(half.DOScale(1f, 0.18f).SetEase(Ease.OutBack));
            snap.Append(half.DOShakeAnchorPos(0.1f, new Vector2(10f, 0f), 10, 90f, false, true));
            return snap;
        }

        private static Tween CreateSegmentSnapIn(RectTransform segment)
        {
            CanvasGroup group = GetOrAddCanvasGroup(segment);

            Sequence snap = DOTween.Sequence().SetUpdate(true);
            snap.Append(segment.DOScale(1.18f, 0.11f).SetEase(Ease.OutBack));
            snap.Join(group.DOFade(1f, 0.08f));
            snap.Append(segment.DOScale(1f, 0.07f).SetEase(Ease.OutQuad));
            snap.Join(segment.DOShakeRotation(0.16f, new Vector3(0f, 0f, 10f), 12, 90f, true));
            return snap;
        }

        private void AppendRingPulse(Sequence sequence, ref float timeline)
        {
            if (view.WheelRoot == null)
            {
                return;
            }

            view.WheelRoot.localScale = Vector3.one;
            sequence.Insert(timeline, view.WheelRoot.DOScale(1.08f, 0.1f).SetEase(Ease.OutQuad));
            sequence.Insert(timeline + 0.1f, view.WheelRoot.DOScale(1f, 0.12f).SetEase(Ease.OutBack));

            timeline += 0.22f;
        }

        private void AppendLetterSlams(Sequence sequence, RectTransform[] letters, ref float timeline, float letterStagger = LetterStagger)
        {
            if (letters == null)
            {
                return;
            }

            for (int index = 0; index < letters.Length; index++)
            {
                RectTransform letter = letters[index];
                if (letter == null)
                {
                    continue;
                }

                sequence.Insert(timeline + index * letterStagger, CreateLetterSlam(letter));
            }

            timeline += letters.Length * letterStagger + 0.1f;
        }

        private Tween CreateLetterSlam(RectTransform letter)
        {
            CanvasGroup group = GetOrAddCanvasGroup(letter);
            Vector2 targetPosition = letterDefaults.TryGetValue(letter, out Vector2 cached)
                ? cached
                : letter.anchoredPosition;

            Sequence slam = DOTween.Sequence().SetUpdate(true);
            slam.Append(group.DOFade(1f, 0.04f));
            slam.Join(letter.DOAnchorPos(targetPosition, 0.16f).SetEase(Ease.OutBack));
            slam.Join(letter.DOScale(1f, 0.16f).SetEase(Ease.OutBack));
            slam.Append(letter.DOShakeAnchorPos(0.12f, new Vector2(8f, 0f), 12, 90f, false, true));
            return slam;
        }

        private static void AppendSparkBurst(Sequence sequence, RectTransform[] left, RectTransform[] right, ref float timeline)
        {
            AppendSparkGroup(sequence, left, timeline);
            AppendSparkGroup(sequence, right, timeline);
            timeline += 0.14f;
        }

        private static void AppendSparkGroup(Sequence sequence, RectTransform[] sparkles, float atTime)
        {
            if (sparkles == null)
            {
                return;
            }

            for (int index = 0; index < sparkles.Length; index++)
            {
                RectTransform sparkle = sparkles[index];
                if (sparkle == null)
                {
                    continue;
                }

                CanvasGroup group = GetOrAddCanvasGroup(sparkle);
                Sequence burst = DOTween.Sequence().SetUpdate(true);
                burst.Append(sparkle.DOScale(1.25f, 0.1f).SetEase(Ease.OutBack));
                burst.Join(group.DOFade(1f, 0.06f));
                burst.Append(sparkle.DOScale(1f, 0.08f).SetEase(Ease.OutQuad));
                sequence.Insert(atTime + index * 0.03f, burst);
            }
        }

        private void AppendFinalBreath(Sequence sequence, ref float timeline, float scale = 1.05f, float duration = 0.14f)
        {
            if (view.LogoAssembly == null)
            {
                return;
            }

            view.LogoAssembly.localScale = Vector3.one;
            sequence.Insert(
                timeline,
                view.LogoAssembly.DOScale(scale, duration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(2, LoopType.Yoyo));

            timeline += duration * 2f + 0.02f;
        }

        private static void AppendLetterOutro(Sequence sequence, RectTransform[] letters, ref float timeline, bool quick)
        {
            if (letters == null)
            {
                return;
            }

            float stagger = quick ? 0.02f : 0.03f;
            float duration = quick ? 0.08f : 0.1f;

            for (int index = letters.Length - 1; index >= 0; index--)
            {
                RectTransform letter = letters[index];
                if (letter == null)
                {
                    continue;
                }

                CanvasGroup group = GetOrAddCanvasGroup(letter);
                float at = timeline + (letters.Length - 1 - index) * stagger;
                sequence.Insert(at, group.DOFade(0f, duration));
                sequence.Insert(at, letter.DOScale(0.6f, duration).SetEase(Ease.InBack));
            }

            timeline += quick ? 0.1f : 0.14f;
        }

        private void AppendWheelOutro(Sequence sequence, ref float timeline, bool quick)
        {
            IReadOnlyList<RectTransform> pieces = view.GetActiveWheelPieces();
            if (pieces.Count == 0)
            {
                return;
            }

            float stagger = quick ? 0.03f : 0.04f;

            for (int index = pieces.Count - 1; index >= 0; index--)
            {
                RectTransform piece = pieces[index];
                if (piece == null)
                {
                    continue;
                }

                CanvasGroup group = GetOrAddCanvasGroup(piece);
                float at = timeline + (pieces.Count - 1 - index) * stagger;
                sequence.Insert(at, piece.DOScale(0f, quick ? 0.08f : 0.1f).SetEase(Ease.InBack));
                sequence.Insert(at, group.DOFade(0f, quick ? 0.06f : 0.08f));
            }

            timeline += quick ? 0.16f : 0.34f;
        }

        private void AppendQuickMaskIntro(Sequence sequence, ref float timeline)
        {
            RectTransform mask = view.QuickTransitionMask;
            CanvasGroup group = view.QuickTransitionMaskGroup;
            if (mask == null)
            {
                return;
            }

            float coverScale = view.GetQuickMaskCoverScale();
            mask.localScale = Vector3.zero;
            if (group != null)
            {
                group.alpha = 0f;
            }

            sequence.Insert(timeline, mask.DOScale(coverScale, QuickMaskIntroDuration).SetEase(Ease.OutExpo));
            if (group != null)
            {
                sequence.Insert(
                    timeline,
                    group.DOFade(1f, QuickMaskIntroDuration * 0.75f).SetEase(Ease.OutQuad));
            }

            timeline += QuickMaskIntroDuration * 0.55f;
        }

        private void AppendQuickMaskOutro(Sequence sequence, ref float timeline)
        {
            RectTransform mask = view.QuickTransitionMask;
            CanvasGroup group = view.QuickTransitionMaskGroup;
            if (mask == null)
            {
                return;
            }

            sequence.Insert(timeline, mask.DOScale(0f, QuickMaskOutroDuration).SetEase(Ease.InCubic));
            if (group != null)
            {
                sequence.Insert(timeline, group.DOFade(0f, QuickMaskOutroDuration).SetEase(Ease.InQuad));
            }

            timeline += QuickMaskOutroDuration;
        }

        private static CanvasGroup GetOrAddCanvasGroup(RectTransform target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = target.gameObject.AddComponent<CanvasGroup>();
            }

            return group;
        }
    }
}
