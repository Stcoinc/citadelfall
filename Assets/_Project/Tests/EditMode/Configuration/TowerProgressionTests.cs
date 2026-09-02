using ClubGamerZone.TowerDefense.Application.Gameplay;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;
using NUnit.Framework;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class TowerProgressionTests
    {
        [Test]
        public void FindUpgrade_ForLevelOneLaser_ReturnsScrapUpgrade()
        {
            var tower = new TowerDefinition(
                new StableId("tower_laser_basic"),
                "tower_laser_basic_name",
                "tower_laser_basic_description",
                "behavior_laser_single_target",
                TargetingMode.First,
                50,
                2,
                12f,
                5.5f,
                0.45f,
                3,
                42,
                "scout",
                "laser",
                false,
                "prefab_tower_laser_basic",
                "icon_tower_laser_basic",
                0,
                new[]
                {
                    new TowerUpgradeDefinition(1, 2, TowerUpgradeCurrency.Scrap, 90, 18f, 5.8f, 0.4f, 64)
                },
                new TowerMergeDefinition[0]);
            var state = new TowerInstanceState(new StableId("tower_laser_basic"), 1);
            var service = new TowerProgressionService();

            var upgrade = service.FindUpgrade(tower, state, TowerUpgradeCurrency.Scrap);

            Assert.That(upgrade, Is.Not.Null);
            Assert.That(upgrade.ToLevel, Is.EqualTo(2));
        }

        [Test]
        public void FindMerge_WhenTwoSameLevelTowers_ReturnsSpecialTower()
        {
            var tower = new TowerDefinition(
                new StableId("tower_laser_basic"),
                "tower_laser_basic_name",
                "tower_laser_basic_description",
                "behavior_laser_single_target",
                TargetingMode.First,
                50,
                2,
                12f,
                5.5f,
                0.45f,
                3,
                42,
                "scout",
                "laser",
                false,
                "prefab_tower_laser_basic",
                "icon_tower_laser_basic",
                0,
                new TowerUpgradeDefinition[0],
                new[]
                {
                    new TowerMergeDefinition(1, new StableId("tower_laser_overcharged"), TowerUpgradeCurrency.Coins, 75)
                });
            var service = new TowerProgressionService();

            var merge = service.FindMerge(
                tower,
                new TowerInstanceState(new StableId("tower_laser_basic"), 1),
                new TowerInstanceState(new StableId("tower_laser_basic"), 1));

            Assert.That(merge, Is.Not.Null);
            Assert.That(merge.ResultTowerId, Is.EqualTo(new StableId("tower_laser_overcharged")));
            Assert.That(merge.Currency, Is.EqualTo(TowerUpgradeCurrency.Coins));
            Assert.That(merge.Cost, Is.EqualTo(75));
        }

        [Test]
        public void BuildStats_ForUpgradedTower_UsesUpgradeStats()
        {
            var tower = new TowerDefinition(
                new StableId("tower_laser_basic"),
                "tower_laser_basic_name",
                "tower_laser_basic_description",
                "behavior_laser_single_target",
                TargetingMode.First,
                50,
                2,
                12f,
                5.5f,
                0.45f,
                3,
                42,
                "scout",
                "laser",
                false,
                "prefab_tower_laser_basic",
                "icon_tower_laser_basic",
                0,
                new[]
                {
                    new TowerUpgradeDefinition(1, 2, TowerUpgradeCurrency.Scrap, 90, 18f, 5.8f, 0.4f, 64)
                },
                new TowerMergeDefinition[0]);
            var presenter = new TowerStatsPresenter();

            var stats = presenter.BuildStats(tower, new TowerInstanceState(new StableId("tower_laser_basic"), 2));

            Assert.That(stats.Damage, Is.EqualTo(18f));
            Assert.That(stats.PowerRating, Is.EqualTo(64));
            Assert.That(stats.PreferredEnemyTag, Is.EqualTo("scout"));
        }

        [Test]
        public void ApplyMerge_WhenRecipeReturnsSameHero_AdvancesOneLevel()
        {
            var heroId = new StableId("hero_warrior");
            var state = new TowerInstanceState(heroId, 1);
            var merge = new TowerMergeDefinition(1, heroId, TowerUpgradeCurrency.Coins, 30);

            var merged = state.ApplyMerge(merge);

            Assert.That(merged.TowerId, Is.EqualTo(heroId));
            Assert.That(merged.Level, Is.EqualTo(2));
        }

        [Test]
        public void BuildStats_UsesEditableNameForMergedLevel()
        {
            var tower = new TowerDefinition(
                new StableId("hero_sorcerer"),
                "Sorcerer",
                "description",
                "behavior_sorcerer",
                TargetingMode.First,
                50,
                0,
                20f,
                6f,
                1f,
                3,
                50,
                "all",
                "shadow",
                false,
                "prefab_hero",
                "icon_sorcerer",
                0,
                new TowerUpgradeDefinition[0],
                new TowerMergeDefinition[0],
                new[] { "Sorcerer", "Sorcerer 2", "Elder Sorcerer" });

            var stats = new TowerStatsPresenter().BuildStats(
                tower,
                new TowerInstanceState(new StableId("hero_sorcerer"), 2));

            Assert.That(stats.DisplayNameKey, Is.EqualTo("Sorcerer 2"));
            Assert.That(tower.GetDisplayName(3), Is.EqualTo("Elder Sorcerer"));
        }
    }
}
