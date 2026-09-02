using System;

namespace ClubGamerZone.TowerDefense.Infrastructure.Firebase.Dtos
{
    [Serializable]
    public sealed class FirebasePlayerProfileDto
    {
        public string userId;
        public string username;
        public string email;
        public string avatarId;
        public int playerLevel;
        public string createdAtUtc;
    }
}
