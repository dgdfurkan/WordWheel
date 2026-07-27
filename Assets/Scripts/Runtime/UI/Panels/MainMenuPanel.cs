using DG.Tweening;
using Runtime.UI.Configuration;
using Runtime.UI.Core;
using Runtime.UI.Enums;
using Runtime.UI.Utilities;
using UnityEngine;

namespace Runtime.UI.Panels
{
    public class MainMenuPanel : UIPanel
    {
        public override UIPanelDisplayMode DisplayMode => UIPanelDisplayMode.Exclusive;

        [Header("Animation Targets")]
        [SerializeField] private RectTransform logo;
        [SerializeField] private RectTransform playButton;
        [SerializeField] private RectTransform garageButton;
        [SerializeField] private RectTransform leaderboardButton;
        [SerializeField] private RectTransform settingsButton;
        [SerializeField] private RectTransform languageButton;

        protected override void ConfigurePanelAnimations(UIPanelAnimationSetup anim)
        {
            anim.Open.Fade(0.28f, Ease.OutQuad);
            anim.Close.Fade(0.22f, Ease.InQuad);

            anim.Children
                .WithPanel(0.06f, 0f)
                .Group("Main Menu")
                    .Stagger(0.04f)
                    .ReverseStaggerOnClose(true)
                    .Entry(logo, "Logo")
                        .PopIn(0.24f, Ease.OutBack)
                        .EndEntry()
                    .Entry(playButton, "Play")
                        .SlideFromTop(160f, 0.2f, Ease.OutBack)
                        .EndEntry()
                    .Entry(garageButton, "Garage")
                        .SlideFromLeft(500f, 0.18f, Ease.OutCubic)
                        .EndEntry()
                    .Entry(leaderboardButton, "Leaderboard")
                        .SlideFromRight(500f, 0.18f, Ease.OutCubic)
                        .EndEntry()
                    .Entry(settingsButton, "Settings")
                        .SlideFromLeft(500f, 0.18f, Ease.OutCubic)
                        .EndEntry()
                    .Entry(languageButton, "Language")
                        .SlideFromRight(500f, 0.18f, Ease.OutCubic)
                        .EndEntry()
                .EndGroup();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            logo = transform.Find("Logo") as RectTransform;
            playButton = transform.Find("PlayButton") as RectTransform;
            garageButton = transform.Find("GarageButton") as RectTransform;
            leaderboardButton = transform.Find("LeaderboardButton") as RectTransform;
            settingsButton = transform.Find("SettingsButton") as RectTransform;
            languageButton = transform.Find("LanguageButton") as RectTransform;
        }
#endif

        public void OnPlayButtonClicked()
        {
            UIAnimationHelper.BounceScale(playButton, 1.08f, 0.2f);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.OpenPanel<LanguagePanel>();
            }).SetUpdate(true);
        }

        public void OnSettingsButtonClicked()
        {
            UIAnimationHelper.BounceScale(settingsButton, 1.08f, 0.2f);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.OpenPanel<SettingsPanel>();
            }).SetUpdate(true);
        }

        public void OnLanguageButtonClicked()
        {
            UIAnimationHelper.BounceScale(languageButton, 1.08f, 0.2f);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.OpenPanel<LanguagePanel>();
            }).SetUpdate(true);
        }

        public void OnGarageButtonClicked()
        {
            UIAnimationHelper.BounceScale(garageButton, 1.08f, 0.2f);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.OpenPanel<GaragePanel>();
            }).SetUpdate(true);
        }

        public void OnLeaderboardButtonClicked()
        {
            UIAnimationHelper.BounceScale(leaderboardButton, 1.08f, 0.2f);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.OpenPanel<LeaderboardPanel>();
            }).SetUpdate(true);
        }

        public void OnQuitButtonClicked()
        {
            UIAnimationHelper.Shake(PanelTransform, 5f, 5, 0.3f);

            DOVirtual.DelayedCall(0.3f, Application.Quit).SetUpdate(true);
        }
    }
}
