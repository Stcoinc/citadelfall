using System;
using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [Serializable]
    public sealed class TowerMergeAuthoringData
    {
        public int RequiredLevel = 1;
        public string ResultTowerId;
        public TowerUpgradeCurrency Currency = TowerUpgradeCurrency.Coins;
        public int Cost = 100;

        public TowerMergeDto ToDto()
        {
            return new TowerMergeDto
            {
                RequiredLevel = RequiredLevel,
                ResultTowerId = ResultTowerId,
                Currency = Currency.ToString(),
                Cost = Cost
            };
        }
    }
}
