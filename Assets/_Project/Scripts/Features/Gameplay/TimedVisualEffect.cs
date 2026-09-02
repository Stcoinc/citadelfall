using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class TimedVisualEffect : MonoBehaviour
    {
        [SerializeField] private float _lifetimeSeconds = 0.35f;
        [SerializeField] private float _scaleSpeed = 1.6f;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private float _elapsedSeconds;
        private Color _startColor;
        private Vector3 _startScale;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            _startColor = _spriteRenderer == null ? Color.white : _spriteRenderer.color;
            _startScale = transform.localScale;
        }

        private void OnEnable()
        {
            _elapsedSeconds = 0f;
            transform.localScale = _startScale;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _startColor;
            }
        }

        private void Update()
        {
            _elapsedSeconds += Time.deltaTime;
            var normalized = Mathf.Clamp01(_elapsedSeconds / Mathf.Max(0.01f, _lifetimeSeconds));
            transform.localScale = _startScale * (1f + normalized * _scaleSpeed);

            if (_spriteRenderer != null)
            {
                var color = _startColor;
                color.a = Mathf.Lerp(_startColor.a, 0f, normalized);
                _spriteRenderer.color = color;
            }

            if (_elapsedSeconds >= _lifetimeSeconds)
            {
                Destroy(gameObject);
            }
        }
    }
}
