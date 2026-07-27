using System;
using UnityEngine;
using TMPro;

namespace WordWheel.Runtime.Controllers
{
    public class WordGateController : MonoBehaviour
    {
        [SerializeField] private TextMeshPro leftText;
        [SerializeField] private TextMeshPro rightText;

        private float _speed;
        private float _spawnZ;
        private float _targetZ;
        private float _despawnZ;
        private float _checkZ;
        private float _startScaleFactor;
        private Vector3 _initialScale;
        private PlayerController _player;
        private Action<int> _onCrossedGate;
        private bool _hasCrossed;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        public void Initialize(
            string leftWord,
            string rightWord,
            float speed,
            float spawnZ,
            float targetZ,
            float despawnZ,
            float checkZ,
            float laneDistance,
            float startScaleFactor,
            PlayerController player,
            Action<int> onCrossedGate)
        {
            _speed = speed;
            _spawnZ = spawnZ;
            _targetZ = targetZ;
            _despawnZ = despawnZ;
            _player = player;
            _checkZ = player != null ? player.transform.position.z : checkZ;
            _startScaleFactor = startScaleFactor;
            _onCrossedGate = onCrossedGate;
            _hasCrossed = false;

            if (leftText != null)
            {
                leftText.text = leftWord;
                Vector3 pos = leftText.transform.localPosition;
                pos.x = -laneDistance;
                leftText.transform.localPosition = pos;
            }

            if (rightText != null)
            {
                rightText.text = rightWord;
                Vector3 pos = rightText.transform.localPosition;
                pos.x = laneDistance;
                rightText.transform.localPosition = pos;
            }

            UpdateScaleAndPosition();
        }

        private void Update()
        {
            if (_speed > 0f)
            {
                transform.position += Vector3.back * (_speed * Time.deltaTime);
            }
            UpdateScaleAndPosition();

            // Check crossing point
            if (!_hasCrossed && _player != null && _player.transform.position.z >= transform.position.z)
            {
                _hasCrossed = true;
                _onCrossedGate?.Invoke(_player.CurrentLane);
            }

            // Despawn
            if (_player != null)
            {
                float distance = transform.position.z - _player.transform.position.z;
                if (distance <= -10f)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                if (transform.position.z <= _despawnZ)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void UpdateScaleAndPosition()
        {
            if (_player == null) return;
            float distance = transform.position.z - _player.transform.position.z;
            float t = Mathf.InverseLerp(_spawnZ, _targetZ, distance);
            transform.localScale = Vector3.Lerp(_initialScale * _startScaleFactor, _initialScale, t);
        }
    }
}
