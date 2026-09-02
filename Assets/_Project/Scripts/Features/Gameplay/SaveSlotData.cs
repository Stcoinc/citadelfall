using System;
using System.Linq;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [Serializable]
    public sealed class SaveSlotData
    {
        public int SlotIndex;
        // Serialized field name is retained so existing local saves remain compatible.
        public string CommanderName = "Adventurer";
        public int HighestUnlockedLevelIndex = 0;
        public int Scrap;
        public int Coins;
        public int Gems;
        public bool CurrenciesInitialized;
        public string LastPlayedLevelId = string.Empty;
        public string UpdatedAtUtc = string.Empty;
        public string[] PurchasedTowerIds = Array.Empty<string>();
        public string[] SelectedArenaHeroIds = Array.Empty<string>();
        public bool ArenaTutorialCompleted;
        public int ArenaMatchesPlayed;
        public int ArenaVictories;
        public int ArenaEnemiesDefeated;
        public int BestEndlessScore;
        public int HighestEndlessRound;

        public bool IsEmpty => string.IsNullOrWhiteSpace(UpdatedAtUtc);

        public bool HasPurchasedTower(string towerId)
        {
            if (string.IsNullOrWhiteSpace(towerId) || PurchasedTowerIds == null)
            {
                return false;
            }

            return PurchasedTowerIds.Contains(towerId);
        }

        public void AddPurchasedTower(string towerId)
        {
            if (string.IsNullOrWhiteSpace(towerId) || HasPurchasedTower(towerId))
            {
                return;
            }

            var current = PurchasedTowerIds ?? Array.Empty<string>();
            PurchasedTowerIds = current.Concat(new[] { towerId }).ToArray();
        }

        public void SetArenaDeck(string[] heroIds, int requiredSize = 5)
        {
            if (heroIds == null || heroIds.Length != requiredSize ||
                heroIds.Any(string.IsNullOrWhiteSpace) ||
                heroIds.Distinct(StringComparer.Ordinal).Count() != heroIds.Length)
            {
                throw new ArgumentException($"Arena deck must contain exactly {requiredSize} unique hero IDs.", nameof(heroIds));
            }

            SelectedArenaHeroIds = heroIds.ToArray();
        }
    }
}
