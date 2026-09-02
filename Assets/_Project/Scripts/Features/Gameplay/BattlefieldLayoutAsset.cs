using System;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [CreateAssetMenu(menuName = "Tower Defense/Content/Battlefield Layout", fileName = "BattlefieldLayout")]
    public sealed class BattlefieldLayoutAsset : ScriptableObject
    {
        [SerializeField] private string _id = "battlefield_default";
        [SerializeField] private Vector2[] _pathPoints = Array.Empty<Vector2>();
        [SerializeField] private Vector2[] _buildSocketPositions = Array.Empty<Vector2>();

        public string Id => _id;

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
            _id = string.IsNullOrWhiteSpace(id) ? "battlefield_default" : id;
            _pathPoints = pathPoints == null ? Array.Empty<Vector2>() : (Vector2[])pathPoints.Clone();
            _buildSocketPositions = buildSocketPositions == null
                ? Array.Empty<Vector2>()
                : (Vector2[])buildSocketPositions.Clone();
        }
    }
}
