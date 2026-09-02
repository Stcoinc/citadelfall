namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class TowerUpgradeDefinition
    {
        public TowerUpgradeDefinition(
            int fromLevel,
            int toLevel,
            TowerUpgradeCurrency currency,
            int cost,
            float damage,
            float range,
            float attackIntervalSeconds,
            int powerRating)
        {
            FromLevel = fromLevel;
            ToLevel = toLevel;
            Currency = currency;
            Cost = cost;
            Damage = damage;
            Range = range;
            AttackIntervalSeconds = attackIntervalSeconds;
            PowerRating = powerRating;
        }

        public int FromLevel { get; }

        public int ToLevel { get; }

        public TowerUpgradeCurrency Currency { get; }

        public int Cost { get; }

        public float Damage { get; }

        public float Range { get; }

        public float AttackIntervalSeconds { get; }

        public int PowerRating { get; }
    }
}
