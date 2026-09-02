namespace ClubGamerZone.TowerDefense.Application.Authentication
{
    public interface IAuthenticationSessionStore
    {
        string LoadRefreshToken();

        void SaveRefreshToken(string refreshToken);

        void Clear();
    }
}
