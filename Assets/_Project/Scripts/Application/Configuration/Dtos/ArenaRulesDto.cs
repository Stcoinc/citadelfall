using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class ArenaRulesDto
    {
        public string Id;
        public int BoardRows;
        public int BoardColumns;
        public int DeckSize;
        public int StartingMana;
        public int BaseSummonCost;
        public int SummonCostStep;
        public int MaximumMergeRank;
        public int StrongholdLives;
        public float BossIntervalSeconds;
        public float MatchDurationSeconds;
        public float EnemySpeedMultiplier;
        public float EnemyHealthMultiplier;
        public float EnemyHealthPerWave;
        public float BossHealthMultiplier;
        public float BaseSpawnIntervalSeconds;
        public float SpawnIntervalReductionPerWave;
        public float MinimumSpawnIntervalSeconds;
        public int WolfIntroductionWave;
        public int GoblinIntroductionWave;
        public int OrcIntroductionWave;
        public int VictoryRewardCoins;
        public int DefeatRewardCoins;
    }
}
