using System;

namespace ClubGamerZone.TowerDefense.Application.Authentication
{
    [Serializable]
    public sealed class EnemyDefeatProgress
    {
        public EnemyDefeatProgress(string enemyId, int defeats)
        {
            EnemyId = enemyId ?? string.Empty;
            Defeats = Math.Max(0, defeats);
        }

        public string EnemyId;

        public int Defeats;
    }
}
