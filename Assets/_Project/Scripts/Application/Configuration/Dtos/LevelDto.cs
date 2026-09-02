using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class LevelDto
    {
        public string Id;
        public string DisplayNameKey;
        public string Mode;
        public int StartingScrap;
        public int BaseHealth;
        public int BuildSocketCount;
        public string WaveSetId;
        public int RewardScrap;
        public int RewardCoins;
        public int RewardGems;
        public string RewardItemId;
    }
}
