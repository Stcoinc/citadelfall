using System;
using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public static class ArenaRules
    {
        public static int GetSummonCost(int baseCost, int costStep, int completedSummons)
        {
            if (baseCost < 0 || costStep < 0 || completedSummons < 0)
            {
                throw new ArgumentOutOfRangeException("Arena summon values cannot be negative.");
            }

            var cost = (long)baseCost + ((long)costStep * completedSummons);
            return cost > int.MaxValue ? int.MaxValue : (int)cost;
        }

        public static bool CanMerge(
            StableId firstHeroId,
            int firstRank,
            StableId secondHeroId,
            int secondRank,
            int maximumMergeRank)
        {
            return !firstHeroId.IsEmpty &&
                   firstHeroId.Equals(secondHeroId) &&
                   firstRank > 0 &&
                   firstRank == secondRank &&
                   firstRank < maximumMergeRank;
        }
    }
}
