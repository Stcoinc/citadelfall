using System.Collections.Generic;
using System.Linq;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class LevelSelectionSceneController : MonoBehaviour
    {
        [SerializeField] private GameObject _slotPanel;
        [SerializeField] private GameObject _levelPanel;
        [SerializeField] private GameObject _towerShopPanel;
        [SerializeField] private GameObject _previousMapButton;
        [SerializeField] private GameObject _nextMapButton;
        [SerializeField] private GameObject[] _mapBackgrounds;
        [SerializeField] private TMP_Text _slotHeaderText;
        [SerializeField] private TMP_Text _mapTitleText;
        [SerializeField] private TMP_Text[] _slotLabels;
        [SerializeField] private TMP_Text[] _levelLabels;
        [SerializeField] private TMP_Text[] _towerShopLabels;
        [SerializeField] private TMP_Text _towerShopMessageText;
        [SerializeField] private UnitShopView _unitShopView;
        [SerializeField] private string[] _purchasableTowerIds =
        {
            "hero_paladin",
            "hero_druid",
            "hero_sorcerer"
        };

        private readonly LocalSaveSlotRepository _saveSlots = new LocalSaveSlotRepository();
        private readonly List<LevelDefinition> _levels = new List<LevelDefinition>();
        private const int LevelsPerMap = 5;
        private int _currentMapIndex;
        private bool _accountSaveRunning;
        private bool _accountSavePending;

        public int UnitShopCoins => AppRuntimeSession.SelectedSlot?.Coins ??
                                    AppRuntimeSession.AuthenticatedPlayer?.Progression?.Coins ?? 0;

        public int UnitShopGems => AppRuntimeSession.SelectedSlot?.Gems ?? 0;

        private void Start()
        {
            if (!AppRuntimeSession.HasContent)
            {
                SceneManager.LoadScene(AppSceneNames.Intro);
                return;
            }

            MigrateLegacyShopConfiguration();

            _levels.Clear();
            _levels.AddRange(AppRuntimeSession.Catalog.Levels.Values.OrderBy(level => level.Id.Value));
            _currentMapIndex = 0;
            if (LevelSelectionNavigationState.ShouldOpenLevelMap(
                    AppRuntimeSession.IsArenaMode || AppRuntimeSession.IsEndlessMode,
                    AppRuntimeSession.SelectedSlot))
            {
                _currentMapIndex = LevelSelectionNavigationState.ResolveMapIndex(
                    _levels.Count,
                    AppRuntimeSession.SelectedSlot.HighestUnlockedLevelIndex,
                    LevelsPerMap);
                ShowLevelMap();
                return;
            }

            ShowSlots();
        }

        private void MigrateLegacyShopConfiguration()
        {
            if (_purchasableTowerIds == null || _purchasableTowerIds.Length != 3 ||
                _purchasableTowerIds.Any(id => string.IsNullOrWhiteSpace(id) || id.StartsWith("tower_")))
            {
                _purchasableTowerIds = new[] { "hero_paladin", "hero_druid", "hero_sorcerer" };
            }

            if (_levelPanel != null)
            {
                foreach (var label in _levelPanel.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (string.Equals(label.text?.Trim(), "TOWER SHOP", System.StringComparison.OrdinalIgnoreCase))
                    {
                        label.text = "UNIT SHOP";
                    }
                }
            }
        }

        public void BackToMain()
        {
            SceneManager.LoadScene(AppSceneNames.MainMenu);
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

        public void PreviousMap()
        {
            if (_currentMapIndex <= 0)
            {
                return;
            }

            _currentMapIndex--;
            RefreshLevelLabels();
            RefreshMapNavigation();
        }

        public void NextMap()
        {
            var maximumMapIndex = Mathf.Max(0, (_levels.Count - 1) / LevelsPerMap);
            if (_currentMapIndex >= maximumMapIndex)
            {
                return;
            }

            _currentMapIndex++;
            RefreshLevelLabels();
            RefreshMapNavigation();
        }

        public void ShowSlots()
        {
            SetActive(_slotPanel, true);
            SetActive(_levelPanel, false);
            SetActive(_towerShopPanel, false);
            SetText(
                _slotHeaderText,
                AppRuntimeSession.IsArenaMode
                    ? "Choose Arena Profile"
                    : AppRuntimeSession.IsEndlessMode
                        ? "Choose Endless Profile"
                    : AppRuntimeSession.SlotModeContinue ? "Continue" : "New Game");
            RefreshSlotLabels();
        }

        public void ShowTurretShop()
        {
            RefreshShopLabels();
            SetShopMessage(string.Empty);
            SetActive(_towerShopPanel, true);
        }

        public void HideTurretShop()
        {
            SetActive(_towerShopPanel, false);
        }

        public void BuyShopTower0()
        {
            BuyShopTower(0);
        }

        public void BuyShopTower1()
        {
            BuyShopTower(1);
        }

        public void BuyShopTower2()
        {
            BuyShopTower(2);
        }

        public void BuyShopUnit(int shopIndex)
        {
            BuyShopTower(shopIndex);
        }

        public IReadOnlyList<UnitShopCardData> GetUnitShopCards()
        {
            var cards = new List<UnitShopCardData>();
            if (_purchasableTowerIds == null || AppRuntimeSession.Catalog == null)
            {
                return cards;
            }

            var slot = AppRuntimeSession.SelectedSlot;
            foreach (var unitId in _purchasableTowerIds.Take(3))
            {
                if (!AppRuntimeSession.Catalog.Towers.TryGetValue(new StableId(unitId), out var unit))
                {
                    continue;
                }

                var owned = IsUnitOwned(unit, slot);
                cards.Add(new UnitShopCardData(
                    unit.Id.Value,
                    unit.DisplayNameKey,
                    unit.DescriptionKey,
                    $"{unit.DamageType.ToUpperInvariant()}  •  DMG {unit.Damage:0}  •  RNG {unit.Range:0.#}",
                    unit.UnlockCostCoins,
                    owned,
                    slot != null && slot.Coins >= unit.UnlockCostCoins));
            }

            return cards;
        }

        private void SelectSlot(int slotIndex)
        {
            var slot = _saveSlots.Load(slotIndex);
            if (slot.IsEmpty && AppRuntimeSession.SlotModeContinue)
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

            AppRuntimeSession.SetSelectedSlot(slot);
            if (AppRuntimeSession.IsArenaMode)
            {
                SceneManager.LoadScene(AppSceneNames.CitadelFallArena);
                return;
            }

            if (AppRuntimeSession.IsEndlessMode)
            {
                SceneManager.LoadScene(AppSceneNames.Endless);
                return;
            }

            _currentMapIndex = 0;
            ShowLevelMap();
        }

        private void ShowLevelMap()
        {
            RefreshLevelLabels();
            RefreshMapNavigation();
            RefreshShopLabels();
            SetActive(_slotPanel, false);
            SetActive(_levelPanel, true);
            SetActive(_towerShopPanel, false);
        }

        private void DeleteSlot(int slotIndex)
        {
            _saveSlots.Delete(slotIndex);
            RefreshSlotLabels();
        }

        private void SelectLevel(int levelIndex)
        {
            var slot = AppRuntimeSession.SelectedSlot;
            var absoluteLevelIndex = _currentMapIndex * LevelsPerMap + levelIndex;
            if (slot == null || levelIndex < 0 || levelIndex >= LevelsPerMap || absoluteLevelIndex >= _levels.Count)
            {
                return;
            }

            if (absoluteLevelIndex > slot.HighestUnlockedLevelIndex)
            {
                return;
            }

            var level = _levels[absoluteLevelIndex];
            slot.LastPlayedLevelId = level.Id.Value;
            _saveSlots.Save(slot);
            AppRuntimeSession.SetSelectedSlot(slot);
            AppRuntimeSession.SetSelectedLevel(level.Id.Value);
            SceneManager.LoadScene(AppSceneNames.Gameplay);
        }

        private void RefreshSlotLabels()
        {
            for (var i = 0; i < _slotLabels.Length; i++)
            {
                var slot = _saveSlots.Load(i);
                var text = slot.IsEmpty
                    ? $"Slot {i + 1}\nEmpty"
                    : AppRuntimeSession.IsEndlessMode
                        ? $"Slot {i + 1}\nHero {slot.CommanderName}\nBest Score {slot.BestEndlessScore:N0}\nHighest Round {slot.HighestEndlessRound}"
                        : $"Slot {i + 1}\nHero {slot.CommanderName}\nUnlocked {slot.HighestUnlockedLevelIndex + 1}/5\nCoins {slot.Coins} Gems {slot.Gems}";
                SetText(_slotLabels[i], text);
            }
        }

        private void RefreshLevelLabels()
        {
            for (var i = 0; i < _levelLabels.Length; i++)
            {
                var absoluteLevelIndex = _currentMapIndex * LevelsPerMap + i;
                if (absoluteLevelIndex >= _levels.Count)
                {
                    SetText(_levelLabels[i], "Locked");
                    continue;
                }

                var level = _levels[absoluteLevelIndex];
                var unlocked = AppRuntimeSession.SelectedSlot != null && absoluteLevelIndex <= AppRuntimeSession.SelectedSlot.HighestUnlockedLevelIndex;
                SetText(_levelLabels[i], unlocked ? level.DisplayNameKey : "Locked");
            }

            var firstLevel = _currentMapIndex * LevelsPerMap + 1;
            var lastLevel = Mathf.Min((_currentMapIndex + 1) * LevelsPerMap, _levels.Count);
            SetText(_mapTitleText, $"MAP {_currentMapIndex + 1}  |  LEVELS {firstLevel}-{lastLevel}");
        }

        private void RefreshMapNavigation()
        {
            var maximumMapIndex = Mathf.Max(0, (_levels.Count - 1) / LevelsPerMap);
            SetActive(_previousMapButton, _currentMapIndex > 0);
            SetActive(_nextMapButton, _currentMapIndex < maximumMapIndex);
            if (_mapBackgrounds != null)
            {
                for (var i = 0; i < _mapBackgrounds.Length; i++)
                {
                    SetActive(_mapBackgrounds[i], i == _currentMapIndex);
                }
            }
        }

        private void BuyShopTower(int shopIndex)
        {
            var slot = AppRuntimeSession.SelectedSlot;
            if (slot == null || _purchasableTowerIds == null || shopIndex < 0 || shopIndex >= _purchasableTowerIds.Length)
            {
                SetShopMessage("Select a save slot first.");
                return;
            }

            var towerId = _purchasableTowerIds[shopIndex];
            if (!AppRuntimeSession.Catalog.Towers.TryGetValue(new StableId(towerId), out var tower))
            {
                SetShopMessage("Unit is missing from content.");
                return;
            }

            if (IsUnitOwned(tower, slot))
            {
                SetShopMessage("That unit is already recruited.");
                return;
            }

            if (slot.Coins < tower.UnlockCostCoins)
            {
                SetShopMessage($"You need {tower.UnlockCostCoins:N0} Coins to recruit {tower.DisplayNameKey}.");
                return;
            }

            slot.Coins -= tower.UnlockCostCoins;
            slot.CurrenciesInitialized = true;
            slot.AddPurchasedTower(towerId);
            AppRuntimeSession.UnlockTower(towerId);
            _saveSlots.Save(slot);
            AppRuntimeSession.SetPlayerCurrencies(slot.Scrap, slot.Coins);
            AppRuntimeSession.SetSelectedSlot(slot);
            QueueAccountProgressionSave();
            SetShopMessage($"{tower.DisplayNameKey} has joined your citadel!");
            RefreshShopLabels();
        }

        private void RefreshShopLabels()
        {
            if (_towerShopLabels == null || _purchasableTowerIds == null || AppRuntimeSession.Catalog == null)
            {
                return;
            }

            var slot = AppRuntimeSession.SelectedSlot;
            for (var i = 0; i < _towerShopLabels.Length; i++)
            {
                if (i >= _purchasableTowerIds.Length)
                {
                    SetText(_towerShopLabels[i], "Unavailable");
                    continue;
                }

                var towerId = _purchasableTowerIds[i];
                if (!AppRuntimeSession.Catalog.Towers.TryGetValue(new StableId(towerId), out var tower))
                {
                    SetText(_towerShopLabels[i], "Missing tower");
                    continue;
                }

                var owned = IsUnitOwned(tower, slot);
                SetText(
                    _towerShopLabels[i],
                    owned
                        ? $"{tower.DisplayNameKey}\nOwned"
                        : $"{tower.DisplayNameKey}\n{tower.UnlockCostCoins} Coins\n{tower.DamageType} / {tower.BuildCost} Scrap");
            }

            _unitShopView?.Refresh(UnitShopCoins, UnitShopGems, GetUnitShopCards());
        }

        private void SetShopMessage(string message)
        {
            SetText(_towerShopMessageText, message);
            _unitShopView?.SetMessage(message);
        }

        private static bool IsUnitOwned(TowerDefinition unit, SaveSlotData slot)
        {
            return unit != null &&
                   (unit.UnlockCostCoins <= 0 ||
                    (AppRuntimeSession.AuthenticatedPlayer?.Progression?.HasUnlockedTower(unit.Id.Value) ?? false) ||
                    (slot != null && slot.HasPurchasedTower(unit.Id.Value)));
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
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
                    await AppRuntimeSession.SavePlayerProgressionAsync(System.Threading.CancellationToken.None);
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

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }
    }
}
