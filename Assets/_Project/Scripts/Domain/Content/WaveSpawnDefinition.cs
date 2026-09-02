using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class WaveSpawnDefinition
    {
        public WaveSpawnDefinition(StableId enemyId, int count, float intervalSeconds)
        {
            EnemyId = enemyId;
            Count = count;
            IntervalSeconds = intervalSeconds;
        }

        public StableId EnemyId { get; }

        public int Count { get; }

        public float IntervalSeconds { get; }
    }
}
