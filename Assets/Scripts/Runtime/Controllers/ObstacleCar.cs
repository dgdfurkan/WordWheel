using System;
using UnityEngine;

namespace WordWheel.Runtime.Controllers
{
    public class ObstacleCar : MonoBehaviour
    {
        private float _speed;
        private float _spawnZ;
        private float _targetZ;
        private float _despawnZ;
        private float _startScaleFactor;
        private Vector3 _initialScale;
        private Action<ObstacleCar> _onDeactivate;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        public void Initialize(
            float speed,
            float spawnZ,
            float targetZ,
            float despawnZ,
            float startScaleFactor,
            Action<ObstacleCar> onDeactivate)
        {
            _speed = speed;
            _spawnZ = spawnZ;
            _targetZ = targetZ;
            _despawnZ = despawnZ;
            _startScaleFactor = startScaleFactor;
            _onDeactivate = onDeactivate;

            UpdateScaleAndPosition();
        }

        private void Update()
        {
            transform.position += Vector3.back * (_speed * Time.deltaTime);
            UpdateScaleAndPosition();

            if (transform.position.z <= _despawnZ)
            {
                Deactivate();
            }
        }

        private void UpdateScaleAndPosition()
        {
            float t = Mathf.InverseLerp(_spawnZ, _targetZ, transform.position.z);
            transform.localScale = Vector3.Lerp(_initialScale * _startScaleFactor, _initialScale, t);
        }

        private void Deactivate()
        {
            _onDeactivate?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
