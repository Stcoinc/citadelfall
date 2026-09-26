using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using ClubGamerZone.TowerDefense.Application.Configuration;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class GameplayContentAuthoringTests
    {
        [Test]
        public void EnemyAndWaveAssets_ExportLinkedStableIds()
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinitionAsset>();
            var waveSet = ScriptableObject.CreateInstance<WaveSetDefinitionAsset>();
            try
            {
                enemy.Configure(new EnemyDto
                {
                    Id = "enemy_rat",
                    DisplayNameKey = "Rat",
                    MaxHealth = 30f,
                    MovementSpeed = 4f,
                    ContactDamage = 1,
                    RewardScrap = 4,
                    ThreatValue = 1,
                    EnemyTag = "swarm",
                    WeakToDamageType = "frost",
                    PrefabId = "prefab_enemy_rat"
                });
                var spawn = new WaveSpawnAuthoringData();
                spawn.Configure(enemy, 12, 0.4f);
                var wave = new WaveAuthoringData();
                wave.Configure("wave_rat_001", 1.5f, new[] { spawn });
                waveSet.Configure("waves_rat_001", false, new[] { wave });

                var dto = waveSet.ToDto();

                Assert.That(dto.Id, Is.EqualTo("waves_rat_001"));
                Assert.That(dto.HasBoss, Is.False);
                Assert.That(dto.Waves[0].Spawns[0].EnemyId, Is.EqualTo("enemy_rat"));
                Assert.That(dto.Waves[0].Spawns[0].Count, Is.EqualTo(12));
            }
            finally
            {
                Object.DestroyImmediate(waveSet);
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void BattlefieldLayout_CopiesScenePositionsIntoTheAsset()
        {
            var layout = ScriptableObject.CreateInstance<BattlefieldLayoutAsset>();
            var sourcePath = new[] { new Vector2(-4f, 2f), new Vector2(5f, -2f) };
            try
            {
                layout.Configure("forest_path", sourcePath, new[] { new Vector2(0f, 1f) });
                sourcePath[0] = Vector2.zero;

                Assert.That(layout.Id, Is.EqualTo("forest_path"));
                Assert.That(layout.PathPointCount, Is.EqualTo(2));
                Assert.That(layout.GetPathPoint(0), Is.EqualTo(new Vector2(-4f, 2f)));
                Assert.That(layout.BuildSocketCount, Is.EqualTo(1));
                Assert.That(layout.ToDto().BackgroundId, Is.EqualTo("background_forest_01"));
            }
            finally
            {
                Object.DestroyImmediate(layout);
            }
        }

        [Test]
        public void StarterContentCatalogBuilder_PreservesHeroRotationPreference()
        {
            var content = new StarterContentDto
            {
                SchemaVersion = 1,
                ContentVersion = "test",
                Towers = new[]
                {
                    new TowerDto
                    {
                        Id = "hero_warrior",
                        DisplayNameKey = "Warrior",
                        DescriptionKey = "Frontline defender",
                        BehaviorId = "behavior_steel_hero",
                        TargetingMode = "Closest",
                        BuildCost = 60,
                        PowerCost = 0,
                        Damage = 42f,
                        Range = 2.8f,
                        AttackIntervalSeconds = 0.65f,
                        MaxLevel = 7,
                        PowerRating = 78,
                        PreferredEnemyTag = "all",
                        DamageType = "steel",
                        HasSplash = false,
                        ShouldRotate = false,
                        PrefabId = "prefab_hero_defender",
                        IconId = "icon_hero_warrior",
                        UnlockCostCoins = 0
                    }
                },
                Enemies = new[]
                {
                    new EnemyDto
                    {
                        Id = "enemy_rat",
                        DisplayNameKey = "Rat",
                        MaxHealth = 12f,
                        MovementSpeed = 1f,
                        ContactDamage = 1,
                        RewardScrap = 1,
                        ThreatValue = 1,
                        EnemyTag = "swarm",
                        WeakToDamageType = "steel",
                        PrefabId = "prefab_enemy_rat"
                    }
                },
                WaveSets = new[]
                {
                    new WaveSetDto
                    {
                        Id = "waves_test",
                        Waves = new[]
                        {
                            new WaveDto
                            {
                                Id = "wave_test",
                                StartDelaySeconds = 0f,
                                Spawns = new[]
                                {
                                    new WaveSpawnDto
                                    {
                                        EnemyId = "enemy_rat",
                                        Count = 1,
                                        IntervalSeconds = 1f
                                    }
                                }
                            }
                        }
                    }
                },
                Levels = new[]
                {
                    new LevelDto
                    {
                        Id = "level_test",
                        DisplayNameKey = "Test",
                        Mode = "ClassicPathDefense",
                        StartingScrap = 100,
                        BaseHealth = 20,
                        BuildSocketCount = 1,
                        BattlefieldId = string.Empty,
                        WaveSetId = "waves_test",
                        RewardScrap = 1,
                        RewardCoins = 1,
                        RewardGems = 0,
                        RewardItemId = string.Empty
                    }
                }
            };
            var builder = new StarterContentCatalogBuilder(new StarterContentValidator(new ContentValidationLimits()));

            var result = builder.Build(content);

            Assert.That(result.Validation.IsValid, Is.True);
            Assert.That(result.Catalog.Towers[new StableId("hero_warrior")].ShouldRotate, Is.False);
        }
    }
}
