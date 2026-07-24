using System.Collections.Generic;
using Runtime.UI.Data;
using UnityEngine;

namespace Runtime.UI.Animation
{
    /// <summary>
    /// Stores and restores default transform states for animated child elements.
    /// </summary>
    public sealed class UIElementAnimationStateRegistry
    {
        private readonly Dictionary<RectTransform, UIElementTransformSnapshot> snapshots =
            new Dictionary<RectTransform, UIElementTransformSnapshot>();

        public void RegisterFromSettings(UIPanelChildAnimationSettings settings)
        {
            snapshots.Clear();

            if (settings == null || !settings.Enabled)
            {
                return;
            }

            foreach (UIPanelElementAnimationGroup group in settings.Groups)
            {
                if (group == null || !group.Enabled)
                {
                    continue;
                }

                foreach (UIPanelElementAnimationEntry entry in group.Elements)
                {
                    if (entry == null || !entry.IsValid)
                    {
                        continue;
                    }

                    RegisterTarget(entry.Target);
                }
            }
        }

        public void RegisterTarget(RectTransform target)
        {
            if (target == null || snapshots.ContainsKey(target))
            {
                return;
            }

            snapshots[target] = new UIElementTransformSnapshot(target);
        }

        public bool TryGetSnapshot(RectTransform target, out UIElementTransformSnapshot snapshot)
        {
            return snapshots.TryGetValue(target, out snapshot);
        }

        public void RestoreAll()
        {
            foreach (UIElementTransformSnapshot snapshot in snapshots.Values)
            {
                snapshot.Restore();
            }
        }

        public void KillAllTweens()
        {
            foreach (UIElementTransformSnapshot snapshot in snapshots.Values)
            {
                snapshot.KillTweens();
            }
        }

        public IReadOnlyCollection<UIElementTransformSnapshot> GetAllSnapshots()
        {
            return snapshots.Values;
        }
    }
}
