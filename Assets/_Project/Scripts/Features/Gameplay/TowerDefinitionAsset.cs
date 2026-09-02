using System.Linq;
using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using ClubGamerZone.TowerDefense.Domain.Content;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [CreateAssetMenu(menuName = "Tower Defense/Content/Tower Definition", fileName = "TowerDefinition")]
    public sealed class TowerDefinitionAsset : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayNameKey;
        [SerializeField] private string _descriptionKey;
        [SerializeField] private string _behaviorId;
        [SerializeField] private string _prefabId;
        [SerializeField] private string _iconId;
        [SerializeField] private Sprite _icon;
        [SerializeField] private GameObject _prefab;

        [Header("Gameplay")]
        [SerializeField] private TargetingMode _targetingMode = TargetingMode.First;
        [SerializeField] private int _buildCost = 50;
        [SerializeField] private int _powerCost = 2;
        [SerializeField] private float _damage = 12f;
        [SerializeField] private float _range = 5.5f;
        [SerializeField] private float _attackIntervalSeconds = 0.45f;
        [SerializeField] private int _maxLevel = 3;
        [SerializeField] private int _powerRating = 42;
        [SerializeField] private string _preferredEnemyTag = "scout";
        [SerializeField] private string _damageType = "laser";
        [SerializeField] private bool _hasSplash;

        [Header("Economy")]
        [SerializeField] private int _unlockCostCoins;

        [Header("Per-Level Names")]
        [Tooltip("One editable player-facing name per level. Element 0 is level 1. Missing entries fall back to 'Base Name 2', 'Base Name 3', and so on.")]
        [SerializeField] private string[] _levelDisplayNames;

        [Header("Progression")]
        [SerializeField] private TowerUpgradeAuthoringData[] _upgrades;
        [SerializeField] private TowerMergeAuthoringData[] _merges;

        public string Id => _id;

        public Sprite Icon => _icon;

        public GameObject Prefab => _prefab;

        public TowerDto ToDto()
        {
            return new TowerDto
            {
                Id = _id,
                DisplayNameKey = _displayNameKey,
                DescriptionKey = _descriptionKey,
                BehaviorId = _behaviorId,
                TargetingMode = _targetingMode.ToString(),
                BuildCost = _buildCost,
                PowerCost = _powerCost,
                Damage = _damage,
                Range = _range,
                AttackIntervalSeconds = _attackIntervalSeconds,
                MaxLevel = _maxLevel,
                PowerRating = _powerRating,
                PreferredEnemyTag = _preferredEnemyTag,
                DamageType = _damageType,
                HasSplash = _hasSplash,
                PrefabId = _prefabId,
                IconId = _iconId,
                UnlockCostCoins = _unlockCostCoins,
                LevelDisplayNames = _levelDisplayNames,
                Upgrades = _upgrades == null ? null : _upgrades.Select(upgrade => upgrade.ToDto()).ToArray(),
                Merges = _merges == null ? null : _merges.Select(merge => merge.ToDto()).ToArray()
            };
        }
    }
}
