using System;
using System.Collections.Generic;
using Runtime.UI.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.UI.Data
{
    [Serializable]
    public class UIPanelChildAnimationSettings
    {
        [ToggleLeft, LabelText("Enable Child Animations")]
        [SerializeField] private bool enabled;

        [ShowIf(nameof(enabled))]
        [Title("Open Timing")]
        [LabelText("Open Sequence")]
        [EnumPaging]
        [SerializeField] private UIPanelChildAnimationTiming timing = UIPanelChildAnimationTiming.AfterPanelStarts;

        [ShowIf("@enabled && timing == UIPanelChildAnimationTiming.AfterPanelStarts")]
        [LabelText("Delay After Panel Starts")]
        [PropertyRange(0f, 1f)]
        [SerializeField] private float panelStartOffset = 0.12f;

        [ShowIf(nameof(enabled))]
        [Title("Close Timing")]
        [InfoBox(
            "On close, child elements always animate out BEFORE the panel hides. This prevents the panel canvas fade from cutting off exit animations.",
            InfoMessageType.Info)]
        [LabelText("Gap Before Panel Close")]
        [PropertyRange(0f, 0.5f)]
        [SerializeField] private float closeDelayBeforePanel = 0.05f;

        [ShowIf(nameof(enabled))]
        [Title("Quick Setup")]
        [Button("Apply Button Stagger Preset", ButtonSizes.Medium), GUIColor(0.45f, 0.85f, 1f)]
        private void ApplyButtonStaggerPreset()
        {
            timing = UIPanelChildAnimationTiming.AfterPanelStarts;
            panelStartOffset = 0.12f;
            closeDelayBeforePanel = 0.08f;
        }

        [ShowIf(nameof(enabled))]
        [Button("Apply Hero + Ornaments Preset", ButtonSizes.Medium), GUIColor(0.55f, 1f, 0.65f)]
        private void ApplyHeroPreset()
        {
            timing = UIPanelChildAnimationTiming.AfterPanelStarts;
            panelStartOffset = 0.18f;
            closeDelayBeforePanel = 0.1f;
        }

        [ShowIf(nameof(enabled))]
        [Title("Groups")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = nameof(UIPanelElementAnimationGroup.GroupName))]
        [SerializeField] private UIPanelElementAnimationGroup[] groups = Array.Empty<UIPanelElementAnimationGroup>();

        public bool Enabled => enabled;
        public UIPanelChildAnimationTiming Timing => timing;
        public float PanelStartOffset => Mathf.Max(0f, panelStartOffset);
        public float CloseDelayBeforePanel => Mathf.Max(0f, closeDelayBeforePanel);
        public UIPanelElementAnimationGroup[] Groups => groups ?? Array.Empty<UIPanelElementAnimationGroup>();

        public void AddGroup(UIPanelElementAnimationGroup group)
        {
            if (group == null)
            {
                return;
            }

            List<UIPanelElementAnimationGroup> updated = new List<UIPanelElementAnimationGroup>(Groups);
            updated.Add(group);
            groups = updated.ToArray();
        }
    }
}
