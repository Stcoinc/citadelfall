using System.Collections.Generic;
using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class WaveDefinition
    {
        public WaveDefinition(StableId id, float startDelaySeconds, IReadOnlyList<WaveSpawnDefinition> spawns)
        {
            Id = id;
            StartDelaySeconds = startDelaySeconds;
            Spawns = spawns;
        }

        public StableId Id { get; }

        public float StartDelaySeconds { get; }

        public IReadOnlyList<WaveSpawnDefinition> Spawns { get; }
    }
}
