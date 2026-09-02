using System;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public static class ArenaEnemyBalance
    {
        public static int GetUnlockedEnemyCount(
            int wave,
            int availableEnemyCount,
            int wolfIntroductionWave,
            int goblinIntroductionWave,
            int orcIntroductionWave)
        {
            if (wave < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(wave));
            }

            if (availableEnemyCount < 1)
            {
                return 0;
            }

            var unlocked = 1;
            if (wave >= wolfIntroductionWave)
            {
                unlocked++;
            }

            if (wave >= goblinIntroductionWave)
            {
                unlocked++;
            }

            if (wave >= orcIntroductionWave)
            {
                unlocked++;
            }

            return Math.Min(unlocked, availableEnemyCount);
        }

        public static float GetScaledHealth(
            float baseHealth,
            int wave,
            float baseMultiplier,
            float perWaveGrowth,
            float bossMultiplier)
        {
            if (baseHealth <= 0f || wave < 1 || baseMultiplier <= 0f || perWaveGrowth < 0f || bossMultiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(baseHealth));
            }

            return baseHealth * baseMultiplier * (1f + ((wave - 1) * perWaveGrowth)) * bossMultiplier;
        }

        public static float GetScaledSpeed(float baseSpeed, float multiplier)
        {
            if (baseSpeed <= 0f || multiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(baseSpeed));
            }

            return baseSpeed * multiplier;
        }

        public static float GetSpawnInterval(
            int wave,
            float baseIntervalSeconds,
            float reductionPerWave,
            float minimumIntervalSeconds)
        {
            if (wave < 1 || baseIntervalSeconds <= 0f || reductionPerWave < 0f || minimumIntervalSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(wave));
            }

            return Math.Max(minimumIntervalSeconds, baseIntervalSeconds - ((wave - 1) * reductionPerWave));
        }
    }
}
