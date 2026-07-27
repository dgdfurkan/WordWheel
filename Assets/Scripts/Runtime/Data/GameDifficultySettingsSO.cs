using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WordWheel.Runtime.Enums;

namespace WordWheel.Runtime
{
    [CreateAssetMenu(fileName = "GameDifficultySettings", menuName = "WordWheel/Data/DifficultySettings")]
    public class GameDifficultySettingsSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public class GameModeSettings
        {
            [FormerlySerializedAs("roadScrollSpeed")]
            [SerializeField] private float playerSpeed = 2.0f;
            [SerializeField] private float playerTransitionSpeed = 15.0f;
            [SerializeField] private float obstacleSpeed = 5.0f;
            [SerializeField] private int obstaclePoolSize = 10;
            [SerializeField] private float minSpawnDelay = 1.5f;
            [SerializeField] private float maxSpawnDelay = 3.0f;

            public float PlayerSpeed => playerSpeed;
            public float PlayerTransitionSpeed => playerTransitionSpeed;
            public float ObstacleSpeed => obstacleSpeed;
            public int ObstaclePoolSize => obstaclePoolSize;
            public float MinSpawnDelay => minSpawnDelay;
            public float MaxSpawnDelay => maxSpawnDelay;
        }

        [Serializable]
        public struct GameModeSettingEntry
        {
            public GameMode mode;
            public GameModeSettings settings;
        }

        [SerializeField] private List<GameModeSettingEntry> modeSettingsList = new List<GameModeSettingEntry>();

        private Dictionary<GameMode, GameModeSettings> _modeSettingsDict = new Dictionary<GameMode, GameModeSettings>();
        private static readonly GameModeSettings _defaultSettings = new GameModeSettings();

        public float PlayerSpeed => GetSettings(GameMode.Normal).PlayerSpeed;
        public float PlayerTransitionSpeed => GetSettings(GameMode.Normal).PlayerTransitionSpeed;
        public float ObstacleSpeed => GetSettings(GameMode.Normal).ObstacleSpeed;

        public GameModeSettings GetSettings(GameMode mode)
        {
            if (_modeSettingsDict.TryGetValue(mode, out var settings))
            {
                return settings;
            }
            if (modeSettingsList != null && modeSettingsList.Count > 0)
            {
                return modeSettingsList[0].settings;
            }
            return _defaultSettings;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            _modeSettingsDict.Clear();
            foreach (var entry in modeSettingsList)
            {
                if (!_modeSettingsDict.ContainsKey(entry.mode))
                {
                    _modeSettingsDict.Add(entry.mode, entry.settings);
                }
            }
        }
    }
}
