using System;

namespace ClubGamerZone.TowerDefense.Core
{
    public readonly struct StableId : IEquatable<StableId>
    {
        public StableId(string value)
        {
            Value = Normalize(value);
        }

        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public static bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            for (var i = 0; i < value.Length; i++)
            {
                var character = value[i];
                var isLowercase = character >= 'a' && character <= 'z';
                var isDigit = character >= '0' && character <= '9';
                var isSeparator = character == '_' || character == '-';

                if (!isLowercase && !isDigit && !isSeparator)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Equals(StableId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is StableId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
