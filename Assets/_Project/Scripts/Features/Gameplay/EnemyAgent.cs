using System;
using ClubGamerZone.TowerDefense.Domain.Content;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class EnemyAgent : MonoBehaviour
    {
        [SerializeField] private EnemySpriteAnimator _spriteAnimator;

        private Transform[] _pathPoints;
        private EnemyDefinition _definition;
        private int _nextPathIndex;
        private float _health;
        private float _distanceTravelled;
        private float _totalPathDistance;
        private bool _isResolved;
        private bool _isDying;

        public event Action<EnemyAgent> Destroyed;

        public event Action<EnemyAgent> ReachedBase;

        public EnemyDefinition Definition => _definition;

        public float Health => _health;

        public float PathProgress => _totalPathDistance <= 0f ? 0f : Mathf.Clamp01(_distanceTravelled / _totalPathDistance);

        public void Initialize(EnemyDefinition definition, Transform[] pathPoints)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _pathPoints = pathPoints ?? throw new ArgumentNullException(nameof(pathPoints));
            _nextPathIndex = 0;
            _health = definition.MaxHealth;
            _distanceTravelled = 0f;
            _totalPathDistance = CalculateTotalPathDistance(pathPoints);
            _isResolved = false;
            _isDying = false;

            if (_spriteAnimator == null)
            {
                _spriteAnimator = GetComponent<EnemySpriteAnimator>();
            }

            if (_pathPoints.Length > 0)
            {
                transform.position = _pathPoints[0].position;
                _nextPathIndex = 1;

                if (_nextPathIndex < _pathPoints.Length && _pathPoints[_nextPathIndex] != null)
                {
                    FaceMovementDirection(_pathPoints[_nextPathIndex].position - transform.position);
                }
            }

            gameObject.SetActive(true);
            _spriteAnimator?.SetEnemy(_definition);
            ApplyVisualStyle();
        }

        public void ApplyDamage(float amount)
        {
            if (_isResolved || _isDying || amount <= 0f)
            {
                return;
            }

            _health = Mathf.Max(0f, _health - amount);

            if (_health <= 0f)
            {
                _isDying = true;
                if (_spriteAnimator == null)
                {
                    Resolve(Destroyed);
                }
                else
                {
                    _spriteAnimator.PlayDeath(() => Resolve(Destroyed));
                }
            }
        }

        private void Update()
        {
            if (_isResolved || _isDying || _definition == null || _pathPoints == null || _pathPoints.Length == 0)
            {
                return;
            }

            if (_nextPathIndex >= _pathPoints.Length)
            {
                Resolve(ReachedBase);
                return;
            }

            var target = _pathPoints[_nextPathIndex];
            var step = _definition.MovementSpeed * Time.deltaTime;
            var previousPosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);
            var movement = transform.position - previousPosition;
            _distanceTravelled += movement.magnitude;
            FaceMovementDirection(movement);

            if (Vector3.Distance(transform.position, target.position) <= 0.01f)
            {
                _nextPathIndex++;
            }
        }

        private void FaceMovementDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            // Citadel Fall creatures are authored in a stable side/three-quarter pose.
            // Rotating the entire sprite around bends made wolves and goblins appear upside down.
            transform.rotation = Quaternion.identity;
            _spriteAnimator?.FaceMovementDirection(direction);
        }

        private static float CalculateTotalPathDistance(Transform[] pathPoints)
        {
            var distance = 0f;

            if (pathPoints == null)
            {
                return distance;
            }

            for (var i = 1; i < pathPoints.Length; i++)
            {
                if (pathPoints[i - 1] != null && pathPoints[i] != null)
                {
                    distance += Vector3.Distance(pathPoints[i - 1].position, pathPoints[i].position);
                }
            }

            return distance;
        }

        private void ApplyVisualStyle()
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.color = Color.white;
        }

        private void Resolve(Action<EnemyAgent> resolution)
        {
            if (_isResolved)
            {
                return;
            }

            _isResolved = true;
            resolution?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
