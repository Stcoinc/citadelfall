using System;
using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [Serializable]
    public sealed class TowerUpgradeAuthoringData
    {
        public int FromLevel = 1;
        public int ToLevel = 2;
        public TowerUpgradeCurrency Currency = TowerUpgradeCurrency.Scrap;
        public int Cost = 100;
        public float Damage = 20f;
        public float Range = 5f;
        public float AttackIntervalSeconds = 0.5f;
        public int PowerRating = 75;

        public TowerUpgradeDto ToDto()
        {
            return new TowerUpgradeDto
            {
                FromLevel = FromLevel,
                ToLevel = ToLevel,
                Currency = Currency.ToString(),
                Cost = Cost,
                Damage = Damage,
                Range = Range,
                AttackIntervalSeconds = AttackIntervalSeconds,
                PowerRating = PowerRating
            };
        }
    }
}
