using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;

namespace ClubGamerZone.TowerDefense.Infrastructure.Json
{
    public interface IContentJsonParser
    {
        StarterContentDto Parse(string json);
    }
}
