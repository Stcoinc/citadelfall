using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
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
                waveSet.Configure("waves_rat_001", new[] { wave });

                var dto = waveSet.ToDto();

                Assert.That(dto.Id, Is.EqualTo("waves_rat_001"));
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
            }
            finally
            {
                Object.DestroyImmediate(layout);
            }
        }
    }
}
