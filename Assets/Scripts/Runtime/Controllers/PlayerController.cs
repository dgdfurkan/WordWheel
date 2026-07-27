using UnityEngine;

namespace WordWheel.Runtime
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameDifficultySettingsSO difficultySettings;
        [SerializeField] private float laneDistance = 2.0f;

        private int _currentLane = 1;
        private Vector3 _targetPosition;

        public int CurrentLane => _currentLane;

        private void OnEnable()
        {
            SwipeInput.OnSwipeLeft += MoveLeft;
            SwipeInput.OnSwipeRight += MoveRight;
        }

        private void OnDisable()
        {
            SwipeInput.OnSwipeLeft -= MoveLeft;
            SwipeInput.OnSwipeRight -= MoveRight;
        }

        private void Start()
        {
            _targetPosition = transform.position;
        }

        private void Update()
        {
            if (difficultySettings == null) return;

            float targetX = (_currentLane - 1) * laneDistance;
            float currentZ = transform.position.z;

            if (Managers.GameFlowManager.Instance != null && Managers.GameFlowManager.Instance.IsGameplayStarted)
            {
                currentZ += Mathf.Abs(difficultySettings.PlayerSpeed) * Time.deltaTime;
            }

            float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * difficultySettings.PlayerTransitionSpeed);
            transform.position = new Vector3(newX, transform.position.y, currentZ);
        }

        private void MoveLeft()
        {
            if (_currentLane > 0)
            {
                _currentLane--;
            }
        }

        private void MoveRight()
        {
            if (_currentLane < 2)
            {
                _currentLane++;
            }
        }
    }
}
