using System.Collections;
using System;
using System.Linq;
using ClubGamerZone.TowerDefense.Application.Configuration;
using ClubGamerZone.TowerDefense.Application.Gameplay;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;
using ClubGamerZone.TowerDefense.Infrastructure.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class MvpGameplayController : MonoBehaviour
    {
        [SerializeField] private TextAsset _starterContentJson;
        [SerializeField] private string _levelId = "level_classic_001";
        [SerializeField] private Transform[] _pathPoints;
        [SerializeField] private TowerPlacementSocket[] _towerSockets;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private GameObject _towerPrefab;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private TMP_Text _selectedTowerText;
        [SerializeField] private TMP_Text _baseHealthValueText;
        [SerializeField] private TMP_Text _scrapValueText;
        [SerializeField] private TMP_Text _coinsValueText;
        [SerializeField] private TMP_Text _waveValueText;
        [SerializeField] private TMP_Text _enemiesValueText;
        [SerializeField] private Button[] _towerOptionButtons;
        [SerializeField] private TMP_Text[] _towerOptionLabels;
        [SerializeField] private Image[] _towerOptionFrames;
        [SerializeField] private string[] _towerOptionIds;
        [SerializeField] private string _emergencyHeroId = "hero_warrior";
        [SerializeField] private Color _selectedTowerFrameColor = new Color(0.35f, 1f, 1f, 1f);
        [SerializeField] private Color _unselectedTowerFrameColor = Color.white;
        [SerializeField] private Color _unavailableTowerFrameColor = new Color(0.28f, 0.3f, 0.32f, 0.82f);
        [SerializeField] private bool _autoBuildFirstTower = true;
        [SerializeField] private bool _startAutomatically = true;
        [SerializeField] private bool _manualWaveStart = false;
        [SerializeField] private int _startingCoins = 500;
        [SerializeField] private int _startingGems = 10;
        [SerializeField, Range(0f, 1f)] private float _towerSellRefundPercent = 0.4f;

        private readonly EnemyRegistry _enemyRegistry = new EnemyRegistry();
        private readonly TowerProgressionService _towerProgressionService = new TowerProgressionService();
        private readonly TowerMergeSelectionCoordinator _mergeSelectionCoordinator = new TowerMergeSelectionCoordinator();
        private MissionRuntimeState _missionState;
        private ContentCatalog _catalog;
        private LevelDefinition _levelDefinition;
        private WaveSetDefinition _waveSetDefinition;
        private TowerDefinition _selectedTowerDefinition;
        private bool _wavesStarted;
        private bool _victoryReported;
        private bool _defeatReported;
        private int _enemiesDefeatedThisMission;
        private bool _endlessMode;
        private int _endlessGlobalWave;
        private int _endlessScore;

        public event Action<LevelDefinition> Victory;

        public event Action Defeat;

        public event Action<string> EnemyDefeated;

        public event Action<int, int> CurrencyChanged;

        public event Action<int, int, int> EndlessWaveCompleted;

        public event Action<int, int> EndlessScoreChanged;

        public event Action EndlessDefeated;

        public MissionOutcome Outcome => _missionState == null ? MissionOutcome.Running : _missionState.Outcome;

        public int RemainingBaseHealth => _missionState == null ? 0 : _missionState.BaseHealth;

        public int CompletedWaves => _missionState == null ? 0 : _missionState.CompletedWaves;

        public int TotalWaves => _missionState == null ? 0 : _missionState.TotalWaves;

        public int EnemiesDefeatedThisMission => _enemiesDefeatedThisMission;

        public int EndlessRound => EndlessModeRules.GetRound(_endlessGlobalWave);

        public int EndlessWaveInRound => EndlessModeRules.GetWaveInRound(_endlessGlobalWave);

        public int EndlessScore => _endlessScore;

        public bool CanStartEndlessWave => _endlessMode && !_wavesStarted && _missionState != null && _missionState.Outcome == MissionOutcome.Running;

        private void Start()
        {
            if (!_startAutomatically)
            {
                return;
            }

            if (!TryLoadCatalog())
            {
                return;
            }

            InitializeMission();

            if (!_manualWaveStart)
            {
                StartWavesPressed();
            }
        }

        public void LoadMission(string levelId, string contentJson)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                ReportStatus("Missing level id.");
                return;
            }

            if (!TryLoadCatalog(contentJson))
            {
                return;
            }

            _levelId = levelId;
            _endlessMode = false;
            InitializeMission();
        }

        public void LoadEndlessMission(string levelId, string contentJson)
        {
            if (string.IsNullOrWhiteSpace(levelId) || !TryLoadCatalog(contentJson))
            {
                ReportStatus("Endless content could not be loaded.");
                return;
            }

            _levelId = levelId;
            _endlessMode = true;
            _endlessGlobalWave = 0;
            _endlessScore = 0;
            InitializeMission();
            ReportStatus("Endless defense ready. Build heroes, then start Wave 1.");
        }

        public void ClearMission()
        {
            StopAllCoroutines();
            ClearLiveEnemies();
            _enemyRegistry.Clear();
            _wavesStarted = false;
            _victoryReported = false;
            _defeatReported = false;
            _enemiesDefeatedThisMission = 0;
            _endlessMode = false;
            _endlessGlobalWave = 0;
            _endlessScore = 0;
            if (_missionState != null)
            {
                _missionState.CurrencyChanged -= HandleCurrencyChanged;
            }

            _missionState = null;

            if (_towerSockets == null)
            {
                return;
            }

            foreach (var socket in _towerSockets)
            {
                if (socket != null)
                {
                    socket.ResetSocket();
                }
            }
        }

        private bool TryLoadCatalog()
        {
            return _starterContentJson != null && TryLoadCatalog(_starterContentJson.text);
        }

        private bool TryLoadCatalog(string contentJson)
        {
            if (string.IsNullOrWhiteSpace(contentJson))
            {
                ReportStatus("Missing starter content JSON.");
                return false;
            }

            var parser = new UnityContentJsonParser();
            var validator = new StarterContentValidator(new ContentValidationLimits());
            var builder = new StarterContentCatalogBuilder(validator);
            var dto = parser.Parse(contentJson);
            var buildResult = builder.Build(dto);

            if (!buildResult.IsSuccess)
            {
                ReportStatus("Starter content is invalid. Check console.");
                Debug.LogError(string.Join("\n", buildResult.Validation.Issues.Select(issue => issue.ToString())));
                return false;
            }

            _catalog = buildResult.Catalog;
            return true;
        }

        private void InitializeMission()
        {
            StopAllCoroutines();
            ClearLiveEnemies();
            _enemyRegistry.Clear();
            _wavesStarted = false;
            _victoryReported = false;
            _defeatReported = false;
            _enemiesDefeatedThisMission = 0;

            if (_towerSockets != null)
            {
                foreach (var socket in _towerSockets)
                {
                    if (socket != null)
                    {
                        socket.ResetSocket();
                    }
                }
            }

            var requestedLevelId = new StableId(_levelId);
            _levelDefinition = _catalog.Levels.TryGetValue(requestedLevelId, out var requestedLevel)
                ? requestedLevel
                : _catalog.Levels.Values.First();

            _waveSetDefinition = _catalog.WaveSets[_levelDefinition.WaveSetId];
            _selectedTowerDefinition = FindDefaultBuildableTower();
            if (_missionState != null)
            {
                _missionState.CurrencyChanged -= HandleCurrencyChanged;
            }

            var selectedSlot = AppRuntimeSession.SelectedSlot;
            var useSavedCurrencies = selectedSlot != null && selectedSlot.CurrenciesInitialized;
            _missionState = new MissionRuntimeState(
                useSavedCurrencies ? selectedSlot.Scrap : _levelDefinition.StartingScrap,
                _levelDefinition.BaseHealth,
                _endlessMode ? int.MaxValue : _waveSetDefinition.Waves.Count,
                useSavedCurrencies ? selectedSlot.Coins : _startingCoins,
                _startingGems);
            _missionState.CurrencyChanged += HandleCurrencyChanged;
            HandleCurrencyChanged(_missionState.Scrap, _missionState.Coins);

            foreach (var socket in _towerSockets)
            {
                if (socket != null)
                {
                    socket.Initialize(
                        _selectedTowerDefinition,
                        GetTowerPrefab(_selectedTowerDefinition),
                        _enemyRegistry,
                        _catalog.Towers,
                        _towerProgressionService,
                        _mergeSelectionCoordinator,
                        TrySpendBuildCost,
                        TrySpendCurrency,
                        AddScrap,
                        HandleTowerBuilt,
                        _towerSellRefundPercent);
                }
            }

            UpdateSelectedTowerLabel();
            RefreshTowerBuildBar();

            if (_autoBuildFirstTower && _towerSockets != null && _towerSockets.Length > 0 && _towerSockets[0] != null)
            {
                _towerSockets[0].TryBuildTower();
            }

            UpdateStatus();
        }

        public void StartWavesPressed()
        {
            if (_missionState == null)
            {
                ReportStatus("Select a mission first.");
                return;
            }

            if (_wavesStarted)
            {
                return;
            }

            if (_endlessMode)
            {
                StartNextEndlessWave();
                return;
            }

            if (!HasPlacedTower())
            {
                ReportStatus("Place at least one hero before starting waves.");
                return;
            }

            _wavesStarted = true;
            StartCoroutine(RunWaves());
        }

        public void StartNextEndlessWave()
        {
            if (!CanStartEndlessWave)
            {
                return;
            }

            if (!HasPlacedTower())
            {
                ReportStatus("Place at least one hero before starting the next wave.");
                return;
            }

            _wavesStarted = true;
            StartCoroutine(RunEndlessWave());
        }

        public void SelectTowerById(string towerId)
        {
            if (_catalog == null)
            {
                ReportStatus("Load a mission before choosing heroes.");
                return;
            }

            if (string.IsNullOrWhiteSpace(towerId) || !_catalog.Towers.TryGetValue(new StableId(towerId), out var tower))
            {
                ReportStatus("Unknown hero.");
                return;
            }

            if (tower.BuildCost < 0)
            {
                ReportStatus("That hero is not available.");
                return;
            }

            if (!IsTowerUnlocked(tower))
            {
                ReportStatus($"{tower.DisplayNameKey} is locked. Buy it from the map shop.");
                return;
            }

            _selectedTowerDefinition = tower;
            ApplySelectedTowerToSockets();
            UpdateSelectedTowerLabel();
            RefreshTowerSelection();
            ReportStatus(
                IsFirstWarriorFree(tower)
                    ? $"Selected {tower.DisplayNameKey}. Your first Warrior is free. Strong vs {tower.PreferredEnemyTag}."
                    : $"Selected {tower.DisplayNameKey}. Cost {tower.BuildCost} Scrap. Strong vs {tower.PreferredEnemyTag}.");
        }

        public void SelectTowerOption0()
        {
            SelectTowerOption(0);
        }

        public void SelectTowerOption1()
        {
            SelectTowerOption(1);
        }

        public void SelectTowerOption2()
        {
            SelectTowerOption(2);
        }

        public void SelectTowerOption3()
        {
            SelectTowerOption(3);
        }

        public void SelectTowerOption4()
        {
            SelectTowerOption(4);
        }

        public void SelectTowerOption5()
        {
            SelectTowerOption(5);
        }

        public void SelectTowerOption6()
        {
            SelectTowerOption(6);
        }

        public void TogglePause()
        {
            Time.timeScale = Time.timeScale > 0f ? 0f : 1f;
            ReportStatus(Time.timeScale > 0f ? "Resumed." : "Paused.");
        }

        public void Resume()
        {
            Time.timeScale = 1f;
            ReportStatus("Resumed.");
        }

        private IEnumerator RunWaves()
        {
            foreach (var wave in _waveSetDefinition.Waves)
            {
                yield return new WaitForSeconds(wave.StartDelaySeconds);
                ReportStatus($"Starting {wave.Id}.");

                foreach (var spawn in wave.Spawns)
                {
                    var enemyDefinition = _catalog.Enemies[spawn.EnemyId];

                    for (var i = 0; i < spawn.Count; i++)
                    {
                        SpawnEnemy(enemyDefinition);
                        yield return new WaitForSeconds(spawn.IntervalSeconds);
                    }
                }

                _missionState.CompleteWave();
                UpdateStatus();
            }
        }

        private IEnumerator RunEndlessWave()
        {
            var round = EndlessRound;
            var waveInRound = EndlessWaveInRound;
            var template = _waveSetDefinition.Waves[_endlessGlobalWave % _waveSetDefinition.Waves.Count];
            yield return new WaitForSeconds(Mathf.Min(1f, template.StartDelaySeconds));
            ReportStatus($"Round {round} • Wave {waveInRound}/{EndlessModeRules.WavesPerRound} started.");

            foreach (var spawn in template.Spawns)
            {
                var source = _catalog.Enemies[spawn.EnemyId];
                var scaled = BuildEndlessEnemy(source, round);
                var count = EndlessModeRules.GetSpawnCount(spawn.Count, round);
                var interval = EndlessModeRules.GetSpawnInterval(spawn.IntervalSeconds, round);
                for (var index = 0; index < count; index++)
                {
                    SpawnEnemy(scaled);
                    yield return new WaitForSeconds(interval);
                }
            }

            while (_missionState != null && _missionState.ActiveEnemies > 0 && _missionState.Outcome == MissionOutcome.Running)
            {
                yield return null;
            }

            if (_missionState == null || _missionState.Outcome != MissionOutcome.Running)
            {
                _wavesStarted = false;
                yield break;
            }

            _missionState.CompleteWave();
            _endlessGlobalWave++;
            if (waveInRound == EndlessModeRules.WavesPerRound)
            {
                _endlessScore += EndlessModeRules.GetRoundCompletionBonus(round);
                EndlessScoreChanged?.Invoke(_endlessScore, round);
            }

            _wavesStarted = false;
            UpdateStatus();
            EndlessWaveCompleted?.Invoke(round, waveInRound, _endlessScore);
        }

        private static EnemyDefinition BuildEndlessEnemy(EnemyDefinition source, int round)
        {
            var healthMultiplier = EndlessModeRules.GetHealthMultiplier(round);
            var speedMultiplier = EndlessModeRules.GetSpeedMultiplier(round);
            var damageMultiplier = EndlessModeRules.GetDamageMultiplier(round);
            return new EnemyDefinition(
                source.Id,
                source.DisplayNameKey,
                source.MaxHealth * healthMultiplier,
                source.MovementSpeed * speedMultiplier,
                Mathf.Max(1, Mathf.CeilToInt(source.ContactDamage * damageMultiplier)),
                source.RewardScrap,
                source.ThreatValue,
                source.EnemyTag,
                source.WeakToDamageType,
                source.PrefabId);
        }

        private int TrySpendBuildCost(TowerDefinition tower)
        {
            if (tower == null || _missionState == null)
            {
                return -1;
            }

            if (IsFirstWarriorFree(tower))
            {
                ReportStatus("Your first Warrior joins the defense for free.");
                return 0;
            }

            var spent = _missionState.TrySpendScrap(tower.BuildCost);
            UpdateStatus();
            return spent ? tower.BuildCost : -1;
        }

        private void HandleCurrencyChanged(int scrap, int coins)
        {
            CurrencyChanged?.Invoke(scrap, coins);
            RefreshTowerBuildBar();
        }

        private void HandleTowerBuilt()
        {
            RefreshTowerBuildBar();
            UpdateSelectedTowerLabel();
        }

        private void AddScrap(int amount)
        {
            if (_missionState == null)
            {
                return;
            }

            _missionState.AddScrap(amount);
            ReportStatus($"Tower sold. Scrap +{amount}.");
            UpdateStatus();
        }

        private bool TrySpendCurrency(TowerUpgradeCurrency currency, int amount)
        {
            bool spent;

            switch (currency)
            {
                case TowerUpgradeCurrency.Scrap:
                    spent = _missionState.TrySpendScrap(amount);
                    break;
                case TowerUpgradeCurrency.Coins:
                    spent = _missionState.TrySpendCoins(amount);
                    break;
                case TowerUpgradeCurrency.Gems:
                    spent = _missionState.TrySpendGems(amount);
                    break;
                default:
                    spent = false;
                    break;
            }

            UpdateStatus();
            return spent;
        }

        private TowerDefinition FindDefaultBuildableTower()
        {
            return _catalog.Towers.Values.FirstOrDefault(tower => tower.BuildCost >= 0 && IsTowerUnlocked(tower))
                ?? _catalog.Towers.Values.FirstOrDefault(tower => tower.BuildCost >= 0)
                ?? _catalog.Towers.Values.First();
        }

        private GameObject GetTowerPrefab(TowerDefinition towerDefinition)
        {
            return _towerPrefab;
        }

        private void ApplySelectedTowerToSockets()
        {
            if (_selectedTowerDefinition == null || _towerSockets == null)
            {
                return;
            }

            foreach (var socket in _towerSockets)
            {
                if (socket != null)
                {
                    socket.SetBuildOption(_selectedTowerDefinition, GetTowerPrefab(_selectedTowerDefinition));
                }
            }
        }

        private void SelectTowerOption(int optionIndex)
        {
            if (_towerOptionIds == null || optionIndex < 0 || optionIndex >= _towerOptionIds.Length)
            {
                ReportStatus("Tower option is not configured.");
                return;
            }

            SelectTowerById(_towerOptionIds[optionIndex]);
        }

        private void RefreshTowerBuildBar()
        {
            if (_catalog == null || _towerOptionButtons == null || _towerOptionIds == null)
            {
                return;
            }

            for (var i = 0; i < _towerOptionButtons.Length; i++)
            {
                var optionId = i < _towerOptionIds.Length ? _towerOptionIds[i] : string.Empty;
                TowerDefinition tower = null;
                var hasTower = !string.IsNullOrWhiteSpace(optionId) && _catalog.Towers.TryGetValue(new StableId(optionId), out tower);
                var visible = hasTower && tower.BuildCost >= 0 && IsTowerUnlocked(tower);
                var canPlace = visible && CanPlaceTower(tower);

                if (_towerOptionButtons[i] != null)
                {
                    _towerOptionButtons[i].gameObject.SetActive(visible);
                    _towerOptionButtons[i].interactable = canPlace;
                }

                if (_towerOptionLabels != null && i < _towerOptionLabels.Length && _towerOptionLabels[i] != null)
                {
                    _towerOptionLabels[i].text = visible
                        ? IsFirstWarriorFree(tower)
                            ? $"{tower.DisplayNameKey}\nFIRST FREE"
                            : canPlace
                                ? $"{tower.DisplayNameKey}\n{tower.BuildCost} Scrap"
                                : $"{tower.DisplayNameKey}\nNEED {tower.BuildCost} Scrap"
                        : "Locked";
                }
            }

            RefreshTowerSelection();
        }

        private void RefreshTowerSelection()
        {
            if (_towerOptionFrames == null || _towerOptionIds == null)
            {
                return;
            }

            var selectedId = _selectedTowerDefinition == null ? string.Empty : _selectedTowerDefinition.Id.Value;
            for (var i = 0; i < _towerOptionFrames.Length; i++)
            {
                if (_towerOptionFrames[i] != null)
                {
                    TowerDefinition tower = null;
                    var hasTower = i < _towerOptionIds.Length &&
                                   _catalog != null &&
                                   _catalog.Towers.TryGetValue(new StableId(_towerOptionIds[i]), out tower);
                    var isSelected = i < _towerOptionIds.Length && _towerOptionIds[i] == selectedId;
                    _towerOptionFrames[i].color = hasTower && !CanPlaceTower(tower)
                        ? _unavailableTowerFrameColor
                        : isSelected ? _selectedTowerFrameColor : _unselectedTowerFrameColor;
                }
            }
        }

        private bool CanPlaceTower(TowerDefinition tower)
        {
            if (tower == null || _missionState == null)
            {
                return false;
            }

            return EmergencyBuildRules.CanAfford(
                _emergencyHeroId,
                tower.Id.Value,
                _missionState.Scrap,
                tower.BuildCost,
                HasDeployedHero());
        }

        private bool IsFirstWarriorFree(TowerDefinition tower)
        {
            return tower != null && EmergencyBuildRules.IsFreeFallbackPlacement(
                _emergencyHeroId,
                tower.Id.Value,
                HasDeployedHero());
        }

        private bool HasDeployedHero()
        {
            return _towerSockets != null && _towerSockets.Any(socket => socket != null && socket.IsOccupied);
        }

        private static bool IsTowerUnlocked(TowerDefinition tower)
        {
            if (tower == null)
            {
                return false;
            }

            return tower.UnlockCostCoins <= 0 ||
                (AppRuntimeSession.AuthenticatedPlayer != null &&
                 AppRuntimeSession.AuthenticatedPlayer.Progression.HasUnlockedTower(tower.Id.Value)) ||
                (AppRuntimeSession.SelectedSlot != null && AppRuntimeSession.SelectedSlot.HasPurchasedTower(tower.Id.Value));
        }

        private void UpdateSelectedTowerLabel()
        {
            if (_selectedTowerText == null || _selectedTowerDefinition == null)
            {
                return;
            }

            var costText = IsFirstWarriorFree(_selectedTowerDefinition)
                ? "FIRST FREE"
                : $"{_selectedTowerDefinition.BuildCost} Scrap";
            _selectedTowerText.text =
                $"Selected: {_selectedTowerDefinition.DisplayNameKey} | " +
                $"{_selectedTowerDefinition.DamageType} | Cost {costText}";
        }

        private void SpawnEnemy(EnemyDefinition enemyDefinition)
        {
            if (_enemyPrefab == null || _pathPoints == null || _pathPoints.Length == 0)
            {
                ReportStatus("Missing enemy prefab or path points.");
                return;
            }

            var enemyObject = Instantiate(_enemyPrefab, _pathPoints[0].position, Quaternion.identity);
            var enemy = enemyObject.GetComponent<EnemyAgent>();

            if (enemy == null)
            {
                Debug.LogError("Enemy prefab needs an EnemyAgent component.");
                Destroy(enemyObject);
                return;
            }

            enemy.Destroyed += HandleEnemyDestroyed;
            enemy.ReachedBase += HandleEnemyReachedBase;
            enemy.Initialize(enemyDefinition, _pathPoints);
            _enemyRegistry.Register(enemy);
            _missionState.RegisterEnemySpawned();
            ReportStatus($"Spawned {enemyDefinition.Id}.");
            UpdateStatus();
        }

        public void CaptureBattlefieldLayout(BattlefieldLayoutAsset layout)
        {
            if (layout == null)
            {
                return;
            }

            var pathPositions = _pathPoints == null
                ? Array.Empty<Vector2>()
                : _pathPoints.Where(point => point != null).Select(point => (Vector2)point.position).ToArray();
            var socketPositions = _towerSockets == null
                ? Array.Empty<Vector2>()
                : _towerSockets.Where(socket => socket != null).Select(socket => (Vector2)socket.transform.position).ToArray();
            layout.Configure(layout.Id, pathPositions, socketPositions);
        }

        public void ApplyBattlefieldLayoutPreview(BattlefieldLayoutAsset layout)
        {
            if (layout == null)
            {
                return;
            }

            var pathCount = Mathf.Min(layout.PathPointCount, _pathPoints == null ? 0 : _pathPoints.Length);
            for (var i = 0; i < pathCount; i++)
            {
                if (_pathPoints[i] != null)
                {
                    _pathPoints[i].position = layout.GetPathPoint(i);
                }
            }

            if (_towerSockets == null)
            {
                return;
            }

            for (var i = 0; i < _towerSockets.Length; i++)
            {
                var socket = _towerSockets[i];
                if (socket == null)
                {
                    continue;
                }

                var isUsed = i < layout.BuildSocketCount;
                socket.gameObject.SetActive(isUsed);
                if (isUsed)
                {
                    socket.transform.position = layout.GetBuildSocketPosition(i);
                }
            }
        }


        private void HandleEnemyDestroyed(EnemyAgent enemy)
        {
            enemy.Destroyed -= HandleEnemyDestroyed;
            enemy.ReachedBase -= HandleEnemyReachedBase;
            _enemyRegistry.Unregister(enemy);
            _missionState.AddScrap(enemy.Definition.RewardScrap);
            _enemiesDefeatedThisMission++;
            if (_endlessMode)
            {
                _endlessScore += EndlessModeRules.GetEnemyScore(enemy.Definition.ThreatValue, EndlessRound);
                EndlessScoreChanged?.Invoke(_endlessScore, EndlessRound);
            }
            EnemyDefeated?.Invoke(enemy.Definition.Id.Value);
            _missionState.RegisterEnemyResolved();
            UpdateStatus();
            TryReportVictory();
        }

        private void HandleEnemyReachedBase(EnemyAgent enemy)
        {
            enemy.Destroyed -= HandleEnemyDestroyed;
            enemy.ReachedBase -= HandleEnemyReachedBase;
            _enemyRegistry.Unregister(enemy);
            _missionState.DamageBase(enemy.Definition.ContactDamage);
            _missionState.RegisterEnemyResolved();
            UpdateStatus();
            if (_endlessMode && _missionState.Outcome == MissionOutcome.Defeat)
            {
                _wavesStarted = false;
                EndlessDefeated?.Invoke();
            }
            TryReportDefeat();
            TryReportVictory();
        }

        private void UpdateStatus()
        {
            if (_missionState == null)
            {
                return;
            }

            ReportStatus(
                $"Base {_missionState.BaseHealth} | Scrap {_missionState.Scrap} | " +
                $"Coins {_missionState.Coins} | Gems {_missionState.Gems} | " +
                $"Wave {_missionState.CompletedWaves}/{_missionState.TotalWaves} | " +
                $"Enemies {_missionState.ActiveEnemies} | {_missionState.Outcome}");
            SetHudValue(_baseHealthValueText, _missionState.BaseHealth.ToString());
            SetHudValue(_scrapValueText, _missionState.Scrap.ToString());
            SetHudValue(_coinsValueText, _missionState.Coins.ToString());
            SetHudValue(
                _waveValueText,
                _endlessMode ? $"R{EndlessRound} W{EndlessWaveInRound}/{EndlessModeRules.WavesPerRound}" : $"{_missionState.CompletedWaves}/{_missionState.TotalWaves}");
            SetHudValue(_enemiesValueText, _missionState.ActiveEnemies.ToString());
            TryReportVictory();
        }

        private void TryReportDefeat()
        {
            if (_defeatReported || _missionState == null || _missionState.Outcome != MissionOutcome.Defeat)
            {
                return;
            }

            _defeatReported = true;
            Defeat?.Invoke();
        }

        private static void SetHudValue(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private bool HasPlacedTower()
        {
            return _towerSockets != null && _towerSockets.Any(socket => socket != null && socket.IsOccupied);
        }

        private static void ClearLiveEnemies()
        {
            var enemies = FindObjectsByType<EnemyAgent>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
        }

        private void TryReportVictory()
        {
            if (_victoryReported || _missionState == null || _missionState.Outcome != MissionOutcome.Victory)
            {
                return;
            }

            _victoryReported = true;
            Victory?.Invoke(_levelDefinition);
        }

        private void ReportStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }

            if (_messageText != null && !message.Contains("|"))
            {
                _messageText.text = message;
            }

            Debug.Log(message);
        }
    }
}
