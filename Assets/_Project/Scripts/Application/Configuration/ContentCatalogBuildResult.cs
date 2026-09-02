using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Application.Configuration
{
    public sealed class ContentCatalogBuildResult
    {
        public ContentCatalogBuildResult(ContentCatalog catalog, ValidationResult validation)
        {
            Catalog = catalog;
            Validation = validation;
        }

        public ContentCatalog Catalog { get; }

        public ValidationResult Validation { get; }

        public bool IsSuccess => Catalog != null && Validation != null && Validation.IsValid;
    }
}
