using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class TowerDto
    {
        public string Id;
        public string DisplayNameKey;
        public string DescriptionKey;
        public string BehaviorId;
        public string TargetingMode;
        public int BuildCost;
        public int PowerCost;
        public float Damage;
        public float Range;
        public float AttackIntervalSeconds;
        public int MaxLevel;
        public int PowerRating;
        public string PreferredEnemyTag;
        public string DamageType;
        public bool HasSplash;
        public bool ShouldRotate = true;
        public string PrefabId;
        public string IconId;
        public int UnlockCostCoins;
        public string[] LevelDisplayNames;
        public TowerUpgradeDto[] Upgrades;
        public TowerMergeDto[] Merges;
    }
}
