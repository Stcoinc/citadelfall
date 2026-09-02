using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class ArenaRulesDefinition
    {
        public ArenaRulesDefinition(
            StableId id,
            int boardRows,
            int boardColumns,
            int deckSize,
            int startingMana,
            int baseSummonCost,
            int summonCostStep,
            int maximumMergeRank,
            int strongholdLives,
            float bossIntervalSeconds,
            float matchDurationSeconds,
            float enemySpeedMultiplier,
            float enemyHealthMultiplier,
            float enemyHealthPerWave,
            float bossHealthMultiplier,
            float baseSpawnIntervalSeconds,
            float spawnIntervalReductionPerWave,
            float minimumSpawnIntervalSeconds,
            int wolfIntroductionWave,
            int goblinIntroductionWave,
            int orcIntroductionWave,
            int victoryRewardCoins,
            int defeatRewardCoins)
        {
            Id = id;
            BoardRows = boardRows;
            BoardColumns = boardColumns;
            DeckSize = deckSize;
            StartingMana = startingMana;
            BaseSummonCost = baseSummonCost;
            SummonCostStep = summonCostStep;
            MaximumMergeRank = maximumMergeRank;
            StrongholdLives = strongholdLives;
            BossIntervalSeconds = bossIntervalSeconds;
            MatchDurationSeconds = matchDurationSeconds;
            EnemySpeedMultiplier = enemySpeedMultiplier;
            EnemyHealthMultiplier = enemyHealthMultiplier;
            EnemyHealthPerWave = enemyHealthPerWave;
            BossHealthMultiplier = bossHealthMultiplier;
            BaseSpawnIntervalSeconds = baseSpawnIntervalSeconds;
            SpawnIntervalReductionPerWave = spawnIntervalReductionPerWave;
            MinimumSpawnIntervalSeconds = minimumSpawnIntervalSeconds;
            WolfIntroductionWave = wolfIntroductionWave;
            GoblinIntroductionWave = goblinIntroductionWave;
            OrcIntroductionWave = orcIntroductionWave;
            VictoryRewardCoins = victoryRewardCoins;
            DefeatRewardCoins = defeatRewardCoins;
        }

        public StableId Id { get; }
        public int BoardRows { get; }
        public int BoardColumns { get; }
        public int DeckSize { get; }
        public int StartingMana { get; }
        public int BaseSummonCost { get; }
        public int SummonCostStep { get; }
        public int MaximumMergeRank { get; }
        public int StrongholdLives { get; }
        public float BossIntervalSeconds { get; }
        public float MatchDurationSeconds { get; }
        public float EnemySpeedMultiplier { get; }
        public float EnemyHealthMultiplier { get; }
        public float EnemyHealthPerWave { get; }
        public float BossHealthMultiplier { get; }
        public float BaseSpawnIntervalSeconds { get; }
        public float SpawnIntervalReductionPerWave { get; }
        public float MinimumSpawnIntervalSeconds { get; }
        public int WolfIntroductionWave { get; }
        public int GoblinIntroductionWave { get; }
        public int OrcIntroductionWave { get; }
        public int VictoryRewardCoins { get; }
        public int DefeatRewardCoins { get; }
        public int SocketCount => BoardRows * BoardColumns;
    }
}
