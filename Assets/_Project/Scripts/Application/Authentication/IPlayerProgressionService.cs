using System.Threading;
using System.Threading.Tasks;

namespace ClubGamerZone.TowerDefense.Application.Authentication
{
    public interface IPlayerProgressionService
    {
        Task SaveProgressionAsync(AuthenticatedPlayer player, CancellationToken cancellationToken);
    }
}
