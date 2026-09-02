using System.Threading;
using System.Threading.Tasks;

namespace ClubGamerZone.TowerDefense.Application.Networking
{
    public interface IRestClient
    {
        Task<RestResponse> SendAsync(RestRequest request, CancellationToken cancellationToken);
    }
}
