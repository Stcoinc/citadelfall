using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class TowerMergeDto
    {
        public int RequiredLevel;
        public string ResultTowerId;
        public string Currency;
        public int Cost;
    }
}
