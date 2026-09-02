using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public sealed class TowerStatsView
    {
        public TowerStatsView(
            string displayNameKey,
            int level,
            int maxLevel,
            int powerRating,
            float damage,
            float range,
            float attackIntervalSeconds,
            TargetingMode targetingMode,
            string preferredEnemyTag,
            string damageType,
            bool hasSplash)
        {
            DisplayNameKey = displayNameKey;
            Level = level;
            MaxLevel = maxLevel;
            PowerRating = powerRating;
            Damage = damage;
            Range = range;
            AttackIntervalSeconds = attackIntervalSeconds;
            TargetingMode = targetingMode;
            PreferredEnemyTag = preferredEnemyTag;
            DamageType = damageType;
            HasSplash = hasSplash;
        }

        public string DisplayNameKey { get; }

        public int Level { get; }

        public int MaxLevel { get; }

        public int PowerRating { get; }

        public float Damage { get; }

        public float Range { get; }

        public float AttackIntervalSeconds { get; }

        public TargetingMode TargetingMode { get; }

        public string PreferredEnemyTag { get; }

        public string DamageType { get; }

        public bool HasSplash { get; }
    }
}
