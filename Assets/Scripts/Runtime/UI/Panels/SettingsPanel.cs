using DG.Tweening;
using Runtime.UI.Core;
using Runtime.UI.Utilities;
using UnityEngine;

namespace Runtime.UI.Panels
{
    public class SettingsPanel : UIPanel
    {
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
