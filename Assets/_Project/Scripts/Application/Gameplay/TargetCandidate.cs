using System;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public sealed class TargetCandidate
    {
        public TargetCandidate(
            string stableId,
            float pathProgress,
            float distanceSquared,
            float health,
            float maxHealth,
            bool isBoss)
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                throw new ArgumentException("Stable ID is required.", nameof(stableId));
            }

            StableId = stableId;
            PathProgress = pathProgress;
            DistanceSquared = distanceSquared;
            Health = health;
            MaxHealth = maxHealth;
            IsBoss = isBoss;
        }

        public string StableId { get; }

        public float PathProgress { get; }

        public float DistanceSquared { get; }

        public float Health { get; }

        public float MaxHealth { get; }

        public bool IsBoss { get; }
    }
}
