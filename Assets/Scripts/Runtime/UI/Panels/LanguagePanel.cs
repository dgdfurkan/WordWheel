using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Runtime.UI.Configuration;
using Runtime.UI.Core;
using Runtime.UI.Enums;
using Runtime.UI.Utilities;
using Runtime.Enums;
using WordWheel.Runtime.Managers;

namespace Runtime.UI.Panels
{
    public class LanguagePanel : UIPanel
    {
        public override UIPanelDisplayMode DisplayMode => UIPanelDisplayMode.Overlay;
        [Header("Animation Targets")]
        [SerializeField] private RectTransform header;
        [SerializeField] private RectTransform nativePart;
        [SerializeField] private RectTransform learningPart;
        [SerializeField] private RectTransform backButton;

        [SerializeField] private Button[] nativeLanguageButtons;
        [SerializeField] private Button[] targetLanguageButtons;

        private Language selectedNativeLanguage = Language.Turkish;
        private Language selectedTargetLanguage = Language.English;

        protected override void ConfigurePanelAnimations(UIPanelAnimationSetup anim)
        {
            anim.Open.Fade(0.28f, Ease.OutQuad);
            anim.Close.Fade(0.22f, Ease.InQuad);

            anim.Children
                .WithPanel(0.06f, 0f)
                .Group("Language Content")
                    .Stagger(0.045f)
                    .ReverseStaggerOnClose(true)
                    .Entry(header, "Header")
                        .PopIn(0.22f, Ease.OutBack)
                        .EndEntry()
                    .Entry(nativePart, "NativePart")
                        .SlideFromLeft(100f, 0.2f, Ease.OutCubic)
                        .EndEntry()
                    .Entry(learningPart, "LearningPart")
                        .SlideFromRight(100f, 0.2f, Ease.OutCubic)
                        .EndEntry()
                    .Entry(backButton, "Back")
                        .DriftIn(0.2f, Ease.OutCubic)
                        .EndEntry()
                .EndGroup();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            Transform panel = transform.Find("Panel");
            if (panel == null)
            {
                return;
            }

            header = panel.Find("Header") as RectTransform;
            nativePart = panel.Find("NativePart") as RectTransform;
            learningPart = panel.Find("LearningPart") as RectTransform;
            backButton = panel.Find("Button") as RectTransform;
        }
#endif

        public void OnNativeLanguageSelected(int languageIndex)
        {
            selectedNativeLanguage = (Language)languageIndex;

            if (nativeLanguageButtons != null && languageIndex < nativeLanguageButtons.Length)
            {
                RectTransform buttonRect = nativeLanguageButtons[languageIndex].GetComponent<RectTransform>();
                UIAnimationHelper.BounceScale(buttonRect, 1.15f, 0.3f);
            }
        }

        public void OnTargetLanguageSelected(int languageIndex)
        {
            if ((Language)languageIndex == selectedNativeLanguage)
            {
                if (targetLanguageButtons != null && languageIndex < targetLanguageButtons.Length)
                {
                    RectTransform buttonRect = targetLanguageButtons[languageIndex].GetComponent<RectTransform>();
                    UIAnimationHelper.Shake(buttonRect, 8f, 8, 0.4f);
                }

                return;
            }

            selectedTargetLanguage = (Language)languageIndex;

            if (targetLanguageButtons != null && languageIndex < targetLanguageButtons.Length)
            {
                RectTransform buttonRect = targetLanguageButtons[languageIndex].GetComponent<RectTransform>();
                UIAnimationHelper.BounceScale(buttonRect, 1.15f, 0.3f);
            }
        }

        public void OnConfirmButtonClicked()
        {
            SaveLanguagePreferences();
            UIAnimationHelper.BounceScale(PanelTransform, 1.05f, 0.3f);

            DOVirtual.DelayedCall(0.25f, () =>
            {
                UIManager.Instance.RunLoadingTransition(
                    () =>
                    {
                        UIManager.Instance.ClosePanel<LanguagePanel>();
                        UIManager.Instance.SwitchToPanel<GameplayPanel>();
                    },
                    onComplete: () => GameFlowManager.Instance.StartGameplay());
            }).SetUpdate(true);
        }

        public void OnBackButtonClicked()
        {
            UIAnimationHelper.BounceScale(PanelTransform, 1.05f, 0.2f);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.ClosePanel<LanguagePanel>();
            }).SetUpdate(true);
        }

        private void SaveLanguagePreferences()
        {
            PlayerPrefs.SetInt("NativeLanguage", (int)selectedNativeLanguage);
            PlayerPrefs.SetInt("TargetLanguage", (int)selectedTargetLanguage);
            PlayerPrefs.Save();
        }

        public Language GetNativeLanguage() => selectedNativeLanguage;
        public Language GetTargetLanguage() => selectedTargetLanguage;
    }
}
