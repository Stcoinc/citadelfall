using System;
using System.Collections.Generic;
using System.Linq;
using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Application.Configuration
{
    public sealed class StarterContentCatalogBuilder : IContentCatalogBuilder
    {
        private readonly IContentValidator _validator;

        public StarterContentCatalogBuilder(IContentValidator validator)
        {
            _validator = validator;
        }

        public ContentCatalogBuildResult Build(StarterContentDto content)
        {
            var validation = _validator.Validate(content);
            if (!validation.IsValid)
            {
                return new ContentCatalogBuildResult(null, validation);
            }

            var towers = content.Towers.ToDictionary(tower => new StableId(tower.Id), BuildTower);
            var enemies = content.Enemies.ToDictionary(enemy => new StableId(enemy.Id), BuildEnemy);
            var levels = content.Levels.ToDictionary(level => new StableId(level.Id), BuildLevel);
            var waveSets = content.WaveSets.ToDictionary(waveSet => new StableId(waveSet.Id), BuildWaveSet);
            var arenaRules = (content.ArenaRules ?? Array.Empty<ArenaRulesDto>())
                .ToDictionary(rules => new StableId(rules.Id), BuildArenaRules);

            var catalog = new ContentCatalog(
                content.SchemaVersion,
                content.ContentVersion,
                towers,
                enemies,
                levels,
                waveSets,
                arenaRules);

            return new ContentCatalogBuildResult(catalog, validation);
        }

        private static TowerDefinition BuildTower(TowerDto tower)
        {
            return new TowerDefinition(
                new StableId(tower.Id),
                tower.DisplayNameKey,
                tower.DescriptionKey,
                tower.BehaviorId,
                ParseEnum<TargetingMode>(tower.TargetingMode),
                tower.BuildCost,
                tower.PowerCost,
                tower.Damage,
                tower.Range,
                tower.AttackIntervalSeconds,
                tower.MaxLevel,
                tower.PowerRating,
                tower.PreferredEnemyTag,
                tower.DamageType,
                tower.HasSplash,
                tower.PrefabId,
                tower.IconId,
                tower.UnlockCostCoins,
                tower.Upgrades == null ? Array.Empty<TowerUpgradeDefinition>() : tower.Upgrades.Select(BuildUpgrade).ToArray(),
                tower.Merges == null ? Array.Empty<TowerMergeDefinition>() : tower.Merges.Select(BuildMerge).ToArray(),
                tower.LevelDisplayNames ?? Array.Empty<string>());
        }

        private static TowerUpgradeDefinition BuildUpgrade(TowerUpgradeDto upgrade)
        {
            return new TowerUpgradeDefinition(
                upgrade.FromLevel,
                upgrade.ToLevel,
                ParseEnum<TowerUpgradeCurrency>(upgrade.Currency),
                upgrade.Cost,
                upgrade.Damage,
                upgrade.Range,
                upgrade.AttackIntervalSeconds,
                upgrade.PowerRating);
        }

        private static TowerMergeDefinition BuildMerge(TowerMergeDto merge)
        {
            return new TowerMergeDefinition(
                merge.RequiredLevel,
                new StableId(merge.ResultTowerId),
                ParseEnum<TowerUpgradeCurrency>(merge.Currency),
                merge.Cost);
        }

        private static EnemyDefinition BuildEnemy(EnemyDto enemy)
        {
            return new EnemyDefinition(
                new StableId(enemy.Id),
                enemy.DisplayNameKey,
                enemy.MaxHealth,
                enemy.MovementSpeed,
                enemy.ContactDamage,
                enemy.RewardScrap,
                enemy.ThreatValue,
                enemy.EnemyTag,
                enemy.WeakToDamageType,
                enemy.PrefabId);
        }

        private static LevelDefinition BuildLevel(LevelDto level)
        {
            return new LevelDefinition(
                new StableId(level.Id),
                level.DisplayNameKey,
                ParseEnum<GameMode>(level.Mode),
                level.StartingScrap,
                level.BaseHealth,
                level.BuildSocketCount,
                new StableId(level.WaveSetId),
                level.RewardScrap,
                level.RewardCoins,
                level.RewardGems,
                level.RewardItemId);
        }

        private static WaveSetDefinition BuildWaveSet(WaveSetDto waveSet)
        {
            return new WaveSetDefinition(
                new StableId(waveSet.Id),
                waveSet.Waves.Select(BuildWave).ToArray());
        }

        private static WaveDefinition BuildWave(WaveDto wave)
        {
            return new WaveDefinition(
                new StableId(wave.Id),
                wave.StartDelaySeconds,
                wave.Spawns.Select(BuildSpawn).ToArray());
        }

        private static WaveSpawnDefinition BuildSpawn(WaveSpawnDto spawn)
        {
            return new WaveSpawnDefinition(
                new StableId(spawn.EnemyId),
                spawn.Count,
                spawn.IntervalSeconds);
        }

        private static ArenaRulesDefinition BuildArenaRules(ArenaRulesDto rules)
        {
            return new ArenaRulesDefinition(
                new StableId(rules.Id),
                rules.BoardRows,
                rules.BoardColumns,
                rules.DeckSize,
                rules.StartingMana,
                rules.BaseSummonCost,
                rules.SummonCostStep,
                rules.MaximumMergeRank,
                rules.StrongholdLives,
                rules.BossIntervalSeconds,
                rules.MatchDurationSeconds,
                ResolvePositive(rules.EnemySpeedMultiplier, 0.5f),
                ResolvePositive(rules.EnemyHealthMultiplier, 0.65f),
                ResolvePositive(rules.EnemyHealthPerWave, 0.08f),
                ResolvePositive(rules.BossHealthMultiplier, 1.35f),
                ResolvePositive(rules.BaseSpawnIntervalSeconds, 2.4f),
                ResolvePositive(rules.SpawnIntervalReductionPerWave, 0.05f),
                ResolvePositive(rules.MinimumSpawnIntervalSeconds, 1f),
                ResolvePositive(rules.WolfIntroductionWave, 3),
                ResolvePositive(rules.GoblinIntroductionWave, 5),
                ResolvePositive(rules.OrcIntroductionWave, 7),
                rules.VictoryRewardCoins,
                rules.DefeatRewardCoins);
        }

        private static float ResolvePositive(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }

        private static int ResolvePositive(int value, int fallback)
        {
            return value > 0 ? value : fallback;
        }

        private static TEnum ParseEnum<TEnum>(string value)
            where TEnum : struct
        {
            return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : default;
        }
    }
}
