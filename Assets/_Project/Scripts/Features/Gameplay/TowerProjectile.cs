using UnityEngine;
using ClubGamerZone.TowerDefense.Application.Gameplay;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class TowerProjectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 12f;
        [SerializeField] private float _hitDistance = 0.08f;
        [SerializeField] private GameObject _impactEffectPrefab;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private EnemyAgent _target;
        private EnemyRegistry _enemyRegistry;
        private float _damage;
        private bool _hasSplash;
        private float _splashRadius;
        private float _splashDamageMultiplier;
        private string _damageType;
        private readonly DamageAffinityCalculator _damageAffinityCalculator = new DamageAffinityCalculator();

        public void Launch(
            EnemyAgent target,
            EnemyRegistry enemyRegistry,
            float damage,
            bool hasSplash,
            float splashRadius,
            float splashDamageMultiplier,
            string damageType,
            GameObject impactEffectPrefab)
        {
            _target = target;
            _enemyRegistry = enemyRegistry;
            _damage = damage;
            _hasSplash = hasSplash;
            _splashRadius = splashRadius;
            _splashDamageMultiplier = splashDamageMultiplier;
            _damageType = damageType;
            _impactEffectPrefab = impactEffectPrefab;
            ApplyCombatStyle();
            gameObject.SetActive(true);
        }

        private void ApplyCombatStyle()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = HeroCombatPalette.GetColor(_damageType);
            }

            var scale = HeroCombatPalette.GetProjectileScale(_damageType);
            transform.localScale = Vector3.one * scale;
        }

        private void Update()
        {
            if (_target == null || !_target.IsTargetable)
            {
                Destroy(gameObject);
                return;
            }

            var targetPosition = _target.transform.position;
            var direction = targetPosition - transform.position;

            if (direction.sqrMagnitude <= _hitDistance * _hitDistance)
            {
                HitTarget(targetPosition);
                return;
            }

            RotateToward(direction);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, _speed * Time.deltaTime);
        }

        private void HitTarget(Vector3 impactPosition)
        {
            if (_hasSplash && _enemyRegistry != null)
            {
                _enemyRegistry.ApplySplashDamage(impactPosition, _splashRadius, _damage, _splashDamageMultiplier, _damageType);
            }
            else
            {
                _target.ApplyDamage(_damageAffinityCalculator.CalculateDamage(_damage, _damageType, _target.Definition));
            }

            SpawnImpact(impactPosition);
            Destroy(gameObject);
        }

        private void SpawnImpact(Vector3 impactPosition)
        {
            if (_impactEffectPrefab == null)
            {
                return;
            }

            var impact = Instantiate(_impactEffectPrefab, impactPosition, Quaternion.identity);
            var impactRenderer = impact.GetComponent<SpriteRenderer>();
            if (impactRenderer != null)
            {
                impactRenderer.color = HeroCombatPalette.GetColor(_damageType);
            }

            impact.transform.localScale = _hasSplash
                ? Vector3.one * Mathf.Max(0.2f, _splashRadius)
                : impact.transform.localScale * HeroCombatPalette.GetProjectileScale(_damageType);
        }

        private void RotateToward(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
    }
}
