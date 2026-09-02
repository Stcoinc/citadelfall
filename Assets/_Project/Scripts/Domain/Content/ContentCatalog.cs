using System.Collections.Generic;
using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Domain.Content
{
    public sealed class ContentCatalog
    {
        public ContentCatalog(
            int schemaVersion,
            string contentVersion,
            IReadOnlyDictionary<StableId, TowerDefinition> towers,
            IReadOnlyDictionary<StableId, EnemyDefinition> enemies,
            IReadOnlyDictionary<StableId, LevelDefinition> levels,
            IReadOnlyDictionary<StableId, WaveSetDefinition> waveSets,
            IReadOnlyDictionary<StableId, ArenaRulesDefinition> arenaRules)
        {
            SchemaVersion = schemaVersion;
            ContentVersion = contentVersion;
            Towers = towers;
            Enemies = enemies;
            Levels = levels;
            WaveSets = waveSets;
            ArenaRules = arenaRules;
        }

        public int SchemaVersion { get; }

        public string ContentVersion { get; }

        public IReadOnlyDictionary<StableId, TowerDefinition> Towers { get; }

        public IReadOnlyDictionary<StableId, EnemyDefinition> Enemies { get; }

        public IReadOnlyDictionary<StableId, LevelDefinition> Levels { get; }

        public IReadOnlyDictionary<StableId, WaveSetDefinition> WaveSets { get; }

        public IReadOnlyDictionary<StableId, ArenaRulesDefinition> ArenaRules { get; }
    }
}
