using System;

namespace ClubGamerZone.TowerDefense.Infrastructure.Firebase.Dtos
{
    [Serializable]
    public sealed class FirebaseAuthResponseDto
    {
        public string localId;
        public string email;
        public string idToken;
        public string refreshToken;
    }
}
