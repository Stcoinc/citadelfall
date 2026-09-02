using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class WaveSpawnDto
    {
        public string EnemyId;
        public int Count;
        public float IntervalSeconds;
    }
}
