using DG.Tweening;
using Runtime.UI.Configuration;
using Runtime.UI.Core;
using Runtime.UI.Enums;
using Runtime.UI.Utilities;
using UnityEngine;

namespace Runtime.UI.Panels
{
    public class SettingsPanel : UIPanel
    {
        public override UIPanelDisplayMode DisplayMode => UIPanelDisplayMode.Overlay;
        [Header("Animation Targets")]
        [SerializeField] private RectTransform header;
        [SerializeField] private RectTransform musicPlace;
        [SerializeField] private RectTransform soundPlace;
        [SerializeField] private RectTransform vibrationPlace;
        [SerializeField] private RectTransform languagePlace;
        [SerializeField] private RectTransform graphicPlace;
        [SerializeField] private RectTransform notificationPlace;
        [SerializeField] private RectTransform backButton;

        protected override void ConfigurePanelAnimations(UIPanelAnimationSetup anim)
        {
            anim.Open.Fade(0.28f, Ease.OutQuad);
            anim.Close.Fade(0.22f, Ease.InQuad);

            anim.Children
                .WithPanel(0.06f, 0f)
                .Group("Settings Content")
                    .Stagger(0.045f)
                    .ReverseStaggerOnClose(true)
                    .Entry(header, "SettingHeader")
                        .PopIn(0.22f, Ease.OutBack)
                        .EndEntry()
                    .Entry(musicPlace, "MusicPlace")
                        .SlideFromLeft(1200f, 0.18f, Ease.OutBack)
                        .EndEntry()
                    .Entry(soundPlace, "SoundPlace")
                        .SlideFromRight(1200f, 0.18f, Ease.OutBack)
                        .EndEntry()
                    .Entry(vibrationPlace, "VibrationPlace")
                        .SlideFromLeft(1200f, 0.18f, Ease.OutCubic)
                        .EndEntry()
                    .Entry(languagePlace, "LanguagePlace")
                        .SlideFromRight(1200f, 0.18f, Ease.OutCubic)
                        .EndEntry()
                    .Entry(graphicPlace, "GraphicPlace")
                        .SlideFromLeft(1200f, 0.18f, Ease.OutBack)
                        .EndEntry()
                    .Entry(notificationPlace, "NotificationPlace")
                        .SlideFromRight(1200f, 0.18f, Ease.OutCubic)
                        .EndEntry()
                    .Entry(backButton, "BackButton")
                        .DriftIn(0.2f, Ease.OutCubic)
                        .EndEntry()
                .EndGroup();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            Transform content = transform.Find("SettinsPanel");
            if (content == null)
            {
                return;
            }

            header = content.Find("SettingHeader") as RectTransform;
            musicPlace = content.Find("MusicPlace") as RectTransform;
            soundPlace = content.Find("SoundPlace") as RectTransform;
            vibrationPlace = content.Find("VibrationPlace") as RectTransform;
            languagePlace = content.Find("LanguagePlace") as RectTransform;
            graphicPlace = content.Find("GraphicPlace") as RectTransform;
            notificationPlace = content.Find("NotificationPlace") as RectTransform;
            backButton = content.Find("BackButton") as RectTransform;
        }
#endif

        public void OnBackButtonClicked()
        {
            UIAnimationHelper.BounceScale(PanelTransform, 1.05f, 0.2f);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.ClosePanel<SettingsPanel>();
            }).SetUpdate(true);
        }

        public void OnVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MasterVolume", value);
        }

        public void OnLanguageChanged(int languageIndex)
        {
            PlayerPrefs.SetInt("UILanguage", languageIndex);
        }

        public void OnDifficultyChanged(int difficultyIndex)
        {
            PlayerPrefs.SetInt("GameDifficulty", difficultyIndex);
        }
    }
}
