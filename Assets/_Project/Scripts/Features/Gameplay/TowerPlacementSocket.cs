using System;
using System.Collections;
using System.Collections.Generic;
using ClubGamerZone.TowerDefense.Application.Gameplay;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class TowerPlacementSocket : MonoBehaviour
    {
        private const float HeroPlacementScale = 0.78f;
        private TowerDefinition _towerDefinition;
        private TowerInstanceState _towerState;
        private TowerDefinition _selectedBuildDefinition;
        private GameObject _selectedBuildPrefab;
        private GameObject _activeTower;
        private TowerWeapon _activeWeapon;
        private EnemyRegistry _enemyRegistry;
        private IReadOnlyDictionary<StableId, TowerDefinition> _towerDefinitions;
        private TowerProgressionService _progressionService;
        private TowerMergeSelectionCoordinator _mergeSelectionCoordinator;
        private Func<TowerDefinition, int> _spendBuildCost;
        private Func<TowerUpgradeCurrency, int, bool> _spendCurrency;
        private Action<int> _addScrap;
        private Action _towerBuilt;
        private float _sellRefundPercent = 0.4f;
        [SerializeField] private TowerDetailsPanel _detailsPanel;
        [SerializeField] private UnitRangeIndicator _rangeIndicator;
        private bool _isOccupied;
        private int _paidBuildCost;
        private SpriteRenderer _spriteRenderer;
        private Color _availableColor;
        private readonly Color _occupiedColor = new Color(1f, 1f, 1f, 0.42f);
        private Color _dragOriginalColor;
        private int _dragOriginalSortingOrder;
        private Vector3 _dragHomeLocalPosition;
        private Coroutine _returnRoutine;

        public bool IsOccupied => _isOccupied;

        public TowerDefinition TowerDefinition => _towerDefinition;

        public TowerInstanceState TowerState => _towerState;

        public TowerUpgradeDefinition NextUpgrade => FindFirstAvailableUpgrade();

        public TowerMergeDefinition NextMerge => FindAvailableMerge();

        public int SellRefundAmount => CalculateSellRefund();

        public Transform ActiveTowerTransform => _activeTower == null ? null : _activeTower.transform;

        public void ShowRangeIndicator(float worldRange)
        {
            _rangeIndicator?.Show(worldRange);
        }

        public void HideRangeIndicator()
        {
            _rangeIndicator?.Hide();
        }

        public void BeginDragPreview()
        {
            if (_activeTower == null)
            {
                return;
            }

            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }

            SetAttackEnabled(false);

            _dragHomeLocalPosition = _activeTower.transform.localPosition;

            var renderer = _activeTower.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                _dragOriginalColor = renderer.color;
                _dragOriginalSortingOrder = renderer.sortingOrder;
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.75f);
                renderer.sortingOrder += 100;
            }
        }

        public void MoveDragPreview(Vector3 worldPosition)
        {
            if (_activeTower != null)
            {
                _activeTower.transform.position = new Vector3(worldPosition.x, worldPosition.y, _activeTower.transform.position.z);
            }
        }

        public void EndDragPreview()
        {
            if (_activeTower == null)
            {
                return;
            }

            _activeTower.transform.localPosition = _dragHomeLocalPosition;
            RestoreDragRenderer();
            SetAttackEnabled(true);
        }

        public void ReturnDragPreview(float duration = 0.2f)
        {
            if (_activeTower == null)
            {
                return;
            }

            RestoreDragRenderer();
            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
            }

            _returnRoutine = StartCoroutine(AnimateDragReturn(duration));
        }

        private IEnumerator AnimateDragReturn(float duration)
        {
            var tower = _activeTower;
            if (tower == null)
            {
                yield break;
            }

            var start = tower.transform.localPosition;
            var elapsed = 0f;
            while (tower != null && elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                var eased = 1f - Mathf.Pow(1f - progress, 3f);
                tower.transform.localPosition = Vector3.LerpUnclamped(start, _dragHomeLocalPosition, eased);
                yield return null;
            }

            if (tower != null)
            {
                tower.transform.localPosition = _dragHomeLocalPosition;
                SetAttackEnabled(true);
            }

            _returnRoutine = null;
        }

        private void RestoreDragRenderer()
        {
            if (_activeTower == null)
            {
                return;
            }

            var renderer = _activeTower.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = _dragOriginalColor;
                renderer.sortingOrder = _dragOriginalSortingOrder;
            }
        }

        private void SetAttackEnabled(bool isEnabled)
        {
            if (_activeWeapon != null)
            {
                _activeWeapon.enabled = isEnabled;
            }
        }

        public void ResetSocket()
        {
            ClearTower();
            _selectedBuildDefinition = null;
            _selectedBuildPrefab = null;
            _towerDefinition = null;
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                _availableColor = _spriteRenderer.color;
            }
        }

        public void Initialize(
            TowerDefinition towerDefinition,
            GameObject towerPrefab,
            EnemyRegistry enemyRegistry,
            Func<int, bool> spendScrap)
        {
            Initialize(
                towerDefinition,
                towerPrefab,
                enemyRegistry,
                null,
                null,
                null,
                definition => spendScrap(definition.BuildCost) ? definition.BuildCost : -1,
                (currency, amount) => currency == TowerUpgradeCurrency.Scrap && spendScrap(amount),
                null,
                null,
                0.4f);
        }

        public void Initialize(
            TowerDefinition towerDefinition,
            GameObject towerPrefab,
            EnemyRegistry enemyRegistry,
            IReadOnlyDictionary<StableId, TowerDefinition> towerDefinitions,
            TowerProgressionService progressionService,
            TowerMergeSelectionCoordinator mergeSelectionCoordinator,
            Func<TowerDefinition, int> spendBuildCost,
            Func<TowerUpgradeCurrency, int, bool> spendCurrency,
            Action<int> addScrap = null,
            Action towerBuilt = null,
            float sellRefundPercent = 0.4f)
        {
            SetBuildOption(
                towerDefinition ?? throw new ArgumentNullException(nameof(towerDefinition)),
                towerPrefab);
            _enemyRegistry = enemyRegistry ?? throw new ArgumentNullException(nameof(enemyRegistry));
            _towerDefinitions = towerDefinitions;
            _progressionService = progressionService;
            _mergeSelectionCoordinator = mergeSelectionCoordinator;
            _spendBuildCost = spendBuildCost ?? throw new ArgumentNullException(nameof(spendBuildCost));
            _spendCurrency = spendCurrency ?? throw new ArgumentNullException(nameof(spendCurrency));
            _addScrap = addScrap;
            _towerBuilt = towerBuilt;
            _sellRefundPercent = Mathf.Clamp01(sellRefundPercent);
        }

        public void SetBuildOption(TowerDefinition towerDefinition, GameObject towerPrefab)
        {
            _selectedBuildDefinition = towerDefinition ?? throw new ArgumentNullException(nameof(towerDefinition));
            _selectedBuildPrefab = towerPrefab;
        }

        public void HandleSelection()
        {
            if (_mergeSelectionCoordinator != null && _mergeSelectionCoordinator.IsSelecting)
            {
                if (!_isOccupied)
                {
                    _mergeSelectionCoordinator.ShowSourceMessage("That socket is empty. Choose another tower with the same type and level.");
                    return;
                }

                _mergeSelectionCoordinator.TryResolve(this);
                return;
            }

            if (!_isOccupied)
            {
                TryBuildTower();
                return;
            }

            ShowDetails();
        }

        public bool TryBuildTower()
        {
            if (_isOccupied || _selectedBuildDefinition == null || _selectedBuildPrefab == null || _spendBuildCost == null)
            {
                ShowDetails();
                return false;
            }

            var paidBuildCost = _spendBuildCost(_selectedBuildDefinition);
            if (paidBuildCost < 0)
            {
                return false;
            }

            _towerDefinition = _selectedBuildDefinition;
            _towerState = new TowerInstanceState(_towerDefinition.Id, 1);
            _paidBuildCost = paidBuildCost;
            CreateTowerObject();

            _isOccupied = true;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _occupiedColor;
            }

            Debug.Log($"Built {_towerDefinition.GetDisplayName(_towerState.Level)} at {name}.");
            _towerBuilt?.Invoke();
            // A placement press is a build action, not an inspection action. Keeping the
            // details panel closed also prevents the newly occupied socket from feeling
            // as though the original press was processed twice.
            _detailsPanel?.Hide();
            return true;
        }

        public bool TrySell()
        {
            if (!_isOccupied || _towerDefinition == null || _towerState == null)
            {
                ShowDetails("Sell failed: no tower is selected.");
                return false;
            }

            var refund = CalculateSellRefund();

            if (_mergeSelectionCoordinator != null && _mergeSelectionCoordinator.IsSelecting)
            {
                _mergeSelectionCoordinator.Cancel();
            }

            ClearTower();
            _addScrap?.Invoke(refund);

            if (_detailsPanel != null)
            {
                _detailsPanel.Hide();
            }

            _towerBuilt?.Invoke();
            Debug.Log($"Sold tower at {name} for {refund} Scrap.");
            return true;
        }

        public bool TryUpgrade()
        {
            if (!_isOccupied || _progressionService == null || _towerDefinition == null || _towerState == null)
            {
                return false;
            }

            var upgrade = FindFirstAvailableUpgrade();

            if (upgrade == null)
            {
                ShowDetails("No upgrade available.");
                return false;
            }

            if (!_spendCurrency(upgrade.Currency, upgrade.Cost))
            {
                ShowDetails($"Need {upgrade.Cost} {upgrade.Currency}.");
                return false;
            }

            _towerState = _towerState.ApplyUpgrade(upgrade);
            ApplyTowerStats();
            Debug.Log($"Upgraded {_towerDefinition.Id} to level {_towerState.Level} at {name}.");
            ShowDetails("Upgrade complete.");
            return true;
        }

        public void BeginMergeSelection()
        {
            if (!_isOccupied || _towerDefinition == null || _towerState == null || _mergeSelectionCoordinator == null)
            {
                ShowDetails("Merge is not ready.");
                return;
            }

            if (!HasMergeAvailable())
            {
                ShowDetails("No merge available.");
                return;
            }

            _mergeSelectionCoordinator.Begin(this);
            var merge = FindAvailableMerge();
            var price = merge == null ? string.Empty : $" Cost: {merge.Cost} {merge.Currency}.";
            ShowDetails($"Merge started. Tap a second {_towerDefinition.GetDisplayName(_towerState.Level)}.{price}");
        }

        public void CancelMergeSelection()
        {
            if (_mergeSelectionCoordinator == null || !_mergeSelectionCoordinator.IsSelecting)
            {
                return;
            }

            _mergeSelectionCoordinator.Cancel();
        }

        public bool TryMergeWith(TowerPlacementSocket other, bool showDetails = true)
        {
            if (!_isOccupied || _progressionService == null)
            {
                return FailMerge("Merge failed.", showDetails);
            }

            if (other == null || !other._isOccupied)
            {
                return FailMerge("Merge failed: choose a second tower, not an empty socket.", showDetails);
            }

            if (other._towerDefinition == null || other._towerState == null)
            {
                return FailMerge("Merge failed: the second tower is not ready.", showDetails);
            }

            if (!_towerDefinition.Id.Equals(other._towerDefinition.Id))
            {
                return FailMerge("Merge failed: both towers must be the same type.", showDetails);
            }

            if (_towerState.Level != other._towerState.Level)
            {
                return FailMerge("Merge failed: both towers must be the same level.", showDetails);
            }

            var merge = _progressionService.FindMerge(_towerDefinition, _towerState, other._towerState);

            if (merge == null)
            {
                return FailMerge("Merge failed: this tower level has no merge recipe.", showDetails);
            }

            if (_towerDefinitions == null || !_towerDefinitions.TryGetValue(merge.ResultTowerId, out var mergedDefinition))
            {
                return FailMerge("Merge failed: merged tower is missing from content.", showDetails);
            }

            if (!_spendCurrency(merge.Currency, merge.Cost))
            {
                return FailMerge($"Merge failed: need {merge.Cost} {merge.Currency}.", showDetails);
            }

            var combinedPaidBuildCost = _paidBuildCost + other._paidBuildCost;
            other.ClearTower();
            _towerDefinition = mergedDefinition;
            _towerState = _towerState.ApplyMerge(merge);
            _paidBuildCost = combinedPaidBuildCost;
            ApplyBuildVisual();
            ApplyTowerStats();
            ApplyMergedVisual();
            var mergedName = _towerDefinition.GetDisplayName(_towerState.Level);
            Debug.Log($"Merged towers into {mergedName} at {name}.");
            if (showDetails)
            {
                ShowDetails($"Merge complete: {mergedName}.");
            }

            return true;
        }

        private bool FailMerge(string message, bool showDetails)
        {
            if (showDetails)
            {
                ShowDetails(message);
            }

            return false;
        }

        private void CreateTowerObject()
        {
            _activeTower = Instantiate(_selectedBuildPrefab, transform.position, Quaternion.identity, transform);
            _activeWeapon = _activeTower.GetComponent<TowerWeapon>();
            ApplyBuildVisual();
            ApplyTowerStats();
        }

        private void ApplyTowerStats()
        {
            if (_activeWeapon != null)
            {
                _activeWeapon.Initialize(_towerDefinition, _towerState, _enemyRegistry);
            }
        }

        private void ApplyMergedVisual()
        {
            if (_activeTower == null)
            {
                return;
            }

            _activeTower.transform.localScale *= 1.18f;
            var renderer = _activeTower.GetComponent<SpriteRenderer>();

            if (renderer != null)
            {
                renderer.color = new Color(1f, 0.82f, 0.25f, 1f);
            }

            _activeTower.GetComponent<TowerAttackAnimator>()?.CaptureCurrentVisualState();
        }

        private void ClearTower()
        {
            HideRangeIndicator();

            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }

            if (_activeTower != null)
            {
                Destroy(_activeTower);
            }

            _activeTower = null;
            _activeWeapon = null;
            _towerState = null;
            _towerDefinition = null;
            _isOccupied = false;
            _paidBuildCost = 0;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _availableColor;
            }
        }

        private int CalculateSellRefund()
        {
            return Mathf.RoundToInt(_paidBuildCost * _sellRefundPercent);
        }

        private TowerUpgradeDefinition FindFirstAvailableUpgrade()
        {
            if (_progressionService == null)
            {
                return null;
            }

            var currencies = new[]
            {
                TowerUpgradeCurrency.Scrap,
                TowerUpgradeCurrency.Coins,
                TowerUpgradeCurrency.Gems
            };

            foreach (var currency in currencies)
            {
                var upgrade = _progressionService.FindUpgrade(_towerDefinition, _towerState, currency);

                if (upgrade != null)
                {
                    return upgrade;
                }
            }

            return null;
        }

        private bool HasMergeAvailable()
        {
            if (_towerDefinition == null || _towerState == null)
            {
                return false;
            }

            foreach (var merge in _towerDefinition.Merges)
            {
                if (merge.RequiredLevel == _towerState.Level)
                {
                    return true;
                }
            }

            return false;
        }

        private TowerMergeDefinition FindAvailableMerge()
        {
            if (_towerDefinition == null || _towerState == null)
            {
                return null;
            }

            foreach (var merge in _towerDefinition.Merges)
            {
                if (merge.RequiredLevel == _towerState.Level)
                {
                    return merge;
                }
            }

            return null;
        }

        private void ApplyBuildVisual()
        {
            if (_activeTower == null || _towerDefinition == null)
            {
                return;
            }

            var renderer = _activeTower.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.color = GetTowerColor(_towerDefinition.DamageType);
            _activeTower.GetComponent<TurretSpriteAnimator>()?.SetTower(_towerDefinition);
            if (_towerDefinition.Id.Value.StartsWith("hero_", StringComparison.Ordinal))
            {
                TransformWorldScaleUtility.SetUniform(_activeTower.transform, HeroPlacementScale);
            }

            _activeTower.GetComponent<TowerAttackAnimator>()?.CaptureCurrentVisualState();
        }

        private static Color GetTowerColor(string damageType)
        {
            switch (damageType)
            {
                case "laser":
                    return new Color(0.25f, 0.86f, 1f, 1f);
                case "cannon":
                    return new Color(1f, 0.7f, 0.2f, 1f);
                case "tesla":
                    return new Color(0.75f, 0.35f, 1f, 1f);
                case "frost":
                    return new Color(0.6f, 1f, 0.95f, 1f);
                default:
                    return Color.white;
            }
        }

        public void ShowDetails(string message = null)
        {
            if (_detailsPanel != null && _towerDefinition != null && _towerState != null)
            {
                _detailsPanel.Show(this, _towerDefinition, _towerState, message);
            }
        }
    }
}
