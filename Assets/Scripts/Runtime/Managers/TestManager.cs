using Runtime.UI.Core;
using Runtime.UI.Panels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WordWheel.Runtime.Managers
{
    public class TestManager : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                UIManager.Instance.TogglePanel<MainMenuPanel>();
            }

            if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                UIManager.Instance.TogglePanel<SettingsPanel>();
            }

            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                UIManager.Instance.TogglePanel<LanguagePanel>();
            }
        }
    }
}
