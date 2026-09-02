using ClubGamerZone.TowerDefense.Application.Gameplay;
using NUnit.Framework;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class EndlessModeRulesTests
    {
        [TestCase(0, 1, 1)]
        [TestCase(4, 1, 5)]
        [TestCase(5, 2, 1)]
        [TestCase(14, 3, 5)]
        public void FiveCompletedWavesAdvanceToTheNextRound(int completedWaves, int round, int wave)
        {
            Assert.That(EndlessModeRules.GetRound(completedWaves), Is.EqualTo(round));
            Assert.That(EndlessModeRules.GetWaveInRound(completedWaves), Is.EqualTo(wave));
        }

        [Test]
        public void PressureGrowsPredictablyWithoutRunawaySpeed()
        {
            Assert.That(EndlessModeRules.GetHealthMultiplier(2), Is.EqualTo(1.12f).Within(0.001f));
            Assert.That(EndlessModeRules.GetDamageMultiplier(2), Is.EqualTo(1.08f).Within(0.001f));
            Assert.That(EndlessModeRules.GetSpawnCount(5, 3), Is.EqualTo(7));
            Assert.That(EndlessModeRules.GetSpeedMultiplier(100), Is.EqualTo(1.5f));
            Assert.That(EndlessModeRules.GetSpawnInterval(1f, 100), Is.GreaterThanOrEqualTo(0.25f));
        }

        [Test]
        public void ScoresRewardThreatAndCompletedRounds()
        {
            Assert.That(EndlessModeRules.GetEnemyScore(1, 1), Is.EqualTo(15));
            Assert.That(EndlessModeRules.GetEnemyScore(4, 3), Is.EqualTo(34));
            Assert.That(EndlessModeRules.GetRoundCompletionBonus(3), Is.EqualTo(300));
        }
    }
}
