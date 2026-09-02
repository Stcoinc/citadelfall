using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [CreateAssetMenu(menuName = "Tower Defense/Content/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinitionAsset : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayNameKey;
        [SerializeField] private string _prefabId;
        [SerializeField] private GameObject _prefab;

        [Header("Gameplay")]
        [SerializeField] private float _maxHealth = 50f;
        [SerializeField] private float _movementSpeed = 3f;
        [SerializeField] private int _contactDamage = 1;
        [SerializeField] private int _rewardScrap = 5;
        [SerializeField] private int _threatValue = 1;
        [SerializeField] private string _enemyTag = "scout";
        [SerializeField] private string _weakToDamageType = "laser";

        public string Id => _id;

        public GameObject Prefab => _prefab;

        public void Configure(EnemyDto dto, GameObject prefab = null)
        {
            if (dto == null)
            {
                return;
            }

            _id = dto.Id;
            _displayNameKey = dto.DisplayNameKey;
            _maxHealth = dto.MaxHealth;
            _movementSpeed = dto.MovementSpeed;
            _contactDamage = dto.ContactDamage;
            _rewardScrap = dto.RewardScrap;
            _threatValue = dto.ThreatValue;
            _enemyTag = dto.EnemyTag;
            _weakToDamageType = dto.WeakToDamageType;
            _prefabId = dto.PrefabId;
            _prefab = prefab;
        }

        public EnemyDto ToDto()
        {
            return new EnemyDto
            {
                Id = _id,
                DisplayNameKey = _displayNameKey,
                MaxHealth = _maxHealth,
                MovementSpeed = _movementSpeed,
                ContactDamage = _contactDamage,
                RewardScrap = _rewardScrap,
                ThreatValue = _threatValue,
                EnemyTag = _enemyTag,
                WeakToDamageType = _weakToDamageType,
                PrefabId = _prefabId
            };
        }
    }
}
