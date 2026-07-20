using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using WordWheel.Runtime.Controllers;
using WordWheel.Runtime.UI;

namespace WordWheel.Runtime.Managers
{
    public class WordGameplayManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ObstacleSpawner obstacleSpawner;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private WordQuestionUI questionUI;
        [SerializeField] private GameObject wordGatePrefab;
        [SerializeField] private GameDifficultySettingsSO difficultySettings;

        [Header("Localization Settings")]
        [SerializeField] private string stringTableName = "WordsStringTable";
        [SerializeField] private string nativeLocaleCode = "tr";
        [SerializeField] private string targetLocaleCode = "en";

        [Header("Spawning Settings")]
        [SerializeField] private float wordPhaseInterval = 15.0f;
        [SerializeField] private float spawnZ = 80.0f;
        [SerializeField] private float targetZ = 20.0f;
        [SerializeField] private float despawnZ = -10.0f;
        [SerializeField] private float checkZ = 0.0f;
        [SerializeField] private float laneDistance = 2.0f;
        [SerializeField] private float startScaleFactor = 0.1f;

        private StringTable _nativeTable;
        private StringTable _targetTable;

        private IEnumerator Start()
        {
            if (obstacleSpawner == null || playerController == null || wordGatePrefab == null)
            {
                Debug.LogError("WordGameplayManager: Missing required references!");
                yield break;
            }

            // Wait for Unity Localization to initialize
            yield return LocalizationSettings.InitializationOperation;

            var nativeLocale = LocalizationSettings.AvailableLocales.GetLocale(nativeLocaleCode);
            var targetLocale = LocalizationSettings.AvailableLocales.GetLocale(targetLocaleCode);

            _nativeTable = LocalizationSettings.StringDatabase.GetTable(stringTableName, nativeLocale);
            _targetTable = LocalizationSettings.StringDatabase.GetTable(stringTableName, targetLocale);

            if (_nativeTable == null || _targetTable == null)
            {
                Debug.LogError($"WordGameplayManager: Failed to load localization tables for native '{nativeLocaleCode}' or target '{targetLocaleCode}'.");
                yield break;
            }

            StartCoroutine(GameplayLoop());
        }

        private IEnumerator GameplayLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(wordPhaseInterval);
                yield return StartCoroutine(RunWordPhase());
            }
        }

        private IEnumerator RunWordPhase()
        {
            // Pause obstacle spawning and clear existing cars
            if (obstacleSpawner != null)
            {
                obstacleSpawner.IsSpawningPaused = true;
                obstacleSpawner.ClearActiveObstacles();
            }

            // Wait a moment for path to clear
            yield return new WaitForSeconds(1.0f);

            var entries = _nativeTable.SharedData.Entries;
            if (entries == null || entries.Count < 2)
            {
                Debug.LogWarning("WordGameplayManager: Not enough entries in string table.");
                ResumeObstacles();
                yield break;
            }

            // Select correct and incorrect entries
            int correctIndex = Random.Range(0, entries.Count);
            var correctEntry = entries[correctIndex];

            int incorrectIndex = Random.Range(0, entries.Count);
            while (incorrectIndex == correctIndex)
            {
                incorrectIndex = Random.Range(0, entries.Count);
            }
            var incorrectEntry = entries[incorrectIndex];

            string nativeWord = _nativeTable.GetEntry(correctEntry.Id)?.LocalizedValue;
            string correctTargetWord = _targetTable.GetEntry(correctEntry.Id)?.LocalizedValue;
            string incorrectTargetWord = _targetTable.GetEntry(incorrectEntry.Id)?.LocalizedValue;

            if (string.IsNullOrEmpty(nativeWord) || string.IsNullOrEmpty(correctTargetWord) || string.IsNullOrEmpty(incorrectTargetWord))
            {
                Debug.LogWarning("WordGameplayManager: Missing translation value for selected entry.");
                ResumeObstacles();
                yield break;
            }

            // Setup lanes: 0 is left, 2 is right
            int correctLane = Random.Range(0, 2) == 0 ? 0 : 2;
            string leftWord = correctLane == 0 ? correctTargetWord : incorrectTargetWord;
            string rightWord = correctLane == 2 ? correctTargetWord : incorrectTargetWord;

            // Spawn Word Gate
            GameObject gateObj = Instantiate(wordGatePrefab, new Vector3(0f, 0f, spawnZ), Quaternion.identity);
            if (!gateObj.TryGetComponent<WordGateController>(out var gateController))
            {
                gateController = gateObj.AddComponent<WordGateController>();
            }

            bool choiceMade = false;
            int chosenLane = -1;
            float speed = Mathf.Abs(difficultySettings != null ? difficultySettings.RoadScrollSpeed : 2.0f);

            gateController.Initialize(
                leftWord,
                rightWord,
                speed,
                spawnZ,
                targetZ,
                despawnZ,
                checkZ,
                laneDistance,
                startScaleFactor,
                playerController,
                (lane) =>
                {
                    chosenLane = lane;
                    choiceMade = true;
                }
            );

            // Show screen word
            if (questionUI != null)
            {
                questionUI.ShowWord(nativeWord);
            }

            // Wait for completion
            while (!choiceMade && gateObj != null)
            {
                yield return null;
            }

            if (choiceMade)
            {
                OnWordChoiceMade(chosenLane, correctLane);
            }
            else if (questionUI != null)
            {
                questionUI.Hide();
            }

            // Show choice result for 2 seconds
            yield return new WaitForSeconds(2.0f);

            ResumeObstacles();
        }

        private void OnWordChoiceMade(int selectedLane, int correctLane)
        {
            if (selectedLane == correctLane)
            {
                Debug.Log($"<color=green>Word Challenge: CORRECT! Selected lane {selectedLane} matches correct lane {correctLane}.</color>");
            }
            else
            {
                Debug.Log($"<color=red>Word Challenge: INCORRECT! Selected lane {selectedLane}, correct lane was {correctLane}.</color>");
            }

            if (questionUI != null)
            {
                questionUI.Hide();
            }
        }

        private void ResumeObstacles()
        {
            if (obstacleSpawner != null)
            {
                obstacleSpawner.IsSpawningPaused = false;
            }
        }
    }
}
