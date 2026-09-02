using System;

namespace ClubGamerZone.TowerDefense.Infrastructure.Firebase.Dtos
{
    [Serializable]
    public sealed class FirebaseEnemyDefeatProgressDto
    {
        public string enemyId;
        public int defeats;
    }
}
