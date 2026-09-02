using System.Collections.Generic;
using System.Linq;

namespace ClubGamerZone.TowerDefense.Core
{
    public sealed class ValidationResult
    {
        public ValidationResult(IEnumerable<ValidationIssue> issues)
        {
            Issues = issues?.ToArray() ?? new ValidationIssue[0];
        }

        public IReadOnlyList<ValidationIssue> Issues { get; }

        public bool IsValid => Issues.All(issue => issue.Severity != ValidationSeverity.Error);

        public static ValidationResult Success { get; } = new ValidationResult(new ValidationIssue[0]);
    }
}
