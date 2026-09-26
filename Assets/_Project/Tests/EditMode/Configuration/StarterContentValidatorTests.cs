using System;
using System.IO;
using System.Linq;
using ClubGamerZone.TowerDefense.Application.Configuration;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Infrastructure.Json;
using NUnit.Framework;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class StarterContentValidatorTests
    {
        [Test]
        public void StarterContentJson_ValidatesSuccessfully()
        {
            var content = LoadStarterContent();
            var validator = CreateValidator();

            var result = validator.Validate(content);

            Assert.That(result.IsValid, Is.True, string.Join("\n", result.Issues));
        }

        [Test]
        public void StarterContentJson_BuildsCatalog()
        {
            var content = LoadStarterContent();
            var builder = new StarterContentCatalogBuilder(CreateValidator());

            var result = builder.Build(content);

            Assert.That(result.IsSuccess, Is.True, string.Join("\n", result.Validation.Issues));
            Assert.That(result.Catalog.Towers.ContainsKey(new StableId("tower_laser_basic")), Is.True);
            Assert.That(result.Catalog.Towers[new StableId("tower_laser_basic")].Upgrades.Count, Is.EqualTo(2));
            Assert.That(result.Catalog.Towers[new StableId("tower_laser_basic")].Merges[0].ResultTowerId, Is.EqualTo(new StableId("tower_laser_overcharged")));
            Assert.That(result.Catalog.Enemies.ContainsKey(new StableId("enemy_scout")), Is.True);
            Assert.That(result.Catalog.Enemies[new StableId("enemy_swarm")].DisplayNameKey, Is.EqualTo("Rat"));
            Assert.That(result.Catalog.Enemies[new StableId("enemy_regenerator")].DisplayNameKey, Is.EqualTo("Wolf"));
            Assert.That(result.Catalog.Enemies[new StableId("enemy_scout")].DisplayNameKey, Is.EqualTo("Goblin"));
            Assert.That(result.Catalog.Enemies[new StableId("enemy_armored")].DisplayNameKey, Is.EqualTo("Orc"));
            Assert.That(result.Catalog.Enemies.ContainsKey(new StableId("enemy_boss_warden")), Is.True);
            Assert.That(result.Catalog.Levels.ContainsKey(new StableId("level_classic_001")), Is.True);
            Assert.That(result.Catalog.WaveSets[new StableId("waves_classic_005")].HasBoss, Is.True);
            Assert.That(result.Catalog.WaveSets[new StableId("waves_classic_010")].HasBoss, Is.True);
            Assert.That(result.Catalog.WaveSets[new StableId("waves_classic_001")].HasBoss, Is.False);
            Assert.That(result.Catalog.Battlefields.Count, Is.GreaterThanOrEqualTo(10));
            Assert.That(
                result.Catalog.Levels[new StableId("level_classic_001")].BattlefieldId,
                Is.EqualTo(new StableId("battlefield_level_001")));
            Assert.That(
                result.Catalog.Battlefields[new StableId("battlefield_level_010")].BackgroundId,
                Is.EqualTo("background_tempest_boss"));
            Assert.That(result.Catalog.Levels.ContainsKey(new StableId("level_ship_001")), Is.True);
            Assert.That(result.Catalog.ArenaRules.ContainsKey(new StableId("arena_standard")), Is.True);
            Assert.That(result.Catalog.ArenaRules[new StableId("arena_standard")].SocketCount, Is.EqualTo(15));
            Assert.That(result.Catalog.ArenaRules[new StableId("arena_standard")].EnemySpeedMultiplier, Is.EqualTo(0.5f));
            Assert.That(result.Catalog.ArenaRules[new StableId("arena_standard")].EnemyHealthMultiplier, Is.EqualTo(0.65f));
            Assert.That(result.Catalog.ArenaRules[new StableId("arena_standard")].WolfIntroductionWave, Is.EqualTo(3));
            Assert.That(result.Catalog.ArenaRules[new StableId("arena_standard")].GoblinIntroductionWave, Is.EqualTo(5));
            Assert.That(result.Catalog.ArenaRules[new StableId("arena_standard")].OrcIntroductionWave, Is.EqualTo(7));
            Assert.That(result.Catalog.Towers.ContainsKey(new StableId("hero_mage")), Is.True);
            Assert.That(result.Catalog.Towers.ContainsKey(new StableId("hero_warrior")), Is.True);
            Assert.That(result.Catalog.Towers.ContainsKey(new StableId("hero_paladin")), Is.True);
            Assert.That(result.Catalog.Towers.ContainsKey(new StableId("hero_archer")), Is.True);
            Assert.That(result.Catalog.Towers.ContainsKey(new StableId("hero_druid")), Is.True);
            Assert.That(result.Catalog.Towers.ContainsKey(new StableId("hero_sorcerer")), Is.True);
            Assert.That(result.Catalog.Towers[new StableId("hero_paladin")].UnlockCostCoins, Is.Zero);
            Assert.That(result.Catalog.Towers[new StableId("hero_druid")].UnlockCostCoins, Is.Zero);
            Assert.That(result.Catalog.Towers[new StableId("hero_sorcerer")].UnlockCostCoins, Is.EqualTo(750));
            Assert.That(result.Catalog.Towers[new StableId("hero_mage")].BuildCost, Is.EqualTo(35));
            Assert.That(result.Catalog.Towers[new StableId("hero_archer")].BuildCost, Is.EqualTo(45));
            Assert.That(result.Catalog.Towers[new StableId("hero_druid")].BuildCost, Is.EqualTo(55));
            Assert.That(result.Catalog.Towers[new StableId("hero_warrior")].BuildCost, Is.EqualTo(60));
            Assert.That(result.Catalog.Towers[new StableId("hero_paladin")].BuildCost, Is.EqualTo(75));
            Assert.That(result.Catalog.Towers[new StableId("hero_sorcerer")].BuildCost, Is.EqualTo(100));
            Assert.That(
                result.Catalog.Towers[new StableId("hero_sorcerer")].BuildCost,
                Is.GreaterThan(result.Catalog.Towers[new StableId("hero_mage")].BuildCost));
            foreach (var heroId in new[] { "hero_mage", "hero_warrior", "hero_paladin", "hero_archer", "hero_druid", "hero_sorcerer" })
            {
                var hero = result.Catalog.Towers[new StableId(heroId)];
                Assert.That(hero.MaxLevel, Is.EqualTo(7), heroId);
                Assert.That(hero.Upgrades.Count, Is.EqualTo(6), heroId);
                Assert.That(hero.Merges.Count, Is.EqualTo(6), heroId);
                Assert.That(hero.Upgrades[0].FromLevel, Is.EqualTo(1), heroId);
                Assert.That(hero.Upgrades[5].ToLevel, Is.EqualTo(7), heroId);
                Assert.That(hero.Merges[0].ResultTowerId, Is.EqualTo(new StableId(heroId)), heroId);
                Assert.That(hero.LevelDisplayNames.Count, Is.EqualTo(7), heroId);
                Assert.That(hero.GetDisplayName(2), Is.EqualTo($"{hero.DisplayNameKey} 2"), heroId);
            }
        }

        [Test]
        public void Validate_InvalidStableId_ReturnsCreatorFacingError()
        {
            var content = LoadStarterContent();
            content.Towers[0].Id = "Laser Basic";
            var validator = CreateValidator();

            var result = validator.Validate(content);

            Assert.That(result.IsValid, Is.False);
            Assert.That(HasIssueAt(result, "$.Towers[0].Id"), Is.True);
        }

        [Test]
        public void Validate_InvalidArenaEnemyBalance_ReturnsCreatorFacingErrors()
        {
            var content = LoadStarterContent();
            content.ArenaRules[0].EnemySpeedMultiplier = 3f;
            content.ArenaRules[0].WolfIntroductionWave = 6;
            content.ArenaRules[0].GoblinIntroductionWave = 5;
            var validator = CreateValidator();

            var result = validator.Validate(content);

            Assert.That(result.IsValid, Is.False);
            Assert.That(HasIssueAt(result, "$.ArenaRules[0].EnemySpeedMultiplier"), Is.True);
            Assert.That(HasIssueAt(result, "$.ArenaRules[0].WolfIntroductionWave"), Is.True);
        }

        [Test]
        public void Validate_UnknownEnemyReference_ReturnsCreatorFacingError()
        {
            var content = LoadStarterContent();
            content.WaveSets[0].Waves[0].Spawns[0].EnemyId = "enemy_missing";
            var validator = CreateValidator();

            var result = validator.Validate(content);

            Assert.That(result.IsValid, Is.False);
            Assert.That(HasIssueAt(result, "$.WaveSets[0].Waves[0].Spawns[0].EnemyId"), Is.True);
        }

        [Test]
        public void Validate_UnknownTowerMergeReference_ReturnsCreatorFacingError()
        {
            var content = LoadStarterContent();
            content.Towers[0].Merges[0].ResultTowerId = "tower_missing";
            var validator = CreateValidator();

            var result = validator.Validate(content);

            Assert.That(result.IsValid, Is.False);
            Assert.That(HasIssueAt(result, "$.Towers[0].Merges[0].ResultTowerId"), Is.True);
        }

        [Test]
        public void CampaignBossWaveSets_AreExplicitAndEndWithBoss()
        {
            var content = LoadStarterContent();
            var expectedBossLevels = new[]
            {
                new { LevelId = "level_classic_005", WaveSetId = "waves_classic_005", BossId = "enemy_boss" },
                new { LevelId = "level_classic_010", WaveSetId = "waves_classic_010", BossId = "enemy_boss_warden" }
            };

            foreach (var expected in expectedBossLevels)
            {
                var level = content.Levels.Single(candidate => candidate.Id == expected.LevelId);
                Assert.That(level.WaveSetId, Is.EqualTo(expected.WaveSetId));

                var waveSet = content.WaveSets.Single(candidate => candidate.Id == expected.WaveSetId);
                Assert.That(waveSet.HasBoss, Is.True);
                var finalSpawn = waveSet.Waves.Last().Spawns.Last();
                Assert.That(finalSpawn.EnemyId, Is.EqualTo(expected.BossId));
                Assert.That(finalSpawn.Count, Is.EqualTo(1));
                Assert.That(
                    waveSet.Waves.SelectMany(wave => wave.Spawns).Count(spawn => spawn.EnemyId == expected.BossId),
                    Is.EqualTo(1));
            }

            var bossWaveSetIds = expectedBossLevels.Select(expected => expected.WaveSetId).ToArray();
            Assert.That(
                content.Levels
                    .Where(level => level.Id != "level_classic_005" && level.Id != "level_classic_010")
                    .All(level => !bossWaveSetIds.Contains(level.WaveSetId)),
                Is.True);
            Assert.That(
                content.WaveSets.Where(waveSet => !bossWaveSetIds.Contains(waveSet.Id)).All(waveSet => !waveSet.HasBoss),
                Is.True);
        }

        [Test]
        public void Validate_HasBossWithoutFinalBoss_ReturnsCreatorFacingError()
        {
            var content = LoadStarterContent();
            var waveSet = content.WaveSets.Single(candidate => candidate.Id == "waves_classic_001");
            waveSet.HasBoss = true;

            var result = CreateValidator().Validate(content);

            Assert.That(result.IsValid, Is.False);
            Assert.That(HasIssueAt(result, "$.WaveSets[0].HasBoss"), Is.True);
        }

        [Test]
        public void Validate_BossSpawnWithoutHasBoss_ReturnsCreatorFacingError()
        {
            var content = LoadStarterContent();
            var bossWaveSetIndex = Array.FindIndex(content.WaveSets, candidate => candidate.Id == "waves_classic_005");
            content.WaveSets[bossWaveSetIndex].HasBoss = false;

            var result = CreateValidator().Validate(content);

            Assert.That(result.IsValid, Is.False);
            Assert.That(HasIssueAt(result, $"$.WaveSets[{bossWaveSetIndex}].HasBoss"), Is.True);
        }

        [Test]
        public void Validate_UnknownBattlefieldReference_ReturnsCreatorFacingError()
        {
            var content = LoadStarterContent();
            content.Levels[0].BattlefieldId = "battlefield_missing";

            var result = CreateValidator().Validate(content);

            Assert.That(result.IsValid, Is.False);
            Assert.That(HasIssueAt(result, "$.Levels[0].BattlefieldId"), Is.True);
        }

        private static StarterContentValidator CreateValidator()
        {
            return new StarterContentValidator(new ContentValidationLimits());
        }

        private static bool HasIssueAt(ValidationResult result, string path)
        {
            foreach (var issue in result.Issues)
            {
                if (issue.Path == path)
                {
                    return true;
                }
            }

            return false;
        }

        private static ClubGamerZone.TowerDefense.Application.Configuration.Dtos.StarterContentDto LoadStarterContent()
        {
            var path = Path.Combine(UnityEngine.Application.dataPath, "_Project/Data/Json/Defaults/starter_content.json");
            var json = File.ReadAllText(path);
            var parser = new UnityContentJsonParser();
            return parser.Parse(json);
        }
    }
}
