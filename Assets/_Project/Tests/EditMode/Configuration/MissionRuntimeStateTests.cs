using ClubGamerZone.TowerDefense.Application.Gameplay;
using NUnit.Framework;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class MissionRuntimeStateTests
    {
        [Test]
        public void TrySpendScrap_WhenEnoughScrap_DeductsCost()
        {
            var state = new MissionRuntimeState(100, 10, 1);

            var spent = state.TrySpendScrap(60);

            Assert.That(spent, Is.True);
            Assert.That(state.Scrap, Is.EqualTo(40));
        }

        [Test]
        public void TrySpendScrap_WhenNotEnoughScrap_ReturnsFalse()
        {
            var state = new MissionRuntimeState(30, 10, 1);

            var spent = state.TrySpendScrap(60);

            Assert.That(spent, Is.False);
            Assert.That(state.Scrap, Is.EqualTo(30));
        }

        [Test]
        public void DamageBase_WhenHealthReachesZero_SetsDefeat()
        {
            var state = new MissionRuntimeState(100, 10, 1);

            state.DamageBase(10);

            Assert.That(state.BaseHealth, Is.EqualTo(0));
            Assert.That(state.Outcome, Is.EqualTo(MissionOutcome.Defeat));
        }

        [Test]
        public void DamageBase_AfterDefeat_DoesNotLeaveDefeatState()
        {
            var state = new MissionRuntimeState(100, 10, 1);

            state.DamageBase(10);
            state.DamageBase(10);

            Assert.That(state.BaseHealth, Is.EqualTo(0));
            Assert.That(state.Outcome, Is.EqualTo(MissionOutcome.Defeat));
        }

        [Test]
        public void CompleteWave_WithNoActiveEnemies_SetsVictory()
        {
            var state = new MissionRuntimeState(100, 10, 1);

            state.CompleteWave();

            Assert.That(state.Outcome, Is.EqualTo(MissionOutcome.Victory));
        }

        [Test]
        public void CompleteWave_WithActiveEnemies_WaitsForEnemiesToResolve()
        {
            var state = new MissionRuntimeState(100, 10, 1);

            state.RegisterEnemySpawned();
            state.CompleteWave();
            state.RegisterEnemyResolved();

            Assert.That(state.Outcome, Is.EqualTo(MissionOutcome.Victory));
        }

        [Test]
        public void CurrencyChanges_ReportTheLatestScrapAndCoinBalances()
        {
            var state = new MissionRuntimeState(100, 10, 1, 25);
            var notificationCount = 0;
            var reportedScrap = -1;
            var reportedCoins = -1;
            state.CurrencyChanged += (scrap, coins) =>
            {
                notificationCount++;
                reportedScrap = scrap;
                reportedCoins = coins;
            };

            Assert.That(state.TrySpendScrap(40), Is.True);
            Assert.That(state.TrySpendCoins(5), Is.True);
            state.AddScrap(10);
            Assert.That(state.TrySpendCoins(100), Is.False);

            Assert.That(notificationCount, Is.EqualTo(3));
            Assert.That(reportedScrap, Is.EqualTo(70));
            Assert.That(reportedCoins, Is.EqualTo(20));
        }

        [Test]
        public void EmergencyFallback_IsFreeOnlyForTheFirstWarrior()
        {
            Assert.That(
                EmergencyBuildRules.IsFreeFallbackPlacement("hero_warrior", "hero_warrior", false),
                Is.True);
            Assert.That(
                EmergencyBuildRules.IsFreeFallbackPlacement("hero_warrior", "hero_mage", false),
                Is.False);
            Assert.That(
                EmergencyBuildRules.IsFreeFallbackPlacement("hero_warrior", "hero_warrior", true),
                Is.False);
        }

        [Test]
        public void Affordability_AllowsFreeFallbackAndGreysUnaffordableHeroes()
        {
            Assert.That(EmergencyBuildRules.CanAfford("hero_warrior", "hero_warrior", 0, 50, false), Is.True);
            Assert.That(EmergencyBuildRules.CanAfford("hero_warrior", "hero_mage", 20, 50, false), Is.False);
            Assert.That(EmergencyBuildRules.CanAfford("hero_warrior", "hero_mage", 50, 50, false), Is.True);
            Assert.That(EmergencyBuildRules.CanAfford("hero_warrior", "hero_warrior", 20, 50, true), Is.False);
        }
    }
}
