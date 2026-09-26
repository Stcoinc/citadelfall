using System;
using System.Collections.Generic;
using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class BattlefieldDefinition
    {
        public BattlefieldDefinition(
            StableId id,
            string backgroundId,
            IReadOnlyList<BattlefieldPoint> pathPoints,
            IReadOnlyList<BattlefieldPoint> buildSocketPositions)
        {
            Id = id;
            BackgroundId = backgroundId ?? string.Empty;
            PathPoints = pathPoints ?? Array.Empty<BattlefieldPoint>();
            BuildSocketPositions = buildSocketPositions ?? Array.Empty<BattlefieldPoint>();
        }

        public StableId Id { get; }

        public string BackgroundId { get; }

        public IReadOnlyList<BattlefieldPoint> PathPoints { get; }

        public IReadOnlyList<BattlefieldPoint> BuildSocketPositions { get; }
    }
}
