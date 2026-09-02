using System.Linq;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public sealed class TowerStatsPresenter
    {
        public TowerStatsView BuildStats(TowerDefinition definition, TowerInstanceState state)
        {
            if (definition == null || state == null)
            {
                return null;
            }

            var matchingUpgrade = definition.Upgrades
                .Where(upgrade => upgrade.ToLevel <= state.Level)
                .OrderByDescending(upgrade => upgrade.ToLevel)
                .FirstOrDefault();

            return new TowerStatsView(
                definition.GetDisplayName(state.Level),
                state.Level,
                definition.MaxLevel,
                matchingUpgrade == null ? definition.PowerRating : matchingUpgrade.PowerRating,
                matchingUpgrade == null ? definition.Damage : matchingUpgrade.Damage,
                matchingUpgrade == null ? definition.Range : matchingUpgrade.Range,
                matchingUpgrade == null ? definition.AttackIntervalSeconds : matchingUpgrade.AttackIntervalSeconds,
                definition.TargetingMode,
                definition.PreferredEnemyTag,
                definition.DamageType,
                definition.HasSplash);
        }
    }
}
