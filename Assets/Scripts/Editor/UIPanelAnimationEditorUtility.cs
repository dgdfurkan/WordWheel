using System.Collections.Generic;
using Runtime.UI.Core;
using Runtime.UI.Data;
using Runtime.UI.Enums;
using UnityEditor;
using UnityEngine;

namespace Runtime.UI.Editor
{
    public static class UIPanelAnimationEditorUtility
    {
        public static void AddSelectionToNewGroup(UIPanel panel)
        {
            if (panel == null)
            {
                return;
            }

            List<RectTransform> selected = GetSelectedRectTransforms(panel);
            if (selected.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "UI Animation",
                    "Select one or more RectTransforms from this panel in the Hierarchy.",
                    "OK");
                return;
            }

            Undo.RecordObject(panel, "Add UI Animation Group");

            List<UIPanelElementAnimationEntry> entries = new List<UIPanelElementAnimationEntry>();
            for (int i = 0; i < selected.Count; i++)
            {
                UIPanelElementAnimationType type = i % 2 == 0
                    ? UIPanelElementAnimationType.PopIn
                    : UIPanelElementAnimationType.FadeAndScale;

                entries.Add(UIPanelElementAnimationEntry.CreateDefault(selected[i], i * 0.08f, type));
            }

            string groupName = $"Group {panel.ChildAnimations.Groups.Length + 1}";
            panel.ChildAnimations.AddGroup(UIPanelElementAnimationGroup.Create(groupName, entries.ToArray()));

            panel.RefreshChildAnimationRegistry();
            EditorUtility.SetDirty(panel);
        }

        public static void FocusSelection()
        {
            if (Selection.activeTransform == null)
            {
                return;
            }

            SceneView.lastActiveSceneView?.FrameSelected();
        }

        public static void DrawPanelGizmos(UIPanel panel)
        {
            if (panel == null || panel.ChildAnimations == null || !panel.ChildAnimations.Enabled)
            {
                return;
            }

            foreach (UIPanelElementAnimationGroup group in panel.ChildAnimations.Groups)
            {
                if (group == null || !group.Enabled)
                {
                    continue;
                }

                foreach (UIPanelElementAnimationEntry entry in group.Elements)
                {
                    if (entry == null || entry.Target == null)
                    {
                        continue;
                    }

                    DrawEntryGizmo(entry);
                }
            }
        }

        private static void DrawEntryGizmo(UIPanelElementAnimationEntry entry)
        {
            RectTransform target = entry.Target;
            Vector3 targetCenter = target.position;

            Handles.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            Handles.DrawWireDisc(targetCenter, Vector3.forward, 12f);
            Handles.Label(targetCenter + Vector3.up * 18f, entry.GetListLabel(), EditorStyles.whiteMiniLabel);

            if (entry.UsesSceneCapture && entry.HasValidSceneCapture)
            {
                Vector3 startWorld = AnchoredPositionToWorld(target, entry.SceneCaptureFrom);
                Vector3 endWorld = AnchoredPositionToWorld(target, entry.SceneCaptureTo);

                Handles.color = new Color(0.3f, 0.95f, 0.45f, 1f);
                Handles.DrawSolidDisc(startWorld, Vector3.forward, 8f);
                Handles.Label(startWorld + Vector3.left * 10f, "START", EditorStyles.boldLabel);

                Handles.color = new Color(0.35f, 0.75f, 1f, 1f);
                Handles.DrawSolidDisc(endWorld, Vector3.forward, 8f);
                Handles.Label(endWorld + Vector3.right * 10f, "END", EditorStyles.boldLabel);

                Handles.color = new Color(1f, 1f, 1f, 0.85f);
                Handles.DrawAAPolyLine(4f, startWorld, endWorld);

                Vector3 direction = (endWorld - startWorld).normalized;
                if (direction.sqrMagnitude > 0.001f)
                {
                    Vector3 arrowHead = endWorld - direction * 16f;
                    Handles.DrawAAPolyLine(3f, arrowHead + Vector3.up * 6f, endWorld, arrowHead - Vector3.up * 6f);
                }

                return;
            }

            if (entry.OpenType is UIPanelElementAnimationType.SlideFromLeft
                or UIPanelElementAnimationType.SlideFromRight
                or UIPanelElementAnimationType.SlideFromTop
                or UIPanelElementAnimationType.SlideFromBottom
                or UIPanelElementAnimationType.FloatUp
                or UIPanelElementAnimationType.DropBounce)
            {
                Vector3 dir = GetSlideDirection(entry.OpenType);
                Handles.color = new Color(0.7f, 0.7f, 1f, 0.8f);
                Handles.DrawAAPolyLine(3f, targetCenter, targetCenter + dir * 80f);
            }
        }

        private static Vector3 GetSlideDirection(UIPanelElementAnimationType type)
        {
            return type switch
            {
                UIPanelElementAnimationType.SlideFromLeft => Vector3.left,
                UIPanelElementAnimationType.SlideFromRight => Vector3.right,
                UIPanelElementAnimationType.SlideFromTop => Vector3.up,
                UIPanelElementAnimationType.SlideFromBottom => Vector3.down,
                UIPanelElementAnimationType.FloatUp => Vector3.up,
                UIPanelElementAnimationType.DropBounce => Vector3.down,
                _ => Vector3.zero
            };
        }

        private static Vector3 AnchoredPositionToWorld(RectTransform target, Vector2 anchoredPosition)
        {
            Vector2 backup = target.anchoredPosition;
            target.anchoredPosition = anchoredPosition;
            Vector3 world = target.position;
            target.anchoredPosition = backup;
            return world;
        }

        private static List<RectTransform> GetSelectedRectTransforms(UIPanel panel)
        {
            List<RectTransform> results = new List<RectTransform>();
            Transform[] selection = Selection.transforms;
            for (int i = 0; i < selection.Length; i++)
            {
                if (!selection[i].TryGetComponent(out RectTransform rect))
                {
                    continue;
                }

                if (rect.GetComponentInParent<UIPanel>() != panel)
                {
                    continue;
                }

                results.Add(rect);
            }

            return results;
        }
    }
}
