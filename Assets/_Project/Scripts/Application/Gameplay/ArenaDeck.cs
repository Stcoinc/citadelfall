using System;
using System.Collections.Generic;
using System.Linq;
using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public sealed class ArenaDeck
    {
        private readonly StableId[] _heroIds;

        public ArenaDeck(IEnumerable<StableId> heroIds, int requiredSize)
        {
            if (heroIds == null)
            {
                throw new ArgumentNullException(nameof(heroIds));
            }

            _heroIds = heroIds.ToArray();
            if (_heroIds.Length != requiredSize)
            {
                throw new ArgumentException($"An arena deck must contain exactly {requiredSize} heroes.", nameof(heroIds));
            }

            if (_heroIds.Any(id => id.IsEmpty))
            {
                throw new ArgumentException("Arena decks cannot contain an empty hero ID.", nameof(heroIds));
            }

            if (_heroIds.Distinct().Count() != _heroIds.Length)
            {
                throw new ArgumentException("Arena decks cannot contain duplicate heroes.", nameof(heroIds));
            }
        }

        public IReadOnlyList<StableId> HeroIds => _heroIds;
    }
}
