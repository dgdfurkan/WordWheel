using System;
using UnityEngine;

namespace WordWheel.Runtime.Controllers
{
    public class ObstacleCar : MonoBehaviour
    {
        private float _speed;
        private float _spawnOffset;
        private float _targetOffset;
        private float _despawnOffset;
        private float _startScaleFactor;
        private Vector3 _initialScale;
        private PlayerController _player;
        private Action<ObstacleCar> _onDeactivate;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        public void Initialize(
            float speed,
            float spawnOffset,
            float targetOffset,
            float despawnOffset,
            float startScaleFactor,
            PlayerController player,
            Action<ObstacleCar> onDeactivate)
        {
            _speed = speed;
            _spawnOffset = spawnOffset;
            _targetOffset = targetOffset;
            _despawnOffset = despawnOffset;
            _startScaleFactor = startScaleFactor;
            _player = player;
            _onDeactivate = onDeactivate;

            UpdateScaleAndPosition();
        }

        private void Update()
        {
            transform.position += Vector3.back * (_speed * Time.deltaTime);
            UpdateScaleAndPosition();

            if (_player != null)
            {
                float distance = transform.position.z - _player.transform.position.z;
                if (distance <= _despawnOffset)
                {
                    Deactivate();
                }
            }
            else
            {
                if (transform.position.z <= _despawnOffset)
                {
                    Deactivate();
                }
            }
        }

        private void UpdateScaleAndPosition()
        {
            if (_player == null) return;
            float distance = transform.position.z - _player.transform.position.z;
            float t = Mathf.InverseLerp(_spawnOffset, _targetOffset, distance);
            transform.localScale = Vector3.Lerp(_initialScale * _startScaleFactor, _initialScale, t);
        }

        private void Deactivate()
        {
            _onDeactivate?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
