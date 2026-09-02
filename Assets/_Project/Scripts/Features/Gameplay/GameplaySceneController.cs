using System.Collections.Generic;
using System.Linq;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class GameplaySceneController : MonoBehaviour
    {
        [SerializeField] private MvpGameplayController _gameplayController;
        [SerializeField] private GameObject _gameplayHud;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private TMP_Text _victoryRewardsText;
        [SerializeField] private GameObject _defeatPanel;
        [SerializeField] private TMP_Text _defeatSummaryText;

        private readonly LocalSaveSlotRepository _saveSlots = new LocalSaveSlotRepository();
        private readonly List<LevelDefinition> _levels = new List<LevelDefinition>();
        private readonly AdventureMissionResultPresenter _missionResultPresenter = new AdventureMissionResultPresenter();
        private bool _accountSaveRunning;
        private bool _accountSavePending;

        private void Awake()
        {
            if (_gameplayController != null)
            {
                _gameplayController.Victory += HandleMissionVictory;
                _gameplayController.Defeat += HandleMissionDefeat;
                _gameplayController.EnemyDefeated += HandleEnemyDefeated;
                _gameplayController.CurrencyChanged += HandleCurrencyChanged;
            }
        }

        private void Start()
        {
            if (!AppRuntimeSession.HasContent || AppRuntimeSession.SelectedSlot == null || string.IsNullOrWhiteSpace(AppRuntimeSession.SelectedLevelId))
            {
                SceneManager.LoadScene(AppSceneNames.Intro);
                return;
            }

            _levels.Clear();
            _levels.AddRange(AppRuntimeSession.Catalog.Levels.Values.OrderBy(level => level.Id.Value));
            SetActive(_gameplayHud, true);
            SetActive(_pausePanel, false);
            SetActive(_victoryPanel, false);
            SetActive(_defeatPanel, false);

            if (_gameplayController != null)
            {
                _gameplayController.Resume();
                _gameplayController.LoadMission(AppRuntimeSession.SelectedLevelId, AppRuntimeSession.ActiveContentJson);
            }
        }

        private void OnDestroy()
        {
            if (_gameplayController != null)
            {
                _gameplayController.Victory -= HandleMissionVictory;
                _gameplayController.Defeat -= HandleMissionDefeat;
                _gameplayController.EnemyDefeated -= HandleEnemyDefeated;
                _gameplayController.CurrencyChanged -= HandleCurrencyChanged;
            }
        }

        public void StartGameplayWaves()
        {
            if (_gameplayController != null)
            {
                _gameplayController.StartWavesPressed();
            }
        }

        public void TogglePause()
        {
            if (_gameplayController != null)
            {
                _gameplayController.TogglePause();
            }

            SetActive(_pausePanel, Time.timeScale <= 0f);
        }

        public void ResumeGameplay()
        {
            if (_gameplayController != null)
            {
                _gameplayController.Resume();
            }

            SetActive(_pausePanel, false);
        }

        public void ReturnToSelection()
        {
            Time.timeScale = 1f;
            if (_gameplayController != null)
            {
                _gameplayController.ClearMission();
            }

            SceneManager.LoadScene(AppSceneNames.LevelSelection);
        }

        public void RestartMission()
        {
            Time.timeScale = 1f;
            if (_gameplayController != null)
            {
                _gameplayController.ClearMission();
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            if (_gameplayController != null)
            {
                _gameplayController.ClearMission();
            }

            SceneManager.LoadScene(AppSceneNames.MainMenu);
        }

        private void HandleMissionVictory(LevelDefinition level)
        {
            var slot = AppRuntimeSession.SelectedSlot;
            if (slot == null || level == null)
            {
                return;
            }

            var completedIndex = _levels.FindIndex(candidate => candidate.Id.Equals(level.Id));
            var previousHighestUnlockedLevelIndex = slot.HighestUnlockedLevelIndex;
            slot.HighestUnlockedLevelIndex = Mathf.Max(
                slot.HighestUnlockedLevelIndex,
                Mathf.Min(completedIndex + 1, _levels.Count - 1));
            slot.Scrap += level.RewardScrap;
            slot.Coins += level.RewardCoins;
            slot.Gems += level.RewardGems;
            slot.CurrenciesInitialized = true;
            _saveSlots.Save(slot);
            AppRuntimeSession.SetPlayerCurrencies(slot.Scrap, slot.Coins);
            AppRuntimeSession.SetSelectedSlot(slot);

            var unlockedNextAdventure = completedIndex >= 0 &&
                                        completedIndex < _levels.Count - 1 &&
                                        slot.HighestUnlockedLevelIndex > previousHighestUnlockedLevelIndex;
            SetText(
                _victoryRewardsText,
                _missionResultPresenter.BuildVictoryText(
                    level,
                    _gameplayController == null ? 0 : _gameplayController.CompletedWaves,
                    _gameplayController == null ? 0 : _gameplayController.TotalWaves,
                    _gameplayController == null ? 0 : _gameplayController.EnemiesDefeatedThisMission,
                    _gameplayController == null ? 0 : _gameplayController.RemainingBaseHealth,
                    unlockedNextAdventure));
            SetActive(_gameplayHud, false);
            SetActive(_pausePanel, false);
            SetActive(_victoryPanel, true);
            QueueAccountProgressionSave();
        }

        private void HandleMissionDefeat()
        {
            SetText(
                _defeatSummaryText,
                "GAME OVER\n\n" +
                $"Waves cleared  {_gameplayController.CompletedWaves}/{_gameplayController.TotalWaves}\n" +
                $"Enemies defeated  {_gameplayController.EnemiesDefeatedThisMission}\n" +
                $"Stronghold health  {_gameplayController.RemainingBaseHealth}");
            SetActive(_gameplayHud, false);
            SetActive(_pausePanel, false);
            SetActive(_victoryPanel, false);
            SetActive(_defeatPanel, true);
        }

        private void HandleEnemyDefeated(string enemyId)
        {
            AppRuntimeSession.RegisterEnemyDefeat(enemyId);
        }

        private void HandleCurrencyChanged(int scrap, int coins)
        {
            var slot = AppRuntimeSession.SelectedSlot;
            if (slot == null)
            {
                return;
            }

            slot.Scrap = Mathf.Max(0, scrap);
            slot.Coins = Mathf.Max(0, coins);
            slot.CurrenciesInitialized = true;
            _saveSlots.Save(slot);
            AppRuntimeSession.SetPlayerCurrencies(slot.Scrap, slot.Coins);
            QueueAccountProgressionSave();
        }

        private void QueueAccountProgressionSave()
        {
            _accountSavePending = true;
            if (!_accountSaveRunning)
            {
                SaveQueuedAccountProgression();
            }
        }

        private async void SaveQueuedAccountProgression()
        {
            _accountSaveRunning = true;
            try
            {
                while (_accountSavePending)
                {
                    _accountSavePending = false;
                    await AppRuntimeSession.SavePlayerProgressionAsync(CancellationToken.None);
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Could not save account progression: {exception.Message}");
            }
            finally
            {
                _accountSaveRunning = false;
                if (_accountSavePending)
                {
                    SaveQueuedAccountProgression();
                }
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }
    }
}
