using Runtime.UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using Runtime.UI.Interfaces;

namespace Runtime.UI.Core
{
    /// <summary>
    /// Centralized UI Panel Manager.
    /// Panel state is owned by each UIPanel instance (single source of truth).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<UIManager>();
                    if (instance == null)
                    {
                        Debug.LogError("[UIManager] No UIManager found in scene! Create one in your Canvas.");
                    }
                }
                return instance;
            }
        }

        [Header("Bootstrap")]
        [SerializeField] private bool openMainMenuOnStart = true;

        private System.Collections.Generic.Dictionary<System.Type, IUIPanel> panelRegistry =
            new System.Collections.Generic.Dictionary<System.Type, IUIPanel>();

        public UnityEvent<System.Type> OnPanelOpened = new UnityEvent<System.Type>();
        public UnityEvent<System.Type> OnPanelClosed = new UnityEvent<System.Type>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[UIManager] Multiple UIManagers detected! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            instance = this;
            InitializePanels();
        }

        private void Start()
        {
            if (openMainMenuOnStart)
            {
                OpenPanel<MainMenuPanel>();
            }
        }

        private void InitializePanels()
        {
            UIPanel[] allPanels = GetComponentsInChildren<UIPanel>(includeInactive: true);

            foreach (UIPanel panel in allPanels)
            {
                RegisterPanel(panel);
            }

            Debug.Log($"[UIManager] Registered {allPanels.Length} panels.");
        }

        public void RegisterPanel(IUIPanel panel)
        {
            System.Type panelType = panel.PanelType;

            if (panelRegistry.ContainsKey(panelType))
            {
                Debug.LogWarning($"[UIManager] Panel type {panelType.Name} already registered!");
                return;
            }

            panelRegistry[panelType] = panel;
        }

        public bool OpenPanel<T>() where T : UIPanel
        {
            System.Type panelType = typeof(T);

            if (!TryGetPanel(panelType, out IUIPanel panel))
            {
                return false;
            }

            if (!CanOpen(panel))
            {
                LogBlockedOpen(panelType, panel);
                return false;
            }

            if (!panel.Open())
            {
                Debug.LogWarning($"[UIManager] Failed to open panel {panelType.Name}.");
                return false;
            }

            OnPanelOpened?.Invoke(panelType);
            return true;
        }

        public bool ClosePanel<T>() where T : UIPanel
        {
            System.Type panelType = typeof(T);

            if (!TryGetPanel(panelType, out IUIPanel panel))
            {
                return false;
            }

            if (!CanClose(panel))
            {
                LogBlockedClose(panelType, panel);
                return false;
            }

            if (!panel.Close())
            {
                Debug.LogWarning($"[UIManager] Failed to close panel {panelType.Name}.");
                return false;
            }

            OnPanelClosed?.Invoke(panelType);
            return true;
        }

        public bool TogglePanel<T>() where T : UIPanel
        {
            System.Type panelType = typeof(T);

            if (!TryGetPanel(panelType, out IUIPanel panel))
            {
                return false;
            }

            if (panel.IsOpen)
            {
                return ClosePanel<T>();
            }

            return OpenPanel<T>();
        }

        public bool IsPanelOpen<T>() where T : UIPanel
        {
            return TryGetPanel(typeof(T), out IUIPanel panel) && panel.IsOpen;
        }

        public bool IsPanelTransitioning<T>() where T : UIPanel
        {
            return TryGetPanel(typeof(T), out IUIPanel panel) && panel.IsTransitioning;
        }

        public T GetPanel<T>() where T : UIPanel
        {
            if (panelRegistry.TryGetValue(typeof(T), out IUIPanel panel))
            {
                return panel as T;
            }

            Debug.LogError($"[UIManager] Panel type {typeof(T).Name} not found!");
            return null;
        }

        public void CloseAllPanels()
        {
            foreach (IUIPanel panel in panelRegistry.Values)
            {
                if (panel == null || !panel.IsOpen || panel.IsTransitioning)
                {
                    continue;
                }

                panel.Close();
            }
        }

        public System.Type[] GetOpenPanels()
        {
            System.Collections.Generic.List<System.Type> openPanelTypes =
                new System.Collections.Generic.List<System.Type>();

            foreach (System.Collections.Generic.KeyValuePair<System.Type, IUIPanel> entry in panelRegistry)
            {
                if (entry.Value != null && entry.Value.IsOpen)
                {
                    openPanelTypes.Add(entry.Key);
                }
            }

            return openPanelTypes.ToArray();
        }

        private bool TryGetPanel(System.Type panelType, out IUIPanel panel)
        {
            if (panelRegistry.TryGetValue(panelType, out panel))
            {
                return true;
            }

            Debug.LogError($"[UIManager] Panel type {panelType.Name} not registered!");
            panel = null;
            return false;
        }

        private static bool CanOpen(IUIPanel panel)
        {
            return panel != null && !panel.IsOpen && !panel.IsTransitioning;
        }

        private static bool CanClose(IUIPanel panel)
        {
            return panel != null && panel.IsOpen && !panel.IsTransitioning;
        }

        private static void LogBlockedOpen(System.Type panelType, IUIPanel panel)
        {
            if (panel.IsOpen)
            {
                Debug.LogWarning($"[UIManager] Panel {panelType.Name} is already open.");
                return;
            }

            if (panel.IsTransitioning)
            {
                Debug.LogWarning($"[UIManager] Panel {panelType.Name} is still transitioning.");
            }
        }

        private static void LogBlockedClose(System.Type panelType, IUIPanel panel)
        {
            if (!panel.IsOpen)
            {
                Debug.LogWarning($"[UIManager] Panel {panelType.Name} is already closed.");
                return;
            }

            if (panel.IsTransitioning)
            {
                Debug.LogWarning($"[UIManager] Panel {panelType.Name} is still transitioning.");
            }
        }
    }
}
