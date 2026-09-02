using ClubGamerZone.TowerDefense.Core;
using System;
using System.Collections.Generic;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class TowerDefinition
    {
        public TowerDefinition(
            StableId id,
            string displayNameKey,
            string descriptionKey,
            string behaviorId,
            TargetingMode targetingMode,
            int buildCost,
            int powerCost,
            float damage,
            float range,
            float attackIntervalSeconds,
            int maxLevel,
            int powerRating,
            string preferredEnemyTag,
            string damageType,
            bool hasSplash,
            string prefabId,
            string iconId,
            int unlockCostCoins,
            IReadOnlyList<TowerUpgradeDefinition> upgrades,
            IReadOnlyList<TowerMergeDefinition> merges,
            IReadOnlyList<string> levelDisplayNames = null)
        {
            Id = id;
            DisplayNameKey = displayNameKey;
            DescriptionKey = descriptionKey;
            BehaviorId = behaviorId;
            TargetingMode = targetingMode;
            BuildCost = buildCost;
            PowerCost = powerCost;
            Damage = damage;
            Range = range;
            AttackIntervalSeconds = attackIntervalSeconds;
            MaxLevel = maxLevel;
            PowerRating = powerRating;
            PreferredEnemyTag = preferredEnemyTag;
            DamageType = damageType;
            HasSplash = hasSplash;
            PrefabId = prefabId;
            IconId = iconId;
            UnlockCostCoins = unlockCostCoins;
            Upgrades = upgrades;
            Merges = merges;
            LevelDisplayNames = levelDisplayNames ?? Array.Empty<string>();
        }

        public StableId Id { get; }

        public string DisplayNameKey { get; }

        public string DescriptionKey { get; }

        public string BehaviorId { get; }

        public TargetingMode TargetingMode { get; }

        public int BuildCost { get; }

        public int PowerCost { get; }

        public float Damage { get; }

        public float Range { get; }

        public float AttackIntervalSeconds { get; }

        public int MaxLevel { get; }

        public int PowerRating { get; }

        public string PreferredEnemyTag { get; }

        public string DamageType { get; }

        public bool HasSplash { get; }

        public string PrefabId { get; }

        public string IconId { get; }

        public int UnlockCostCoins { get; }

        public IReadOnlyList<TowerUpgradeDefinition> Upgrades { get; }

        public IReadOnlyList<TowerMergeDefinition> Merges { get; }

        public IReadOnlyList<string> LevelDisplayNames { get; }

        public string GetDisplayName(int level)
        {
            var index = Math.Max(1, level) - 1;
            if (index < LevelDisplayNames.Count && !string.IsNullOrWhiteSpace(LevelDisplayNames[index]))
            {
                return LevelDisplayNames[index];
            }

            return level <= 1 ? DisplayNameKey : $"{DisplayNameKey} {level}";
        }
    }
}
