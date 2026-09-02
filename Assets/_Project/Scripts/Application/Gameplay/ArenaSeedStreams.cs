namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public static class ArenaSeedStreams
    {
        private const uint HeroSalt = 0xA341316Cu;
        private const uint SocketSalt = 0xC8013EA4u;
        private const uint EnemySalt = 0xAD90777Du;

        public static uint Hero(uint matchSeed)
        {
            return Mix(matchSeed, HeroSalt);
        }

        public static uint Socket(uint matchSeed)
        {
            return Mix(matchSeed, SocketSalt);
        }

        public static uint Enemy(uint matchSeed)
        {
            return Mix(matchSeed, EnemySalt);
        }

        private static uint Mix(uint matchSeed, uint salt)
        {
            var value = (matchSeed == 0 ? 0x9E3779B9u : matchSeed) ^ salt;
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return value == 0 ? salt : value;
        }
    }
}
