using Runtime.UI.Core;
using Runtime.UI.Panels;
using UnityEngine;
using WordWheel.Runtime;
using WordWheel.Runtime.Controllers;

namespace WordWheel.Runtime.Managers
{
    public class GameFlowManager : MonoBehaviour
    {
        private static GameFlowManager instance;

        public static GameFlowManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<GameFlowManager>();
                    if (instance == null)
                    {
                        var managerObject = new GameObject(nameof(GameFlowManager));
                        instance = managerObject.AddComponent<GameFlowManager>();
                    }
                }

                return instance;
            }
        }

        [Header("Gameplay Systems")]
        [SerializeField] private ObstacleSpawner obstacleSpawner;
        [SerializeField] private WordGameplayManager wordGameplayManager;
        [SerializeField] private EnvironmentScroller environmentScroller;

        private bool gameplayStarted;

        public bool IsGameplayStarted => gameplayStarted;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[GameFlowManager] Duplicate instance detected. Destroying this one.");
                Destroy(gameObject);
                return;
            }

            instance = this;
            AutoAssignReferences();
            PauseGameplaySystems();
        }

        private void OnValidate()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void Start()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnPanelOpened.AddListener(OnPanelOpened);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnPanelOpened.RemoveListener(OnPanelOpened);
            }
        }

        public void StartGameplay()
        {
            if (gameplayStarted)
            {
                return;
            }

            gameplayStarted = true;
            ResumeGameplaySystems();
            Debug.Log("[GameFlowManager] Gameplay started.");
        }

        public void StopGameplay()
        {
            if (!gameplayStarted)
            {
                return;
            }

            gameplayStarted = false;
            PauseGameplaySystems();

            if (obstacleSpawner != null)
            {
                obstacleSpawner.ClearActiveObstacles();
            }

            Debug.Log("[GameFlowManager] Gameplay stopped.");
        }

        private void OnPanelOpened(System.Type panelType)
        {
            if (panelType == typeof(MainMenuPanel))
            {
                StopGameplay();
            }
        }

        private void AutoAssignReferences()
        {
            if (obstacleSpawner == null)
            {
                obstacleSpawner = FindAnyObjectByType<ObstacleSpawner>();
            }

            if (wordGameplayManager == null)
            {
                wordGameplayManager = FindAnyObjectByType<WordGameplayManager>();
            }

            if (environmentScroller == null)
            {
                environmentScroller = FindAnyObjectByType<EnvironmentScroller>();
            }
        }

        private void PauseGameplaySystems()
        {
            if (obstacleSpawner != null)
            {
                obstacleSpawner.IsSpawningPaused = true;
            }

            if (wordGameplayManager != null)
            {
                wordGameplayManager.PauseGameplay();
            }

            if (environmentScroller != null)
            {
                environmentScroller.IsScrollingPaused = true;
            }
        }

        private void ResumeGameplaySystems()
        {
            if (obstacleSpawner != null)
            {
                obstacleSpawner.IsSpawningPaused = false;
            }

            if (wordGameplayManager != null)
            {
                wordGameplayManager.BeginGameplay();
            }

            if (environmentScroller != null)
            {
                environmentScroller.IsScrollingPaused = false;
            }
        }
    }
}
