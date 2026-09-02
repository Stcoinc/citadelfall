using System;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Application.Gameplay
{
    public sealed class DamageAffinityCalculator
    {
        private const float WeaknessMultiplier = 1.5f;

        public float CalculateDamage(float baseDamage, string damageType, EnemyDefinition enemyDefinition)
        {
            if (baseDamage <= 0f)
            {
                return 0f;
            }

            if (enemyDefinition == null || string.IsNullOrWhiteSpace(damageType))
            {
                return baseDamage;
            }

            return string.Equals(damageType, enemyDefinition.WeakToDamageType, StringComparison.OrdinalIgnoreCase)
                ? baseDamage * WeaknessMultiplier
                : baseDamage;
        }
    }
}
