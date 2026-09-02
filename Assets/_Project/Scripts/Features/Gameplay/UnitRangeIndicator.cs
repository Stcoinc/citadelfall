using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class UnitRangeIndicator : MonoBehaviour
    {
        private const int SegmentCount = 96;

        [SerializeField] private LineRenderer _lineRenderer;

        public float WorldRange { get; private set; }

        public void Show(float worldRange)
        {
            WorldRange = Mathf.Max(0f, worldRange);
            gameObject.SetActive(WorldRange > 0f);

            if (WorldRange <= 0f || _lineRenderer == null)
            {
                return;
            }

            _lineRenderer.positionCount = SegmentCount;
            _lineRenderer.loop = true;
            _lineRenderer.useWorldSpace = false;

            var parentScale = transform.parent == null ? Vector3.one : transform.parent.lossyScale;
            var inverseScaleX = 1f / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x));
            var inverseScaleY = 1f / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y));

            for (var index = 0; index < SegmentCount; index++)
            {
                var angle = index * Mathf.PI * 2f / SegmentCount;
                _lineRenderer.SetPosition(index, new Vector3(
                    Mathf.Cos(angle) * WorldRange * inverseScaleX,
                    Mathf.Sin(angle) * WorldRange * inverseScaleY,
                    0f));
            }
        }

        public void Hide()
        {
            WorldRange = 0f;
            gameObject.SetActive(false);
        }
    }
}
