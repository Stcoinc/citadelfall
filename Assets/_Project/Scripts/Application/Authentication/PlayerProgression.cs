using System;
using System.Collections.Generic;
using System.Linq;

namespace ClubGamerZone.TowerDefense.Application.Authentication
{
    [Serializable]
    public sealed class PlayerProgression
    {
        public const int ExperiencePerLevel = 1000;
        public const int ExperiencePerEnemy = 10;
        public const int DefeatsPerCard = 1000;

        public PlayerProgression(
            int experience = 0,
            string[] unlockedTowerIds = null,
            EnemyDefeatProgress[] enemyDefeats = null,
            string[] unlockedCardIds = null,
            int scrap = 0,
            int coins = 0,
            bool currenciesInitialized = false,
            int bestEndlessScore = 0,
            int highestEndlessRound = 0)
        {
            Experience = Math.Max(0, experience);
            UnlockedTowerIds = unlockedTowerIds ?? Array.Empty<string>();
            EnemyDefeats = enemyDefeats ?? Array.Empty<EnemyDefeatProgress>();
            UnlockedCardIds = unlockedCardIds ?? Array.Empty<string>();
            Scrap = Math.Max(0, scrap);
            Coins = Math.Max(0, coins);
            CurrenciesInitialized = currenciesInitialized;
            BestEndlessScore = Math.Max(0, bestEndlessScore);
            HighestEndlessRound = Math.Max(0, highestEndlessRound);
        }

        public int Experience { get; private set; }

        public int Level => 1 + Experience / ExperiencePerLevel;

        public string[] UnlockedTowerIds { get; private set; }

        public EnemyDefeatProgress[] EnemyDefeats { get; private set; }

        public string[] UnlockedCardIds { get; private set; }

        public int Scrap { get; private set; }

        public int Coins { get; private set; }

        public bool CurrenciesInitialized { get; private set; }

        public int BestEndlessScore { get; private set; }

        public int HighestEndlessRound { get; private set; }

        public void RegisterEnemyDefeat(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return;
            }

            Experience += ExperiencePerEnemy;
            var progress = EnemyDefeats.FirstOrDefault(entry => entry != null && entry.EnemyId == enemyId);
            if (progress == null)
            {
                progress = new EnemyDefeatProgress(enemyId, 0);
                EnemyDefeats = EnemyDefeats.Concat(new[] { progress }).ToArray();
            }

            progress.Defeats++;
            var unlockedTier = progress.Defeats / DefeatsPerCard;
            for (var tier = 1; tier <= unlockedTier; tier++)
            {
                UnlockCard(BuildCardId(enemyId, tier));
            }
        }

        public int GetEnemyDefeats(string enemyId)
        {
            var progress = EnemyDefeats.FirstOrDefault(entry => entry != null && entry.EnemyId == enemyId);
            return progress == null ? 0 : progress.Defeats;
        }

        public int GetUnlockedCardTier(string enemyId)
        {
            return GetEnemyDefeats(enemyId) / DefeatsPerCard;
        }

        public bool HasUnlockedTower(string towerId)
        {
            return !string.IsNullOrWhiteSpace(towerId) &&
                   UnlockedTowerIds.Any(id => string.Equals(id, towerId, StringComparison.Ordinal));
        }

        public void UnlockTower(string towerId)
        {
            if (!string.IsNullOrWhiteSpace(towerId) && !HasUnlockedTower(towerId))
            {
                UnlockedTowerIds = UnlockedTowerIds.Concat(new[] { towerId }).ToArray();
            }
        }

        public void SetCurrencies(int scrap, int coins)
        {
            Scrap = Math.Max(0, scrap);
            Coins = Math.Max(0, coins);
            CurrenciesInitialized = true;
        }

        public bool RecordEndlessScore(int score, int round)
        {
            var changed = false;
            if (score > BestEndlessScore)
            {
                BestEndlessScore = score;
                changed = true;
            }

            if (round > HighestEndlessRound)
            {
                HighestEndlessRound = round;
                changed = true;
            }

            return changed;
        }

        public static string BuildCardId(string enemyId, int tier)
        {
            return $"{enemyId}_card_{Math.Max(1, tier)}";
        }

        private void UnlockCard(string cardId)
        {
            if (!UnlockedCardIds.Contains(cardId))
            {
                UnlockedCardIds = UnlockedCardIds.Concat(new[] { cardId }).ToArray();
            }
        }
    }
}
