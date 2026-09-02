using System;

namespace ClubGamerZone.TowerDefense.Infrastructure.Firebase.Dtos
{
    [Serializable]
    public sealed class FirebaseRefreshTokenResponseDto
    {
        public string access_token;
        public string expires_in;
        public string token_type;
        public string refresh_token;
        public string id_token;
        public string user_id;
        public string project_id;
    }
}
