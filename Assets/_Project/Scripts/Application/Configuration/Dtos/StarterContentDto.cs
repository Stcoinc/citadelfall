using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class StarterContentDto
    {
        public int SchemaVersion;
        public string ContentVersion;
        public TowerDto[] Towers;
        public EnemyDto[] Enemies;
        public BattlefieldDto[] Battlefields;
        public LevelDto[] Levels;
        public WaveSetDto[] WaveSets;
        public ArenaRulesDto[] ArenaRules;
    }
}
