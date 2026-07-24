using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Runtime.UI.Core;
using Runtime.UI.Utilities;
using Runtime.Enums;

namespace Runtime.UI.Panels
{
    public class LanguagePanel : UIPanel
    {
        [SerializeField] private Button[] nativeLanguageButtons;
        [SerializeField] private Button[] targetLanguageButtons;

        private Language selectedNativeLanguage = Language.Turkish;
        private Language selectedTargetLanguage = Language.English;

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
                UIManager.Instance.ClosePanel<LanguagePanel>();
                UIManager.Instance.OpenPanel<GameplayPanel>();
            }).SetUpdate(true);
        }

        public void OnBackButtonClicked()
        {
            UIManager.Instance.ClosePanel<LanguagePanel>();
            UIManager.Instance.OpenPanel<MainMenuPanel>();
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
