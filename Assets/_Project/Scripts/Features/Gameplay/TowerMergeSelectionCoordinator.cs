namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    public sealed class TowerMergeSelectionCoordinator
    {
        private TowerPlacementSocket _sourceSocket;

        public bool IsSelecting => _sourceSocket != null;

        public TowerPlacementSocket SourceSocket => _sourceSocket;

        public void Begin(TowerPlacementSocket sourceSocket)
        {
            _sourceSocket = sourceSocket;
        }

        public void ShowSourceMessage(string message)
        {
            if (_sourceSocket != null)
            {
                _sourceSocket.ShowDetails(message);
            }
        }

        public bool TryResolve(TowerPlacementSocket targetSocket)
        {
            if (_sourceSocket == null)
            {
                return false;
            }

            if (targetSocket == null)
            {
                ShowSourceMessage("Choose another tower with the same type and level.");
                return false;
            }

            if (ReferenceEquals(_sourceSocket, targetSocket))
            {
                ShowSourceMessage("That is the selected tower. Choose a second matching tower.");
                return false;
            }

            var source = _sourceSocket;
            _sourceSocket = null;
            return source.TryMergeWith(targetSocket);
        }

        public void Cancel()
        {
            _sourceSocket = null;
        }
    }
}
