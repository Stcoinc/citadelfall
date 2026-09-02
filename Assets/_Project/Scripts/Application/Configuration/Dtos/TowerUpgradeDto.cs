using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class TowerUpgradeDto
    {
        public int FromLevel;
        public int ToLevel;
        public string Currency;
        public int Cost;
        public float Damage;
        public float Range;
        public float AttackIntervalSeconds;
        public int PowerRating;
    }
}
