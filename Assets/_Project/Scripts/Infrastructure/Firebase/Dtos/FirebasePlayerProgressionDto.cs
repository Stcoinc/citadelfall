using System;

namespace ClubGamerZone.TowerDefense.Infrastructure.Firebase.Dtos
{
    [Serializable]
    public sealed class FirebasePlayerProgressionDto
    {
        public int experience;
        public int level;
        public string[] unlockedTowerIds;
        public FirebaseEnemyDefeatProgressDto[] enemyDefeats;
        public string[] unlockedCardIds;
        public int scrap;
        public int coins;
        public bool currenciesInitialized;
        public int bestEndlessScore;
        public int highestEndlessRound;
        public string updatedAtUtc;
    }
}
