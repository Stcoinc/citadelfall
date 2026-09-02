using ClubGamerZone.TowerDefense.Domain.Content;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class TurretSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _turretSprites;
        [SerializeField] private Sprite[] _heroSprites;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public void SetTower(TowerDefinition definition)
        {
            if (_spriteRenderer == null || definition == null)
            {
                return;
            }

            var heroIndex = ResolveHeroIndex(definition.Id.Value);
            if (heroIndex >= 0 && _heroSprites != null && heroIndex < _heroSprites.Length)
            {
                _spriteRenderer.sprite = _heroSprites[heroIndex];
                _spriteRenderer.color = Color.white;
                return;
            }

            if (_turretSprites == null || _turretSprites.Length == 0)
            {
                return;
            }

            var index = ResolveTowerIndex(definition.Id.Value, definition.DamageType);
            if (index >= 0 && index < _turretSprites.Length)
            {
                _spriteRenderer.sprite = _turretSprites[index];
                _spriteRenderer.color = Color.white;
            }
        }

        private static int ResolveHeroIndex(string towerId)
        {
            switch (towerId)
            {
                case "hero_mage": return 0;
                case "hero_warrior": return 1;
                case "hero_paladin": return 2;
                case "hero_archer": return 3;
                case "hero_druid": return 4;
                case "hero_sorcerer": return 5;
                default: return -1;
            }
        }

        private static int ResolveTowerIndex(string towerId, string damageType)
        {
            switch (towerId)
            {
                case "tower_laser_basic": return 0;
                case "tower_cannon_basic": return 1;
                case "tower_tesla_basic": return 2;
                case "tower_frost_basic": return 3;
                case "tower_sniper_basic": return 4;
                case "tower_poison_basic": return 5;
                case "tower_artillery_basic": return 6;
                default: return ResolveDamageTypeIndex(damageType);
            }
        }

        private static int ResolveDamageTypeIndex(string damageType)
        {
            switch (damageType)
            {
                case "laser": return 0;
                case "cannon": return 1;
                case "tesla": return 2;
                case "frost": return 3;
                case "sniper": return 4;
                case "poison": return 5;
                case "artillery": return 6;
                default: return -1;
            }
        }
    }
}
