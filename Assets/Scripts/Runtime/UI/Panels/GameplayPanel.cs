using DG.Tweening;
using Runtime.UI.Core;
using Runtime.UI.Utilities;
using UnityEngine;
using WordWheel.Runtime.Managers;

namespace Runtime.UI.Panels
{
    public class GameplayPanel : UIPanel
    {
        protected override void HandlePanelOpened()
        {
            GameFlowManager.Instance.StartGameplay();
        }

        public void OnPauseButtonClicked()
        {
            UIAnimationHelper.Pulse(PanelTransform, 1.05f, 0.3f, 1);

            DOVirtual.DelayedCall(0.15f, () =>
            {
                UIManager.Instance.OpenPanel<PausePanel>();
            }).SetUpdate(true);
        }

        public void ShowGameOverPopup(int score)
        {
            Debug.Log($"[GameplayPanel] Game Over! Score: {score}");
        }
    }
}
