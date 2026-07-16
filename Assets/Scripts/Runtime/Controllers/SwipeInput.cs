using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WordWheel.Runtime
{
    public class SwipeInput : MonoBehaviour
    {
        public static event Action OnSwipeLeft;
        public static event Action OnSwipeRight;

        [SerializeField] private float minSwipeDistancePercent = 0.08f;

        private Vector2 _startPosition;
        private bool _isSwiping;

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            if (pointer.press.wasPressedThisFrame)
            {
                _startPosition = pointer.position.ReadValue();
                _isSwiping = true;
            }

            if (_isSwiping && pointer.press.wasReleasedThisFrame)
            {
                Vector2 endPosition = pointer.position.ReadValue();
                _isSwiping = false;

                Vector2 swipeDelta = endPosition - _startPosition;
                float minDistance = Screen.width * minSwipeDistancePercent;

                if (Mathf.Abs(swipeDelta.x) >= minDistance && Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                {
                    if (swipeDelta.x > 0)
                    {
                        OnSwipeRight?.Invoke();
                    }
                    else
                    {
                        OnSwipeLeft?.Invoke();
                    }
                }
            }
        }
    }
}
