using System.Text;
using ClubGamerZone.TowerDefense.Application.Gameplay;
using ClubGamerZone.TowerDefense.Domain.Content;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class TowerDetailsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _powerText;
        [SerializeField] private TMP_Text _attackText;
        [SerializeField] private TMP_Text _rangeText;
        [SerializeField] private TMP_Text _attackSpeedText;
        [SerializeField] private TMP_Text _targetingText;
        [SerializeField] private TMP_Text _preferredEnemyText;
        [SerializeField] private TMP_Text _splashText;
        [SerializeField] private TMP_Text _upgradeText;
        [SerializeField] private TMP_Text _mergeText;
        [SerializeField] private TMP_Text _actionMessageText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _mergeButton;
        [SerializeField] private Button _sellButton;
        [SerializeField] private TMP_Text _upgradeButtonText;
        [SerializeField] private TMP_Text _mergeButtonText;
        [SerializeField] private TMP_Text _sellButtonText;

        private readonly TowerStatsPresenter _statsPresenter = new TowerStatsPresenter();
        private TowerPlacementSocket _selectedSocket;
        private EnemyAgent _selectedEnemy;

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (_selectedEnemy == null)
            {
                return;
            }

            if (!_selectedEnemy.gameObject.activeInHierarchy)
            {
                Hide();
                return;
            }

            SetText(_attackSpeedText, $"Health {_selectedEnemy.Health:0.#}/{_selectedEnemy.Definition.MaxHealth:0.#}");
        }

        public void Show(TowerPlacementSocket selectedSocket, TowerDefinition definition, TowerInstanceState state, string actionMessage = null)
        {
            var stats = _statsPresenter.BuildStats(definition, state);

            if (stats == null)
            {
                Hide();
                return;
            }

            if (_selectedSocket != null && _selectedSocket != selectedSocket)
            {
                _selectedSocket.HideRangeIndicator();
            }

            _selectedSocket = selectedSocket;
            _selectedEnemy = null;
            _selectedSocket?.ShowRangeIndicator(stats.Range);
            SetPanelActive(true);
            SetText(_nameText, stats.DisplayNameKey);
            SetText(_levelText, $"Level {stats.Level}/{stats.MaxLevel}");
            SetText(_powerText, $"Power {stats.PowerRating}");
            SetText(_attackText, $"Attack {stats.Damage:0.#}");
            SetText(_rangeText, $"Range {stats.Range:0.#}");
            SetText(_attackSpeedText, $"Interval {stats.AttackIntervalSeconds:0.##}s");
            SetText(_targetingText, $"Targeting {stats.TargetingMode}");
            SetText(_preferredEnemyText, $"Type {stats.DamageType} / Weak target {stats.PreferredEnemyTag}");
            SetText(_splashText, stats.HasSplash ? "Splash: Yes" : "Splash: No");
            SetText(_upgradeText, BuildUpgradeText(definition, state));
            SetText(_mergeText, BuildMergeText(definition, state));
            SetText(_actionMessageText, string.IsNullOrWhiteSpace(actionMessage) ? string.Empty : actionMessage);
            SetButtonInteractable(_upgradeButton, HasUpgrade(definition, state));
            SetButtonInteractable(_mergeButton, HasMerge(definition, state));
            SetButtonInteractable(_sellButton, selectedSocket != null && selectedSocket.SellRefundAmount >= 0);
            SetText(_upgradeButtonText, BuildUpgradeButtonText(selectedSocket, definition, state));
            SetText(_mergeButtonText, BuildMergeButtonText(selectedSocket, definition, state));
            SetText(_sellButtonText, BuildSellButtonText(selectedSocket));
        }

        public void ShowBuildPreview(TowerDefinition definition, int buildCost, string actionMessage = null)
        {
            var state = definition == null ? null : new TowerInstanceState(definition.Id, 1);
            var stats = _statsPresenter.BuildStats(definition, state);

            if (stats == null)
            {
                Hide();
                return;
            }

            _selectedSocket?.HideRangeIndicator();
            _selectedSocket = null;
            _selectedEnemy = null;
            SetPanelActive(true);
            SetText(_nameText, stats.DisplayNameKey);
            SetText(_levelText, $"Preview Level {stats.Level}/{stats.MaxLevel}");
            SetText(_powerText, $"Power {stats.PowerRating}");
            SetText(_attackText, $"Attack {stats.Damage:0.#}");
            SetText(_rangeText, $"Range {stats.Range:0.#}");
            SetText(_attackSpeedText, $"Interval {stats.AttackIntervalSeconds:0.##}s");
            SetText(_targetingText, $"Targeting {stats.TargetingMode}");
            SetText(_preferredEnemyText, $"Type {stats.DamageType} / Weak target {stats.PreferredEnemyTag}");
            SetText(_splashText, stats.HasSplash ? "Splash: Yes" : "Splash: No");
            SetText(_upgradeText, BuildUpgradeText(definition, state));
            SetText(_mergeText, BuildMergeText(definition, state));
            SetText(_actionMessageText, string.IsNullOrWhiteSpace(actionMessage)
                ? $"Choose a socket to place this unit. Cost {FormatBuildCost(buildCost)}."
                : actionMessage);
            SetButtonInteractable(_upgradeButton, false);
            SetButtonInteractable(_mergeButton, false);
            SetButtonInteractable(_sellButton, false);
            SetText(_upgradeButtonText, "UPGRADE\nPLACE FIRST");
            SetText(_mergeButtonText, "MERGE\nPLACE FIRST");
            SetText(_sellButtonText, "SELL\nPLACE FIRST");
        }

        public void ShowEnemy(EnemyAgent enemy)
        {
            if (enemy == null || enemy.Definition == null)
            {
                Hide();
                return;
            }

            _selectedSocket?.HideRangeIndicator();
            _selectedSocket = null;
            _selectedEnemy = enemy;
            SetPanelActive(true);
            SetText(_nameText, enemy.Definition.DisplayNameKey);
            SetText(_levelText, "Enemy status");
            SetText(_powerText, $"Threat {enemy.Definition.ThreatValue}");
            SetText(_attackText, $"Contact damage {enemy.Definition.ContactDamage}");
            SetText(_rangeText, $"Speed {enemy.Definition.MovementSpeed:0.##}");
            SetText(_attackSpeedText, $"Health {enemy.Health:0.#}/{enemy.Definition.MaxHealth:0.#}");
            SetText(_targetingText, $"Tag {enemy.Definition.EnemyTag}");
            SetText(_preferredEnemyText, $"Weak to {enemy.Definition.WeakToDamageType}");
            SetText(_splashText, $"Reward: {enemy.Definition.RewardScrap} Scrap");
            SetText(_upgradeText, $"Max health: {enemy.Definition.MaxHealth:0.#}");
            SetText(_mergeText, $"Prefab: {enemy.Definition.PrefabId}");
            SetText(_actionMessageText, string.Empty);
            SetButtonInteractable(_upgradeButton, false);
            SetButtonInteractable(_mergeButton, false);
            SetButtonInteractable(_sellButton, false);
            SetText(_upgradeButtonText, "UPGRADE\nN/A");
            SetText(_mergeButtonText, "MERGE\nN/A");
            SetText(_sellButtonText, "SELL\nN/A");
        }

        public void Hide()
        {
            _selectedSocket?.HideRangeIndicator();
            _selectedSocket = null;
            _selectedEnemy = null;
            SetPanelActive(false);
        }

        public void OnClosePressed()
        {
            if (_selectedSocket != null)
            {
                _selectedSocket.CancelMergeSelection();
            }

            Hide();
        }

        public void OnUpgradePressed()
        {
            if (_selectedSocket != null)
            {
                _selectedSocket.TryUpgrade();
            }
        }

        public void OnMergePressed()
        {
            if (_selectedSocket != null)
            {
                _selectedSocket.BeginMergeSelection();
            }
        }

        public void OnSellPressed()
        {
            if (_selectedSocket != null)
            {
                _selectedSocket.TrySell();
            }
        }

        private void SetPanelActive(bool isActive)
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(isActive);
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetButtonInteractable(Button button, bool isInteractable)
        {
            if (button != null)
            {
                button.interactable = isInteractable;
            }
        }

        private static bool HasUpgrade(TowerDefinition definition, TowerInstanceState state)
        {
            foreach (var upgrade in definition.Upgrades)
            {
                if (upgrade.FromLevel == state.Level)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasMerge(TowerDefinition definition, TowerInstanceState state)
        {
            foreach (var merge in definition.Merges)
            {
                if (merge.RequiredLevel == state.Level)
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildUpgradeText(TowerDefinition definition, TowerInstanceState state)
        {
            var builder = new StringBuilder();

            foreach (var upgrade in definition.Upgrades)
            {
                if (upgrade.FromLevel == state.Level)
                {
                    builder.Append(upgrade.Currency);
                    builder.Append(" ");
                    builder.Append(upgrade.Cost);
                    builder.Append(": Lv ");
                    builder.Append(upgrade.ToLevel);
                    builder.Append(" / Power ");
                    builder.Append(upgrade.PowerRating);
                    builder.AppendLine();
                }
            }

            if (builder.Length > 0)
            {
                return builder.ToString().TrimEnd();
            }

            return state.Level >= definition.MaxLevel
                ? "Maximum level reached"
                : "Upgrade path not configured";
        }

        private static string BuildMergeText(TowerDefinition definition, TowerInstanceState state)
        {
            foreach (var merge in definition.Merges)
            {
                if (merge.RequiredLevel == state.Level)
                {
                    return merge.ResultTowerId.Equals(definition.Id)
                        ? $"Combine two {definition.GetDisplayName(state.Level)} heroes. Cost {merge.Cost} {merge.Currency}. Result: {definition.GetDisplayName(state.Level + 1)}."
                        : $"Requires another same tower at Lv {state.Level}. Cost {merge.Cost} {merge.Currency}. Result: {merge.ResultTowerId}.";
                }
            }

            return state.Level >= definition.MaxLevel
                ? "Maximum merge level reached"
                : "Merge path not configured";
        }

        private static string BuildUpgradeButtonText(TowerPlacementSocket socket, TowerDefinition definition, TowerInstanceState state)
        {
            var upgrade = socket == null ? FindUpgrade(definition, state) : socket.NextUpgrade;

            if (upgrade != null)
            {
                return $"UPGRADE\n{upgrade.Currency} {upgrade.Cost}";
            }

            return state.Level >= definition.MaxLevel
                ? "UPGRADE\nMAX"
                : "UPGRADE\nNOT SET";
        }

        private static string BuildMergeButtonText(TowerPlacementSocket socket, TowerDefinition definition, TowerInstanceState state)
        {
            var merge = socket == null ? FindMerge(definition, state) : socket.NextMerge;

            if (merge != null)
            {
                return $"MERGE\n{merge.Currency} {merge.Cost}";
            }

            return state.Level >= definition.MaxLevel
                ? "MERGE\nMAX"
                : "MERGE\nNOT SET";
        }

        private static string BuildSellButtonText(TowerPlacementSocket socket)
        {
            return socket == null
                ? "SELL\nScrap 0"
                : $"SELL\nScrap {socket.SellRefundAmount}";
        }

        private static string FormatBuildCost(int buildCost)
        {
            return buildCost <= 0 ? "Free" : $"{buildCost} Scrap";
        }

        private static TowerUpgradeDefinition FindUpgrade(TowerDefinition definition, TowerInstanceState state)
        {
            foreach (var upgrade in definition.Upgrades)
            {
                if (upgrade.FromLevel == state.Level)
                {
                    return upgrade;
                }
            }

            return null;
        }

        private static TowerMergeDefinition FindMerge(TowerDefinition definition, TowerInstanceState state)
        {
            foreach (var merge in definition.Merges)
            {
                if (merge.RequiredLevel == state.Level)
                {
                    return merge;
                }
            }

            return null;
        }
    }
}
