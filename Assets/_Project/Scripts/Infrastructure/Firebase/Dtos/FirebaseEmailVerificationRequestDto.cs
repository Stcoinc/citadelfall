using System;

namespace ClubGamerZone.TowerDefense.Infrastructure.Firebase.Dtos
{
    [Serializable]
    public sealed class FirebaseEmailVerificationRequestDto
    {
        public string requestType = "VERIFY_EMAIL";
        public string idToken;
    }
}
