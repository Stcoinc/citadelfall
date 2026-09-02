using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class LevelDefinition
    {
        public LevelDefinition(
            StableId id,
            string displayNameKey,
            GameMode mode,
            int startingScrap,
            int baseHealth,
            int buildSocketCount,
            StableId waveSetId,
            int rewardScrap,
            int rewardCoins,
            int rewardGems,
            string rewardItemId)
        {
            Id = id;
            DisplayNameKey = displayNameKey;
            Mode = mode;
            StartingScrap = startingScrap;
            BaseHealth = baseHealth;
            BuildSocketCount = buildSocketCount;
            WaveSetId = waveSetId;
            RewardScrap = rewardScrap;
            RewardCoins = rewardCoins;
            RewardGems = rewardGems;
            RewardItemId = rewardItemId ?? string.Empty;
        }

        public StableId Id { get; }

        public string DisplayNameKey { get; }

        public GameMode Mode { get; }

        public int StartingScrap { get; }

        public int BaseHealth { get; }

        public int BuildSocketCount { get; }

        public StableId WaveSetId { get; }

        public int RewardScrap { get; }

        public int RewardCoins { get; }

        public int RewardGems { get; }

        public string RewardItemId { get; }
    }
}
