using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class EnemyDefinition
    {
        public EnemyDefinition(
            StableId id,
            string displayNameKey,
            float maxHealth,
            float movementSpeed,
            int contactDamage,
            int rewardScrap,
            int threatValue,
            string enemyTag,
            string weakToDamageType,
            string prefabId)
        {
            Id = id;
            DisplayNameKey = displayNameKey;
            MaxHealth = maxHealth;
            MovementSpeed = movementSpeed;
            ContactDamage = contactDamage;
            RewardScrap = rewardScrap;
            ThreatValue = threatValue;
            EnemyTag = enemyTag;
            WeakToDamageType = weakToDamageType;
            PrefabId = prefabId;
        }

        public StableId Id { get; }

        public string DisplayNameKey { get; }

        public float MaxHealth { get; }

        public float MovementSpeed { get; }

        public int ContactDamage { get; }

        public int RewardScrap { get; }

        public int ThreatValue { get; }

        public string EnemyTag { get; }

        public string WeakToDamageType { get; }

        public string PrefabId { get; }
    }
}
