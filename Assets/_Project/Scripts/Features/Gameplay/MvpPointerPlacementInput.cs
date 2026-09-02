using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class MvpPointerPlacementInput : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private TowerDetailsPanel _detailsPanel;
        [SerializeField] private float _dragThresholdPixels = 18f;
        [SerializeField] private MergeInteractionFeedback _mergeFeedback;

        private TowerPlacementSocket _pressedSocket;
        private Vector2 _pressPosition;
        private bool _isDragging;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_mergeFeedback == null)
            {
                _mergeFeedback = GetComponent<MergeInteractionFeedback>();
            }

            if (_mergeFeedback == null)
            {
                Debug.LogWarning($"{nameof(MvpPointerPlacementInput)} has no authored {nameof(MergeInteractionFeedback)} on {name}; merge feedback is disabled.", this);
            }
        }

        private void Update()
        {
            if (_camera == null)
            {
                return;
            }

            if (TryGetPointerPress(out var screenPosition, out var pointerId))
            {
                if (!IsPointerOverUi(pointerId))
                {
                    BeginPointerInteraction(screenPosition);
                }
            }

            if (_pressedSocket != null && TryGetPointerPosition(out screenPosition))
            {
                if (!_isDragging && Vector2.Distance(_pressPosition, screenPosition) >= _dragThresholdPixels)
                {
                    _isDragging = true;
                    _detailsPanel?.Hide();
                    _pressedSocket.BeginDragPreview();
                }

                if (_isDragging)
                {
                    _pressedSocket.MoveDragPreview(_camera.ScreenToWorldPoint(screenPosition));
                }
            }

            if (_pressedSocket != null && TryGetPointerRelease(out screenPosition))
            {
                CompletePointerInteraction(screenPosition);
            }
        }

        private static bool TryGetPointerPress(out Vector2 screenPosition, out int pointerId)
        {
            if (Touchscreen.current != null)
            {
                var primaryTouch = Touchscreen.current.primaryTouch;

                if (primaryTouch.press.wasPressedThisFrame)
                {
                    screenPosition = primaryTouch.position.ReadValue();
                    pointerId = primaryTouch.touchId.ReadValue();
                    return true;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                pointerId = -1;
                return true;
            }

            screenPosition = default;
            pointerId = -1;
            return false;
        }

        private static bool TryGetPointerPosition(out Vector2 screenPosition)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        private static bool TryGetPointerRelease(out Vector2 screenPosition)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        private static bool IsPointerOverUi(int pointerId)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return pointerId < 0
                ? EventSystem.current.IsPointerOverGameObject()
                : EventSystem.current.IsPointerOverGameObject(pointerId);
        }

        private void BeginPointerInteraction(Vector2 screenPosition)
        {
            var socket = FindSocket(screenPosition);

            if (socket != null)
            {
                if (socket.IsOccupied)
                {
                    _pressedSocket = socket;
                    _pressPosition = screenPosition;
                    _isDragging = false;
                }
                else
                {
                    socket.HandleSelection();
                }
                return;
            }

            var hit = FindHit(screenPosition);
            if (hit == null)
            {
                return;
            }

            var enemy = hit.GetComponent<EnemyAgent>();

            if (enemy != null && _detailsPanel != null)
            {
                _detailsPanel.ShowEnemy(enemy);
            }
        }

        private void CompletePointerInteraction(Vector2 screenPosition)
        {
            var source = _pressedSocket;
            _pressedSocket = null;
            if (source == null)
            {
                return;
            }

            if (!_isDragging)
            {
                source.HandleSelection();
                return;
            }

            var target = FindSocket(screenPosition);
            if (target == null || target == source || !target.IsOccupied)
            {
                source.ReturnDragPreview();
                _mergeFeedback?.PlayInvalid();
                return;
            }

            if (target.TryMergeWith(source, false))
            {
                _detailsPanel?.Hide();
                _mergeFeedback?.PlayMerge(target.ActiveTowerTransform);
            }
            else
            {
                source.ReturnDragPreview();
                _mergeFeedback?.PlayInvalid();
            }
        }

        private Collider2D FindHit(Vector2 screenPosition)
        {
            var worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            return Physics2D.OverlapPoint(worldPosition);
        }

        private TowerPlacementSocket FindSocket(Vector2 screenPosition)
        {
            var worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            foreach (var hit in Physics2D.OverlapPointAll(worldPosition))
            {
                var socket = hit.GetComponent<TowerPlacementSocket>() ?? hit.GetComponentInParent<TowerPlacementSocket>();
                if (socket != null)
                {
                    return socket;
                }
            }

            return null;
        }
    }
}
