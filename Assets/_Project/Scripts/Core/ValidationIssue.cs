using System;

namespace ClubGamerZone.TowerDefense.Core
{
    public sealed class ValidationIssue
    {
        public ValidationIssue(ValidationSeverity severity, string path, string message)
        {
            Severity = severity;
            Path = string.IsNullOrWhiteSpace(path) ? "$" : path;
            Message = string.IsNullOrWhiteSpace(message) ? "Validation issue." : message;
        }

        public ValidationSeverity Severity { get; }

        public string Path { get; }

        public string Message { get; }

        public override string ToString()
        {
            return $"{Severity}: {Path} - {Message}";
        }
    }
}
