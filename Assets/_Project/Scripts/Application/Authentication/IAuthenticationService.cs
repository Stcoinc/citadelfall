using System.Threading;
using System.Threading.Tasks;

namespace ClubGamerZone.TowerDefense.Application.Authentication
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> RegisterAsync(string username, string avatarId, string email, string password, CancellationToken cancellationToken);

        Task<AuthenticationResult> SignInWithEmailAsync(string email, string password, CancellationToken cancellationToken);

        Task<AuthenticationResult> SignInWithUsernameAsync(string username, string password, CancellationToken cancellationToken);

        Task<AuthenticationResult> RestoreSessionAsync(string refreshToken, CancellationToken cancellationToken);

        Task<AuthenticationResult> ClaimUsernameAsync(AuthenticatedPlayer player, string username, CancellationToken cancellationToken);
    }
}
