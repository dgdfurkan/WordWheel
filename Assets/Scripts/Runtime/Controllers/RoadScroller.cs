using UnityEngine;

namespace WordWheel.Runtime
{
    public class RoadScroller : MonoBehaviour
    {
        [SerializeField] private GameDifficultySettingsSO difficultySettings;
        [SerializeField] private Vector2 scrollDirection = new Vector2(0, -1);
        [SerializeField] private bool startPaused = true;

        private Renderer _renderer;
        private Vector2 _currentOffset;
        private bool _isScrollingPaused;

        public bool IsScrollingPaused
        {
            get => _isScrollingPaused;
            set => _isScrollingPaused = value;
        }

        private void Awake()
        {
            _isScrollingPaused = startPaused;
            TryGetComponent(out _renderer);
            if (_renderer != null)
            {
                _currentOffset = _renderer.material.mainTextureOffset;
            }   
        }

        private void Update()
        {
            if (_isScrollingPaused) return;
            if (_renderer == null || difficultySettings == null) return;
            _currentOffset += scrollDirection * (difficultySettings.RoadScrollSpeed * Time.deltaTime);
            _renderer.material.mainTextureOffset = _currentOffset;
        }
    }
}
