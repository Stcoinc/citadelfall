using System;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public sealed class TowerInstanceState
    {
        public TowerInstanceState(StableId towerId, int level)
        {
            if (towerId.IsEmpty)
            {
                throw new ArgumentException("Tower ID is required.", nameof(towerId));
            }

            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            TowerId = towerId;
            Level = level;
        }

        public StableId TowerId { get; }

        public int Level { get; }

        public TowerInstanceState ApplyUpgrade(TowerUpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                throw new ArgumentNullException(nameof(upgrade));
            }

            if (upgrade.FromLevel != Level)
            {
                throw new InvalidOperationException("Upgrade does not match the current tower level.");
            }

            return new TowerInstanceState(TowerId, upgrade.ToLevel);
        }

        public TowerInstanceState ApplyMerge(TowerMergeDefinition merge)
        {
            if (merge == null)
            {
                throw new ArgumentNullException(nameof(merge));
            }

            if (merge.RequiredLevel != Level)
            {
                throw new InvalidOperationException("Merge does not match the current tower level.");
            }

            var resultLevel = merge.ResultTowerId.Equals(TowerId) ? Level + 1 : 1;
            return new TowerInstanceState(merge.ResultTowerId, resultLevel);
        }
    }
}
