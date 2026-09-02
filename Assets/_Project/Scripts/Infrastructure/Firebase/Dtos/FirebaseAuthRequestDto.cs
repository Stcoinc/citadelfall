using System;

namespace ClubGamerZone.TowerDefense.Infrastructure.Firebase.Dtos
{
    [Serializable]
    public sealed class FirebaseAuthRequestDto
    {
        public string email;
        public string password;
        public bool returnSecureToken = true;
    }
}
