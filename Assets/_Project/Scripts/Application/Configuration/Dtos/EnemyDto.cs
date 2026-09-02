using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class EnemyDto
    {
        public string Id;
        public string DisplayNameKey;
        public float MaxHealth;
        public float MovementSpeed;
        public int ContactDamage;
        public int RewardScrap;
        public int ThreatValue;
        public string EnemyTag;
        public string WeakToDamageType;
        public string PrefabId;
    }
}
