using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    public static class HeroCombatPalette
    {
        public static Color GetColor(string damageType)
        {
            switch (damageType)
            {
                case "arcane": return new Color32(74, 201, 255, 255);
                case "steel": return new Color32(226, 235, 242, 255);
                case "holy": return new Color32(255, 210, 74, 255);
                case "piercing": return new Color32(255, 146, 54, 255);
                case "nature": return new Color32(91, 220, 105, 255);
                case "shadow": return new Color32(185, 88, 255, 255);
                default: return Color.white;
            }
        }

        public static float GetProjectileScale(string damageType)
        {
            switch (damageType)
            {
                case "steel": return 0.8f;
                case "piercing": return 0.9f;
                case "holy": return 1.15f;
                case "nature": return 1.1f;
                case "shadow": return 1.3f;
                default: return 1f;
            }
        }
    }
}
