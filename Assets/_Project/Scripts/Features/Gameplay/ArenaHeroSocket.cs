using System.Collections;
using ClubGamerZone.TowerDefense.Application.Gameplay;
using ClubGamerZone.TowerDefense.Domain.Content;
using TMPro;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ArenaHeroSocket : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _platformRenderer;
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private UnitRangeIndicator _rangeIndicator;

        private GameObject _activeHero;
        private TowerWeapon _activeWeapon;
        private GameObject _heroPrefab;
        private EnemyRegistry _enemyRegistry;
        private TowerDefinition _definition;
        private int _mergeRank;
        private Color _normalColor = Color.white;
        private Vector3 _dragHomeLocalPosition;
        private Color _dragOriginalColor;
        private int _dragOriginalSortingOrder;
        private Coroutine _returnRoutine;

        public bool IsOccupied => _activeHero != null;
        public TowerDefinition Definition => _definition;
        public int MergeRank => _mergeRank;
        public Transform ActiveHeroTransform => _activeHero == null ? null : _activeHero.transform;

        public void BeginDragPreview()
        {
            if (_activeHero == null)
            {
                return;
            }

            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }

            SetAttackEnabled(false);

            _dragHomeLocalPosition = _activeHero.transform.localPosition;
            var renderer = _activeHero.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                _dragOriginalColor = renderer.color;
                _dragOriginalSortingOrder = renderer.sortingOrder;
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.78f);
                renderer.sortingOrder += 100;
            }
        }

        public void MoveDragPreview(Vector3 worldPosition)
        {
            if (_activeHero != null)
            {
                _activeHero.transform.position = new Vector3(worldPosition.x, worldPosition.y, _activeHero.transform.position.z);
            }
        }

        public void ReturnDragPreview(float duration = 0.2f)
        {
            if (_activeHero == null)
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
            var hero = _activeHero;
            var start = hero.transform.localPosition;
            var elapsed = 0f;
            while (hero != null && elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                var eased = 1f - Mathf.Pow(1f - progress, 3f);
                hero.transform.localPosition = Vector3.LerpUnclamped(start, _dragHomeLocalPosition, eased);
                yield return null;
            }

            if (hero != null)
            {
                hero.transform.localPosition = _dragHomeLocalPosition;
                SetAttackEnabled(true);
            }

            _returnRoutine = null;
        }

        private void RestoreDragRenderer()
        {
            var renderer = _activeHero == null ? null : _activeHero.GetComponent<SpriteRenderer>();
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

        private void Awake()
        {
            if (_platformRenderer == null)
            {
                _platformRenderer = GetComponent<SpriteRenderer>();
            }

            if (_platformRenderer != null)
            {
                _normalColor = _platformRenderer.color;
            }

            RefreshVisuals();
        }

        public void Initialize(GameObject heroPrefab, EnemyRegistry enemyRegistry)
        {
            _heroPrefab = heroPrefab;
            _enemyRegistry = enemyRegistry;
            Clear();
        }

        public bool Summon(TowerDefinition definition)
        {
            if (IsOccupied || definition == null || _heroPrefab == null || _enemyRegistry == null)
            {
                return false;
            }

            _definition = definition;
            _mergeRank = 1;
            CreateHero();
            RefreshVisuals();
            return true;
        }

        public bool MergeFrom(ArenaHeroSocket source, int maximumMergeRank)
        {
            if (source == null || source == this || !IsOccupied || !source.IsOccupied ||
                !ArenaRules.CanMerge(_definition.Id, _mergeRank, source._definition.Id, source._mergeRank, maximumMergeRank))
            {
                return false;
            }

            source.Clear();
            _mergeRank++;
            RebuildHero();
            RefreshVisuals();
            return true;
        }

        public void SetSelected(bool selected)
        {
            if (_platformRenderer != null)
            {
                _platformRenderer.color = selected
                    ? new Color(1f, 0.82f, 0.24f, 1f)
                    : _normalColor;
            }

            if (selected && IsOccupied && _definition != null)
            {
                _rangeIndicator?.Show(_definition.Range);
            }
            else
            {
                _rangeIndicator?.Hide();
            }
        }

        public void Clear()
        {
            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }

            if (_activeHero != null)
            {
                Destroy(_activeHero);
            }

            _activeHero = null;
            _activeWeapon = null;
            _definition = null;
            _mergeRank = 0;
            SetSelected(false);
            RefreshVisuals();
        }

        private void CreateHero()
        {
            _activeHero = Instantiate(_heroPrefab, transform.position, Quaternion.identity, transform);
            _activeHero.transform.localPosition = new Vector3(0f, 0.12f, -0.1f);
            TransformWorldScaleUtility.SetUniform(
                _activeHero.transform,
                0.86f + ((_mergeRank - 1) * 0.07f));

            var effectiveDefinition = CreateRankedDefinition(_definition, _mergeRank);
            _activeHero.GetComponent<TurretSpriteAnimator>()?.SetTower(effectiveDefinition);
            _activeHero.GetComponent<TowerAttackAnimator>()?.CaptureCurrentVisualState();
            _activeWeapon = _activeHero.GetComponent<TowerWeapon>();
            _activeWeapon?.Initialize(
                effectiveDefinition,
                new TowerInstanceState(effectiveDefinition.Id, 1),
                _enemyRegistry);
        }

        private void RebuildHero()
        {
            if (_activeHero != null)
            {
                Destroy(_activeHero);
            }

            CreateHero();
        }

        private void RefreshVisuals()
        {
            if (_rankText != null)
            {
                _rankText.gameObject.SetActive(IsOccupied);
                _rankText.text = IsOccupied ? $"R{_mergeRank}" : string.Empty;
            }
        }

        private static TowerDefinition CreateRankedDefinition(TowerDefinition source, int rank)
        {
            var damageMultiplier = 1f + ((rank - 1) * 0.55f);
            var intervalMultiplier = Mathf.Max(0.55f, 1f - ((rank - 1) * 0.05f));
            return new TowerDefinition(
                source.Id,
                source.DisplayNameKey,
                source.DescriptionKey,
                source.BehaviorId,
                source.TargetingMode,
                source.BuildCost,
                source.PowerCost,
                source.Damage * damageMultiplier,
                source.Range,
                source.AttackIntervalSeconds * intervalMultiplier,
                source.MaxLevel,
                Mathf.RoundToInt(source.PowerRating * damageMultiplier),
                source.PreferredEnemyTag,
                source.DamageType,
                source.HasSplash,
                source.PrefabId,
                source.IconId,
                source.UnlockCostCoins,
                source.Upgrades,
                source.Merges,
                source.LevelDisplayNames,
                source.ShouldRotate);
        }
    }
}
