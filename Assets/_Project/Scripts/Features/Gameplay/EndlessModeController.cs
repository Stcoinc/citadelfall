using System;
using System.Threading;
using ClubGamerZone.TowerDefense.Application.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class EndlessModeController : MonoBehaviour
    {
        [SerializeField] private MvpGameplayController _gameplayController;
        [SerializeField] private string _endlessLevelId = "level_classic_001";
        [SerializeField] private TMP_Text _roundText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _bestScoreText;
        [SerializeField] private TMP_Text _nextWaveText;
        [SerializeField] private Button _nextWaveButton;
        [SerializeField] private GameObject _defeatPanel;
        [SerializeField] private TMP_Text _defeatSummaryText;

        private readonly LocalSaveSlotRepository _saveSlots = new LocalSaveSlotRepository();
        private bool _accountSaveRunning;
        private bool _accountSavePending;

        private void Awake()
        {
            if (_gameplayController == null)
            {
                return;
            }

            _gameplayController.EndlessWaveCompleted += HandleWaveCompleted;
            _gameplayController.EndlessScoreChanged += HandleScoreChanged;
            _gameplayController.EndlessDefeated += HandleDefeat;
            _gameplayController.EnemyDefeated += HandleEnemyDefeated;
            _gameplayController.CurrencyChanged += HandleCurrencyChanged;
        }

        private void Start()
        {
            if (!AppRuntimeSession.HasContent || AppRuntimeSession.SelectedSlot == null)
            {
                SceneManager.LoadScene(AppSceneNames.Intro);
                return;
            }

            SetActive(_defeatPanel, false);
            _gameplayController.Resume();
            _gameplayController.LoadEndlessMission(_endlessLevelId, AppRuntimeSession.ActiveContentJson);
            RefreshHud();
        }

        private void OnDestroy()
        {
            if (_gameplayController == null)
            {
                return;
            }

            _gameplayController.EndlessWaveCompleted -= HandleWaveCompleted;
            _gameplayController.EndlessScoreChanged -= HandleScoreChanged;
            _gameplayController.EndlessDefeated -= HandleDefeat;
            _gameplayController.EnemyDefeated -= HandleEnemyDefeated;
            _gameplayController.CurrencyChanged -= HandleCurrencyChanged;
        }

        public void StartNextWave()
        {
            _gameplayController?.StartNextEndlessWave();
            RefreshHud();
        }

        public void TogglePause()
        {
            _gameplayController?.TogglePause();
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            _gameplayController?.ClearMission();
            SceneManager.LoadScene(AppSceneNames.MainMenu);
        }

        public void RestartEndless()
        {
            Time.timeScale = 1f;
            _gameplayController?.ClearMission();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void HandleWaveCompleted(int completedRound, int completedWave, int score)
        {
            PersistScore(score, completedRound);
            RefreshHud();
        }

        private void HandleScoreChanged(int score, int round)
        {
            PersistScore(score, round);
            RefreshHud();
        }

        private void HandleDefeat()
        {
            var score = _gameplayController == null ? 0 : _gameplayController.EndlessScore;
            var round = _gameplayController == null ? 1 : _gameplayController.EndlessRound;
            PersistScore(score, round);
            SetText(_defeatSummaryText, $"FINAL SCORE  {score:N0}\nROUND REACHED  {round}\nPERSONAL BEST  {Mathf.Max(score, AppRuntimeSession.SelectedSlot.BestEndlessScore):N0}");
            SetActive(_defeatPanel, true);
            if (_nextWaveButton != null)
            {
                _nextWaveButton.interactable = false;
            }
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
            QueueAccountSave();
        }

        private void PersistScore(int score, int round)
        {
            var slot = AppRuntimeSession.SelectedSlot;
            if (slot == null)
            {
                return;
            }

            var changed = false;
            if (score > slot.BestEndlessScore)
            {
                slot.BestEndlessScore = score;
                changed = true;
            }

            if (round > slot.HighestEndlessRound)
            {
                slot.HighestEndlessRound = round;
                changed = true;
            }

            if (changed)
            {
                _saveSlots.Save(slot);
            }

            if (AppRuntimeSession.AuthenticatedPlayer?.Progression?.RecordEndlessScore(score, round) == true)
            {
                QueueAccountSave();
            }
        }

        private void RefreshHud()
        {
            if (_gameplayController == null)
            {
                return;
            }

            SetText(_roundText, $"ROUND {_gameplayController.EndlessRound}");
            SetText(_scoreText, $"SCORE {_gameplayController.EndlessScore:N0}");
            SetText(_bestScoreText, $"BEST {Mathf.Max(_gameplayController.EndlessScore, AppRuntimeSession.SelectedSlot?.BestEndlessScore ?? 0):N0}");
            SetText(_nextWaveText, $"START WAVE {_gameplayController.EndlessWaveInRound}/{EndlessModeRules.WavesPerRound}");
            if (_nextWaveButton != null)
            {
                _nextWaveButton.interactable = _gameplayController.CanStartEndlessWave;
            }
        }

        private void QueueAccountSave()
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
                Debug.LogWarning($"Endless progression remains local until synchronization succeeds: {exception.Message}");
            }
            finally
            {
                _accountSaveRunning = false;
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
