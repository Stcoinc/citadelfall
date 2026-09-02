using System.Collections;
using ClubGamerZone.TowerDefense.Domain.Content;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class TowerAttackAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private TowerAttackAnimationStyle _style = TowerAttackAnimationStyle.ArcaneCast;
        [SerializeField] private float _durationSeconds = 0.18f;
        [SerializeField] private float _scalePunch = 0.18f;
        [SerializeField] private float _lungeDistance = 0.1f;
        [SerializeField] private float _liftDistance = 0.04f;
        [SerializeField] private Color _flashColor = Color.white;

        private Coroutine _activeAnimation;
        private Vector3 _baseLocalPosition;
        private Vector3 _baseLocalScale;
        private Color _baseColor = Color.white;
        private bool _hasBaseState;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            CaptureBaseState();
        }

        private void OnDisable()
        {
            ResetVisuals();
        }

        public void Configure(TowerDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            _style = ResolveStyle(definition.Id.Value, definition.DamageType);
            _flashColor = HeroCombatPalette.GetColor(definition.DamageType);
            CaptureBaseState(force: true);
        }

        public void CaptureCurrentVisualState()
        {
            CaptureBaseState(force: true);
        }

        public void PlayAttack(Vector3 targetDirection)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            CaptureBaseState();

            if (_activeAnimation != null)
            {
                StopCoroutine(_activeAnimation);
                ResetVisuals();
            }

            _activeAnimation = StartCoroutine(AnimateAttack(targetDirection));
        }

        private IEnumerator AnimateAttack(Vector3 targetDirection)
        {
            var duration = Mathf.Max(0.05f, _durationSeconds);
            var elapsed = 0f;
            var direction = targetDirection.sqrMagnitude <= 0.0001f ? Vector3.right : targetDirection.normalized;
            direction.z = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var punch = Mathf.Sin(t * Mathf.PI);
                var release = Mathf.Clamp01((t - 0.35f) / 0.65f);

                ApplyStyle(direction, punch, release);
                yield return null;
            }

            ResetVisuals();
            _activeAnimation = null;
        }

        private void ApplyStyle(Vector3 direction, float punch, float release)
        {
            var scaleAmount = 1f + (_scalePunch * punch);
            var offset = Vector3.up * (_liftDistance * punch);

            switch (_style)
            {
                case TowerAttackAnimationStyle.MeleeLunge:
                    offset += direction * (_lungeDistance * punch);
                    scaleAmount = 1f + (_scalePunch * 0.65f * punch);
                    break;
                case TowerAttackAnimationStyle.HolySmite:
                    offset += Vector3.up * (_liftDistance * punch);
                    scaleAmount = 1f + (_scalePunch * 0.9f * punch);
                    break;
                case TowerAttackAnimationStyle.ArrowRelease:
                    offset -= direction * (_lungeDistance * 0.45f * (1f - release) * punch);
                    scaleAmount = 1f + (_scalePunch * 0.35f * punch);
                    break;
                case TowerAttackAnimationStyle.NatureCast:
                    offset += Vector3.up * (_liftDistance * 1.25f * punch);
                    scaleAmount = 1f + (_scalePunch * 0.75f * punch);
                    break;
                case TowerAttackAnimationStyle.ShadowBurst:
                    offset -= Vector3.up * (_liftDistance * 0.45f * punch);
                    scaleAmount = 1f + (_scalePunch * 1.15f * punch);
                    break;
                case TowerAttackAnimationStyle.MechanicalRecoil:
                    offset -= direction * (_lungeDistance * punch);
                    scaleAmount = 1f + (_scalePunch * 0.4f * punch);
                    break;
            }

            transform.localPosition = _baseLocalPosition + offset;
            transform.localScale = _baseLocalScale * scaleAmount;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.Lerp(_baseColor, _flashColor, punch * 0.55f);
            }
        }

        private void CaptureBaseState(bool force = false)
        {
            if (_hasBaseState && !force)
            {
                return;
            }

            _baseLocalPosition = transform.localPosition;
            _baseLocalScale = transform.localScale;
            _baseColor = _spriteRenderer == null ? Color.white : _spriteRenderer.color;
            _hasBaseState = true;
        }

        private void ResetVisuals()
        {
            if (!_hasBaseState)
            {
                return;
            }

            transform.localPosition = _baseLocalPosition;
            transform.localScale = _baseLocalScale;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _baseColor;
            }
        }

        private static TowerAttackAnimationStyle ResolveStyle(string towerId, string damageType)
        {
            switch (towerId)
            {
                case "hero_warrior": return TowerAttackAnimationStyle.MeleeLunge;
                case "hero_paladin": return TowerAttackAnimationStyle.HolySmite;
                case "hero_archer": return TowerAttackAnimationStyle.ArrowRelease;
                case "hero_mage": return TowerAttackAnimationStyle.ArcaneCast;
                case "hero_druid": return TowerAttackAnimationStyle.NatureCast;
                case "hero_sorcerer": return TowerAttackAnimationStyle.ShadowBurst;
            }

            switch (damageType)
            {
                case "steel": return TowerAttackAnimationStyle.MeleeLunge;
                case "holy": return TowerAttackAnimationStyle.HolySmite;
                case "piercing": return TowerAttackAnimationStyle.ArrowRelease;
                case "nature": return TowerAttackAnimationStyle.NatureCast;
                case "shadow": return TowerAttackAnimationStyle.ShadowBurst;
                default: return TowerAttackAnimationStyle.MechanicalRecoil;
            }
        }
    }
}
