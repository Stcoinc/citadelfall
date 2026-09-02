using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClubGamerZone.TowerDefense.Application.Authentication;
using ClubGamerZone.TowerDefense.Application.Networking;
using ClubGamerZone.TowerDefense.Infrastructure.Firebase.Dtos;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Infrastructure.Firebase
{
    public sealed class FirebaseAuthenticationService : IAuthenticationService, IPlayerProgressionService
    {
        private enum UsernameReservationResult
        {
            Reserved,
            Taken,
            Unavailable
        }

        private sealed class UsernameReservationAttempt
        {
            public UsernameReservationAttempt(UsernameReservationResult result, string errorMessage = "")
            {
                Result = result;
                ErrorMessage = errorMessage ?? string.Empty;
            }

            public UsernameReservationResult Result { get; }

            public string ErrorMessage { get; }
        }

        private static readonly Regex UsernameRegex = new Regex("^[a-zA-Z0-9_]{3,20}$", RegexOptions.Compiled);

        private readonly IRestClient _restClient;
        private readonly string _apiKey;
        private readonly string _databaseUrl;

        public FirebaseAuthenticationService(IRestClient restClient, string apiKey, string databaseUrl)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _apiKey = apiKey ?? string.Empty;
            _databaseUrl = databaseUrl ?? string.Empty;
        }

        public async Task<AuthenticationResult> RegisterAsync(string username, string avatarId, string email, string password, CancellationToken cancellationToken)
        {
            var normalizedUsername = NormalizeUsername(username);
            if (!IsUsernameValid(normalizedUsername))
            {
                return AuthenticationResult.Failure("Username must be 3-20 letters, numbers, or underscores.");
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                return AuthenticationResult.Failure("Enter a valid email address.");
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                return AuthenticationResult.Failure("Password must be at least 6 characters.");
            }

            var existingUsername = await LoadUsernameAsync(normalizedUsername, string.Empty, cancellationToken);
            if (existingUsername != null)
            {
                return AuthenticationResult.Failure("That username is already taken.");
            }

            var authResponse = await SendAuthRequestAsync("accounts:signUp", email, password, cancellationToken);
            if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.idToken))
            {
                return AuthenticationResult.Failure("Could not create account.");
            }

            var usernameRecord = new FirebaseUsernameRecordDto
            {
                userId = authResponse.localId,
                email = authResponse.email,
                username = normalizedUsername
            };
            var reservation = await TryReserveUsernameAsync(usernameRecord, authResponse.idToken, cancellationToken);
            var usernameReserved = reservation.Result == UsernameReservationResult.Reserved;
            if (!usernameReserved)
            {
                normalizedUsername = string.Empty;
            }

            var profile = new FirebasePlayerProfileDto
            {
                userId = authResponse.localId,
                username = normalizedUsername,
                email = authResponse.email,
                avatarId = avatarId ?? string.Empty,
                playerLevel = 1,
                createdAtUtc = DateTime.UtcNow.ToString("O")
            };
            await PutJsonAsync(BuildDatabaseUrl($"players/{authResponse.localId}/profile", authResponse.idToken), JsonUtility.ToJson(profile), cancellationToken);
            var progression = new PlayerProgression();
            await SaveProgressionAsync(
                new AuthenticatedPlayer(authResponse.localId, authResponse.email, normalizedUsername, avatarId, authResponse.idToken, authResponse.refreshToken, false, 1, progression),
                cancellationToken);
            await SendEmailVerificationAsync(authResponse.idToken, cancellationToken);

            return AuthenticationResult.Success(
                new AuthenticatedPlayer(authResponse.localId, authResponse.email, normalizedUsername, avatarId, authResponse.idToken, authResponse.refreshToken, false, 1, progression),
                usernameReserved
                    ? "Account created. Check your email to verify it."
                    : reservation.Result == UsernameReservationResult.Taken
                        ? "Account created, but that username was claimed moments ago. Choose another permanent username in your profile."
                        : "Account created, but the username could not be reserved while offline. Choose it in your profile when connected.");
        }

        public async Task<AuthenticationResult> ClaimUsernameAsync(AuthenticatedPlayer player, string username, CancellationToken cancellationToken)
        {
            if (player == null || string.IsNullOrWhiteSpace(player.UserId) || string.IsNullOrWhiteSpace(player.IdToken))
            {
                return AuthenticationResult.Failure("Log in before choosing a username.");
            }

            if (!string.IsNullOrWhiteSpace(player.Username))
            {
                return AuthenticationResult.Failure("Your permanent username has already been set.");
            }

            var normalizedUsername = NormalizeUsername(username);
            if (!IsUsernameValid(normalizedUsername))
            {
                return AuthenticationResult.Failure("Username must be 3-20 letters, numbers, or underscores.");
            }

            var record = new FirebaseUsernameRecordDto
            {
                userId = player.UserId,
                email = player.Email,
                username = normalizedUsername
            };
            var reservation = await TryReserveUsernameAsync(record, player.IdToken, cancellationToken);
            if (reservation.Result != UsernameReservationResult.Reserved)
            {
                return AuthenticationResult.Failure(
                    reservation.Result == UsernameReservationResult.Taken
                        ? "That username is already taken."
                        : string.IsNullOrWhiteSpace(reservation.ErrorMessage)
                            ? "Username availability could not be checked. Connect to the internet and try again."
                            : reservation.ErrorMessage);
            }

            var profile = await LoadPlayerProfileAsync(player.UserId, player.IdToken, cancellationToken) ?? new FirebasePlayerProfileDto
            {
                userId = player.UserId,
                email = player.Email,
                avatarId = player.AvatarId,
                playerLevel = player.PlayerLevel,
                createdAtUtc = DateTime.UtcNow.ToString("O")
            };
            profile.username = normalizedUsername;
            var profileResponse = await PutJsonAsync(
                BuildDatabaseUrl($"players/{player.UserId}/profile", player.IdToken),
                JsonUtility.ToJson(profile),
                cancellationToken);
            if (!profileResponse.IsSuccess)
            {
                return AuthenticationResult.Failure("The username was reserved, but the profile could not be updated. Try again while online.");
            }

            return AuthenticationResult.Success(player.WithUsername(normalizedUsername), "Permanent username set.");
        }

        public async Task<AuthenticationResult> SignInWithEmailAsync(string email, string password, CancellationToken cancellationToken)
        {
            var authResponse = await SendAuthRequestAsync("accounts:signInWithPassword", email, password, cancellationToken);
            if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.idToken))
            {
                return AuthenticationResult.Failure("Login failed.");
            }

            var profile = await LoadPlayerProfileAsync(authResponse.localId, authResponse.idToken, cancellationToken);
            var progression = await LoadPlayerProgressionAsync(authResponse.localId, authResponse.idToken, cancellationToken);
            var username = profile == null ? string.Empty : profile.username;
            var avatarId = profile == null ? string.Empty : profile.avatarId;
            var playerLevel = progression == null
                ? (profile == null || profile.playerLevel < 1 ? 1 : profile.playerLevel)
                : progression.Level;

            return AuthenticationResult.Success(
                new AuthenticatedPlayer(authResponse.localId, authResponse.email, username, avatarId, authResponse.idToken, authResponse.refreshToken, false, playerLevel, progression),
                "Logged in.");
        }

        public async Task<AuthenticationResult> SignInWithUsernameAsync(string username, string password, CancellationToken cancellationToken)
        {
            var normalizedUsername = NormalizeUsername(username);
            var record = await LoadUsernameAsync(normalizedUsername, string.Empty, cancellationToken);
            if (record == null || string.IsNullOrWhiteSpace(record.email))
            {
                return AuthenticationResult.Failure("Username was not found.");
            }

            var result = await SignInWithEmailAsync(record.email, password, cancellationToken);
            if (!result.IsSuccess || !string.IsNullOrWhiteSpace(result.Player.Username))
            {
                return result;
            }

            var player = result.Player;
            return AuthenticationResult.Success(
                new AuthenticatedPlayer(
                    player.UserId,
                    player.Email,
                    normalizedUsername,
                    player.AvatarId,
                    player.IdToken,
                    player.RefreshToken,
                    player.EmailVerified,
                    player.PlayerLevel,
                    player.Progression),
                result.Message);
        }

        public async Task<AuthenticationResult> RestoreSessionAsync(string refreshToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(_apiKey))
            {
                return AuthenticationResult.Failure("No saved session.");
            }

            var url = $"https://securetoken.googleapis.com/v1/token?key={Uri.EscapeDataString(_apiKey)}";
            var body = $"grant_type=refresh_token&refresh_token={Uri.EscapeDataString(refreshToken)}";
            var response = await _restClient.SendAsync(
                new RestRequest(
                    RestHttpMethod.Post,
                    url,
                    body,
                    new Dictionary<string, string>
                    {
                        { "Content-Type", "application/x-www-form-urlencoded" }
                    }),
                cancellationToken);
            if (!response.IsSuccess || string.IsNullOrWhiteSpace(response.Body))
            {
                return AuthenticationResult.Failure("Saved session could not be restored.");
            }

            var token = JsonUtility.FromJson<FirebaseRefreshTokenResponseDto>(response.Body);
            if (token == null || string.IsNullOrWhiteSpace(token.id_token) || string.IsNullOrWhiteSpace(token.user_id))
            {
                return AuthenticationResult.Failure("Saved session is invalid.");
            }

            var profile = await LoadPlayerProfileAsync(token.user_id, token.id_token, cancellationToken);
            var progression = await LoadPlayerProgressionAsync(token.user_id, token.id_token, cancellationToken);
            var username = profile == null ? string.Empty : profile.username;
            var avatarId = profile == null ? string.Empty : profile.avatarId;
            var email = profile == null ? string.Empty : profile.email;
            var rotatedRefreshToken = string.IsNullOrWhiteSpace(token.refresh_token) ? refreshToken : token.refresh_token;
            return AuthenticationResult.Success(
                new AuthenticatedPlayer(
                    token.user_id,
                    email,
                    username,
                    avatarId,
                    token.id_token,
                    rotatedRefreshToken,
                    false,
                    progression.Level,
                    progression),
                "Session restored.");
        }

        private async Task<FirebaseAuthResponseDto> SendAuthRequestAsync(string methodName, string email, string password, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return null;
            }

            var url = $"https://identitytoolkit.googleapis.com/v1/{methodName}?key={Uri.EscapeDataString(_apiKey)}";
            var body = JsonUtility.ToJson(new FirebaseAuthRequestDto
            {
                email = email,
                password = password,
                returnSecureToken = true
            });
            var response = await _restClient.SendAsync(new RestRequest(RestHttpMethod.Post, url, body, null), cancellationToken);
            return response.IsSuccess ? JsonUtility.FromJson<FirebaseAuthResponseDto>(response.Body) : null;
        }

        private async Task SendEmailVerificationAsync(string idToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(idToken))
            {
                return;
            }

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={Uri.EscapeDataString(_apiKey)}";
            var body = JsonUtility.ToJson(new FirebaseEmailVerificationRequestDto { idToken = idToken });
            await _restClient.SendAsync(new RestRequest(RestHttpMethod.Post, url, body, null), cancellationToken);
        }

        private async Task<FirebaseUsernameRecordDto> LoadUsernameAsync(string normalizedUsername, string idToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(_databaseUrl))
            {
                return null;
            }

            var response = await _restClient.SendAsync(
                new RestRequest(RestHttpMethod.Get, BuildDatabaseUrl($"usernames/{normalizedUsername}", idToken), string.Empty, null),
                cancellationToken);

            return response.IsSuccess && !string.IsNullOrWhiteSpace(response.Body) && response.Body != "null"
                ? JsonUtility.FromJson<FirebaseUsernameRecordDto>(response.Body)
                : null;
        }

        private async Task<FirebasePlayerProfileDto> LoadPlayerProfileAsync(string userId, string idToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(_databaseUrl))
            {
                return null;
            }

            var response = await _restClient.SendAsync(
                new RestRequest(RestHttpMethod.Get, BuildDatabaseUrl($"players/{userId}/profile", idToken), string.Empty, null),
                cancellationToken);

            return response.IsSuccess && !string.IsNullOrWhiteSpace(response.Body) && response.Body != "null"
                ? JsonUtility.FromJson<FirebasePlayerProfileDto>(response.Body)
                : null;
        }

        private async Task<PlayerProgression> LoadPlayerProgressionAsync(string userId, string idToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(_databaseUrl))
            {
                return new PlayerProgression();
            }

            var response = await _restClient.SendAsync(
                new RestRequest(RestHttpMethod.Get, BuildDatabaseUrl($"players/{userId}/progression", idToken), string.Empty, null),
                cancellationToken);
            if (!response.IsSuccess || string.IsNullOrWhiteSpace(response.Body) || response.Body == "null")
            {
                return new PlayerProgression();
            }

            var dto = JsonUtility.FromJson<FirebasePlayerProgressionDto>(response.Body);
            if (dto == null)
            {
                return new PlayerProgression();
            }

            var defeats = dto.enemyDefeats == null
                ? Array.Empty<EnemyDefeatProgress>()
                : dto.enemyDefeats
                    .Where(entry => entry != null)
                    .Select(entry => new EnemyDefeatProgress(entry.enemyId, entry.defeats))
                    .ToArray();
            return new PlayerProgression(
                dto.experience,
                dto.unlockedTowerIds,
                defeats,
                dto.unlockedCardIds,
                dto.scrap,
                dto.coins,
                dto.currenciesInitialized,
                dto.bestEndlessScore,
                dto.highestEndlessRound);
        }

        public async Task SaveProgressionAsync(AuthenticatedPlayer player, CancellationToken cancellationToken)
        {
            if (player == null || player.Progression == null || string.IsNullOrWhiteSpace(player.UserId))
            {
                return;
            }

            var progression = player.Progression;
            var dto = new FirebasePlayerProgressionDto
            {
                experience = progression.Experience,
                level = progression.Level,
                unlockedTowerIds = progression.UnlockedTowerIds,
                enemyDefeats = progression.EnemyDefeats
                    .Where(entry => entry != null)
                    .Select(entry => new FirebaseEnemyDefeatProgressDto { enemyId = entry.EnemyId, defeats = entry.Defeats })
                    .ToArray(),
                unlockedCardIds = progression.UnlockedCardIds,
                scrap = progression.Scrap,
                coins = progression.Coins,
                currenciesInitialized = progression.CurrenciesInitialized,
                bestEndlessScore = progression.BestEndlessScore,
                highestEndlessRound = progression.HighestEndlessRound,
                updatedAtUtc = DateTime.UtcNow.ToString("O")
            };
            await PutJsonAsync(
                BuildDatabaseUrl($"players/{player.UserId}/progression", player.IdToken),
                JsonUtility.ToJson(dto),
                cancellationToken);
        }

        private async Task<UsernameReservationAttempt> TryReserveUsernameAsync(FirebaseUsernameRecordDto record, string idToken, CancellationToken cancellationToken)
        {
            var existing = await LoadUsernameAsync(record.username, idToken, cancellationToken);
            if (existing != null)
            {
                return new UsernameReservationAttempt(
                    string.Equals(existing.userId, record.userId, StringComparison.Ordinal)
                        ? UsernameReservationResult.Reserved
                        : UsernameReservationResult.Taken);
            }

            var response = await _restClient.SendAsync(
                new RestRequest(
                    RestHttpMethod.Put,
                    BuildDatabaseUrl($"usernames/{record.username}", idToken),
                    JsonUtility.ToJson(record),
                    new Dictionary<string, string> { { "if-match", "null_etag" } }),
                cancellationToken);
            if (response.IsSuccess)
            {
                return new UsernameReservationAttempt(UsernameReservationResult.Reserved);
            }

            if (response.StatusCode != 412)
            {
                var permissionDenied = response.StatusCode == 401 || response.StatusCode == 403;
                return new UsernameReservationAttempt(
                    UsernameReservationResult.Unavailable,
                    permissionDenied
                        ? "Firebase blocked username creation. Deploy the authenticated /usernames rules, then try again."
                        : $"Username service is unavailable (HTTP {response.StatusCode}). Connect to the internet and try again.");
            }

            var winner = !string.IsNullOrWhiteSpace(response.Body) && response.Body != "null"
                ? JsonUtility.FromJson<FirebaseUsernameRecordDto>(response.Body)
                : await LoadUsernameAsync(record.username, idToken, cancellationToken);
            return new UsernameReservationAttempt(
                winner != null && string.Equals(winner.userId, record.userId, StringComparison.Ordinal)
                    ? UsernameReservationResult.Reserved
                    : UsernameReservationResult.Taken);
        }

        private async Task<RestResponse> PutJsonAsync(string url, string body, CancellationToken cancellationToken)
        {
            return await _restClient.SendAsync(new RestRequest(RestHttpMethod.Put, url, body, null), cancellationToken);
        }

        private string BuildDatabaseUrl(string path, string idToken)
        {
            var url = $"{_databaseUrl.TrimEnd('/')}/{path}.json";
            return string.IsNullOrWhiteSpace(idToken) ? url : $"{url}?auth={Uri.EscapeDataString(idToken)}";
        }

        private static string NormalizeUsername(string username)
        {
            return string.IsNullOrWhiteSpace(username) ? string.Empty : username.Trim().ToLowerInvariant();
        }

        private static bool IsUsernameValid(string username)
        {
            return !string.IsNullOrWhiteSpace(username) && UsernameRegex.IsMatch(username);
        }
    }
}
