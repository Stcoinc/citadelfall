using System;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public sealed class ArenaSummonSequence
    {
        private uint _state;

        public ArenaSummonSequence(uint seed)
        {
            _state = seed == 0 ? 0x9E3779B9u : seed;
        }

        public int NextDeckIndex(int deckSize)
        {
            if (deckSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deckSize));
            }

            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (int)(_state % (uint)deckSize);
        }
    }
}
