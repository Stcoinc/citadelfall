using System.Collections.Generic;
using ClubGamerZone.TowerDefense.Application.Gameplay;
using ClubGamerZone.TowerDefense.Domain.Content;
using NUnit.Framework;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class TargetingSelectorTests
    {
        [Test]
        public void SelectTarget_First_ReturnsHighestPathProgress()
        {
            var selector = new TargetingSelector();

            var selected = selector.SelectTarget(CreateCandidates(), TargetingMode.First);

            Assert.That(selected.StableId, Is.EqualTo("front"));
        }

        [Test]
        public void SelectTarget_Last_ReturnsLowestPathProgress()
        {
            var selector = new TargetingSelector();

            var selected = selector.SelectTarget(CreateCandidates(), TargetingMode.Last);

            Assert.That(selected.StableId, Is.EqualTo("back"));
        }

        [Test]
        public void SelectTarget_Closest_ReturnsLowestDistance()
        {
            var selector = new TargetingSelector();

            var selected = selector.SelectTarget(CreateCandidates(), TargetingMode.Closest);

            Assert.That(selected.StableId, Is.EqualTo("close"));
        }

        [Test]
        public void SelectTarget_LowestHealth_ReturnsLowestHealth()
        {
            var selector = new TargetingSelector();

            var selected = selector.SelectTarget(CreateCandidates(), TargetingMode.LowestHealth);

            Assert.That(selected.StableId, Is.EqualTo("weak"));
        }

        [Test]
        public void SelectTarget_BossPriority_ReturnsBossBeforePathLeader()
        {
            var selector = new TargetingSelector();

            var selected = selector.SelectTarget(CreateCandidates(), TargetingMode.BossPriority);

            Assert.That(selected.StableId, Is.EqualTo("boss"));
        }

        private static IReadOnlyList<TargetCandidate> CreateCandidates()
        {
            return new[]
            {
                new TargetCandidate("front", 0.9f, 25f, 70f, 100f, false),
                new TargetCandidate("back", 0.1f, 9f, 90f, 100f, false),
                new TargetCandidate("close", 0.4f, 1f, 80f, 100f, false),
                new TargetCandidate("weak", 0.3f, 16f, 10f, 100f, false),
                new TargetCandidate("boss", 0.2f, 36f, 500f, 500f, true)
            };
        }
    }
}
