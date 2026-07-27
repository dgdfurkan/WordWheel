using System;
using System.Collections;
using Runtime.UI.Interfaces;
using Runtime.UI.Panels;
using Runtime.UI.Panels.Loading;
using UnityEngine;
using UnityEngine.Events;
using WordWheel.Runtime.Managers;

namespace Runtime.UI.Core
{
    /// <summary>
    /// Centralized UI Panel Manager.
    /// Panel state is owned by each UIPanel instance (single source of truth).
    /// Exclusive panels replace the current screen. Overlay panels stack on top without closing the screen below.
    /// </summary>
    [DefaultExecutionOrder(-200)]
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
        [SerializeField] private bool openLoadingPanelOnStart = true;
        [SerializeField] private bool openMainMenuOnStart = true;

        private System.Collections.Generic.Dictionary<System.Type, IUIPanel> panelRegistry =
            new System.Collections.Generic.Dictionary<System.Type, IUIPanel>();

        private UIOverlayScrim overlayScrim;

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

            if (openLoadingPanelOnStart)
            {
                BootstrapStartupWithEarlyLoading();
                return;
            }

            InitializePanels();
        }

        private void BootstrapStartupWithEarlyLoading()
        {
            LoadingPanel loadingPanel = GetComponentInChildren<LoadingPanel>(includeInactive: true);
            if (loadingPanel == null)
            {
                EnsureLoadingPanelExists();
                loadingPanel = GetPanel<LoadingPanel>();
            }
            else
            {
                loadingPanel.EnsureInitialized();
                RegisterPanel(loadingPanel);
            }

            BeginStartupSplash();
            StartCoroutine(CompleteDeferredPanelBootstrap());
        }

        private IEnumerator CompleteDeferredPanelBootstrap()
        {
            yield return null;
            InitializePanels();
        }

        private void Start()
        {
            if (!openLoadingPanelOnStart && openMainMenuOnStart)
            {
                OpenPanel<MainMenuPanel>();
            }
        }

        private void BeginStartupSplash()
        {
            if (!TryGetPanel(out LoadingPanel loadingPanel))
            {
                return;
            }

            loadingPanel.BringToFront();
            loadingPanel.PlaySplash(() =>
            {
                ClosePanelThenOpen<LoadingPanel, MainMenuPanel>();
            });
        }

        public void RunLoadingTransition(
            Action midAction,
            Action onComplete = null,
            LoadingPresentation presentation = LoadingPresentation.Quick,
            float holdAfterIntro = 0.15f)
        {
            EnsureLoadingPanelExists();
            LoadingPanel panel = GetPanel<LoadingPanel>();
            LoadingTransitionRequest request = LoadingTransitionRequest.Transition(midAction, onComplete, holdAfterIntro);
            request.Presentation = presentation;
            panel.RunTransition(request);
        }

        public bool IsLoadingSessionActive()
        {
            return TryGetPanel(out LoadingPanel panel) && panel.IsSessionActive;
        }

        public void StartGameWithLoadingTransition()
        {
            RunLoadingTransition(
                () => SwitchToPanel<GameplayPanel>(),
                onComplete: () => GameFlowManager.Instance.StartGameplay(),
                presentation: LoadingPresentation.Quick);
        }

        public void ShowLoadingOverlay(LoadingTransitionRequest request)
        {
            EnsureLoadingPanelExists();
            GetPanel<LoadingPanel>().RunTransition(request);
        }

        private void EnsureLoadingPanelExists()
        {
            if (TryGetPanel(out LoadingPanel _))
            {
                return;
            }

            GameObject panelObject = new GameObject(
                "LoadingPanel",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(LoadingPanelView),
                typeof(LoadingPanel));

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            rectTransform.SetParent(transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            LoadingPanel panel = panelObject.GetComponent<LoadingPanel>();
            panel.BootstrapStartHiddenState();
            RegisterPanel(panel);

            Debug.Log("[UIManager] Runtime LoadingPanel created and registered.");
        }

        private void InitializePanels()
        {
            panelRegistry.Clear();

            UIPanel[] allPanels = GetComponentsInChildren<UIPanel>(includeInactive: true);

            foreach (UIPanel panel in allPanels)
            {
                panel.EnsureInitialized();

                bool skipBootstrapHidden = openLoadingPanelOnStart && panel is LoadingPanel;
                if (!skipBootstrapHidden)
                {
                    panel.BootstrapStartHiddenState();
                }

                RegisterPanel(panel);
            }

            Debug.Log($"[UIManager] Registered {panelRegistry.Count} panels.");
        }

        public void RegisterPanel(IUIPanel panel)
        {
            if (panel == null)
            {
                return;
            }

            System.Type panelType = panel.PanelType;

            if (panelRegistry.ContainsKey(panelType))
            {
                Debug.LogWarning($"[UIManager] Panel type {panelType.Name} already registered!");
                return;
            }

            panelRegistry[panelType] = panel;
        }

        public bool TryGetPanel<T>(out T panel) where T : UIPanel
        {
            if (panelRegistry.TryGetValue(typeof(T), out IUIPanel found) && found is T typedPanel)
            {
                panel = typedPanel;
                return true;
            }

            panel = null;
            return false;
        }

        public bool OpenPanel<T>() where T : UIPanel
        {
            if (!TryGetPanel(out T panel))
            {
                Debug.LogWarning($"[UIManager] Cannot open {typeof(T).Name}. Panel is not registered under this Canvas.");
                return false;
            }

            return OpenPanelInternal(panel);
        }

        public bool ClosePanel<T>() where T : UIPanel
        {
            if (!TryGetPanel(out T panel))
            {
                return false;
            }

            return ClosePanelInternal(panel);
        }

        public bool ClosePanelThenOpen<TClose, TOpen>()
            where TClose : UIPanel
            where TOpen : UIPanel
        {
            if (!TryGetPanel(out TClose closePanel) || !TryGetPanel(out TOpen openPanel))
            {
                return false;
            }

            return ClosePanelThenOpenInternal(closePanel, openPanel);
        }

        public bool EnsurePanelVisible<T>() where T : UIPanel
        {
            if (!TryGetPanel(out T panel))
            {
                return false;
            }

            return EnsurePanelVisibleInternal(panel);
        }

        public bool TogglePanel<T>() where T : UIPanel
        {
            if (!TryGetPanel(out T panel))
            {
                return false;
            }

            if (panel.IsTransitioning)
            {
                return false;
            }

            if (panel.IsOpen && !panel.gameObject.activeSelf)
            {
                panel.ForceSyncClosedState();
                return OpenPanelInternal(panel);
            }

            if (panel.IsOpen)
            {
                return ClosePanelInternal(panel);
            }

            return OpenPanelInternal(panel);
        }

        public bool IsPanelOpen<T>() where T : UIPanel
        {
            return TryGetPanel(out T panel) && panel.IsOpen;
        }

        public bool IsPanelTransitioning<T>() where T : UIPanel
        {
            return TryGetPanel(out T panel) && panel.IsTransitioning;
        }

        public T GetPanel<T>() where T : UIPanel
        {
            if (TryGetPanel(out T panel))
            {
                return panel;
            }

            Debug.LogError($"[UIManager] Panel type {typeof(T).Name} not found!");
            return null;
        }

        public void CloseAllPanels()
        {
            foreach (IUIPanel panel in panelRegistry.Values)
            {
                if (panel is UIPanel uiPanel)
                {
                    uiPanel.EnsureInitialized();
                    uiPanel.ForceClose();
                }
            }
        }

        /// <summary>
        /// Replaces the current exclusive screen. Closes other exclusive panels and all overlays.
        /// </summary>
        public bool SwitchToPanel<T>() where T : UIPanel
        {
            if (!TryGetPanel(out T targetPanel))
            {
                Debug.LogWarning($"[UIManager] Cannot switch to {typeof(T).Name}. Panel is not registered under this Canvas.");
                return false;
            }

            SyncAllPanelStates();

            if (targetPanel.IsDisplayed)
            {
                return true;
            }

            if (targetPanel.DisplayMode == Enums.UIPanelDisplayMode.Overlay)
            {
                return OpenPanelInternal(targetPanel);
            }

            CloseAllOverlays();
            CloseExclusivePanelsExcept(null);
            return EnsurePanelVisibleInternal(targetPanel);
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

        private bool OpenPanelInternal(UIPanel panel)
        {
            panel.EnsureInitialized();
            panel.HealOpenState();

            if (panel.DisplayMode == Enums.UIPanelDisplayMode.Overlay)
            {
                PrepareForOverlayOpen(panel);
            }

            if (panel.IsTransitioning)
            {
                panel.PrepareForImmediateOpen();
            }

            if (!panel.Open())
            {
                panel.gameObject.SetActive(true);
                panel.BringToFront();

                if (!panel.Open())
                {
                    Debug.LogWarning($"[UIManager] Failed to open panel {panel.PanelType.Name}.");
                    return false;
                }
            }

            OnPanelOpened?.Invoke(panel.PanelType);
            RefreshOverlayScrim();
            return true;
        }

        private bool ClosePanelInternal(UIPanel panel)
        {
            panel.EnsureInitialized();
            panel.SyncRuntimeState();

            if (!CanClose(panel))
            {
                LogBlockedClose(panel.PanelType, panel);
                return false;
            }

            if (!panel.Close())
            {
                Debug.LogWarning($"[UIManager] Failed to close panel {panel.PanelType.Name}.");
                return false;
            }

            OnPanelClosed?.Invoke(panel.PanelType);
            return true;
        }

        private bool ClosePanelThenOpenInternal(UIPanel closePanel, UIPanel openPanel)
        {
            closePanel.EnsureInitialized();
            closePanel.SyncRuntimeState();
            openPanel.EnsureInitialized();
            openPanel.SyncRuntimeState();

            if (openPanel.IsDisplayed)
            {
                if (closePanel != openPanel && IsPanelVisible(closePanel))
                {
                    ClosePanelInternal(closePanel);
                }

                return true;
            }

            if (!IsPanelVisible(closePanel))
            {
                return EnsurePanelVisibleInternal(openPanel);
            }

            if (closePanel.IsTransitioning)
            {
                closePanel.ForceClose();
                return EnsurePanelVisibleInternal(openPanel);
            }

            if (CanClose(closePanel))
            {
                UnityEngine.Events.UnityAction openAfterClose = null;
                openAfterClose = () =>
                {
                    closePanel.OnPanelClosed.RemoveListener(openAfterClose);
                    EnsurePanelVisibleInternal(openPanel);
                };

                closePanel.OnPanelClosed.AddListener(openAfterClose);

                if (!closePanel.Close())
                {
                    closePanel.OnPanelClosed.RemoveListener(openAfterClose);
                    closePanel.ForceClose();
                    return EnsurePanelVisibleInternal(openPanel);
                }

                return true;
            }

            if (IsPanelVisible(closePanel))
            {
                closePanel.ForceClose();
            }

            return EnsurePanelVisibleInternal(openPanel);
        }

        private bool EnsurePanelVisibleInternal(UIPanel panel)
        {
            if (panel == null)
            {
                return false;
            }

            panel.EnsureInitialized();
            panel.HealOpenState();

            if (panel.IsDisplayed)
            {
                panel.BringToFront();
                RefreshOverlayScrim();
                return true;
            }

            if (panel.DisplayMode == Enums.UIPanelDisplayMode.Overlay)
            {
                PrepareForOverlayOpen(panel);
            }
            else
            {
                CloseAllOverlays();
                CloseExclusivePanelsExcept(panel);
            }

            if (!panel.Open())
            {
                panel.PrepareForImmediateOpen();
                if (!panel.Open())
                {
                    Debug.LogWarning($"[UIManager] Failed to open panel {panel.PanelType.Name}.");
                    return false;
                }
            }

            OnPanelOpened?.Invoke(panel.PanelType);
            RefreshOverlayScrim();
            return true;
        }

        private void PrepareForOverlayOpen(UIPanel overlayPanel)
        {
            CloseAllOverlaysExcept(overlayPanel);
        }

        private void CloseAllOverlays()
        {
            CloseAllOverlaysExcept(null);
        }

        private void CloseAllOverlaysExcept(UIPanel keepOpen)
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Type, IUIPanel> entry in panelRegistry)
            {
                if (entry.Value is not UIPanel panel || panel == keepOpen)
                {
                    continue;
                }

                if (panel.DisplayMode != Enums.UIPanelDisplayMode.Overlay || panel is LoadingPanel)
                {
                    continue;
                }

                if (IsPanelVisible(panel))
                {
                    panel.EnsureInitialized();
                    panel.ForceClose();
                }
            }
        }

        private void CloseExclusivePanelsExcept(UIPanel keepOpen)
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Type, IUIPanel> entry in panelRegistry)
            {
                if (entry.Value is not UIPanel panel || panel == keepOpen)
                {
                    continue;
                }

                if (panel.DisplayMode != Enums.UIPanelDisplayMode.Exclusive)
                {
                    continue;
                }

                if (IsPanelVisible(panel))
                {
                    panel.EnsureInitialized();
                    panel.ForceClose();
                }
            }
        }

        private static bool IsPanelVisible(UIPanel panel)
        {
            return panel != null
                && (panel.gameObject.activeSelf || panel.IsOpen || panel.IsTransitioning);
        }

        private void SyncAllPanelStates()
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Type, IUIPanel> entry in panelRegistry)
            {
                if (entry.Value is UIPanel panel)
                {
                    panel.EnsureInitialized();
                    panel.SyncRuntimeState();
                }
            }
        }

        internal void RefreshOverlayScrim()
        {
            EnsureOverlayScrim();

            System.Collections.Generic.List<Transform> visibleOverlays =
                new System.Collections.Generic.List<Transform>();

            bool loadingPanelVisible = false;

            foreach (System.Collections.Generic.KeyValuePair<System.Type, IUIPanel> entry in panelRegistry)
            {
                if (entry.Value is not UIPanel panel)
                {
                    continue;
                }

                if (panel.DisplayMode != Enums.UIPanelDisplayMode.Overlay)
                {
                    continue;
                }

                if (!panel.gameObject.activeSelf)
                {
                    continue;
                }

                if (!panel.IsOpen && !panel.IsTransitioning)
                {
                    continue;
                }

                if (panel is LoadingPanel)
                {
                    loadingPanelVisible = true;
                }

                visibleOverlays.Add(panel.transform);
            }

            if (visibleOverlays.Count == 0)
            {
                overlayScrim.Hide();
                return;
            }

            if (loadingPanelVisible &&
                TryGetPanel(out LoadingPanel loadingPanel) &&
                loadingPanel.UsesQuickTransitionMask)
            {
                overlayScrim.Hide();
                return;
            }

            if (loadingPanelVisible && !HasOtherOverlayThanLoading(visibleOverlays) && !HasVisibleExclusivePanel())
            {
                overlayScrim.Hide();
                return;
            }

            float showDuration = loadingPanelVisible ? 0.18f : 0.25f;
            overlayScrim.Show(showDuration);
            overlayScrim.PlaceBelow(visibleOverlays.ToArray());
        }

        private static bool HasOtherOverlayThanLoading(
            System.Collections.Generic.List<Transform> visibleOverlays)
        {
            for (int index = 0; index < visibleOverlays.Count; index++)
            {
                Transform overlay = visibleOverlays[index];
                if (overlay != null && overlay.GetComponent<LoadingPanel>() == null)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasVisibleExclusivePanel()
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Type, IUIPanel> entry in panelRegistry)
            {
                if (entry.Value is not UIPanel panel)
                {
                    continue;
                }

                if (panel.DisplayMode != Enums.UIPanelDisplayMode.Exclusive)
                {
                    continue;
                }

                if (IsPanelVisible(panel))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureOverlayScrim()
        {
            if (overlayScrim != null)
            {
                return;
            }

            overlayScrim = GetComponentInChildren<UIOverlayScrim>(includeInactive: true);
            if (overlayScrim == null)
            {
                overlayScrim = UIOverlayScrim.Create(transform);
            }
        }

        private bool HasVisibleOverlayPanel()
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Type, IUIPanel> entry in panelRegistry)
            {
                if (entry.Value is not UIPanel panel)
                {
                    continue;
                }

                if (panel.DisplayMode != Enums.UIPanelDisplayMode.Overlay || panel is LoadingPanel)
                {
                    continue;
                }

                if (panel.gameObject.activeSelf && (panel.IsOpen || panel.IsTransitioning))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CanOpen(IUIPanel panel)
        {
            return panel is UIPanel uiPanel && uiPanel.CanStartOpen();
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
