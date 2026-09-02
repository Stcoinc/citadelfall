namespace ClubGamerZone.TowerDefense.Application.Configuration
{
    public sealed class ContentValidationLimits
    {
        public int MinimumSchemaVersion { get; } = 1;

        public int MaximumSupportedSchemaVersion { get; } = 1;

        public int MaximumTowers { get; } = 256;

        public int MaximumEnemies { get; } = 512;

        public int MaximumLevels { get; } = 512;

        public int MaximumWaveSets { get; } = 512;

        public int MaximumWavesPerSet { get; } = 200;

        public int MaximumSpawnsPerWave { get; } = 64;

        public float MinimumDamage { get; } = 0f;

        public float MaximumDamage { get; } = 1000000f;

        public float MinimumRange { get; } = 0.1f;

        public float MaximumRange { get; } = 1000f;

        public float MinimumAttackIntervalSeconds { get; } = 0.05f;

        public float MaximumAttackIntervalSeconds { get; } = 60f;

        public float MinimumMovementSpeed { get; } = 0.01f;

        public float MaximumMovementSpeed { get; } = 100f;

        public float MinimumHealth { get; } = 1f;

        public float MaximumHealth { get; } = 10000000f;

        public int MaximumCost { get; } = 1000000;

        public int MaximumSpawnCount { get; } = 1000;
    }
}
