using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class TowerMergeDefinition
    {
        public TowerMergeDefinition(int requiredLevel, StableId resultTowerId, TowerUpgradeCurrency currency, int cost)
        {
            RequiredLevel = requiredLevel;
            ResultTowerId = resultTowerId;
            Currency = currency;
            Cost = cost;
        }

        public int RequiredLevel { get; }

        public StableId ResultTowerId { get; }

        public TowerUpgradeCurrency Currency { get; }

        public int Cost { get; }
    }
}
