using System;

namespace ClubGamerZone.TowerDefense.Application.Configuration.Dtos
{
    [Serializable]
    public sealed class BattlefieldDto
    {
        public string Id;
        public string BackgroundId;
        public BattlefieldPointDto[] PathPoints;
        public BattlefieldPointDto[] BuildSocketPositions;
    }
}
