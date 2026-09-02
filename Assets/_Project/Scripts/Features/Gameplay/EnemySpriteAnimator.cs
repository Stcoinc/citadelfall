using System;
using ClubGamerZone.TowerDefense.Domain.Content;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class EnemySpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _directionalFrames;
        [SerializeField] private Sprite[] _walkFrames;
        [SerializeField] private Sprite[] _deathFrames;
        [SerializeField] private int _walkFramesPerEnemy = 8;
        [SerializeField] private int _deathFramesPerEnemy = 8;
        [SerializeField] private float _walkFramesPerSecond = 8f;
        [SerializeField] private float _deathFramesPerSecond = 10f;

        private int _enemyRow;
        private int _directionRow;
        private float _elapsed;
        private bool _isDying;
        private Action _deathFinished;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public void SetEnemy(EnemyDefinition definition)
        {
            _enemyRow = ResolveEnemyRow(
                definition == null ? string.Empty : definition.Id.Value,
                definition == null ? string.Empty : definition.EnemyTag);
            _elapsed = 0f;
            _directionRow = 0;
            _isDying = false;
            _deathFinished = null;
            SetWalkFrame(0);
        }

        public void FaceMovementDirection(Vector3 direction)
        {
            if (_spriteRenderer == null || direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var nextDirection = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
                ? direction.x >= 0f ? 0 : 1
                : direction.y >= 0f ? 2 : 3;
            if (nextDirection != _directionRow)
            {
                _directionRow = nextDirection;
                _elapsed = 0f;
                SetWalkFrame(0);
            }

            _spriteRenderer.flipX = false;
        }

        public void PlayDeath(Action finished)
        {
            _isDying = true;
            _elapsed = 0f;
            _deathFinished = finished;
            SetDeathFrame(0);
        }

        private void Update()
        {
            if (_isDying)
            {
                UpdateDeathAnimation();
                return;
            }

            UpdateWalkAnimation();
        }

        private void UpdateWalkAnimation()
        {
            if (_walkFrames == null || _walkFrames.Length == 0 || _walkFramesPerEnemy <= 0)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            var frame = Mathf.FloorToInt(_elapsed * Mathf.Max(0.01f, _walkFramesPerSecond)) % _walkFramesPerEnemy;
            SetWalkFrame(frame);
        }

        private void UpdateDeathAnimation()
        {
            if (_deathFrames == null || _deathFrames.Length == 0 || _deathFramesPerEnemy <= 0)
            {
                _deathFinished?.Invoke();
                _deathFinished = null;
                return;
            }

            _elapsed += Time.deltaTime;
            var frame = Mathf.FloorToInt(_elapsed * Mathf.Max(0.01f, _deathFramesPerSecond));
            if (frame >= _deathFramesPerEnemy)
            {
                _deathFinished?.Invoke();
                _deathFinished = null;
                return;
            }

            SetDeathFrame(frame);
        }

        private void SetWalkFrame(int frame)
        {
            if (HasDirectionalFrames())
            {
                SetDirectionalFrame(_directionRow, frame);
                return;
            }

            if (_spriteRenderer == null || _walkFrames == null)
            {
                return;
            }

            var index = (_enemyRow * _walkFramesPerEnemy) + Mathf.Clamp(frame, 0, _walkFramesPerEnemy - 1);
            if (index >= 0 && index < _walkFrames.Length)
            {
                _spriteRenderer.sprite = _walkFrames[index];
            }
        }

        private void SetDeathFrame(int frame)
        {
            if (HasDirectionalFrames())
            {
                SetDirectionalFrame(4, frame);
                return;
            }

            if (_spriteRenderer == null || _deathFrames == null)
            {
                return;
            }

            var index = (_enemyRow * _deathFramesPerEnemy) + Mathf.Clamp(frame, 0, _deathFramesPerEnemy - 1);
            if (index >= 0 && index < _deathFrames.Length)
            {
                _spriteRenderer.sprite = _deathFrames[index];
            }
        }

        private bool HasDirectionalFrames()
        {
            return _directionalFrames != null && _directionalFrames.Length >= 6 * 5 * 8;
        }

        private void SetDirectionalFrame(int animationRow, int frame)
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            const int framesPerAnimation = 8;
            const int animationsPerEnemy = 5;
            var index = (_enemyRow * animationsPerEnemy * framesPerAnimation) +
                        (Mathf.Clamp(animationRow, 0, animationsPerEnemy - 1) * framesPerAnimation) +
                        Mathf.Clamp(frame, 0, framesPerAnimation - 1);
            if (index >= 0 && index < _directionalFrames.Length)
            {
                _spriteRenderer.sprite = _directionalFrames[index];
            }
        }

        private static int ResolveEnemyRow(string enemyId, string enemyTag)
        {
            switch (enemyId)
            {
                case "enemy_swarm": return 0;
                case "enemy_regenerator": return 1;
                case "enemy_scout": return 2;
                case "enemy_armored": return 3;
                case "enemy_shielded": return 3;
                case "enemy_boss": return 4;
                case "enemy_boss_warden": return 5;
            }

            switch (enemyTag)
            {
                case "rat": return 0;
                case "wolf": return 1;
                case "goblin": return 2;
                case "orc":
                case "orc_elite": return 3;
                case "boss_troll": return 4;
                case "boss_warden": return 5;
                default: return 2;
            }
        }
    }
}
