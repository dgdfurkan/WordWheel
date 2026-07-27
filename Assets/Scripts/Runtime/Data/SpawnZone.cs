using UnityEngine;

namespace WordWheel.Runtime.Data
{
    [System.Serializable]
    public class SpawnZone
    {
        public string zoneName = "New Zone";
        public Color zoneColor = new Color(0f, 1f, 0f, 0.35f);
        public bool isEnabled = true;

        [Header("Zone Boundaries")]
        public float xPosition = 5f;
        public float width = 3f;

        [Header("Spawn Configuration")]
        public GameObject[] prefabs;
        public int spawnCount = 10;
        public float minSpacing = 1.5f;
        public bool randomYRotation = true;

        [Header("Options")]
        public bool mirrorToLeft = true;

        public float MinX => xPosition - width * 0.5f;
        public float MaxX => xPosition + width * 0.5f;
        public float CenterX => xPosition;
    }
}
