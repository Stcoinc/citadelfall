using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;

namespace ClubGamerZone.TowerDefense.Application.Configuration
{
    public interface IContentCatalogBuilder
    {
        ContentCatalogBuildResult Build(StarterContentDto content);
    }
}
