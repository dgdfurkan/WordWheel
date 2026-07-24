using DG.Tweening;
using Runtime.UI.Core;
using Runtime.UI.Utilities;
using UnityEngine;

namespace Runtime.UI.Panels
{
    public class MainMenuPanel : UIPanel
    {
        public void OnPlayButtonClicked()
        {
            UIAnimationHelper.BounceScale(PanelTransform, 1.05f, 0.2f);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.ClosePanel<MainMenuPanel>();
                UIManager.Instance.OpenPanel<LanguagePanel>();
            }).SetUpdate(true);
        }

        public void OnSettingsButtonClicked()
        {
            UIAnimationHelper.BounceScale(PanelTransform, 1.05f, 0.2f);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.OpenPanel<SettingsPanel>();
            }).SetUpdate(true);
        }

        public void OnQuitButtonClicked()
        {
            UIAnimationHelper.Shake(PanelTransform, 5f, 5, 0.3f);

            DOVirtual.DelayedCall(0.3f, Application.Quit).SetUpdate(true);
        }
    }
}
