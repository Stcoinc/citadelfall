using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using ClubGamerZone.TowerDefense.Domain.Content;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [CreateAssetMenu(menuName = "Tower Defense/Content/Level Definition", fileName = "LevelDefinition")]
    public sealed class LevelDefinitionAsset : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayNameKey;
        [SerializeField] private GameMode _mode = GameMode.ClassicPathDefense;

        [Header("Mission")]
        [SerializeField] private int _startingScrap = 100;
        [SerializeField] private int _baseHealth = 20;
        [SerializeField] private int _buildSocketCount = 6;
        [SerializeField] private WaveSetDefinitionAsset _waveSet;
        [Tooltip("Optional editor template. Runtime paths and build sockets remain authored in the Unity gameplay scene.")]
        [SerializeField] private BattlefieldLayoutAsset _battlefieldLayout;

        [Header("Rewards")]
        [SerializeField] private int _rewardScrap;
        [SerializeField] private int _rewardCoins;
        [SerializeField] private int _rewardGems;
        [SerializeField] private string _rewardItemId;

        public string Id => _id;

        public BattlefieldLayoutAsset BattlefieldLayout => _battlefieldLayout;

        public void Configure(LevelDto dto, WaveSetDefinitionAsset waveSet, BattlefieldLayoutAsset battlefieldLayout)
        {
            if (dto == null)
            {
                return;
            }

            _id = dto.Id;
            _displayNameKey = dto.DisplayNameKey;
            if (!System.Enum.TryParse(dto.Mode, true, out _mode))
            {
                _mode = GameMode.ClassicPathDefense;
            }

            _startingScrap = dto.StartingScrap;
            _baseHealth = dto.BaseHealth;
            _buildSocketCount = dto.BuildSocketCount;
            _waveSet = waveSet;
            _battlefieldLayout = battlefieldLayout;
            _rewardScrap = dto.RewardScrap;
            _rewardCoins = dto.RewardCoins;
            _rewardGems = dto.RewardGems;
            _rewardItemId = dto.RewardItemId;
        }

        public LevelDto ToDto()
        {
            return new LevelDto
            {
                Id = _id,
                DisplayNameKey = _displayNameKey,
                Mode = _mode.ToString(),
                StartingScrap = _startingScrap,
                BaseHealth = _baseHealth,
                BuildSocketCount = _buildSocketCount,
                WaveSetId = _waveSet == null ? string.Empty : _waveSet.Id,
                RewardScrap = _rewardScrap,
                RewardCoins = _rewardCoins,
                RewardGems = _rewardGems,
                RewardItemId = _rewardItemId
            };
        }
    }
}
