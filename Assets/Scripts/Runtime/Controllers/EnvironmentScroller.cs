using UnityEngine;
using WordWheel.Runtime;

namespace WordWheel.Runtime.Controllers
{
    public class EnvironmentScroller : MonoBehaviour
    {
        [SerializeField] private GameDifficultySettingsSO difficultySettings;
        [SerializeField] private bool useRoadSpeed = true;
        [SerializeField] private float customSpeed = 2.0f;
        [SerializeField] private Transform[] segments;
        [SerializeField] private float segmentLength = 80.0f;
        [SerializeField] private float despawnZ = -40.0f;

        private void Update()
        {
            if (segments == null || segments.Length == 0) return;
            if (useRoadSpeed && difficultySettings == null) return;

            float speed = Mathf.Abs(useRoadSpeed ? difficultySettings.RoadScrollSpeed : customSpeed);
            float totalLength = segmentLength * segments.Length;

            for (int i = 0; i < segments.Length; i++)
            {
                Transform segment = segments[i];
                if (segment == null) continue;

                // Move backward
                segment.position += Vector3.back * (speed * Time.deltaTime);

                // Seamless loop
                if (segment.position.z <= despawnZ)
                {
                    segment.position += Vector3.forward * totalLength;
                }
            }
        }
    }
}
