using DG.Tweening;
using Runtime.UI.Core;
using Runtime.UI.Enums;
using Runtime.UI.Utilities;
using UnityEngine;
using WordWheel.Runtime.Managers;

namespace Runtime.UI.Panels
{
    public class PausePanel : UIPanel
    {
        public override UIPanelDisplayMode DisplayMode => UIPanelDisplayMode.Overlay;
        protected override void HandlePanelOpened()
        {
            Time.timeScale = 0f;
        }

        protected override void HandlePanelClosed()
        {
            Time.timeScale = 1f;
        }

        public void OnResumeButtonClicked()
        {
            UIAnimationHelper.BounceScale(PanelTransform, 1.05f, 0.2f, useUnscaledTime: true);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.ClosePanel<PausePanel>();
            }).SetUpdate(true);
        }

        public void OnSettingsButtonClicked()
        {
            UIAnimationHelper.BounceScale(PanelTransform, 1.05f, 0.2f, useUnscaledTime: true);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.OpenPanel<SettingsPanel>();
            }).SetUpdate(true);
        }

        public void OnMainMenuButtonClicked()
        {
            UIAnimationHelper.Shake(PanelTransform, 5f, 5, 0.3f, useUnscaledTime: true);

            DOVirtual.DelayedCall(0.3f, () =>
            {
                Time.timeScale = 1f;

                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.StopGameplay();
                }

                UIManager.Instance.SwitchToPanel<MainMenuPanel>();
            }).SetUpdate(true);
        }
    }
}
