using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClubGamerZone.TowerDefense.Application.Configuration;
using ClubGamerZone.TowerDefense.Application.Networking;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;
using ClubGamerZone.TowerDefense.Infrastructure.Http;
using ClubGamerZone.TowerDefense.Infrastructure.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class FullGameFlowController : MonoBehaviour
    {
        [Header("Firebase")]
        [SerializeField] private string _databaseUrl = "https://tower-defense-engine-default-rtdb.firebaseio.com";
        [SerializeField] private string _settingsEnvironment = "development";

        [Header("Content")]
        [SerializeField] private TextAsset _localContentJson;

        [Header("Screens")]
        [SerializeField] private GameObject _bootScreen;
        [SerializeField] private GameObject _mainScreen;
        [SerializeField] private GameObject _slotScreen;
        [SerializeField] private GameObject _mapScreen;
        [SerializeField] private GameObject _gameplayScreen;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _victoryPanel;

        [Header("Text")]
        [SerializeField] private TMP_Text _bootStatusText;
        [SerializeField] private TMP_Text _slotHeaderText;
        [SerializeField] private TMP_Text[] _slotLabels;
        [SerializeField] private TMP_Text[] _levelLabels;
        [SerializeField] private TMP_Text _victoryRewardsText;

        [Header("Gameplay")]
        [SerializeField] private MvpGameplayController _gameplayController;

        private readonly LocalSaveSlotRepository _saveSlots = new LocalSaveSlotRepository();
        private readonly IRestClient _restClient = new UnityRestClient();
        private readonly List<LevelDefinition> _levels = new List<LevelDefinition>();
        private CancellationTokenSource _loadCancellation;
        private ContentCatalog _catalog;
        private SaveSlotData _selectedSlot;
        private string _activeContentJson;
        private bool _slotModeContinue;
        private bool _accountSaveRunning;
        private bool _accountSavePending;

        private void Awake()
        {
            if (_gameplayController != null)
            {
                _gameplayController.Victory += HandleMissionVictory;
                _gameplayController.CurrencyChanged += HandleCurrencyChanged;
            }
        }

        private async void Start()
        {
            ShowOnly(_bootScreen);
            SetText(_bootStatusText, "Loading fleet data...");
            _loadCancellation = new CancellationTokenSource();
            _loadCancellation.CancelAfter(TimeSpan.FromSeconds(5));
            await LoadSettingsAsync(_loadCancellation.Token);
            ShowMain();
        }

        private void OnDestroy()
        {
            if (_gameplayController != null)
            {
                _gameplayController.Victory -= HandleMissionVictory;
                _gameplayController.CurrencyChanged -= HandleCurrencyChanged;
            }

            _loadCancellation?.Cancel();
            _loadCancellation?.Dispose();
        }

        public void ShowMain()
        {
            Time.timeScale = 1f;
            ShowOnly(_mainScreen);
        }

        public void ShowNewGameSlots()
        {
            _slotModeContinue = false;
            SetText(_slotHeaderText, "New Game");
            RefreshSlotLabels();
            ShowOnly(_slotScreen);
        }

        public void ShowContinueSlots()
        {
            _slotModeContinue = true;
            SetText(_slotHeaderText, "Continue");
            RefreshSlotLabels();
            ShowOnly(_slotScreen);
        }

        public void SelectSlot(int slotIndex)
        {
            var slot = _saveSlots.Load(slotIndex);

            if (slot.IsEmpty && _slotModeContinue)
            {
                return;
            }

            if (slot.IsEmpty)
            {
                slot.CommanderName = string.IsNullOrWhiteSpace(AppRuntimeSession.AuthenticatedPlayer?.Username)
                    ? "Adventurer"
                    : AppRuntimeSession.AuthenticatedPlayer.Username;
                slot.HighestUnlockedLevelIndex = 0;
                slot.Scrap = 0;
                slot.Coins = 0;
                slot.Gems = 0;
                _saveSlots.Save(slot);
            }

            SynchronizeCurrencies(slot);
            _selectedSlot = slot;
            ShowMap();
        }

        public void SelectSlot0()
        {
            SelectSlot(0);
        }

        public void SelectSlot1()
        {
            SelectSlot(1);
        }

        public void SelectSlot2()
        {
            SelectSlot(2);
        }

        public void DeleteSlot(int slotIndex)
        {
            _saveSlots.Delete(slotIndex);
            RefreshSlotLabels();
        }

        public void DeleteSlot0()
        {
            DeleteSlot(0);
        }

        public void DeleteSlot1()
        {
            DeleteSlot(1);
        }

        public void DeleteSlot2()
        {
            DeleteSlot(2);
        }

        public void ShowMap()
        {
            RefreshLevelLabels();
            ShowOnly(_mapScreen);
        }

        public void SelectLevel(int levelIndex)
        {
            if (_selectedSlot == null || levelIndex < 0 || levelIndex >= _levels.Count)
            {
                return;
            }

            if (levelIndex > _selectedSlot.HighestUnlockedLevelIndex)
            {
                return;
            }

            var level = _levels[levelIndex];
            _selectedSlot.LastPlayedLevelId = level.Id.Value;
            _saveSlots.Save(_selectedSlot);

            ShowOnly(_gameplayScreen);
            SetActive(_pausePanel, false);
            SetActive(_victoryPanel, false);

            if (_gameplayController != null)
            {
                _gameplayController.Resume();
                _gameplayController.LoadMission(level.Id.Value, _activeContentJson);
            }
        }

        public void SelectLevel0()
        {
            SelectLevel(0);
        }

        public void SelectLevel1()
        {
            SelectLevel(1);
        }

        public void SelectLevel2()
        {
            SelectLevel(2);
        }

        public void SelectLevel3()
        {
            SelectLevel(3);
        }

        public void SelectLevel4()
        {
            SelectLevel(4);
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

        public void ReturnToMap()
        {
            Time.timeScale = 1f;
            if (_gameplayController != null)
            {
                _gameplayController.ClearMission();
            }

            ShowMap();
        }

        private async System.Threading.Tasks.Task LoadSettingsAsync(CancellationToken cancellationToken)
        {
            _activeContentJson = _localContentJson == null ? string.Empty : _localContentJson.text;
            var loadedRemote = await TryLoadRemoteContentAsync(cancellationToken);

            if (!TryBuildCatalog(_activeContentJson))
            {
                SetText(_bootStatusText, "Local content is invalid. Check console.");
                return;
            }

            SetText(_bootStatusText, loadedRemote ? "Cloud balance loaded." : "Offline defaults loaded.");
        }

        private async System.Threading.Tasks.Task<bool> TryLoadRemoteContentAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_databaseUrl))
            {
                return false;
            }

            try
            {
                var baseUrl = _databaseUrl.TrimEnd('/');
                var activeVersionUrl = $"{baseUrl}/gameSettings/{_settingsEnvironment}/activeVersion.json";
                var activeResponse = await _restClient.SendAsync(
                    new RestRequest(RestHttpMethod.Get, activeVersionUrl, string.Empty, null),
                    cancellationToken);

                if (!activeResponse.IsSuccess || string.IsNullOrWhiteSpace(activeResponse.Body) || activeResponse.Body == "null")
                {
                    return false;
                }

                var activeVersion = activeResponse.Body.Trim().Trim('"');
                var contentUrl = $"{baseUrl}/gameSettings/{_settingsEnvironment}/versions/{activeVersion}/starterContent.json";
                var contentResponse = await _restClient.SendAsync(
                    new RestRequest(RestHttpMethod.Get, contentUrl, string.Empty, null),
                    cancellationToken);

                if (!contentResponse.IsSuccess || string.IsNullOrWhiteSpace(contentResponse.Body) || contentResponse.Body == "null")
                {
                    return false;
                }

                _activeContentJson = contentResponse.Body;
                return true;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("Remote settings timed out, using local defaults.");
                return false;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Remote settings unavailable, using local defaults. {exception.Message}");
                return false;
            }
        }

        private bool TryBuildCatalog(string contentJson)
        {
            var parser = new UnityContentJsonParser();
            var validator = new StarterContentValidator(new ContentValidationLimits());
            var builder = new StarterContentCatalogBuilder(validator);
            var result = builder.Build(parser.Parse(contentJson));

            if (!result.IsSuccess)
            {
                Debug.LogError(string.Join("\n", result.Validation.Issues.Select(issue => issue.ToString())));
                return false;
            }

            _catalog = result.Catalog;
            _levels.Clear();
            _levels.AddRange(_catalog.Levels.Values.OrderBy(level => level.Id.Value));
            return true;
        }

        private void HandleMissionVictory(LevelDefinition level)
        {
            if (_selectedSlot == null || level == null)
            {
                return;
            }

            var completedIndex = _levels.FindIndex(candidate => candidate.Id.Equals(level.Id));
            _selectedSlot.HighestUnlockedLevelIndex = Mathf.Max(
                _selectedSlot.HighestUnlockedLevelIndex,
                Mathf.Min(completedIndex + 1, _levels.Count - 1));
            _selectedSlot.Scrap += level.RewardScrap;
            _selectedSlot.Coins += level.RewardCoins;
            _selectedSlot.Gems += level.RewardGems;
            _selectedSlot.CurrenciesInitialized = true;
            _saveSlots.Save(_selectedSlot);
            AppRuntimeSession.SetPlayerCurrencies(_selectedSlot.Scrap, _selectedSlot.Coins);
            QueueAccountProgressionSave();

            SetText(
                _victoryRewardsText,
                $"Mission Complete\nScrap +{level.RewardScrap}\nCoins +{level.RewardCoins}\nGems +{level.RewardGems}\nItem: {level.RewardItemId}");
            SetActive(_victoryPanel, true);
        }

        private void HandleCurrencyChanged(int scrap, int coins)
        {
            if (_selectedSlot == null)
            {
                return;
            }

            _selectedSlot.Scrap = Mathf.Max(0, scrap);
            _selectedSlot.Coins = Mathf.Max(0, coins);
            _selectedSlot.CurrenciesInitialized = true;
            _saveSlots.Save(_selectedSlot);
            AppRuntimeSession.SetPlayerCurrencies(_selectedSlot.Scrap, _selectedSlot.Coins);
            QueueAccountProgressionSave();
        }

        private void SynchronizeCurrencies(SaveSlotData slot)
        {
            var progression = AppRuntimeSession.AuthenticatedPlayer?.Progression;
            if (progression == null)
            {
                return;
            }

            if (progression.CurrenciesInitialized)
            {
                slot.Scrap = progression.Scrap;
                slot.Coins = progression.Coins;
                slot.CurrenciesInitialized = true;
                _saveSlots.Save(slot);
                return;
            }

            if (slot.CurrenciesInitialized)
            {
                AppRuntimeSession.SetPlayerCurrencies(slot.Scrap, slot.Coins);
                QueueAccountProgressionSave();
            }
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
            catch (Exception exception)
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

        private void RefreshSlotLabels()
        {
            for (var i = 0; i < _slotLabels.Length; i++)
            {
                var slot = _saveSlots.Load(i);
                var text = slot.IsEmpty
                    ? $"Slot {i + 1}\nEmpty"
                    : $"Slot {i + 1}\nHero {slot.CommanderName}\nUnlocked {slot.HighestUnlockedLevelIndex + 1}/5\nCoins {slot.Coins} Gems {slot.Gems}";
                SetText(_slotLabels[i], text);
            }
        }

        private void RefreshLevelLabels()
        {
            for (var i = 0; i < _levelLabels.Length; i++)
            {
                if (i >= _levels.Count)
                {
                    SetText(_levelLabels[i], "Locked");
                    continue;
                }

                var level = _levels[i];
                var unlocked = _selectedSlot != null && i <= _selectedSlot.HighestUnlockedLevelIndex;
                SetText(_levelLabels[i], unlocked ? level.DisplayNameKey : "Locked");
            }
        }

        private void ShowOnly(GameObject visibleScreen)
        {
            SetActive(_bootScreen, ReferenceEquals(visibleScreen, _bootScreen));
            SetActive(_mainScreen, ReferenceEquals(visibleScreen, _mainScreen));
            SetActive(_slotScreen, ReferenceEquals(visibleScreen, _slotScreen));
            SetActive(_mapScreen, ReferenceEquals(visibleScreen, _mapScreen));
            SetActive(_gameplayScreen, ReferenceEquals(visibleScreen, _gameplayScreen));
            SetActive(_pausePanel, false);
            SetActive(_victoryPanel, false);
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
