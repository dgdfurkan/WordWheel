using DG.Tweening;
using UnityEngine;

namespace Runtime.UI.Animation
{
    /// <summary>
    /// Immutable snapshot of a UI element's default visual state.
    /// </summary>
    public sealed class UIElementTransformSnapshot
    {
        public RectTransform Target { get; }
        public CanvasGroup CanvasGroup { get; }
        public Vector2 AnchoredPosition { get; }
        public Vector3 LocalScale { get; }
        public float RotationZ { get; }
        public float Alpha { get; }
        public bool HasCanvasGroup { get; }

        public UIElementTransformSnapshot(RectTransform target)
        {
            Target = target;
            CanvasGroup = target.GetComponent<CanvasGroup>();
            HasCanvasGroup = CanvasGroup != null;
            AnchoredPosition = target.anchoredPosition;
            LocalScale = target.localScale;
            RotationZ = target.localEulerAngles.z;
            Alpha = HasCanvasGroup ? CanvasGroup.alpha : 1f;
        }

        public CanvasGroup GetOrCreateCanvasGroup()
        {
            if (Target == null)
            {
                return null;
            }

            CanvasGroup existing = Target.GetComponent<CanvasGroup>();
            if (existing != null)
            {
                return existing;
            }

            return Target.gameObject.AddComponent<CanvasGroup>();
        }

        public void Restore()
        {
            if (Target == null)
            {
                return;
            }

            Target.anchoredPosition = AnchoredPosition;
            Target.localScale = LocalScale;
            Target.localEulerAngles = new Vector3(0f, 0f, RotationZ);

            CanvasGroup currentGroup = Target.GetComponent<CanvasGroup>();
            if (currentGroup != null)
            {
                currentGroup.alpha = Alpha;
            }
        }

        public void KillTweens()
        {
            if (Target == null)
            {
                return;
            }

            Target.DOKill();

            CanvasGroup currentGroup = Target.GetComponent<CanvasGroup>();
            if (currentGroup != null)
            {
                currentGroup.DOKill();
            }
        }
    }
}
