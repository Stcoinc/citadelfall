using System;
using System.Linq;
using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [Serializable]
    public sealed class WaveSpawnAuthoringData
    {
        [SerializeField] private EnemyDefinitionAsset _enemy;
        [SerializeField, Min(1)] private int _count = 1;
        [SerializeField, Min(0.01f)] private float _intervalSeconds = 1f;

        public EnemyDefinitionAsset Enemy => _enemy;

        public void Configure(EnemyDefinitionAsset enemy, int count, float intervalSeconds)
        {
            _enemy = enemy;
            _count = count;
            _intervalSeconds = intervalSeconds;
        }

        public WaveSpawnDto ToDto()
        {
            return new WaveSpawnDto
            {
                EnemyId = _enemy == null ? string.Empty : _enemy.Id,
                Count = _count,
                IntervalSeconds = _intervalSeconds
            };
        }
    }

    [Serializable]
    public sealed class WaveAuthoringData
    {
        [SerializeField] private string _id;
        [SerializeField, Min(0f)] private float _startDelaySeconds = 1f;
        [SerializeField] private WaveSpawnAuthoringData[] _spawns;

        public void Configure(string id, float startDelaySeconds, WaveSpawnAuthoringData[] spawns)
        {
            _id = id;
            _startDelaySeconds = startDelaySeconds;
            _spawns = spawns;
        }

        public WaveDto ToDto()
        {
            return new WaveDto
            {
                Id = _id,
                StartDelaySeconds = _startDelaySeconds,
                Spawns = _spawns == null
                    ? Array.Empty<WaveSpawnDto>()
                    : _spawns.Where(spawn => spawn != null).Select(spawn => spawn.ToDto()).ToArray()
            };
        }
    }

    [CreateAssetMenu(menuName = "Tower Defense/Content/Wave Set Definition", fileName = "WaveSetDefinition")]
    public sealed class WaveSetDefinitionAsset : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;

        [Header("Encounter")]
        [Tooltip("Enable when this wave set contains a boss. The boss must be the final spawn group of the final wave and have Count 1.")]
        [SerializeField] private bool _hasBoss;

        [Header("Waves")]
        [SerializeField] private WaveAuthoringData[] _waves;

        public string Id => _id;

        public bool HasBoss => _hasBoss;

        public void Configure(string id, bool hasBoss, WaveAuthoringData[] waves)
        {
            _id = id;
            _hasBoss = hasBoss;
            _waves = waves;
        }

        public WaveSetDto ToDto()
        {
            return new WaveSetDto
            {
                Id = _id,
                HasBoss = _hasBoss,
                Waves = _waves == null
                    ? Array.Empty<WaveDto>()
                    : _waves.Where(wave => wave != null).Select(wave => wave.ToDto()).ToArray()
            };
        }
    }
}
