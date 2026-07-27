using System;
using System.Collections.Generic;
using DG.Tweening;
using Runtime.UI.Data;
using Runtime.UI.Enums;
using UnityEngine;

namespace Runtime.UI.Configuration
{
    public sealed class UIPanelAnimationSetup
    {
        private readonly UIPanelAnimationSettings openSettings;
        private readonly UIPanelAnimationSettings closeSettings;
        private readonly UIPanelChildAnimationSettings childSettings;
        private readonly List<UIPanelElementAnimationGroup> groups = new List<UIPanelElementAnimationGroup>();

        internal bool ChildEnabled { get; private set; }
        internal UIPanelChildAnimationTiming ChildTiming { get; private set; } = UIPanelChildAnimationTiming.AfterPanelStarts;
        internal float PanelStartOffset { get; private set; } = 0.12f;
        internal float CloseGap { get; private set; } = 0.05f;
        internal bool ChildConfigured { get; private set; }

        internal bool HasChanges { get; private set; }

        internal UIPanelAnimationSetup(
            UIPanelAnimationSettings open,
            UIPanelAnimationSettings close,
            UIPanelChildAnimationSettings children)
        {
            openSettings = open;
            closeSettings = close;
            childSettings = children;
        }

        public PanelAnimationBuilder Open => new PanelAnimationBuilder(openSettings, MarkChanged);
        public PanelAnimationBuilder Close => new PanelAnimationBuilder(closeSettings, MarkChanged);
        public ChildAnimationBuilder Children => new ChildAnimationBuilder(this);

        internal void MarkChanged()
        {
            HasChanges = true;
        }

        internal void Commit()
        {
            if (!ChildConfigured)
            {
                return;
            }

            childSettings.Apply(ChildEnabled, ChildTiming, PanelStartOffset, CloseGap, groups.ToArray());
        }

        internal void SetChildTiming(
            bool enabled,
            UIPanelChildAnimationTiming timing,
            float startOffset,
            float closeDelay)
        {
            ChildConfigured = true;
            ChildEnabled = enabled;
            ChildTiming = timing;
            PanelStartOffset = startOffset;
            CloseGap = closeDelay;
            MarkChanged();
        }

        internal void AddGroup(UIPanelElementAnimationGroup group)
        {
            if (group == null)
            {
                return;
            }

            groups.Add(group);
            MarkChanged();
        }
    }

    public sealed class PanelAnimationBuilder
    {
        private readonly UIPanelAnimationSettings settings;
        private readonly Action markChanged;

        internal PanelAnimationBuilder(UIPanelAnimationSettings settings, Action markChanged)
        {
            this.settings = settings;
            this.markChanged = markChanged;
        }

        public PanelAnimationBuilder None()
        {
            settings.Apply(UIPanelAnimationType.None);
            markChanged();
            return this;
        }

        public PanelAnimationBuilder Fade(float duration = 0.4f, Ease ease = Ease.OutQuad, bool unscaledTime = false)
        {
            settings.Apply(UIPanelAnimationType.Fade, duration, ease, unscaledTime: unscaledTime);
            markChanged();
            return this;
        }

        public PanelAnimationBuilder SlideFromLeft(
            float duration = 0.4f,
            Ease ease = Ease.OutQuad,
            float slideOffset = 100f,
            bool useScreenWidth = true,
            bool unscaledTime = false)
        {
            settings.Apply(
                UIPanelAnimationType.SlideFromLeft,
                duration,
                ease,
                slideOffset,
                useScreenWidth,
                unscaledTime);
            markChanged();
            return this;
        }

        public PanelAnimationBuilder SlideToRight(
            float duration = 0.4f,
            Ease ease = Ease.OutQuad,
            float slideOffset = 100f,
            bool useScreenWidth = true,
            bool unscaledTime = false)
        {
            settings.Apply(
                UIPanelAnimationType.SlideToRight,
                duration,
                ease,
                slideOffset,
                useScreenWidth,
                unscaledTime);
            markChanged();
            return this;
        }

        public PanelAnimationBuilder Configure(
            UIPanelAnimationType type,
            float duration = 0.4f,
            Ease ease = Ease.OutQuad,
            float slideOffset = 100f,
            bool useScreenWidth = true,
            bool unscaledTime = false)
        {
            settings.Apply(type, duration, ease, slideOffset, useScreenWidth, unscaledTime);
            markChanged();
            return this;
        }
    }

