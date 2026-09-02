using ClubGamerZone.TowerDefense.Features.Gameplay;
using NUnit.Framework;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class LevelSelectionNavigationStateTests
    {
        [Test]
        public void ShouldOpenLevelMap_WhenAdventureProfileIsAlreadySelected_ReturnsTrue()
        {
            var selectedSlot = new SaveSlotData { SlotIndex = 1 };

            Assert.That(
                LevelSelectionNavigationState.ShouldOpenLevelMap(false, selectedSlot),
                Is.True);
        }

        [Test]
        public void ShouldOpenLevelMap_WithoutProfileOrInArenaMode_ReturnsFalse()
        {
            Assert.That(LevelSelectionNavigationState.ShouldOpenLevelMap(false, null), Is.False);
            Assert.That(
                LevelSelectionNavigationState.ShouldOpenLevelMap(true, new SaveSlotData()),
                Is.False);
        }

        [TestCase(0, 0)]
        [TestCase(4, 0)]
        [TestCase(5, 1)]
        [TestCase(9, 1)]
        [TestCase(99, 1)]
        public void ResolveMapIndex_FocusesThePageContainingLatestUnlockedLevel(
            int highestUnlockedLevelIndex,
            int expectedMapIndex)
        {
            Assert.That(
                LevelSelectionNavigationState.ResolveMapIndex(10, highestUnlockedLevelIndex, 5),
                Is.EqualTo(expectedMapIndex));
        }
    }
}
