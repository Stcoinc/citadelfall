using System;

namespace ClubGamerZone.TowerDefense.Infrastructure.Firebase.Dtos
{
    [Serializable]
    public sealed class FirebaseUsernameRecordDto
    {
        public string userId;
        public string email;
        public string username;
    }
}
