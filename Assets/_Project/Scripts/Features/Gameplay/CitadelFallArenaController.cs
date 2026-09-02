using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClubGamerZone.TowerDefense.Application.Configuration;
using ClubGamerZone.TowerDefense.Application.Gameplay;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;
using ClubGamerZone.TowerDefense.Infrastructure.Json;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class CitadelFallArenaController : MonoBehaviour
    {
        private static readonly string[] DefaultDeckIds =
        {
            "hero_mage",
            "hero_warrior",
            "hero_paladin",
            "hero_archer",
            "hero_druid"
        };

        private static readonly string[] HeroChoiceIds =
        {
            "hero_mage",
            "hero_warrior",
            "hero_paladin",
            "hero_archer",
            "hero_druid",
            "hero_sorcerer"
        };

        private static readonly string[] ArenaEnemyIds =
        {
            "enemy_swarm",
            "enemy_regenerator",
            "enemy_scout",
            "enemy_armored"
        };

        private static readonly string[] ArenaBossIds =
        {
            "enemy_boss",
            "enemy_boss_warden"
        };

        [Header("Content")]
        [SerializeField] private TextAsset _starterContentJson;
        [SerializeField] private string _arenaRulesId = "arena_standard";
        [SerializeField] private uint _matchSeed = 20260804;

        [Header("Battlefield")]
        [SerializeField] private ArenaHeroSocket[] _sockets;
        [SerializeField] private Transform[] _pathPoints;
        [SerializeField] private GameObject _heroPrefab;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private Camera _worldCamera;

        [Header("HUD")]
        [SerializeField] private TMP_Text _manaText;
        [SerializeField] private TMP_Text _summonCostText;
        [SerializeField] private TMP_Text _livesText;
        [SerializeField] private TMP_Text _waveText;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private TMP_Text[] _deckLabels;
        [SerializeField] private Image[] _deckImages;
        [SerializeField] private Sprite[] _heroSprites;
        [SerializeField] private GameObject _deckBuilderPanel;
        [SerializeField] private Image[] _deckChoiceFrames;
        [SerializeField] private TMP_Text[] _deckChoiceLabels;
        [SerializeField] private Button _summonButton;
        [SerializeField] private Button _startButton;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TMP_Text _resultTitleText;
        [SerializeField] private TMP_Text _resultSummaryText;
        [SerializeField] private float _dragThresholdPixels = 18f;
        [SerializeField] private MergeInteractionFeedback _mergeFeedback;

        private readonly EnemyRegistry _enemyRegistry = new EnemyRegistry();
        private ContentCatalog _catalog;
        private ArenaRulesDefinition _rules;
        private ArenaDeck _deck;
        private ArenaSummonSequence _heroSequence;
        private ArenaSummonSequence _socketSequence;
        private ArenaSummonSequence _enemySequence;
        private ArenaHeroSocket _selectedSocket;
        private int _mana;
        private int _completedSummons;
        private int _strongholdLives;
        private int _wave;
        private float _remainingSeconds;
        private float _nextBossAt;
        private bool _matchRunning;
        private int _lastDisplayedSecond = -1;
        private int _defeatedEnemies;
        private int _leakedEnemies;
        private int _completedMerges;
        private bool _tutorialActive;
        private int _tutorialStep;
        private readonly List<string> _selectedDeckIds = new List<string>();
        private ArenaHeroSocket _pressedSocket;
        private Vector2 _pressScreenPosition;
        private bool _isDraggingHero;

        private void Awake()
        {
            if (_mergeFeedback == null)
            {
                _mergeFeedback = GetComponent<MergeInteractionFeedback>();
            }

            if (_mergeFeedback == null)
            {
                Debug.LogWarning($"{nameof(CitadelFallArenaController)} has no authored {nameof(MergeInteractionFeedback)} on {name}; merge feedback is disabled.", this);
            }
        }

        private void Start()
        {
            if (!TryLoadContent())
            {
                SetMessage("Arena content could not be loaded.");
                return;
            }

            InitializeArena();
        }

        private void Update()
        {
            HandleWorldPointer();

            if (!_matchRunning)
            {
                return;
            }

            _remainingSeconds = Mathf.Max(0f, _remainingSeconds - Time.deltaTime);
            if (_remainingSeconds <= 0f)
            {
                FinishMatch(true, "ARENA DEFENDED");
            }

            RefreshTimer();
        }

        public void StartMatchPressed()
        {
            if (_matchRunning || _rules == null || _selectedDeckIds.Count != _rules.DeckSize)
            {
                SetMessage($"Choose exactly {_rules?.DeckSize ?? 5} heroes before starting.");
                return;
            }

            _matchRunning = true;
            _remainingSeconds = _rules.MatchDurationSeconds;
            _nextBossAt = _rules.MatchDurationSeconds - _rules.BossIntervalSeconds;
            _wave = 1;
            if (_startButton != null)
            {
                _startButton.gameObject.SetActive(false);
            }

            if (_deckBuilderPanel != null)
            {
                _deckBuilderPanel.SetActive(false);
            }

            SetMessage("The enemy approaches. Summon your heroes!");
            if (_tutorialActive)
            {
                _tutorialStep = 1;
                SetMessage("STEP 1 • SPEND MANA TO SUMMON YOUR FIRST HERO");
            }

            StartCoroutine(SpawnPressureLoop());
            RefreshHud();
        }

        public void SummonPressed()
        {
            if (!_matchRunning)
            {
                SetMessage("Start the Arena match first.");
                return;
            }

            var emptySockets = _sockets.Where(socket => socket != null && !socket.IsOccupied).ToArray();
            if (emptySockets.Length == 0)
            {
                SetMessage("The formation is full. Merge matching heroes to make room.");
                return;
            }

            var summonCost = ArenaRules.GetSummonCost(_rules.BaseSummonCost, _rules.SummonCostStep, _completedSummons);
            if (_mana < summonCost)
            {
                SetMessage($"Need {summonCost - _mana} more mana.");
                return;
            }

            var heroId = _deck.HeroIds[_heroSequence.NextDeckIndex(_deck.HeroIds.Count)];
            var socketIndex = _socketSequence.NextDeckIndex(emptySockets.Length);
            if (!emptySockets[socketIndex].Summon(_catalog.Towers[heroId]))
            {
                SetMessage("Summon failed. Try another socket.");
                return;
            }

            _mana -= summonCost;
            _completedSummons++;
            SetMessage($"Summoned {_catalog.Towers[heroId].DisplayNameKey}.");
            AdvanceSummonTutorial();
            RefreshHud();
        }

        public void ReturnToMenuPressed()
        {
            SceneManager.LoadScene(AppSceneNames.MainMenu);
        }

        public void RestartPressed()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void SurrenderPressed()
        {
            if (_matchRunning)
            {
                FinishMatch(false, "YOU WITHDREW FROM THE ARENA");
            }
        }

        public void ToggleDeckHero(string heroId)
        {
            if (_matchRunning || !HeroChoiceIds.Contains(heroId) ||
                !_catalog.Towers.ContainsKey(new StableId(heroId)))
            {
                return;
            }

            if (!IsHeroUnlocked(heroId))
            {
                SetMessage($"{_catalog.Towers[new StableId(heroId)].DisplayNameKey} is locked. Recruit this hero in the Unit Shop.");
                return;
            }

            if (_selectedDeckIds.Contains(heroId))
            {
                _selectedDeckIds.Remove(heroId);
            }
            else if (_selectedDeckIds.Count < _rules.DeckSize)
            {
                _selectedDeckIds.Add(heroId);
            }
            else
            {
                SetMessage("The deck is full. Remove one hero before adding another.");
                return;
            }

            RebuildSelectedDeck();
            SaveSelectedDeck();
            RefreshDeckUi();
        }

        private bool TryLoadContent()
        {
            if (AppRuntimeSession.Catalog != null)
            {
                _catalog = AppRuntimeSession.Catalog;
            }
            else if (_starterContentJson != null)
            {
                var dto = new UnityContentJsonParser().Parse(_starterContentJson.text);
                var build = new StarterContentCatalogBuilder(new StarterContentValidator(new ContentValidationLimits())).Build(dto);
                if (!build.IsSuccess)
                {
                    Debug.LogError(string.Join("\n", build.Validation.Issues));
                    return false;
                }

                _catalog = build.Catalog;
            }

            return _catalog != null &&
                   _catalog.ArenaRules.TryGetValue(new StableId(_arenaRulesId), out _rules);
        }

        private void InitializeArena()
        {
            var deckIds = ResolveDeckIds();
            _selectedDeckIds.Clear();
            _selectedDeckIds.AddRange(deckIds);
            RebuildSelectedDeck();
            _heroSequence = new ArenaSummonSequence(ArenaSeedStreams.Hero(_matchSeed));
            _socketSequence = new ArenaSummonSequence(ArenaSeedStreams.Socket(_matchSeed));
            _enemySequence = new ArenaSummonSequence(ArenaSeedStreams.Enemy(_matchSeed));
            _mana = _rules.StartingMana;
            _strongholdLives = _rules.StrongholdLives;
            _remainingSeconds = _rules.MatchDurationSeconds;
            _completedSummons = 0;
            _defeatedEnemies = 0;
            _leakedEnemies = 0;
            _completedMerges = 0;
            _tutorialActive = AppRuntimeSession.SelectedSlot == null ||
                              !AppRuntimeSession.SelectedSlot.ArenaTutorialCompleted;
            _tutorialStep = 0;
            _wave = 0;
            SetActive(_resultPanel, false);

            foreach (var socket in _sockets)
            {
                socket?.Initialize(_heroPrefab, _enemyRegistry);
            }

            RefreshDeckUi();
            SetMessage("Five heroes. Fifteen sockets. Hold the citadel.");
            RefreshHud();
        }

        private string[] ResolveDeckIds()
        {
            var saved = AppRuntimeSession.SelectedSlot?.SelectedArenaHeroIds;
            if (saved != null && saved.Length == _rules.DeckSize &&
                saved.Distinct(StringComparer.Ordinal).Count() == saved.Length &&
                saved.All(id => _catalog.Towers.ContainsKey(new StableId(id)) && IsHeroUnlocked(id)))
            {
                return saved;
            }

            return DefaultDeckIds
                .Where(id => _catalog.Towers.ContainsKey(new StableId(id)) && IsHeroUnlocked(id))
                .Take(_rules.DeckSize)
                .ToArray();
        }

        private void RebuildSelectedDeck()
        {
            _deck = _selectedDeckIds.Count == _rules.DeckSize
                ? new ArenaDeck(_selectedDeckIds.Select(id => new StableId(id)), _rules.DeckSize)
                : null;
        }

        private void SaveSelectedDeck()
        {
            if (_deck == null || AppRuntimeSession.SelectedSlot == null)
            {
                return;
            }

            AppRuntimeSession.SelectedSlot.SetArenaDeck(_selectedDeckIds.ToArray(), _rules.DeckSize);
            new LocalSaveSlotRepository().Save(AppRuntimeSession.SelectedSlot);
        }

        private void RefreshDeckUi()
        {
            for (var i = 0; i < _deckLabels.Length; i++)
            {
                var hasHero = i < _selectedDeckIds.Count;
                var heroId = hasHero ? _selectedDeckIds[i] : string.Empty;
                if (_deckLabels[i] != null)
                {
                    _deckLabels[i].text = hasHero
                        ? _catalog.Towers[new StableId(heroId)].DisplayNameKey.ToUpperInvariant()
                        : "EMPTY";
                }

                if (_deckImages != null && i < _deckImages.Length && _deckImages[i] != null)
                {
                    _deckImages[i].sprite = hasHero ? FindHeroSprite(heroId) : null;
                    _deckImages[i].enabled = hasHero;
                }
            }

            for (var i = 0; i < _deckChoiceFrames.Length; i++)
            {
                if (_deckChoiceFrames[i] == null || i >= HeroChoiceIds.Length)
                {
                    continue;
                }

                var selected = _selectedDeckIds.Contains(HeroChoiceIds[i]);
                var unlocked = IsHeroUnlocked(HeroChoiceIds[i]);
                _deckChoiceFrames[i].color = !unlocked
                    ? new Color32(42, 44, 49, 235)
                    : selected
                        ? new Color32(202, 153, 43, 255)
                        : new Color32(28, 76, 107, 255);
                if (_deckChoiceLabels != null && i < _deckChoiceLabels.Length && _deckChoiceLabels[i] != null)
                {
                    var heroName = _catalog.Towers[new StableId(HeroChoiceIds[i])].DisplayNameKey.ToUpperInvariant();
                    _deckChoiceLabels[i].text = unlocked ? heroName : $"{heroName}\nLOCKED";
                }
            }

            if (_startButton != null)
            {
                _startButton.interactable = _selectedDeckIds.Count == _rules.DeckSize;
            }
        }

        private Sprite FindHeroSprite(string heroId)
        {
            var index = Array.IndexOf(HeroChoiceIds, heroId);
            return index >= 0 && _heroSprites != null && index < _heroSprites.Length
                ? _heroSprites[index]
                : null;
        }

        private bool IsHeroUnlocked(string heroId)
        {
            if (_catalog == null || !_catalog.Towers.TryGetValue(new StableId(heroId), out var hero))
            {
                return false;
            }

            return hero.UnlockCostCoins <= 0 ||
                   (AppRuntimeSession.AuthenticatedPlayer?.Progression?.HasUnlockedTower(heroId) ?? false) ||
                   (AppRuntimeSession.SelectedSlot?.HasPurchasedTower(heroId) ?? false);
        }

        private IEnumerator SpawnPressureLoop()
        {
            while (_matchRunning)
            {
                var elapsed = _rules.MatchDurationSeconds - _remainingSeconds;
                _wave = Mathf.Max(1, Mathf.FloorToInt(elapsed / 15f) + 1);
                var isBossTime = _remainingSeconds <= _nextBossAt;
                var definition = SelectEnemy(isBossTime);
                SpawnEnemy(CreateScaledEnemy(definition, _wave, isBossTime));

                if (isBossTime)
                {
                    _nextBossAt -= _rules.BossIntervalSeconds;
                    SetMessage($"BOSS WAVE {_wave}");
                }

                yield return new WaitForSeconds(ArenaEnemyBalance.GetSpawnInterval(
                    _wave,
                    _rules.BaseSpawnIntervalSeconds,
                    _rules.SpawnIntervalReductionPerWave,
                    _rules.MinimumSpawnIntervalSeconds));
            }
        }

        private EnemyDefinition SelectEnemy(bool boss)
        {
            if (boss)
            {
                return SelectConfiguredEnemy(ArenaBossIds);
            }

            var unlockedEnemyCount = ArenaEnemyBalance.GetUnlockedEnemyCount(
                _wave,
                ArenaEnemyIds.Length,
                _rules.WolfIntroductionWave,
                _rules.GoblinIntroductionWave,
                _rules.OrcIntroductionWave);
            return SelectConfiguredEnemy(ArenaEnemyIds, unlockedEnemyCount);
        }

        private EnemyDefinition SelectConfiguredEnemy(string[] ids, int maximumCount = int.MaxValue)
        {
            var available = ids
                .Take(Mathf.Min(ids.Length, maximumCount))
                .Select(id => new StableId(id))
                .Where(id => _catalog.Enemies.ContainsKey(id))
                .Select(id => _catalog.Enemies[id])
                .ToArray();

            if (available.Length == 0)
            {
                throw new InvalidOperationException("Arena content has no configured fantasy enemies for this wave.");
            }

            return available[_enemySequence.NextDeckIndex(available.Length)];
        }

        private EnemyDefinition CreateScaledEnemy(EnemyDefinition source, int wave, bool boss)
        {
            var bossHealthMultiplier = boss ? _rules.BossHealthMultiplier : 1f;
            return new EnemyDefinition(
                source.Id,
                source.DisplayNameKey,
                ArenaEnemyBalance.GetScaledHealth(
                    source.MaxHealth,
                    wave,
                    _rules.EnemyHealthMultiplier,
                    _rules.EnemyHealthPerWave,
                    bossHealthMultiplier),
                ArenaEnemyBalance.GetScaledSpeed(source.MovementSpeed, _rules.EnemySpeedMultiplier),
                source.ContactDamage,
                source.RewardScrap,
                source.ThreatValue,
                source.EnemyTag,
                source.WeakToDamageType,
                source.PrefabId);
        }

        private void SpawnEnemy(EnemyDefinition definition)
        {
            if (_enemyPrefab == null || _pathPoints == null || _pathPoints.Length < 2)
            {
                return;
            }

            var enemyObject = Instantiate(_enemyPrefab, _pathPoints[0].position, Quaternion.identity);
            var enemy = enemyObject.GetComponent<EnemyAgent>();
            if (enemy == null)
            {
                Destroy(enemyObject);
                return;
            }

            enemy.Destroyed += HandleEnemyDestroyed;
            enemy.ReachedBase += HandleEnemyReachedBase;
            enemy.Initialize(definition, _pathPoints);
            _enemyRegistry.Register(enemy);
        }

        private void HandleEnemyDestroyed(EnemyAgent enemy)
        {
            DetachEnemy(enemy);
            _mana += Mathf.Max(2, enemy.Definition.RewardScrap);
            _defeatedEnemies++;
            AppRuntimeSession.RegisterEnemyDefeat(enemy.Definition.Id.Value);
            RefreshHud();
        }

        private void HandleEnemyReachedBase(EnemyAgent enemy)
        {
            DetachEnemy(enemy);
            _leakedEnemies++;
            _strongholdLives = Mathf.Max(0, _strongholdLives - 1);
            if (_strongholdLives == 0)
            {
                FinishMatch(false, "THE STRONGHOLD HAS FALLEN");
            }

            RefreshHud();
        }

        private void DetachEnemy(EnemyAgent enemy)
        {
            enemy.Destroyed -= HandleEnemyDestroyed;
            enemy.ReachedBase -= HandleEnemyReachedBase;
            _enemyRegistry.Unregister(enemy);
        }

        private void HandleWorldPointer()
        {
            if (TryGetPointerPress(out var screenPosition, out var pointerId) && !IsPointerOverUi(pointerId))
            {
                var socket = FindArenaSocket(screenPosition);
                if (socket != null && socket.IsOccupied)
                {
                    _pressedSocket = socket;
                    _pressScreenPosition = screenPosition;
                    _isDraggingHero = false;
                }
                else if (socket != null)
                {
                    SetMessage("Empty socket. Use SUMMON to call a hero.");
                }
            }

            if (_pressedSocket != null && TryGetPointerPosition(out screenPosition))
            {
                if (!_isDraggingHero && Vector2.Distance(_pressScreenPosition, screenPosition) >= _dragThresholdPixels)
                {
                    _isDraggingHero = true;
                    _pressedSocket.BeginDragPreview();
                    SelectSocket(null);
                }

                if (_isDraggingHero)
                {
                    var cameraToUse = _worldCamera == null ? Camera.main : _worldCamera;
                    if (cameraToUse != null)
                    {
                        _pressedSocket.MoveDragPreview(cameraToUse.ScreenToWorldPoint(screenPosition));
                    }
                }
            }

            if (_pressedSocket != null && TryGetPointerRelease(out screenPosition))
            {
                CompleteHeroPointerInteraction(screenPosition);
            }
        }

        private void CompleteHeroPointerInteraction(Vector2 screenPosition)
        {
            var source = _pressedSocket;
            _pressedSocket = null;
            if (source == null)
            {
                return;
            }

            if (!_isDraggingHero)
            {
                HandleSocketPressed(source);
                return;
            }

            var target = FindArenaSocket(screenPosition);
            if (target != null && target != source && target.MergeFrom(source, _rules.MaximumMergeRank))
            {
                _completedMerges++;
                _mergeFeedback?.PlayMerge(target.ActiveHeroTransform);
                var mergedName = target.Definition.GetDisplayName(target.MergeRank);
                if (_tutorialActive)
                {
                    _tutorialStep = 4;
                    SetMessage("STEP 4 • STRONGER HEROES EARN MANA. STOP ENEMIES BEFORE THE GATE!");
                }
                else
                {
                    SetMessage($"Merge complete: {mergedName}!");
                }
                return;
            }

            source.ReturnDragPreview();
            _mergeFeedback?.PlayInvalid();
            SetMessage("Merge requires the same hero and the same rank.");
        }

        private ArenaHeroSocket FindArenaSocket(Vector2 screenPosition)
        {
            var cameraToUse = _worldCamera == null ? Camera.main : _worldCamera;
            if (cameraToUse == null)
            {
                return null;
            }

            var worldPosition = cameraToUse.ScreenToWorldPoint(screenPosition);
            foreach (var hit in Physics2D.OverlapPointAll(worldPosition))
            {
                var socket = hit.GetComponent<ArenaHeroSocket>() ?? hit.GetComponentInParent<ArenaHeroSocket>();
                if (socket != null)
                {
                    return socket;
                }
            }

            return null;
        }

        private static bool TryGetPointerPress(out Vector2 screenPosition, out int pointerId)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                pointerId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                pointerId = -1;
                return true;
            }

            screenPosition = default;
            pointerId = -1;
            return false;
        }

        private static bool TryGetPointerPosition(out Vector2 screenPosition)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        private static bool TryGetPointerRelease(out Vector2 screenPosition)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        private static bool IsPointerOverUi(int pointerId)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return pointerId < 0
                ? EventSystem.current.IsPointerOverGameObject()
                : EventSystem.current.IsPointerOverGameObject(pointerId);
        }

        private void HandleSocketPressed(ArenaHeroSocket socket)
        {
            if (!socket.IsOccupied)
            {
                SetMessage("Empty socket. Use SUMMON to call a hero.");
                return;
            }

            if (_selectedSocket == null)
            {
                SelectSocket(socket);
                return;
            }

            if (_selectedSocket == socket)
            {
                SelectSocket(null);
                return;
            }

            if (socket.MergeFrom(_selectedSocket, _rules.MaximumMergeRank))
            {
                _completedMerges++;
                _mergeFeedback?.PlayMerge(socket.ActiveHeroTransform);
                var mergedName = socket.Definition.GetDisplayName(socket.MergeRank);
                SelectSocket(null);
                SetMessage($"Merge complete: {mergedName}!");
                return;
            }

            _mergeFeedback?.PlayInvalid();
            SetMessage("Those heroes do not match. Select or drag the same hero and rank.");
            SelectSocket(socket);
        }

        private void SelectSocket(ArenaHeroSocket socket)
        {
            _selectedSocket?.SetSelected(false);
            _selectedSocket = socket;
            _selectedSocket?.SetSelected(true);
            if (_selectedSocket != null)
            {
                SetMessage($"{_selectedSocket.Definition.GetDisplayName(_selectedSocket.MergeRank)} selected. Drag it—or tap another matching hero—to merge.");
            }
        }

        private void FinishMatch(bool victory, string message)
        {
            if (!_matchRunning)
            {
                return;
            }

            _matchRunning = false;
            StopAllCoroutines();
            if (_summonButton != null)
            {
                _summonButton.interactable = false;
            }

            SetMessage(message);
            var rewardCoins = ApplyMatchRewards(victory);
            SetText(_resultTitleText, victory ? "THE CITADEL STANDS" : "THE STRONGHOLD FELL");
            SetText(
                _resultSummaryText,
                $"WAVE {_wave}   •   {_defeatedEnemies} DEFEATED\n" +
                $"{_completedSummons} SUMMONS   •   {_completedMerges} MERGES   •   {_leakedEnemies} LEAKED\n" +
                $"+{rewardCoins} COINS");
            SetActive(_resultPanel, true);
            CompleteTutorial();
            SaveArenaProgression();
            Debug.Log(victory ? "Citadel Fall Arena victory." : "Citadel Fall Arena defeat.");
        }

        private int ApplyMatchRewards(bool victory)
        {
            var rewardCoins = victory ? _rules.VictoryRewardCoins : _rules.DefeatRewardCoins;
            var slot = AppRuntimeSession.SelectedSlot;
            if (slot == null)
            {
                return rewardCoins;
            }

            slot.Coins = Mathf.Max(0, slot.Coins + rewardCoins);
            slot.CurrenciesInitialized = true;
            slot.ArenaMatchesPlayed++;
            slot.ArenaVictories += victory ? 1 : 0;
            slot.ArenaEnemiesDefeated += _defeatedEnemies;
            new LocalSaveSlotRepository().Save(slot);
            AppRuntimeSession.SetPlayerCurrencies(slot.Scrap, slot.Coins);
            return rewardCoins;
        }

        private async void SaveArenaProgression()
        {
            try
            {
                await AppRuntimeSession.SavePlayerProgressionAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Arena progression will remain local until sync succeeds: {exception.Message}");
            }
        }

        private void AdvanceSummonTutorial()
        {
            if (!_tutorialActive)
            {
                return;
            }

            if (_completedSummons == 1)
            {
                _tutorialStep = 2;
                SetMessage("STEP 2 • SUMMON MORE HEROES. EACH SUMMON COSTS MORE MANA");
            }

            if (_tutorialStep < 3 && HasMergePair())
            {
                _tutorialStep = 3;
                SetMessage("STEP 3 • TAP TWO MATCHING HEROES OF THE SAME RANK TO MERGE");
            }
        }

        private bool HasMergePair()
        {
            return _sockets
                .Where(socket => socket != null && socket.IsOccupied)
                .GroupBy(socket => $"{socket.Definition.Id.Value}:{socket.MergeRank}")
                .Any(group => group.Count() >= 2);
        }

        private void CompleteTutorial()
        {
            if (!_tutorialActive)
            {
                return;
            }

            _tutorialActive = false;
            var slot = AppRuntimeSession.SelectedSlot;
            if (slot == null)
            {
                return;
            }

            slot.ArenaTutorialCompleted = true;
            new LocalSaveSlotRepository().Save(slot);
        }

        private void RefreshHud()
        {
            SetText(_manaText, _mana.ToString());
            SetText(_summonCostText, ArenaRules.GetSummonCost(_rules.BaseSummonCost, _rules.SummonCostStep, _completedSummons).ToString());
            SetText(_livesText, _strongholdLives.ToString());
            SetText(_waveText, _wave.ToString());
            RefreshTimer();
            if (_summonButton != null)
            {
                _summonButton.interactable = _matchRunning && _sockets.Any(socket => socket != null && !socket.IsOccupied);
            }
        }

        private void SetMessage(string message)
        {
            SetText(_messageText, message);
        }

        private void RefreshTimer()
        {
            var wholeSeconds = Mathf.CeilToInt(_remainingSeconds);
            if (wholeSeconds == _lastDisplayedSecond)
            {
                return;
            }

            _lastDisplayedSecond = wholeSeconds;
            if (_timerText != null)
            {
                _timerText.SetText("{0:00}:{1:00}", wholeSeconds / 60, wholeSeconds % 60);
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
