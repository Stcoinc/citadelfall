using ClubGamerZone.TowerDefense.Application.Authentication;
using NUnit.Framework;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class PlayerProgressionTests
    {
        [Test]
        public void RegisterEnemyDefeat_AtOneThousand_UnlocksFirstCardTier()
        {
            var progression = new PlayerProgression(
                0,
                null,
                new[] { new EnemyDefeatProgress("enemy_rat", 999) });

            progression.RegisterEnemyDefeat("enemy_rat");

            Assert.That(progression.GetEnemyDefeats("enemy_rat"), Is.EqualTo(1000));
            Assert.That(progression.GetUnlockedCardTier("enemy_rat"), Is.EqualTo(1));
            Assert.That(progression.UnlockedCardIds, Does.Contain("enemy_rat_card_1"));
        }

        [Test]
        public void RegisterEnemyDefeat_AtTwoThousand_KeepsBothCardTiers()
        {
            var progression = new PlayerProgression(
                0,
                null,
                new[] { new EnemyDefeatProgress("enemy_orc", 1999) },
                new[] { "enemy_orc_card_1" });

            progression.RegisterEnemyDefeat("enemy_orc");

            Assert.That(progression.GetUnlockedCardTier("enemy_orc"), Is.EqualTo(2));
            Assert.That(progression.UnlockedCardIds, Is.EquivalentTo(new[] { "enemy_orc_card_1", "enemy_orc_card_2" }));
        }

        [Test]
        public void Experience_UsesAccountLevelCurve()
        {
            var progression = new PlayerProgression(1990);

            progression.RegisterEnemyDefeat("enemy_wolf");

            Assert.That(progression.Experience, Is.EqualTo(2000));
            Assert.That(progression.Level, Is.EqualTo(3));
        }

        [Test]
        public void UnlockTower_IsAccountWideAndIdempotent()
        {
            var progression = new PlayerProgression();

            progression.UnlockTower("tower_archer");
            progression.UnlockTower("tower_archer");

            Assert.That(progression.HasUnlockedTower("tower_archer"), Is.True);
            Assert.That(progression.UnlockedTowerIds, Is.EqualTo(new[] { "tower_archer" }));
        }

        [Test]
        public void SetCurrencies_PersistsAValidZeroBalance()
        {
            var progression = new PlayerProgression();

            progression.SetCurrencies(0, 0);

            Assert.That(progression.Scrap, Is.Zero);
            Assert.That(progression.Coins, Is.Zero);
            Assert.That(progression.CurrenciesInitialized, Is.True);
        }

        [Test]
        public void EndlessPersonalBest_OnlyMovesForward()
        {
            var progression = new PlayerProgression();

            Assert.That(progression.RecordEndlessScore(1250, 4), Is.True);
            Assert.That(progression.RecordEndlessScore(900, 3), Is.False);

            Assert.That(progression.BestEndlessScore, Is.EqualTo(1250));
            Assert.That(progression.HighestEndlessRound, Is.EqualTo(4));
        }
    }
}
