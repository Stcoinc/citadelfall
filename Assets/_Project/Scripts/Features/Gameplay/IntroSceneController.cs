using System;
using System.Linq;
using System.Threading;
using ClubGamerZone.TowerDefense.Application.Configuration;
using ClubGamerZone.TowerDefense.Application.Networking;
using ClubGamerZone.TowerDefense.Domain.Content;
using ClubGamerZone.TowerDefense.Infrastructure.Http;
using ClubGamerZone.TowerDefense.Infrastructure.Json;
using ClubGamerZone.TowerDefense.Application.Authentication;
using ClubGamerZone.TowerDefense.Infrastructure.Firebase;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class IntroSceneController : MonoBehaviour
    {
        [Header("Firebase")]
        [SerializeField] private string _firebaseApiKey = "AIzaSyBYdIoRXpL3P2ViSd2t1R5WZc1LJY5kVzI";
        [SerializeField] private string _databaseUrl = "https://tower-defense-engine-default-rtdb.firebaseio.com";
        [SerializeField] private string _settingsEnvironment = "development";

        [Header("Content")]
        [SerializeField] private TextAsset _localContentJson;

        [Header("UI")]
        [SerializeField] private TMP_Text _statusText;

        private readonly IRestClient _restClient = new UnityRestClient();
        private CancellationTokenSource _loadCancellation;

        private async void Start()
        {
            SetStatus("Loading game data...");
            _loadCancellation = new CancellationTokenSource();
            _loadCancellation.CancelAfter(TimeSpan.FromSeconds(5));

            var contentJson = _localContentJson == null ? string.Empty : _localContentJson.text;
            var loadedRemote = await TryLoadRemoteContentAsync(_loadCancellation.Token);
            if (!string.IsNullOrWhiteSpace(loadedRemote))
            {
                contentJson = loadedRemote;
            }

            if (!TryBuildCatalog(contentJson, out var catalog))
            {
                SetStatus("Content is invalid. Check console.");
                return;
            }

            AppRuntimeSession.SetContent(contentJson, catalog, !string.IsNullOrWhiteSpace(loadedRemote));
            _loadCancellation.Dispose();
            _loadCancellation = new CancellationTokenSource();
            _loadCancellation.CancelAfter(TimeSpan.FromSeconds(8));
            var restoredSession = await TryRestoreAuthenticationAsync(_loadCancellation.Token);
            SetStatus(
                restoredSession
                    ? "Welcome back."
                    : AppRuntimeSession.LoadedRemoteContent ? "Cloud balance loaded." : "Offline defaults loaded.");
            Destroy(this);
            SceneManager.LoadScene(AppSceneNames.MainMenu);
        }

        private void OnDestroy()
        {
            _loadCancellation?.Cancel();
            _loadCancellation?.Dispose();
        }

        private async System.Threading.Tasks.Task<string> TryLoadRemoteContentAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_databaseUrl))
            {
                return string.Empty;
            }

            try
            {
                var baseUrl = _databaseUrl.TrimEnd('/');
                var activeVersionUrl = $"{baseUrl}/gameSettings/{_settingsEnvironment}/activeVersion.json";
                var activeResponse = await _restClient.SendAsync(
                    new RestRequest(RestHttpMethod.Get, activeVersionUrl, string.Empty, null),
                    cancellationToken);

                if (!activeResponse.IsSuccess || string.IsNullOrWhiteSpace(activeResponse.Body) || activeResponse.Body == "null")
                {
                    return string.Empty;
                }

                var activeVersion = activeResponse.Body.Trim().Trim('"');
                var contentUrl = $"{baseUrl}/gameSettings/{_settingsEnvironment}/versions/{activeVersion}/starterContent.json";
                var contentResponse = await _restClient.SendAsync(
                    new RestRequest(RestHttpMethod.Get, contentUrl, string.Empty, null),
                    cancellationToken);

                return contentResponse.IsSuccess && !string.IsNullOrWhiteSpace(contentResponse.Body) && contentResponse.Body != "null"
                    ? contentResponse.Body
                    : string.Empty;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("Remote settings timed out, using local defaults.");
                return string.Empty;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Remote settings unavailable, using local defaults. {exception.Message}");
                return string.Empty;
            }
        }

        private async System.Threading.Tasks.Task<bool> TryRestoreAuthenticationAsync(CancellationToken cancellationToken)
        {
            var sessionStore = new ProtectedAuthenticationSessionStore();
            var refreshToken = sessionStore.LoadRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            SetStatus("Restoring your account...");
            try
            {
                var firebaseService = new FirebaseAuthenticationService(new UnityRestClient(), _firebaseApiKey, _databaseUrl);
                AppRuntimeSession.SetPlayerProgressionService(firebaseService);
                var result = await firebaseService.RestoreSessionAsync(refreshToken, cancellationToken);
                if (result == null || !result.IsSuccess)
                {
                    Debug.LogWarning(result == null ? "Saved session could not be restored." : result.Message);
                    return false;
                }

                AppRuntimeSession.SetAuthenticatedPlayer(result.Player);
                sessionStore.SaveRefreshToken(result.Player.RefreshToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("Saved session restore timed out; continuing logged out.");
                return false;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Saved session restore failed; continuing logged out. {exception.Message}");
                return false;
            }
        }

        private static bool TryBuildCatalog(string contentJson, out ContentCatalog catalog)
        {
            var parser = new UnityContentJsonParser();
            var validator = new StarterContentValidator(new ContentValidationLimits());
            var builder = new StarterContentCatalogBuilder(validator);
            var result = builder.Build(parser.Parse(contentJson));

            catalog = result.Catalog;
            if (result.IsSuccess)
            {
                return true;
            }

            Debug.LogError(string.Join("\n", result.Validation.Issues.Select(issue => issue.ToString())));
            return false;
        }

        private void SetStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }
        }
    }
}
