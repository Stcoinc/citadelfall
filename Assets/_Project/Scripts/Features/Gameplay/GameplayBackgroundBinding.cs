using System;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [Serializable]
    public sealed class GameplayBackgroundBinding
    {
        [SerializeField] private string _id;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _tint = Color.white;

        public string Id => _id;

        public Sprite Sprite => _sprite;

        public Color Tint => _tint;
    }
}
