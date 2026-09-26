using System;
using System.Collections.Generic;
using System.Linq;
using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Application.Configuration
{
    public sealed class StarterContentValidator : IContentValidator
    {
        private readonly ContentValidationLimits _limits;

        public StarterContentValidator(ContentValidationLimits limits)
        {
            _limits = limits;
        }

        public ValidationResult Validate(StarterContentDto content)
        {
            var issues = new List<ValidationIssue>();
            if (content == null)
            {
                issues.Add(Error("$", "Content JSON could not be parsed."));
                return new ValidationResult(issues);
            }

            ValidateRoot(content, issues);
            ValidateTowers(content.Towers, issues);
            ValidateTowerReferences(content.Towers, issues);
            ValidateEnemies(content.Enemies, issues);
            ValidateWaveSets(content.WaveSets, content.Enemies, content.SchemaVersion >= 3, issues);
            ValidateBattlefields(content.Battlefields, content.SchemaVersion >= 2, issues);
            ValidateLevels(content.Levels, content.WaveSets, content.Battlefields, content.SchemaVersion >= 2, issues);
            ValidateArenaRules(content.ArenaRules, issues);

            return new ValidationResult(issues);
        }

        private void ValidateRoot(StarterContentDto content, List<ValidationIssue> issues)
        {
            if (content.SchemaVersion < _limits.MinimumSchemaVersion || content.SchemaVersion > _limits.MaximumSupportedSchemaVersion)
            {
                issues.Add(Error("$.SchemaVersion", $"SchemaVersion must be between {_limits.MinimumSchemaVersion} and {_limits.MaximumSupportedSchemaVersion}."));
            }

            if (string.IsNullOrWhiteSpace(content.ContentVersion))
            {
                issues.Add(Error("$.ContentVersion", "ContentVersion is required."));
            }
        }

        private void ValidateTowers(TowerDto[] towers, List<ValidationIssue> issues)
        {
            if (towers == null || towers.Length == 0)
            {
                issues.Add(Error("$.Towers", "At least one tower is required."));
                return;
            }

            if (towers.Length > _limits.MaximumTowers)
            {
                issues.Add(Error("$.Towers", $"Tower count cannot exceed {_limits.MaximumTowers}."));
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < towers.Length; i++)
            {
                var tower = towers[i];
                var path = $"$.Towers[{i}]";
                if (tower == null)
                {
                    issues.Add(Error(path, "Tower entry cannot be null."));
                    continue;
                }

                ValidateStableId(tower.Id, $"{path}.Id", ids, issues);
                ValidateRequired(tower.DisplayNameKey, $"{path}.DisplayNameKey", issues);
                ValidateRequired(tower.BehaviorId, $"{path}.BehaviorId", issues);
                ValidateEnum<TargetingMode>(tower.TargetingMode, $"{path}.TargetingMode", issues);
                ValidateIntRange(tower.BuildCost, 0, _limits.MaximumCost, $"{path}.BuildCost", issues);
                ValidateIntRange(tower.PowerCost, 0, _limits.MaximumCost, $"{path}.PowerCost", issues);
                ValidateFloatRange(tower.Damage, _limits.MinimumDamage, _limits.MaximumDamage, $"{path}.Damage", issues);
                ValidateFloatRange(tower.Range, _limits.MinimumRange, _limits.MaximumRange, $"{path}.Range", issues);
                ValidateFloatRange(tower.AttackIntervalSeconds, _limits.MinimumAttackIntervalSeconds, _limits.MaximumAttackIntervalSeconds, $"{path}.AttackIntervalSeconds", issues);
                ValidateIntRange(tower.MaxLevel, 1, 100, $"{path}.MaxLevel", issues);
                ValidateIntRange(tower.PowerRating, 1, _limits.MaximumCost, $"{path}.PowerRating", issues);
                ValidateRequired(tower.PreferredEnemyTag, $"{path}.PreferredEnemyTag", issues);
                ValidateRequired(tower.DamageType, $"{path}.DamageType", issues);
                ValidateRequired(tower.PrefabId, $"{path}.PrefabId", issues);
                ValidateIntRange(tower.UnlockCostCoins, 0, _limits.MaximumCost, $"{path}.UnlockCostCoins", issues);
                ValidateLevelDisplayNames(tower.LevelDisplayNames, tower.MaxLevel, $"{path}.LevelDisplayNames", issues);
                ValidateTowerUpgrades(tower.Upgrades, tower.MaxLevel, $"{path}.Upgrades", issues);
                ValidateTowerMerges(tower.Merges, tower.MaxLevel, $"{path}.Merges", issues);
            }
        }

        private static void ValidateLevelDisplayNames(string[] names, int maxLevel, string path, List<ValidationIssue> issues)
        {
            if (names == null)
            {
                return;
            }

            if (names.Length > maxLevel)
            {
                issues.Add(Error(path, $"Level name count cannot exceed MaxLevel ({maxLevel})."));
            }

            for (var i = 0; i < names.Length; i++)
            {
                ValidateRequired(names[i], $"{path}[{i}]", issues);
            }
        }

        private void ValidateTowerUpgrades(TowerUpgradeDto[] upgrades, int maxLevel, string path, List<ValidationIssue> issues)
        {
            if (upgrades == null)
            {
                return;
            }

            var transitions = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < upgrades.Length; i++)
            {
                var upgrade = upgrades[i];
                var upgradePath = $"{path}[{i}]";

                if (upgrade == null)
                {
                    issues.Add(Error(upgradePath, "Upgrade entry cannot be null."));
                    continue;
                }

                ValidateIntRange(upgrade.FromLevel, 1, maxLevel, $"{upgradePath}.FromLevel", issues);
                ValidateIntRange(upgrade.ToLevel, 1, maxLevel, $"{upgradePath}.ToLevel", issues);
                if (upgrade.ToLevel <= upgrade.FromLevel)
                {
                    issues.Add(Error($"{upgradePath}.ToLevel", "ToLevel must be higher than FromLevel."));
                }

                var transition = $"{upgrade.FromLevel}->{upgrade.ToLevel}";
                if (!transitions.Add(transition))
                {
                    issues.Add(Error(upgradePath, $"Duplicate upgrade transition '{transition}'."));
                }

                ValidateEnum<TowerUpgradeCurrency>(upgrade.Currency, $"{upgradePath}.Currency", issues);
                if (!string.Equals(upgrade.Currency, TowerUpgradeCurrency.Scrap.ToString(), StringComparison.Ordinal))
                {
                    issues.Add(Error($"{upgradePath}.Currency", "Tower upgrades must use Scrap."));
                }

                ValidateIntRange(upgrade.Cost, 0, _limits.MaximumCost, $"{upgradePath}.Cost", issues);
                ValidateFloatRange(upgrade.Damage, _limits.MinimumDamage, _limits.MaximumDamage, $"{upgradePath}.Damage", issues);
                ValidateFloatRange(upgrade.Range, _limits.MinimumRange, _limits.MaximumRange, $"{upgradePath}.Range", issues);
                ValidateFloatRange(upgrade.AttackIntervalSeconds, _limits.MinimumAttackIntervalSeconds, _limits.MaximumAttackIntervalSeconds, $"{upgradePath}.AttackIntervalSeconds", issues);
                ValidateIntRange(upgrade.PowerRating, 1, _limits.MaximumCost, $"{upgradePath}.PowerRating", issues);
            }
        }

        private void ValidateTowerMerges(TowerMergeDto[] merges, int maxLevel, string path, List<ValidationIssue> issues)
        {
            if (merges == null)
            {
                return;
            }

            var levels = new HashSet<int>();
            for (var i = 0; i < merges.Length; i++)
            {
                var merge = merges[i];
                var mergePath = $"{path}[{i}]";

                if (merge == null)
                {
                    issues.Add(Error(mergePath, "Merge entry cannot be null."));
                    continue;
                }

                ValidateIntRange(merge.RequiredLevel, 1, maxLevel, $"{mergePath}.RequiredLevel", issues);
                ValidateEnum<TowerUpgradeCurrency>(merge.Currency, $"{mergePath}.Currency", issues);
                if (!string.Equals(merge.Currency, TowerUpgradeCurrency.Coins.ToString(), StringComparison.Ordinal))
                {
                    issues.Add(Error($"{mergePath}.Currency", "Tower merges must use Coins."));
                }

                ValidateIntRange(merge.Cost, 0, _limits.MaximumCost, $"{mergePath}.Cost", issues);
                if (!levels.Add(merge.RequiredLevel))
                {
                    issues.Add(Error($"{mergePath}.RequiredLevel", $"Duplicate merge for level {merge.RequiredLevel}."));
                }

                ValidateStableId(merge.ResultTowerId, $"{mergePath}.ResultTowerId", null, issues);
            }
        }

        private static void ValidateTowerReferences(TowerDto[] towers, List<ValidationIssue> issues)
        {
            var towerIds = BuildValidIdSet(towers);

            if (towers == null)
            {
                return;
            }

            for (var i = 0; i < towers.Length; i++)
            {
                var tower = towers[i];
                if (tower == null || tower.Merges == null)
                {
                    continue;
                }

                for (var mergeIndex = 0; mergeIndex < tower.Merges.Length; mergeIndex++)
                {
                    var merge = tower.Merges[mergeIndex];
                    if (merge == null || string.IsNullOrWhiteSpace(merge.ResultTowerId))
                    {
                        continue;
                    }

                    if (!towerIds.Contains(merge.ResultTowerId))
                    {
                        issues.Add(Error($"$.Towers[{i}].Merges[{mergeIndex}].ResultTowerId", $"Unknown tower id '{merge.ResultTowerId}'."));
                    }
                }
            }
        }

        private void ValidateEnemies(EnemyDto[] enemies, List<ValidationIssue> issues)
        {
            if (enemies == null || enemies.Length == 0)
            {
                issues.Add(Error("$.Enemies", "At least one enemy is required."));
                return;
            }

            if (enemies.Length > _limits.MaximumEnemies)
            {
                issues.Add(Error("$.Enemies", $"Enemy count cannot exceed {_limits.MaximumEnemies}."));
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                var path = $"$.Enemies[{i}]";
                if (enemy == null)
                {
                    issues.Add(Error(path, "Enemy entry cannot be null."));
                    continue;
                }

                ValidateStableId(enemy.Id, $"{path}.Id", ids, issues);
                ValidateRequired(enemy.DisplayNameKey, $"{path}.DisplayNameKey", issues);
                ValidateFloatRange(enemy.MaxHealth, _limits.MinimumHealth, _limits.MaximumHealth, $"{path}.MaxHealth", issues);
                ValidateFloatRange(enemy.MovementSpeed, _limits.MinimumMovementSpeed, _limits.MaximumMovementSpeed, $"{path}.MovementSpeed", issues);
                ValidateIntRange(enemy.ContactDamage, 0, _limits.MaximumCost, $"{path}.ContactDamage", issues);
                ValidateIntRange(enemy.RewardScrap, 0, _limits.MaximumCost, $"{path}.RewardScrap", issues);
                ValidateIntRange(enemy.ThreatValue, 0, _limits.MaximumCost, $"{path}.ThreatValue", issues);
                ValidateRequired(enemy.EnemyTag, $"{path}.EnemyTag", issues);
                ValidateRequired(enemy.WeakToDamageType, $"{path}.WeakToDamageType", issues);
                ValidateRequired(enemy.PrefabId, $"{path}.PrefabId", issues);
            }
        }

        private void ValidateWaveSets(
            WaveSetDto[] waveSets,
            EnemyDto[] enemies,
            bool bossDeclarationRequired,
            List<ValidationIssue> issues)
        {
            if (waveSets == null || waveSets.Length == 0)
            {
                issues.Add(Error("$.WaveSets", "At least one wave set is required."));
                return;
            }

            if (waveSets.Length > _limits.MaximumWaveSets)
            {
                issues.Add(Error("$.WaveSets", $"Wave set count cannot exceed {_limits.MaximumWaveSets}."));
            }

            var enemyIds = BuildValidIdSet(enemies);
            var bossEnemyIds = new HashSet<string>(
                (enemies ?? Array.Empty<EnemyDto>())
                    .Where(enemy => enemy != null &&
                                    StableId.IsValid(enemy.Id) &&
                                    !string.IsNullOrWhiteSpace(enemy.EnemyTag) &&
                                    enemy.EnemyTag.StartsWith("boss", StringComparison.OrdinalIgnoreCase))
                    .Select(enemy => enemy.Id),
                StringComparer.Ordinal);
            var waveSetIds = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < waveSets.Length; i++)
            {
                var waveSet = waveSets[i];
                var path = $"$.WaveSets[{i}]";
                if (waveSet == null)
                {
                    issues.Add(Error(path, "Wave set entry cannot be null."));
                    continue;
                }

                ValidateStableId(waveSet.Id, $"{path}.Id", waveSetIds, issues);
                ValidateWaves(waveSet.Waves, enemyIds, $"{path}.Waves", issues);
                if (bossDeclarationRequired)
                {
                    ValidateBossDeclaration(waveSet, bossEnemyIds, path, issues);
                }
            }
        }

        private static void ValidateBossDeclaration(
            WaveSetDto waveSet,
            HashSet<string> bossEnemyIds,
            string path,
            List<ValidationIssue> issues)
        {
            var waves = waveSet.Waves ?? Array.Empty<WaveDto>();
            var bossSpawns = waves
                .SelectMany((wave, waveIndex) => (wave?.Spawns ?? Array.Empty<WaveSpawnDto>())
                    .Select((spawn, spawnIndex) => new { WaveIndex = waveIndex, SpawnIndex = spawnIndex, Spawn = spawn }))
                .Where(entry => entry.Spawn != null && bossEnemyIds.Contains(entry.Spawn.EnemyId))
                .ToArray();

            if (!waveSet.HasBoss)
            {
                if (bossSpawns.Length > 0)
                {
                    issues.Add(Error($"{path}.HasBoss", "HasBoss must be enabled when the wave set contains a boss-tagged enemy."));
                }

                return;
            }

            if (bossSpawns.Length != 1)
            {
                issues.Add(Error($"{path}.HasBoss", "A boss wave set must contain exactly one boss spawn group."));
                return;
            }

            var bossSpawn = bossSpawns[0];
            var finalWaveIndex = waves.Length - 1;
            var finalSpawns = finalWaveIndex >= 0
                ? waves[finalWaveIndex]?.Spawns ?? Array.Empty<WaveSpawnDto>()
                : Array.Empty<WaveSpawnDto>();
            var finalSpawnIndex = finalSpawns.Length - 1;
            if (bossSpawn.WaveIndex != finalWaveIndex || bossSpawn.SpawnIndex != finalSpawnIndex)
            {
                issues.Add(Error($"{path}.Waves", "The boss must be the final spawn group of the final wave."));
            }

            if (bossSpawn.Spawn.Count != 1)
            {
                issues.Add(Error(
                    $"{path}.Waves[{bossSpawn.WaveIndex}].Spawns[{bossSpawn.SpawnIndex}].Count",
                    "A boss spawn group must have Count 1."));
            }
        }

        private void ValidateWaves(WaveDto[] waves, HashSet<string> enemyIds, string path, List<ValidationIssue> issues)
        {
            if (waves == null || waves.Length == 0)
            {
                issues.Add(Error(path, "At least one wave is required."));
                return;
            }

            if (waves.Length > _limits.MaximumWavesPerSet)
            {
                issues.Add(Error(path, $"Wave count cannot exceed {_limits.MaximumWavesPerSet}."));
            }

            var waveIds = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < waves.Length; i++)
            {
                var wave = waves[i];
                var wavePath = $"{path}[{i}]";
                if (wave == null)
                {
                    issues.Add(Error(wavePath, "Wave entry cannot be null."));
                    continue;
                }

                ValidateStableId(wave.Id, $"{wavePath}.Id", waveIds, issues);
                ValidateFloatRange(wave.StartDelaySeconds, 0f, 3600f, $"{wavePath}.StartDelaySeconds", issues);
                ValidateSpawns(wave.Spawns, enemyIds, $"{wavePath}.Spawns", issues);
            }
        }

        private void ValidateSpawns(WaveSpawnDto[] spawns, HashSet<string> enemyIds, string path, List<ValidationIssue> issues)
        {
            if (spawns == null || spawns.Length == 0)
            {
                issues.Add(Error(path, "At least one spawn group is required."));
                return;
            }

            if (spawns.Length > _limits.MaximumSpawnsPerWave)
            {
                issues.Add(Error(path, $"Spawn group count cannot exceed {_limits.MaximumSpawnsPerWave}."));
            }

            for (var i = 0; i < spawns.Length; i++)
            {
                var spawn = spawns[i];
                var spawnPath = $"{path}[{i}]";
                if (spawn == null)
                {
                    issues.Add(Error(spawnPath, "Spawn entry cannot be null."));
                    continue;
                }

                ValidateStableId(spawn.EnemyId, $"{spawnPath}.EnemyId", null, issues);
                if (!string.IsNullOrWhiteSpace(spawn.EnemyId) && !enemyIds.Contains(spawn.EnemyId))
                {
                    issues.Add(Error($"{spawnPath}.EnemyId", $"Unknown enemy id '{spawn.EnemyId}'."));
                }

                ValidateIntRange(spawn.Count, 1, _limits.MaximumSpawnCount, $"{spawnPath}.Count", issues);
                ValidateFloatRange(spawn.IntervalSeconds, 0.01f, 3600f, $"{spawnPath}.IntervalSeconds", issues);
            }
        }

        private void ValidateBattlefields(BattlefieldDto[] battlefields, bool required, List<ValidationIssue> issues)
        {
            if (battlefields == null || battlefields.Length == 0)
            {
                if (required)
                {
                    issues.Add(Error("$.Battlefields", "At least one battlefield is required for schema version 2."));
                }

                return;
            }

            if (battlefields.Length > _limits.MaximumBattlefields)
            {
                issues.Add(Error("$.Battlefields", $"Battlefield count cannot exceed {_limits.MaximumBattlefields}."));
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < battlefields.Length; i++)
            {
                var battlefield = battlefields[i];
                var path = $"$.Battlefields[{i}]";
                if (battlefield == null)
                {
                    issues.Add(Error(path, "Battlefield entry cannot be null."));
                    continue;
                }

                ValidateStableId(battlefield.Id, $"{path}.Id", ids, issues);
                ValidateStableId(battlefield.BackgroundId, $"{path}.BackgroundId", null, issues);
                ValidateBattlefieldPoints(
                    battlefield.PathPoints,
                    2,
                    _limits.MaximumPathPointsPerBattlefield,
                    $"{path}.PathPoints",
                    issues);
                ValidateBattlefieldPoints(
                    battlefield.BuildSocketPositions,
                    1,
                    _limits.MaximumBuildSocketsPerBattlefield,
                    $"{path}.BuildSocketPositions",
                    issues);
            }
        }

        private void ValidateBattlefieldPoints(
            BattlefieldPointDto[] points,
            int minimumCount,
            int maximumCount,
            string path,
            List<ValidationIssue> issues)
        {
            if (points == null || points.Length < minimumCount || points.Length > maximumCount)
            {
                issues.Add(Error(path, $"Point count must be between {minimumCount} and {maximumCount}."));
                return;
            }

            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                if (point == null)
                {
                    issues.Add(Error($"{path}[{i}]", "Point cannot be null."));
                    continue;
                }

                ValidateFloatRange(
                    point.X,
                    -_limits.MaximumBattlefieldCoordinateMagnitude,
                    _limits.MaximumBattlefieldCoordinateMagnitude,
                    $"{path}[{i}].X",
                    issues);
                ValidateFloatRange(
                    point.Y,
                    -_limits.MaximumBattlefieldCoordinateMagnitude,
                    _limits.MaximumBattlefieldCoordinateMagnitude,
                    $"{path}[{i}].Y",
                    issues);
            }
        }

        private void ValidateLevels(
            LevelDto[] levels,
            WaveSetDto[] waveSets,
            BattlefieldDto[] battlefields,
            bool battlefieldRequired,
            List<ValidationIssue> issues)
        {
            if (levels == null || levels.Length == 0)
            {
                issues.Add(Error("$.Levels", "At least one level is required."));
                return;
            }

            if (levels.Length > _limits.MaximumLevels)
            {
                issues.Add(Error("$.Levels", $"Level count cannot exceed {_limits.MaximumLevels}."));
            }

            var waveSetIds = BuildValidIdSet(waveSets);
            var battlefieldIds = BuildValidIdSet(battlefields);
            var battlefieldById = (battlefields ?? Array.Empty<BattlefieldDto>())
                .Where(battlefield => battlefield != null && StableId.IsValid(battlefield.Id))
                .GroupBy(battlefield => battlefield.Id)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            var levelIds = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < levels.Length; i++)
            {
                var level = levels[i];
                var path = $"$.Levels[{i}]";
                if (level == null)
                {
                    issues.Add(Error(path, "Level entry cannot be null."));
                    continue;
                }

                ValidateStableId(level.Id, $"{path}.Id", levelIds, issues);
                ValidateRequired(level.DisplayNameKey, $"{path}.DisplayNameKey", issues);
                ValidateEnum<GameMode>(level.Mode, $"{path}.Mode", issues);
                ValidateIntRange(level.StartingScrap, 0, _limits.MaximumCost, $"{path}.StartingScrap", issues);
                ValidateIntRange(level.BaseHealth, 1, (int)_limits.MaximumHealth, $"{path}.BaseHealth", issues);
                ValidateIntRange(level.BuildSocketCount, 1, 1000, $"{path}.BuildSocketCount", issues);
                if (battlefieldRequired || !string.IsNullOrWhiteSpace(level.BattlefieldId))
                {
                    ValidateStableId(level.BattlefieldId, $"{path}.BattlefieldId", null, issues);
                    if (!string.IsNullOrWhiteSpace(level.BattlefieldId) && !battlefieldIds.Contains(level.BattlefieldId))
                    {
                        issues.Add(Error($"{path}.BattlefieldId", $"Unknown battlefield id '{level.BattlefieldId}'."));
                    }
                    else if (battlefieldById.TryGetValue(level.BattlefieldId, out var battlefield) &&
                             battlefield.BuildSocketPositions != null &&
                             level.BuildSocketCount > battlefield.BuildSocketPositions.Length)
                    {
                        issues.Add(Error($"{path}.BuildSocketCount", "Build socket count exceeds the referenced battlefield layout."));
                    }
                }
                ValidateIntRange(level.RewardScrap, 0, _limits.MaximumCost, $"{path}.RewardScrap", issues);
                ValidateIntRange(level.RewardCoins, 0, _limits.MaximumCost, $"{path}.RewardCoins", issues);
                ValidateIntRange(level.RewardGems, 0, _limits.MaximumCost, $"{path}.RewardGems", issues);
                ValidateStableId(level.WaveSetId, $"{path}.WaveSetId", null, issues);
                if (!string.IsNullOrWhiteSpace(level.WaveSetId) && !waveSetIds.Contains(level.WaveSetId))
                {
                    issues.Add(Error($"{path}.WaveSetId", $"Unknown wave set id '{level.WaveSetId}'."));
                }
            }
        }

        private void ValidateArenaRules(ArenaRulesDto[] arenaRules, List<ValidationIssue> issues)
        {
            if (arenaRules == null)
            {
                return;
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < arenaRules.Length; i++)
            {
                var rules = arenaRules[i];
                var path = $"$.ArenaRules[{i}]";
                if (rules == null)
                {
                    issues.Add(Error(path, "Arena rules entry cannot be null."));
                    continue;
                }

                ValidateStableId(rules.Id, $"{path}.Id", ids, issues);
                ValidateIntRange(rules.BoardRows, 1, 10, $"{path}.BoardRows", issues);
                ValidateIntRange(rules.BoardColumns, 1, 10, $"{path}.BoardColumns", issues);
                ValidateIntRange(rules.DeckSize, 1, 10, $"{path}.DeckSize", issues);
                ValidateIntRange(rules.StartingMana, 0, _limits.MaximumCost, $"{path}.StartingMana", issues);
                ValidateIntRange(rules.BaseSummonCost, 1, _limits.MaximumCost, $"{path}.BaseSummonCost", issues);
                ValidateIntRange(rules.SummonCostStep, 0, _limits.MaximumCost, $"{path}.SummonCostStep", issues);
                ValidateIntRange(rules.MaximumMergeRank, 2, 10, $"{path}.MaximumMergeRank", issues);
                ValidateIntRange(rules.StrongholdLives, 1, 100, $"{path}.StrongholdLives", issues);
                ValidateFloatRange(rules.BossIntervalSeconds, 10f, 3600f, $"{path}.BossIntervalSeconds", issues);
                ValidateFloatRange(rules.MatchDurationSeconds, rules.BossIntervalSeconds, 7200f, $"{path}.MatchDurationSeconds", issues);
                ValidateOptionalFloatRange(rules.EnemySpeedMultiplier, 0.1f, 2f, $"{path}.EnemySpeedMultiplier", issues);
                ValidateOptionalFloatRange(rules.EnemyHealthMultiplier, 0.1f, 3f, $"{path}.EnemyHealthMultiplier", issues);
                ValidateOptionalFloatRange(rules.EnemyHealthPerWave, 0.01f, 1f, $"{path}.EnemyHealthPerWave", issues);
                ValidateOptionalFloatRange(rules.BossHealthMultiplier, 1f, 10f, $"{path}.BossHealthMultiplier", issues);
                ValidateOptionalFloatRange(rules.BaseSpawnIntervalSeconds, 0.25f, 10f, $"{path}.BaseSpawnIntervalSeconds", issues);
                ValidateOptionalFloatRange(rules.SpawnIntervalReductionPerWave, 0.01f, 1f, $"{path}.SpawnIntervalReductionPerWave", issues);
                ValidateOptionalFloatRange(rules.MinimumSpawnIntervalSeconds, 0.25f, 10f, $"{path}.MinimumSpawnIntervalSeconds", issues);
                ValidateOptionalIntRange(rules.WolfIntroductionWave, 2, 100, $"{path}.WolfIntroductionWave", issues);
                ValidateOptionalIntRange(rules.GoblinIntroductionWave, 2, 100, $"{path}.GoblinIntroductionWave", issues);
                ValidateOptionalIntRange(rules.OrcIntroductionWave, 2, 100, $"{path}.OrcIntroductionWave", issues);

                var baseSpawnInterval = rules.BaseSpawnIntervalSeconds > 0f ? rules.BaseSpawnIntervalSeconds : 2.4f;
                var minimumSpawnInterval = rules.MinimumSpawnIntervalSeconds > 0f ? rules.MinimumSpawnIntervalSeconds : 1f;
                if (minimumSpawnInterval > baseSpawnInterval)
                {
                    issues.Add(Error($"{path}.MinimumSpawnIntervalSeconds", "Minimum spawn interval cannot exceed the base spawn interval."));
                }

                var wolfWave = rules.WolfIntroductionWave > 0 ? rules.WolfIntroductionWave : 3;
                var goblinWave = rules.GoblinIntroductionWave > 0 ? rules.GoblinIntroductionWave : 5;
                var orcWave = rules.OrcIntroductionWave > 0 ? rules.OrcIntroductionWave : 7;
                if (wolfWave >= goblinWave || goblinWave >= orcWave)
                {
                    issues.Add(Error($"{path}.WolfIntroductionWave", "Enemy introduction waves must be strictly ordered: Wolf, Goblin, then Orc."));
                }

                ValidateIntRange(rules.VictoryRewardCoins, 0, _limits.MaximumCost, $"{path}.VictoryRewardCoins", issues);
                ValidateIntRange(rules.DefeatRewardCoins, 0, _limits.MaximumCost, $"{path}.DefeatRewardCoins", issues);
            }
        }

        private static HashSet<string> BuildValidIdSet(EnemyDto[] enemies)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (enemies == null)
            {
                return ids;
            }

            foreach (var enemy in enemies)
            {
                if (enemy != null && StableId.IsValid(enemy.Id))
                {
                    ids.Add(enemy.Id);
                }
            }

            return ids;
        }

        private static HashSet<string> BuildValidIdSet(WaveSetDto[] waveSets)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (waveSets == null)
            {
                return ids;
            }

            foreach (var waveSet in waveSets)
            {
                if (waveSet != null && StableId.IsValid(waveSet.Id))
                {
                    ids.Add(waveSet.Id);
                }
            }

            return ids;
        }

        private static HashSet<string> BuildValidIdSet(BattlefieldDto[] battlefields)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (battlefields == null)
            {
                return ids;
            }

            foreach (var battlefield in battlefields)
            {
                if (battlefield != null && StableId.IsValid(battlefield.Id))
                {
                    ids.Add(battlefield.Id);
                }
            }

            return ids;
        }

        private static HashSet<string> BuildValidIdSet(TowerDto[] towers)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (towers == null)
            {
                return ids;
            }

            foreach (var tower in towers)
            {
                if (tower != null && StableId.IsValid(tower.Id))
                {
                    ids.Add(tower.Id);
                }
            }

            return ids;
        }

        private static void ValidateStableId(string value, string path, HashSet<string> knownIds, List<ValidationIssue> issues)
        {
            if (!StableId.IsValid(value))
            {
                issues.Add(Error(path, "Stable ID must use lowercase letters, digits, underscores, or hyphens."));
                return;
            }

            if (knownIds != null && !knownIds.Add(value))
            {
                issues.Add(Error(path, $"Duplicate stable ID '{value}'."));
            }
        }

        private static void ValidateRequired(string value, string path, List<ValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                issues.Add(Error(path, "Value is required."));
            }
        }

        private static void ValidateEnum<TEnum>(string value, string path, List<ValidationIssue> issues)
            where TEnum : struct
        {
            if (string.IsNullOrWhiteSpace(value) || !Enum.TryParse<TEnum>(value, true, out _))
            {
                issues.Add(Error(path, $"Unknown {typeof(TEnum).Name} value '{value}'."));
            }
        }

        private static void ValidateIntRange(int value, int minimum, int maximum, string path, List<ValidationIssue> issues)
        {
            if (value < minimum || value > maximum)
            {
                issues.Add(Error(path, $"Value must be between {minimum} and {maximum}."));
            }
        }

        private static void ValidateOptionalIntRange(int value, int minimum, int maximum, string path, List<ValidationIssue> issues)
        {
            if (value != 0)
            {
                ValidateIntRange(value, minimum, maximum, path, issues);
            }
        }

        private static void ValidateOptionalFloatRange(float value, float minimum, float maximum, string path, List<ValidationIssue> issues)
        {
            if (Math.Abs(value) > float.Epsilon)
            {
                ValidateFloatRange(value, minimum, maximum, path, issues);
            }
        }

        private static void ValidateFloatRange(float value, float minimum, float maximum, string path, List<ValidationIssue> issues)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < minimum || value > maximum)
            {
                issues.Add(Error(path, $"Value must be between {minimum} and {maximum}."));
            }
        }

        private static ValidationIssue Error(string path, string message)
        {
            return new ValidationIssue(ValidationSeverity.Error, path, message);
        }
    }
}
