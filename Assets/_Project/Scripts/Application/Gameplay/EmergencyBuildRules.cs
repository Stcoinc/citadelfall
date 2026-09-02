using System;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public static class EmergencyBuildRules
    {
        public static bool IsFreeFallbackPlacement(
            string fallbackHeroId,
            string selectedHeroId,
            bool hasDeployedHero)
        {
            return !hasDeployedHero &&
                   !string.IsNullOrWhiteSpace(fallbackHeroId) &&
                   string.Equals(fallbackHeroId, selectedHeroId, StringComparison.Ordinal);
        }

        public static bool CanAfford(
            string fallbackHeroId,
            string selectedHeroId,
            int availableScrap,
            int buildCost,
            bool hasDeployedHero)
        {
            return IsFreeFallbackPlacement(fallbackHeroId, selectedHeroId, hasDeployedHero) ||
                   (buildCost >= 0 && availableScrap >= buildCost);
        }
    }
}