    public sealed class ChildAnimationBuilder
    {
        private readonly UIPanelAnimationSetup setup;

        internal ChildAnimationBuilder(UIPanelAnimationSetup setup)
        {
            this.setup = setup;
        }

        public ChildAnimationBuilder Enable(bool enabled = true)
        {
            setup.SetChildTiming(enabled, UIPanelChildAnimationTiming.AfterPanelStarts, 0.12f, 0.05f);
            return this;
        }

        public ChildAnimationBuilder Disable()
        {
            setup.SetChildTiming(false, UIPanelChildAnimationTiming.AfterPanelStarts, 0f, 0f);
            return this;
        }

        public ChildAnimationBuilder AfterPanelStarts(float offset = 0.12f)
        {
            setup.SetChildTiming(true, UIPanelChildAnimationTiming.AfterPanelStarts, offset, setup.CloseGap);
            return this;
        }

        public ChildAnimationBuilder WithPanel(float startOffset, float closeDelayBeforePanel)
        {
            setup.SetChildTiming(true, UIPanelChildAnimationTiming.AfterPanelStarts, startOffset, closeDelayBeforePanel);
            return this;
        }

        public ElementGroupBuilder Group(string groupName)
        {
            if (!setup.ChildConfigured)
            {
                setup.SetChildTiming(true, UIPanelChildAnimationTiming.AfterPanelStarts, 0.12f, 0.05f);
            }

            return new ElementGroupBuilder(setup, groupName);
        }
    }

    public sealed class ElementGroupBuilder
    {
        private readonly UIPanelAnimationSetup setup;
        private readonly string groupName;
        private readonly List<UIPanelElementAnimationEntry> entries = new List<UIPanelElementAnimationEntry>();
        private float staggerInterval = 0.08f;
        private float groupStartDelay;
        private bool reverseStaggerOnClose = true;
        private bool groupEnabled = true;
        private bool playOnOpen = true;
        private bool playOnClose = true;

        internal ElementGroupBuilder(UIPanelAnimationSetup setup, string groupName)
        {
            this.setup = setup;
            this.groupName = groupName;
        }

        public ElementGroupBuilder Stagger(float interval, float startDelay = 0f)
        {
            staggerInterval = interval;
            groupStartDelay = startDelay;
            return this;
        }

        public ElementGroupBuilder NoStagger()
        {
            staggerInterval = 0f;
            return this;
        }

        public ElementGroupBuilder ReverseStaggerOnClose(bool value = true)
        {
            reverseStaggerOnClose = value;
            return this;
        }

        public ElementGroupBuilder PlayOnOpen(bool value = true)
        {
            playOnOpen = value;
            return this;
        }

        public ElementGroupBuilder PlayOnClose(bool value = true)
        {
            playOnClose = value;
            return this;
        }

        public ElementAnimationBuilder Entry(RectTransform target, string label = null)
        {
            return new ElementAnimationBuilder(this, target, label);
        }

        internal void AddEntry(UIPanelElementAnimationEntry entry)
        {
            if (entry == null || entry.Target == null)
            {
                return;
            }

            entries.Add(entry);
        }

        public ChildAnimationBuilder EndGroup()
        {
            setup.AddGroup(UIPanelElementAnimationGroup.Create(
                groupName,
                entries.ToArray(),
                staggerInterval,
                groupStartDelay,
                reverseStaggerOnClose,
                groupEnabled,
                playOnOpen,
                playOnClose));

            return setup.Children;
        }
    }

    public sealed class ElementAnimationBuilder
    {
        private readonly ElementGroupBuilder group;
        private readonly RectTransform target;
        private readonly string label;
        private UIPanelElementAnimationType type = UIPanelElementAnimationType.PopIn;
        private float delay;
        private float duration = 0.45f;
        private Ease entryEase = Ease.OutCubic;
        private float slideDistance = 120f;
        private bool screenRelativeSlide;
        private Vector2 moveOffset;
        private bool mirrorClose = true;
        private bool unscaledTime;
        private Vector2? captureFrom;
        private Vector2? captureTo;

        internal ElementAnimationBuilder(ElementGroupBuilder group, RectTransform target, string label)
        {
            this.group = group;
            this.target = target;
            this.label = string.IsNullOrWhiteSpace(label) ? target?.name : label;
        }

