using System.Collections.Generic;
using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class WaveSetDefinition
    {
        public WaveSetDefinition(StableId id, bool hasBoss, IReadOnlyList<WaveDefinition> waves)
        {
            Id = id;
            HasBoss = hasBoss;
            Waves = waves;
        }

        public StableId Id { get; }

        public bool HasBoss { get; }

        public IReadOnlyList<WaveDefinition> Waves { get; }
    }
}
