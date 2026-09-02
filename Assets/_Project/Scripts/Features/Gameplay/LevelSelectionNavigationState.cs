using System;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    public static class LevelSelectionNavigationState
    {
        public static bool ShouldOpenLevelMap(bool isArenaMode, SaveSlotData selectedSlot)
        {
            return !isArenaMode && selectedSlot != null;
        }

        public static int ResolveMapIndex(int levelCount, int highestUnlockedLevelIndex, int levelsPerMap)
        {
            if (levelCount <= 0)
            {
                return 0;
            }

            if (levelsPerMap <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelsPerMap));
            }

            var focusedLevelIndex = Math.Max(0, Math.Min(highestUnlockedLevelIndex, levelCount - 1));
            return focusedLevelIndex / levelsPerMap;
        }
    }
}
