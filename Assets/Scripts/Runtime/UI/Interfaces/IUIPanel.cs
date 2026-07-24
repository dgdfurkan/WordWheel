using UnityEngine;

namespace Runtime.UI.Interfaces
{
    public interface IUIPanel
    {
        bool Open();
        bool Close();
        bool IsOpen { get; }
        bool IsTransitioning { get; }
        RectTransform PanelTransform { get; }
        System.Type PanelType { get; }
    }
}
