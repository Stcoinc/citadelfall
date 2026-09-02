using System.Collections.Generic;
using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class WaveSetDefinition
    {
        public WaveSetDefinition(StableId id, IReadOnlyList<WaveDefinition> waves)
        {
            Id = id;
            Waves = waves;
        }

        public StableId Id { get; }

        public IReadOnlyList<WaveDefinition> Waves { get; }
    }
}
