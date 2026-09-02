namespace ClubGamerZone.TowerDefense.Application.Authentication
{
    public sealed class AuthenticationResult
    {
        public AuthenticationResult(AuthenticationState state, AuthenticatedPlayer player, string message)
        {
            State = state;
            Player = player;
            Message = message ?? string.Empty;
        }

        public AuthenticationState State { get; }

        public AuthenticatedPlayer Player { get; }

        public string Message { get; }

        public bool IsSuccess => State == AuthenticationState.Authenticated && Player != null;

        public static AuthenticationResult Success(AuthenticatedPlayer player, string message)
        {
            return new AuthenticationResult(AuthenticationState.Authenticated, player, message);
        }

        public static AuthenticationResult Failure(string message)
        {
            return new AuthenticationResult(AuthenticationState.Failed, null, message);
        }
    }
}
