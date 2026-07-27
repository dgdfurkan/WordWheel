using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.UI.Data
{
    [Serializable]
    public class UIPanelElementAnimationGroup
    {
        [LabelText("Group Name")]
        [SerializeField] private string groupName = "Group";

        [HorizontalGroup("Toggles")]
        [ToggleLeft, LabelText("Enabled")]
        [SerializeField] private bool enabled = true;

        [HorizontalGroup("Toggles")]
        [ToggleLeft, LabelText("On Open")]
        [SerializeField] private bool playOnOpen = true;

        [HorizontalGroup("Toggles")]
        [ToggleLeft, LabelText("On Close")]
        [SerializeField] private bool playOnClose = true;

        [LabelText("Group Delay")]
        [SerializeField] private float groupStartDelay;

        [LabelText("Stagger Interval")]
        [PropertyRange(0f, 0.5f)]
        [SerializeField] private float staggerInterval = 0.08f;

        [ToggleLeft, LabelText("Reverse Stagger On Close")]
        [SerializeField] private bool reverseStaggerOnClose = true;

        [Title("Elements")]
        [ListDrawerSettings(
            ShowIndexLabels = false,
            ListElementLabelName = nameof(UIPanelElementAnimationEntry.GetListLabel),
            DraggableItems = true,
            ShowFoldout = true)]
        [SerializeField] private UIPanelElementAnimationEntry[] elements = Array.Empty<UIPanelElementAnimationEntry>();

        public string GroupName => groupName;
        public bool Enabled => enabled;
        public bool PlayOnOpen => playOnOpen;
        public bool PlayOnClose => playOnClose;
        public float GroupStartDelay => Mathf.Max(0f, groupStartDelay);
        public float StaggerInterval => Mathf.Max(0f, staggerInterval);
        public bool ReverseStaggerOnClose => reverseStaggerOnClose;
        public UIPanelElementAnimationEntry[] Elements => elements ?? Array.Empty<UIPanelElementAnimationEntry>();

        public static UIPanelElementAnimationGroup Create(string name, UIPanelElementAnimationEntry[] groupElements)
        {
            return new UIPanelElementAnimationGroup
            {
                groupName = name,
                elements = groupElements ?? Array.Empty<UIPanelElementAnimationEntry>()
            };
        }

        public static UIPanelElementAnimationGroup Create(
            string name,
            UIPanelElementAnimationEntry[] groupElements,
            float stagger,
            float groupDelay = 0f,
            bool reverseStaggerOnClose = true,
            bool groupEnabled = true,
            bool animateOnOpen = true,
            bool animateOnClose = true)
        {
            return new UIPanelElementAnimationGroup
            {
                groupName = name,
                enabled = groupEnabled,
                playOnOpen = animateOnOpen,
                playOnClose = animateOnClose,
                groupStartDelay = groupDelay,
                staggerInterval = stagger,
                reverseStaggerOnClose = reverseStaggerOnClose,
                elements = groupElements ?? Array.Empty<UIPanelElementAnimationEntry>()
            };
        }
    }
}
