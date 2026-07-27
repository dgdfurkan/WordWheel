using UnityEngine;
using WordWheel.Runtime;

namespace WordWheel.Runtime.Controllers
{
    public class EnvironmentScroller : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform[] segments;
        [SerializeField] private float bufferZ = 5.0f;
        [SerializeField] private bool startPaused = true;

        private bool _isScrollingPaused;
        private float _calculatedSegmentLength;

        public bool IsScrollingPaused
        {
            get => _isScrollingPaused;
            set => _isScrollingPaused = value;
        }

        private void Awake()
        {
            _isScrollingPaused = startPaused;
        }

        private void Start()
        {
            SanitizeSegments();
            CalculateSegmentLength();

            if (playerTransform == null)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
        }

        private void CalculateSegmentLength()
        {
            if (segments == null || segments.Length == 0) return;

            foreach (var seg in segments)
            {
                if (seg == null) continue;

                var rend = seg.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    _calculatedSegmentLength = rend.bounds.size.z;
                    if (_calculatedSegmentLength > 0.01f) return;
                }

                var col = seg.GetComponentInChildren<Collider>();
                if (col != null)
                {
                    _calculatedSegmentLength = col.bounds.size.z;
                    if (_calculatedSegmentLength > 0.01f) return;
                }
            }
        }

        private void SanitizeSegments()
        {
            if (segments == null || segments.Length == 0) return;

            var uniqueSegments = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < segments.Length; i++)
            {
                Transform seg = segments[i];
                if (seg == null) continue;

                if (seg.childCount == 0 && seg.parent != null && seg.parent != transform)
                {
                    seg = seg.parent;
                }

                if (!uniqueSegments.Contains(seg))
                {
                    uniqueSegments.Add(seg);
                }
            }

            segments = uniqueSegments.ToArray();
        }

        private void Update()
        {
            if (_isScrollingPaused) return;
            if (segments == null || segments.Length == 0) return;
            if (playerTransform == null) return;
            if (_calculatedSegmentLength <= 0.01f) return;

            float totalLength = _calculatedSegmentLength * segments.Length;
            float despawnThreshold = -(_calculatedSegmentLength * 0.5f + bufferZ);

            for (int i = 0; i < segments.Length; i++)
            {
                Transform segment = segments[i];
                if (segment == null) continue;

                float relativeZ = segment.position.z - playerTransform.position.z;
                if (relativeZ <= despawnThreshold)
                {
                    segment.position += Vector3.forward * totalLength;
                }
            }
        }
    }
}