        public ElementAnimationBuilder Delay(float value)
        {
            delay = value;
            return this;
        }

        public ElementAnimationBuilder Duration(float value)
        {
            duration = value;
            return this;
        }

        public ElementAnimationBuilder WithEase(Ease value)
        {
            entryEase = value;
            return this;
        }

        public ElementAnimationBuilder MirrorClose(bool mirror = true)
        {
            mirrorClose = mirror;
            return this;
        }

        public ElementAnimationBuilder SlideDistance(float distance, bool screenRelative = false)
        {
            slideDistance = distance;
            screenRelativeSlide = screenRelative;
            return this;
        }

        public ElementAnimationBuilder MoveOffset(Vector2 offset)
        {
            moveOffset = offset;
            return this;
        }

        public ElementAnimationBuilder SceneCapture(Vector2 from, Vector2 to)
        {
            type = UIPanelElementAnimationType.SceneCapture;
            captureFrom = from;
            captureTo = to;
            return this;
        }

        public ElementAnimationBuilder PopIn(float animDuration = 0.45f, Ease animEase = Ease.OutBack, float animDelay = 0f)
        {
            type = UIPanelElementAnimationType.PopIn;
            duration = animDuration;
            entryEase = animEase;
            delay = animDelay;
            return this;
        }

        public ElementAnimationBuilder PunchScale(float animDuration = 0.6f, Ease animEase = Ease.OutCubic, float animDelay = 0f)
        {
            type = UIPanelElementAnimationType.PunchScale;
            duration = animDuration;
            entryEase = animEase;
            delay = animDelay;
            return this;
        }

        public ElementAnimationBuilder SlideFromLeft(float distance = 120f, float animDuration = 0.45f, Ease animEase = Ease.OutCubic, float animDelay = 0f)
        {
            type = UIPanelElementAnimationType.SlideFromLeft;
            slideDistance = distance;
            duration = animDuration;
            entryEase = animEase;
            delay = animDelay;
            return this;
        }

        public ElementAnimationBuilder SlideFromRight(float distance = 120f, float animDuration = 0.45f, Ease animEase = Ease.OutCubic, float animDelay = 0f)
        {
            type = UIPanelElementAnimationType.SlideFromRight;
            slideDistance = distance;
            duration = animDuration;
            entryEase = animEase;
            delay = animDelay;
            return this;
        }

        public ElementAnimationBuilder SlideFromTop(float distance = 120f, float animDuration = 0.45f, Ease animEase = Ease.OutCubic, float animDelay = 0f)
        {
            type = UIPanelElementAnimationType.SlideFromTop;
            slideDistance = distance;
            duration = animDuration;
            entryEase = animEase;
            delay = animDelay;
            return this;
        }

        public ElementAnimationBuilder SlideFromBottom(float distance = 120f, float animDuration = 0.45f, Ease animEase = Ease.OutCubic, float animDelay = 0f)
        {
            type = UIPanelElementAnimationType.SlideFromBottom;
            slideDistance = distance;
            duration = animDuration;
            entryEase = animEase;
            delay = animDelay;
            return this;
        }

        public ElementAnimationBuilder FadeAndScale(float animDuration = 0.45f, Ease animEase = Ease.OutCubic, float animDelay = 0f)
        {
            type = UIPanelElementAnimationType.FadeAndScale;
            duration = animDuration;
            entryEase = animEase;
            delay = animDelay;
            return this;
        }

        public ElementAnimationBuilder DriftIn(float animDuration = 0.45f, Ease animEase = Ease.OutCubic, float animDelay = 0f, Vector2 offset = default)
        {
            type = UIPanelElementAnimationType.DriftIn;
            duration = animDuration;
            entryEase = animEase;
            delay = animDelay;
            moveOffset = offset;
            return this;
        }

        public ElementGroupBuilder EndEntry()
        {
            if (target != null)
            {
                UIPanelElementAnimationEntry entry = new UIPanelElementAnimationEntry();
                entry.ApplyScriptConfiguration(
                    target,
                    label,
                    type,
                    delay,
                    duration,
                    entryEase,
                    slideDistance,
                    screenRelativeSlide,
                    moveOffset,
                    mirrorClose: mirrorClose,
                    unscaledTime: unscaledTime,
                    captureFrom: captureFrom,
                    captureTo: captureTo);

                group.AddEntry(entry);
            }

            return group;
        }
    }
}
