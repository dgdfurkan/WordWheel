using UnityEngine;

namespace WordWheel.Runtime
{
    public class RoadScroller : MonoBehaviour
    {
        [SerializeField] private GameDifficultySettingsSO difficultySettings;
        [SerializeField] private Vector2 scrollDirection = new Vector2(0, -1);

        private Renderer _renderer;
        private Vector2 _currentOffset;

        private void Awake()
        {
            TryGetComponent(out _renderer);
            if (_renderer != null)
            {
                _currentOffset = _renderer.material.mainTextureOffset;
            }   
        }

        private void Update()
        {
            if (_renderer == null || difficultySettings == null) return;
            _currentOffset += scrollDirection * (difficultySettings.RoadScrollSpeed * Time.deltaTime);
            _renderer.material.mainTextureOffset = _currentOffset;
        }
    }
}
