using System.Collections.Generic;
using System.Linq;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public sealed class TargetingSelector : ITargetingSelector
    {
        public TargetCandidate SelectTarget(IReadOnlyList<TargetCandidate> candidates, TargetingMode targetingMode)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            switch (targetingMode)
            {
                case TargetingMode.Last:
                    return candidates.OrderBy(candidate => candidate.PathProgress).FirstOrDefault();
                case TargetingMode.Closest:
                    return candidates.OrderBy(candidate => candidate.DistanceSquared).FirstOrDefault();
                case TargetingMode.LowestHealth:
                    return candidates.OrderBy(candidate => candidate.Health).FirstOrDefault();
                case TargetingMode.HighestHealth:
                    return candidates.OrderByDescending(candidate => candidate.Health).FirstOrDefault();
                case TargetingMode.BossPriority:
                    return candidates
                        .OrderByDescending(candidate => candidate.IsBoss)
                        .ThenByDescending(candidate => candidate.PathProgress)
                        .FirstOrDefault();
                case TargetingMode.First:
                default:
                    return candidates.OrderByDescending(candidate => candidate.PathProgress).FirstOrDefault();
            }
        }
    }
}
