using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class WaveSetDto
    {
        public string Id;
        public WaveDto[] Waves;
    }
}
