using System;
using ClubGamerZone.TowerDefense.Application.Gameplay;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class ArenaFoundationTests
    {
        [Test]
        public void Deck_RequiresFiveUniqueHeroes()
        {
            var deck = new ArenaDeck(new[]
            {
                new StableId("tower_mage"),
                new StableId("tower_warrior"),
                new StableId("tower_paladin"),
                new StableId("tower_archer"),
                new StableId("tower_druid")
            }, 5);

            Assert.That(deck.HeroIds.Count, Is.EqualTo(5));
            Assert.Throws<ArgumentException>(() => new ArenaDeck(new[]
            {
                new StableId("tower_mage"),
                new StableId("tower_mage"),
                new StableId("tower_paladin"),
                new StableId("tower_archer"),
                new StableId("tower_druid")
            }, 5));
        }

        [Test]
        public void SummonSequence_IsDeterministicForSharedSeed()
        {
            var first = new ArenaSummonSequence(12345);
            var second = new ArenaSummonSequence(12345);

            for (var i = 0; i < 20; i++)
            {
                Assert.That(first.NextDeckIndex(5), Is.EqualTo(second.NextDeckIndex(5)));
            }
        }

        [Test]
        public void SeedStreams_KeepEnemyPressureIndependentFromPlayerSummons()
        {
            const uint seed = 20260805;
            var enemyWithoutSummons = new ArenaSummonSequence(ArenaSeedStreams.Enemy(seed));
            var enemyAfterManySummons = new ArenaSummonSequence(ArenaSeedStreams.Enemy(seed));
            var heroes = new ArenaSummonSequence(ArenaSeedStreams.Hero(seed));
            var sockets = new ArenaSummonSequence(ArenaSeedStreams.Socket(seed));

            for (var summon = 0; summon < 25; summon++)
            {
                heroes.NextDeckIndex(5);
                sockets.NextDeckIndex(15);
            }

            for (var waveSpawn = 0; waveSpawn < 40; waveSpawn++)
            {
                Assert.That(
                    enemyAfterManySummons.NextDeckIndex(4),
                    Is.EqualTo(enemyWithoutSummons.NextDeckIndex(4)));
            }
        }

        [Test]
        public void SeedStreams_AreDistinctForOneMatchSeed()
        {
            const uint seed = 77;

            Assert.That(ArenaSeedStreams.Hero(seed), Is.Not.EqualTo(ArenaSeedStreams.Socket(seed)));
            Assert.That(ArenaSeedStreams.Hero(seed), Is.Not.EqualTo(ArenaSeedStreams.Enemy(seed)));
            Assert.That(ArenaSeedStreams.Socket(seed), Is.Not.EqualTo(ArenaSeedStreams.Enemy(seed)));
        }

        [Test]
        public void HeroCombatPalette_GivesEveryStartingHeroAReadableIdentity()
        {
            var types = new[] { "arcane", "steel", "holy", "piercing", "nature", "shadow" };
            var colors = new System.Collections.Generic.HashSet<Color>();

            foreach (var type in types)
            {
                colors.Add(HeroCombatPalette.GetColor(type));
                Assert.That(HeroCombatPalette.GetProjectileScale(type), Is.GreaterThan(0f));
            }

            Assert.That(colors.Count, Is.EqualTo(types.Length));
        }

        [Test]
        public void SummonCost_IncreasesAfterEachSummon()
        {
            Assert.That(ArenaRules.GetSummonCost(10, 10, 0), Is.EqualTo(10));
            Assert.That(ArenaRules.GetSummonCost(10, 10, 3), Is.EqualTo(40));
        }

        [Test]
        public void EnemyProgression_IntroducesOneFamilyAtATime()
        {
            Assert.That(ArenaEnemyBalance.GetUnlockedEnemyCount(1, 4, 3, 5, 7), Is.EqualTo(1));
            Assert.That(ArenaEnemyBalance.GetUnlockedEnemyCount(2, 4, 3, 5, 7), Is.EqualTo(1));
            Assert.That(ArenaEnemyBalance.GetUnlockedEnemyCount(3, 4, 3, 5, 7), Is.EqualTo(2));
            Assert.That(ArenaEnemyBalance.GetUnlockedEnemyCount(5, 4, 3, 5, 7), Is.EqualTo(3));
            Assert.That(ArenaEnemyBalance.GetUnlockedEnemyCount(7, 4, 3, 5, 7), Is.EqualTo(4));
        }

        [Test]
        public void EarlyArenaBalance_HalvesSpeedAndSoftensHealthGrowth()
        {
            Assert.That(ArenaEnemyBalance.GetScaledSpeed(4.2f, 0.5f), Is.EqualTo(2.1f).Within(0.001f));
            Assert.That(
                ArenaEnemyBalance.GetScaledHealth(32f, 1, 0.65f, 0.08f, 1f),
                Is.EqualTo(20.8f).Within(0.001f));
            Assert.That(
                ArenaEnemyBalance.GetScaledHealth(32f, 2, 0.65f, 0.08f, 1f),
                Is.EqualTo(22.464f).Within(0.001f));
        }

        [Test]
        public void SpawnCadence_StartsGentleAndNeverExceedsConfiguredPressure()
        {
            Assert.That(ArenaEnemyBalance.GetSpawnInterval(1, 2.4f, 0.05f, 1f), Is.EqualTo(2.4f));
            Assert.That(ArenaEnemyBalance.GetSpawnInterval(2, 2.4f, 0.05f, 1f), Is.EqualTo(2.35f));
            Assert.That(ArenaEnemyBalance.GetSpawnInterval(100, 2.4f, 0.05f, 1f), Is.EqualTo(1f));
        }

        [Test]
        public void Merge_RequiresMatchingHeroAndRankBelowMaximum()
        {
            var mage = new StableId("tower_mage");

            Assert.That(ArenaRules.CanMerge(mage, 2, mage, 2, 7), Is.True);
            Assert.That(ArenaRules.CanMerge(mage, 2, new StableId("tower_druid"), 2, 7), Is.False);
            Assert.That(ArenaRules.CanMerge(mage, 2, mage, 3, 7), Is.False);
            Assert.That(ArenaRules.CanMerge(mage, 7, mage, 7, 7), Is.False);
        }

        [Test]
        public void SaveSlot_PersistsOnlyACompleteUniqueArenaDeck()
        {
            var slot = new SaveSlotData();
            var deck = new[]
            {
                "tower_mage", "tower_warrior", "tower_paladin", "tower_archer", "tower_druid"
            };

            slot.SetArenaDeck(deck);
            slot.ArenaTutorialCompleted = true;

            Assert.That(slot.SelectedArenaHeroIds, Is.EqualTo(deck));
            Assert.That(slot.ArenaTutorialCompleted, Is.True);
            Assert.Throws<ArgumentException>(() => slot.SetArenaDeck(new[]
            {
                "tower_mage", "tower_mage", "tower_paladin", "tower_archer", "tower_druid"
            }));
        }

        [Test]
        public void AppSession_ArenaFlowRequiresAProfileBeforeMatchLaunch()
        {
            AppRuntimeSession.SetArenaMode();

            Assert.That(AppRuntimeSession.IsArenaMode, Is.True);
            Assert.That(AppRuntimeSession.SelectedSlot, Is.Null);
            Assert.That(AppRuntimeSession.SelectedLevelId, Is.Empty);

            var slot = new SaveSlotData { SlotIndex = 2 };
            AppRuntimeSession.SetSelectedSlot(slot);

            Assert.That(AppRuntimeSession.SelectedSlot, Is.SameAs(slot));
            Assert.That(AppRuntimeSession.IsArenaMode, Is.True);
        }

        [Test]
        public void AppSession_AdventureFlowClearsArenaIntent()
        {
            AppRuntimeSession.SetArenaMode();
            AppRuntimeSession.SetSlotMode(true);

            Assert.That(AppRuntimeSession.PlayMode, Is.EqualTo(AppPlayMode.Adventure));
            Assert.That(AppRuntimeSession.SlotModeContinue, Is.True);
            Assert.That(AppRuntimeSession.SelectedSlot, Is.Null);
        }

        [Test]
        public void AppSession_EndlessFlowUsesItsOwnRoutingIntent()
        {
            AppRuntimeSession.SetEndlessMode();

            Assert.That(AppRuntimeSession.IsEndlessMode, Is.True);
            Assert.That(AppRuntimeSession.IsArenaMode, Is.False);
            Assert.That(AppRuntimeSession.SelectedSlot, Is.Null);
            Assert.That(AppSceneNames.Endless, Is.EqualTo("Endless"));
        }
    }
}
