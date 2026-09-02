using System;
using ClubGamerZone.TowerDefense.Application.Gameplay;
using ClubGamerZone.TowerDefense.Domain.Content;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class TowerWeapon : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeedDegrees = 720f;
        [SerializeField] private Transform _projectileSpawnPoint;
        [SerializeField] private TowerProjectile _projectilePrefab;
        [SerializeField] private GameObject _impactEffectPrefab;
        [SerializeField] private float _splashRadius = 1.1f;
        [SerializeField] private float _splashDamageMultiplier = 0.65f;

        private TowerDefinition _definition;
        private TowerStatsView _stats;
        private readonly TowerStatsPresenter _statsPresenter = new TowerStatsPresenter();
        private readonly DamageAffinityCalculator _damageAffinityCalculator = new DamageAffinityCalculator();
        private EnemyRegistry _enemyRegistry;
        private float _cooldownRemaining;
        private EnemyAgent _currentTarget;
        private TowerAttackAnimator _attackAnimator;

        public TowerDefinition Definition => _definition;

        public void Initialize(TowerDefinition definition, EnemyRegistry enemyRegistry)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            Initialize(definition, new TowerInstanceState(definition.Id, 1), enemyRegistry);
        }

        public void Initialize(TowerDefinition definition, TowerInstanceState state, EnemyRegistry enemyRegistry)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _stats = _statsPresenter.BuildStats(definition, state ?? throw new ArgumentNullException(nameof(state)));
            _enemyRegistry = enemyRegistry ?? throw new ArgumentNullException(nameof(enemyRegistry));
            _cooldownRemaining = 0f;
            _currentTarget = null;
            _attackAnimator = GetComponent<TowerAttackAnimator>();
            _attackAnimator?.Configure(definition);
        }

        private void Update()
        {
            if (_definition == null || _enemyRegistry == null)
            {
                return;
            }

            _currentTarget = _enemyRegistry.FindTarget(transform.position, _stats.Range, _stats.TargetingMode);

            if (_currentTarget == null)
            {
                return;
            }

            var targetDirection = _currentTarget.transform.position - transform.position;
            if (_definition.ShouldRotate)
            {
                RotateToward(_currentTarget.transform.position);
            }

            _cooldownRemaining -= Time.deltaTime;

            if (_cooldownRemaining > 0f)
            {
                return;
            }

            FireAt(_currentTarget, targetDirection);
            _cooldownRemaining = _stats.AttackIntervalSeconds;
        }

        private void FireAt(EnemyAgent target, Vector3 targetDirection)
        {
            _attackAnimator?.PlayAttack(targetDirection);

            if (_projectilePrefab == null)
            {
                ApplyInstantDamage(target);
                return;
            }

            var spawnPoint = _projectileSpawnPoint == null ? transform : _projectileSpawnPoint;
            var projectile = Instantiate(_projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            projectile.Launch(
                target,
                _enemyRegistry,
                _stats.Damage,
                _stats.HasSplash,
                _splashRadius,
                _splashDamageMultiplier,
                _definition.DamageType,
                _impactEffectPrefab);
        }

        private void ApplyInstantDamage(EnemyAgent target)
        {
            if (_stats.HasSplash)
            {
                _enemyRegistry.ApplySplashDamage(
                    target.transform.position,
                    _splashRadius,
                    _stats.Damage,
                    _splashDamageMultiplier,
                    _definition.DamageType);
                return;
            }

            target.ApplyDamage(_damageAffinityCalculator.CalculateDamage(_stats.Damage, _definition.DamageType, target.Definition));
        }

        private void RotateToward(Vector3 targetPosition)
        {
            var direction = targetPosition - transform.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeedDegrees * Time.deltaTime);
        }
    }
}
