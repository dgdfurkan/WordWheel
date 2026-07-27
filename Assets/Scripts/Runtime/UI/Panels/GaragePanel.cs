using DG.Tweening;
using Runtime.UI.Configuration;
using Runtime.UI.Core;
using Runtime.UI.Enums;
using Runtime.UI.Utilities;
using UnityEngine;

namespace Runtime.UI.Panels
{
    public class GaragePanel : UIPanel
    {
        public override UIPanelDisplayMode DisplayMode => UIPanelDisplayMode.Overlay;

        [Header("Animation Targets")]
        [SerializeField] private RectTransform header;
        [SerializeField] private RectTransform backButton;
        [SerializeField] private RectTransform selectedPreview;
        [SerializeField] private RectTransform scrollView;
        [SerializeField] private RectTransform[] vehicleSlots = new RectTransform[6];

        protected override void ConfigurePanelAnimations(UIPanelAnimationSetup anim)
        {
            ResolveAnimationTargets();

            anim.Open.Fade(0.28f, Ease.OutQuad);
            anim.Close.Fade(0.22f, Ease.InQuad);

            anim.Children
                .WithPanel(0.06f, 0f)
                .Group("Garage Content")
                    .Stagger(0.045f)
                    .ReverseStaggerOnClose(true)
                    .Entry(header, "Header")
                        .PopIn(0.22f, Ease.OutBack)
                        .EndEntry()
                    .Entry(backButton, "Back")
                        .DriftIn(0.2f, Ease.OutCubic)
                        .EndEntry()
                    .Entry(selectedPreview, "Selected")
                        .PopIn(0.24f, Ease.OutBack)
                        .EndEntry()
                    .Entry(scrollView, "ScrollView")
                        .SlideFromBottom(100f, 0.22f, Ease.OutCubic)
                        .EndEntry()
                    .Entry(GetVehicleSlot(0), "VehicleSlot1")
                        .PopIn(0.2f, Ease.OutBack)
                        .EndEntry()
                    .Entry(GetVehicleSlot(1), "VehicleSlot2")
                        .PopIn(0.2f, Ease.OutBack)
                        .EndEntry()
                    .Entry(GetVehicleSlot(2), "VehicleSlot3")
                        .PopIn(0.2f, Ease.OutBack)
                        .EndEntry()
                    .Entry(GetVehicleSlot(3), "VehicleSlot4")
                        .PopIn(0.2f, Ease.OutBack)
                        .EndEntry()
                    .Entry(GetVehicleSlot(4), "VehicleSlot5")
                        .PopIn(0.2f, Ease.OutBack)
                        .EndEntry()
                    .Entry(GetVehicleSlot(5), "VehicleSlot6")
                        .PopIn(0.2f, Ease.OutBack)
                        .EndEntry()
                .EndGroup();
        }

        public void OnBackButtonClicked()
        {
            RectTransform target = backButton != null ? backButton : PanelTransform;
            UIAnimationHelper.BounceScale(target, 1.08f, 0.2f);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.ClosePanel<GaragePanel>();
            }).SetUpdate(true);
        }

        private RectTransform GetVehicleSlot(int index)
        {
            if (vehicleSlots == null || index < 0 || index >= vehicleSlots.Length)
            {
                return null;
            }

            return vehicleSlots[index];
        }

        private void ResolveAnimationTargets()
        {
            Transform bg = transform.Find("BG");
            if (bg == null)
            {
                return;
            }

            if (header == null)
            {
                header = bg.Find("Header") as RectTransform;
            }

            if (backButton == null)
            {
                backButton = bg.Find("Header (1)") as RectTransform;
            }

            if (selectedPreview == null)
            {
                selectedPreview = bg.Find("Selected") as RectTransform;
            }

            if (scrollView == null)
            {
                scrollView = bg.Find("Scroll View") as RectTransform;
            }

            Transform content = bg.Find("Scroll View/Viewport/Content");
            if (content == null)
            {
                return;
            }

            if (vehicleSlots == null || vehicleSlots.Length != 6)
            {
                vehicleSlots = new RectTransform[6];
            }

            for (int index = 0; index < vehicleSlots.Length && index < content.childCount; index++)
            {
                if (vehicleSlots[index] == null)
                {
                    vehicleSlots[index] = content.GetChild(index) as RectTransform;
                }
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            ResolveAnimationTargets();
        }
#endif
    }
}
