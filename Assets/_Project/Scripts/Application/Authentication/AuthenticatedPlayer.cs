namespace ClubGamerZone.TowerDefense.Application.Authentication
{
    public sealed class AuthenticatedPlayer
    {
        public AuthenticatedPlayer(string userId, string email, string username, string avatarId, string idToken, string refreshToken, bool emailVerified, int playerLevel = 1, PlayerProgression progression = null)
        {
            UserId = userId ?? string.Empty;
            Email = email ?? string.Empty;
            Username = username ?? string.Empty;
            AvatarId = avatarId ?? string.Empty;
            IdToken = idToken ?? string.Empty;
            RefreshToken = refreshToken ?? string.Empty;
            EmailVerified = emailVerified;
            Progression = progression ?? new PlayerProgression((playerLevel < 1 ? 1 : playerLevel - 1) * PlayerProgression.ExperiencePerLevel);
        }

        public string UserId { get; }

        public string Email { get; }

        public string Username { get; }

        public string AvatarId { get; }

        public string IdToken { get; }

        public string RefreshToken { get; }

        public bool EmailVerified { get; }

        public int PlayerLevel => Progression.Level;

        public PlayerProgression Progression { get; }

        public AuthenticatedPlayer WithUsername(string username)
        {
            return new AuthenticatedPlayer(
                UserId,
                Email,
                username,
                AvatarId,
                IdToken,
                RefreshToken,
                EmailVerified,
                PlayerLevel,
                Progression);
        }
    }
}
