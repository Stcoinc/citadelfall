using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using ClubGamerZone.TowerDefense.Core;

namespace ClubGamerZone.TowerDefense.Application.Configuration
{
    public interface IContentValidator
    {
        ValidationResult Validate(StarterContentDto content);
    }
}
