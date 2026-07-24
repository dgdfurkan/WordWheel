using System;
using DG.Tweening;
using Runtime.UI.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.UI.Data
{
    [Serializable]
    public class UIPanelAnimationSettings
    {
        [LabelText("Type")]
        [EnumPaging]
        [SerializeField] private UIPanelAnimationType animationType = UIPanelAnimationType.Fade;

        [HideIf("@animationType == UIPanelAnimationType.None")]
        [LabelText("Duration")]
        [SerializeField] private float duration = 0.4f;

        [HideIf("@animationType == UIPanelAnimationType.None")]
        [SerializeField] private Ease ease = Ease.OutQuad;

        [ShowIf("@UsesSlide(animationType)")]
        [LabelText("Slide Distance")]
        [SerializeField] private float slideOffset = 100f;

        [ShowIf("@UsesSlide(animationType)")]
        [SerializeField] private bool useScreenWidthForSlide = true;

        [LabelText("Unscaled Time")]
        [SerializeField] private bool useUnscaledTime;

        public UIPanelAnimationType AnimationType => animationType;
        public float Duration => Mathf.Max(0f, duration);
        public Ease Ease => ease;
        public float SlideOffset => slideOffset;
        public bool UseScreenWidthForSlide => useScreenWidthForSlide;
        public bool UseUnscaledTime => useUnscaledTime;

        private static bool UsesSlide(UIPanelAnimationType type)
        {
            return type is UIPanelAnimationType.SlideFromLeft
                or UIPanelAnimationType.SlideToRight
                or UIPanelAnimationType.Entrance
                or UIPanelAnimationType.Exit;
        }
    }
}
