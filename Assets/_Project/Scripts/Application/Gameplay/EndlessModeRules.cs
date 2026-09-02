using System;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public static class EndlessModeRules
    {
        public const int WavesPerRound = 5;

        public static int GetRound(int completedWaves) => Math.Max(0, completedWaves) / WavesPerRound + 1;

        public static int GetWaveInRound(int completedWaves) => Math.Max(0, completedWaves) % WavesPerRound + 1;

        public static float GetHealthMultiplier(int round) => 1f + Math.Max(0, round - 1) * 0.12f;

        public static float GetSpeedMultiplier(int round) => Math.Min(1.5f, 1f + Math.Max(0, round - 1) * 0.02f);

        public static float GetDamageMultiplier(int round) => 1f + Math.Max(0, round - 1) * 0.08f;

        public static int GetSpawnCount(int baseCount, int round)
        {
            if (baseCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseCount));
            }

            var extraPerRound = Math.Max(1, (baseCount + 4) / 5);
            return baseCount + Math.Max(0, round - 1) * extraPerRound;
        }

        public static float GetSpawnInterval(float baseIntervalSeconds, int round)
        {
            if (baseIntervalSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(baseIntervalSeconds));
            }

            return Math.Max(0.25f, baseIntervalSeconds * (float)Math.Pow(0.97f, Math.Max(0, round - 1)));
        }

        public static int GetEnemyScore(int threatValue, int round) =>
            10 + Math.Max(0, threatValue) * 5 + Math.Max(0, round - 1) * 2;

        public static int GetRoundCompletionBonus(int round) => Math.Max(1, round) * 100;
    }
}
