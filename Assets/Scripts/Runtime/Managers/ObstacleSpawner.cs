using System.Collections.Generic;
using UnityEngine;
using WordWheel.Runtime;
using WordWheel.Runtime.Enums;
using WordWheel.Runtime.Controllers;

namespace WordWheel.Runtime.Managers
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private GameDifficultySettingsSO difficultySettings;
        [SerializeField] private GameMode gameMode = GameMode.Normal;
        [SerializeField] private GameObject[] obstaclePrefabs;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnZ = 80.0f;
        [SerializeField] private float targetZ = 20.0f;
        [SerializeField] private float despawnZ = 5.0f;
        [SerializeField] private float laneDistance = 2.0f;
        [SerializeField] private float startScaleFactor = 0.1f;

        private Queue<ObstacleCar> _pool;
        private List<ObstacleCar> _activeCars;
        private float _nextSpawnTime;

        private void Start()
        {
            if (difficultySettings == null)
            {
                Debug.LogError("DifficultySettings not assigned!");
                return;
            }
            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            {
                Debug.LogError("No obstacle prefabs assigned!");
                return;
            }

            var settings = difficultySettings.GetSettings(gameMode);
            int poolSize = settings.ObstaclePoolSize;

            _pool = new Queue<ObstacleCar>(poolSize);
            _activeCars = new List<ObstacleCar>();

            for (int i = 0; i < poolSize; i++)
            {
                CreateNewObstacleInPool();
            }

            ScheduleNextSpawn();
        }

        private void Update()
        {
            if (Time.time >= _nextSpawnTime)
            {
                SpawnCar();
                ScheduleNextSpawn();
            }
        }

        private ObstacleCar CreateNewObstacleInPool()
        {
            int randomIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject obj = Instantiate(obstaclePrefabs[randomIndex], transform);
            obj.SetActive(false);

            if (!obj.TryGetComponent<ObstacleCar>(out var car))
            {
                car = obj.AddComponent<ObstacleCar>();
            }

            _pool.Enqueue(car);
            return car;
        }

        private void SpawnCar()
        {
            if (_pool.Count == 0)
            {
                CreateNewObstacleInPool();
            }

            ObstacleCar car = _pool.Dequeue();
            _activeCars.Add(car);

            int laneIndex = Random.Range(0, 3);
            float targetX = (laneIndex - 1) * laneDistance;
            car.transform.position = new Vector3(targetX, 0f, spawnZ);
            car.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            var settings = difficultySettings.GetSettings(gameMode);
            car.gameObject.SetActive(true);
            car.Initialize(
                settings.ObstacleSpeed,
                spawnZ,
                targetZ,
                despawnZ,
                startScaleFactor,
                ReturnToPool
            );
        }

        private void ReturnToPool(ObstacleCar car)
        {
            _activeCars.Remove(car);
            _pool.Enqueue(car);
        }

        private void ScheduleNextSpawn()
        {
            var settings = difficultySettings.GetSettings(gameMode);
            float delay = Random.Range(settings.MinSpawnDelay, settings.MaxSpawnDelay);
            _nextSpawnTime = Time.time + delay;
        }
    }
}
