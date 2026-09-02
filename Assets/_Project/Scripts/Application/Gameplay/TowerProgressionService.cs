using System.Linq;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public sealed class TowerProgressionService
    {
        public TowerUpgradeDefinition FindUpgrade(TowerDefinition definition, TowerInstanceState state, TowerUpgradeCurrency currency)
        {
            if (definition == null || state == null || !definition.Id.Equals(state.TowerId))
            {
                return null;
            }

            return definition.Upgrades.FirstOrDefault(upgrade =>
                upgrade.FromLevel == state.Level &&
                upgrade.Currency == currency);
        }

        public TowerMergeDefinition FindMerge(TowerDefinition definition, TowerInstanceState first, TowerInstanceState second)
        {
            if (definition == null || first == null || second == null)
            {
                return null;
            }

            if (!definition.Id.Equals(first.TowerId) || !first.TowerId.Equals(second.TowerId) || first.Level != second.Level)
            {
                return null;
            }

            return definition.Merges.FirstOrDefault(merge => merge.RequiredLevel == first.Level);
        }

        public TowerInstanceState CreateLevelOne(StableId towerId)
        {
            return new TowerInstanceState(towerId, 1);
        }
    }
}
