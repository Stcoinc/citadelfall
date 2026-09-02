using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class WaveDto
    {
        public string Id;
        public float StartDelaySeconds;
        public WaveSpawnDto[] Spawns;
    }
}
