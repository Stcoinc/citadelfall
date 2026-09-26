using System;
using System.Linq;
using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [CreateAssetMenu(menuName = "Tower Defense/Content/Battlefield Layout", fileName = "BattlefieldLayout")]
    public sealed class BattlefieldLayoutAsset : ScriptableObject
    {
        [SerializeField] private string _id = "battlefield_default";
        [SerializeField] private string _backgroundId = "background_forest_01";
        [SerializeField] private Vector2[] _pathPoints = Array.Empty<Vector2>();
        [SerializeField] private Vector2[] _buildSocketPositions = Array.Empty<Vector2>();

        public string Id => _id;

        public string BackgroundId => _backgroundId;

        public int PathPointCount => _pathPoints == null ? 0 : _pathPoints.Length;

        public int BuildSocketCount => _buildSocketPositions == null ? 0 : _buildSocketPositions.Length;

        public Vector2 GetPathPoint(int index)
        {
            return _pathPoints[index];
        }

        public Vector2 GetBuildSocketPosition(int index)
        {
            return _buildSocketPositions[index];
        }

        public void SetPathPoint(int index, Vector2 position)
        {
            _pathPoints[index] = position;
        }

        public void SetBuildSocketPosition(int index, Vector2 position)
        {
            _buildSocketPositions[index] = position;
        }

        public void Configure(string id, Vector2[] pathPoints, Vector2[] buildSocketPositions)
        {
            Configure(id, _backgroundId, pathPoints, buildSocketPositions);
        }

        public void Configure(string id, string backgroundId, Vector2[] pathPoints, Vector2[] buildSocketPositions)
        {
            _id = string.IsNullOrWhiteSpace(id) ? "battlefield_default" : id;
            _backgroundId = string.IsNullOrWhiteSpace(backgroundId) ? "background_forest_01" : backgroundId;
            _pathPoints = pathPoints == null ? Array.Empty<Vector2>() : (Vector2[])pathPoints.Clone();
            _buildSocketPositions = buildSocketPositions == null
                ? Array.Empty<Vector2>()
                : (Vector2[])buildSocketPositions.Clone();
        }

        public void Configure(BattlefieldDto dto)
        {
            if (dto == null)
            {
                return;
            }

            Configure(
                dto.Id,
                dto.BackgroundId,
                (dto.PathPoints ?? Array.Empty<BattlefieldPointDto>()).Select(ToVector2).ToArray(),
                (dto.BuildSocketPositions ?? Array.Empty<BattlefieldPointDto>()).Select(ToVector2).ToArray());
        }

        public BattlefieldDto ToDto()
        {
            return new BattlefieldDto
            {
                Id = _id,
                BackgroundId = _backgroundId,
                PathPoints = (_pathPoints ?? Array.Empty<Vector2>()).Select(ToDto).ToArray(),
                BuildSocketPositions = (_buildSocketPositions ?? Array.Empty<Vector2>()).Select(ToDto).ToArray()
            };
        }

        private static Vector2 ToVector2(BattlefieldPointDto point)
        {
            return point == null ? Vector2.zero : new Vector2(point.X, point.Y);
        }

        private static BattlefieldPointDto ToDto(Vector2 point)
        {
            return new BattlefieldPointDto { X = point.x, Y = point.y };
        }
    }
}
