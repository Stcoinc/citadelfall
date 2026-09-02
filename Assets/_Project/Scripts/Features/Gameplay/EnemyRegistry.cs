using System.Collections.Generic;
using System.Linq;
using ClubGamerZone.TowerDefense.Application.Gameplay;
using ClubGamerZone.TowerDefense.Domain.Content;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    public sealed class EnemyRegistry
    {
        private readonly List<EnemyAgent> _enemies = new List<EnemyAgent>();
        private readonly ITargetingSelector _targetingSelector = new TargetingSelector();
        private readonly DamageAffinityCalculator _damageAffinityCalculator = new DamageAffinityCalculator();

        public IReadOnlyList<EnemyAgent> Enemies => _enemies;

        public void Clear()
        {
            _enemies.Clear();
        }

        public void Register(EnemyAgent enemy)
        {
            if (enemy != null && !_enemies.Contains(enemy))
            {
                _enemies.Add(enemy);
            }
        }

        public void Unregister(EnemyAgent enemy)
        {
            _enemies.Remove(enemy);
        }

        public EnemyAgent FindTarget(Vector3 origin, float range, TargetingMode targetingMode)
        {
            var sqrRange = range * range;
            var enemiesInRange = _enemies
                .Where(enemy => enemy != null)
                .Where(enemy => (enemy.transform.position - origin).sqrMagnitude <= sqrRange)
                .ToList();

            var candidates = enemiesInRange
                .Select(enemy => new TargetCandidate(
                    enemy.GetInstanceID().ToString(),
                    enemy.PathProgress,
                    (enemy.transform.position - origin).sqrMagnitude,
                    enemy.Health,
                    enemy.Definition.MaxHealth,
                    false))
                .ToList();
            var selected = _targetingSelector.SelectTarget(candidates, targetingMode);

            if (selected == null)
            {
                return null;
            }

            return enemiesInRange.FirstOrDefault(enemy => enemy.GetInstanceID().ToString() == selected.StableId);
        }

        public void ApplySplashDamage(Vector3 origin, float radius, float primaryDamage, float splashDamageMultiplier)
        {
            ApplySplashDamage(origin, radius, primaryDamage, splashDamageMultiplier, null);
        }

        public void ApplySplashDamage(Vector3 origin, float radius, float primaryDamage, float splashDamageMultiplier, string damageType)
        {
            if (radius <= 0f || primaryDamage <= 0f)
            {
                return;
            }

            var sqrRadius = radius * radius;
            var splashDamage = primaryDamage * Mathf.Clamp01(splashDamageMultiplier);
            var targets = _enemies
                .Where(enemy => enemy != null)
                .Where(enemy => (enemy.transform.position - origin).sqrMagnitude <= sqrRadius)
                .ToList();

            foreach (var target in targets)
            {
                target.ApplyDamage(_damageAffinityCalculator.CalculateDamage(splashDamage, damageType, target.Definition));
            }
        }
    }
}
