using System.Collections.Generic;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public interface ITargetingSelector
    {
        TargetCandidate SelectTarget(IReadOnlyList<TargetCandidate> candidates, TargetingMode targetingMode);
    }
}
