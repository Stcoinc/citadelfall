using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    internal static class TransformWorldScaleUtility
    {
        public static void SetUniform(Transform target, float worldScale)
        {
            if (target == null)
            {
                return;
            }

            var desiredScale = Mathf.Max(0f, worldScale);
            if (target.parent == null)
            {
                target.localScale = Vector3.one * desiredScale;
                return;
            }

            var parentScale = target.parent.lossyScale;
            target.localScale = new Vector3(
                DivideByScale(desiredScale, parentScale.x),
                DivideByScale(desiredScale, parentScale.y),
                DivideByScale(desiredScale, parentScale.z));
        }

        private static float DivideByScale(float value, float parentScale)
        {
            return value / Mathf.Max(0.0001f, Mathf.Abs(parentScale));
        }
    }
}
