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
            _targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * difficultySettings.PlayerTransitionSpeed);
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
