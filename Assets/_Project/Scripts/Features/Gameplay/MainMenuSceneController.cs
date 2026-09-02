using System;
using System.Linq;
using System.Threading;
using ClubGamerZone.TowerDefense.Application.Authentication;
using ClubGamerZone.TowerDefense.Infrastructure.Firebase;
using ClubGamerZone.TowerDefense.Infrastructure.Http;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class MainMenuSceneController : MonoBehaviour
    {
        [Header("Firebase")]
        [SerializeField] private string _firebaseApiKey = "AIzaSyBYdIoRXpL3P2ViSd2t1R5WZc1LJY5kVzI";
        [SerializeField] private string _databaseUrl = "https://tower-defense-engine-default-rtdb.firebaseio.com";

        [Header("Authentication Panels")]
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _registerPanel;
        [SerializeField] private TMP_Text _authMessageText;
        [SerializeField] private TMP_InputField _loginIdentifierInput;
        [SerializeField] private TMP_InputField _loginPasswordInput;
        [SerializeField] private TMP_InputField _registerUsernameInput;
        [SerializeField] private TMP_InputField _registerEmailInput;
        [SerializeField] private TMP_InputField _registerPasswordInput;
        [SerializeField] private TMP_InputField _registerConfirmPasswordInput;
        [SerializeField] private Button[] _avatarButtons;
        [SerializeField] private string _selectedAvatarId = string.Empty;

        [Header("Authenticated Player")]
        [SerializeField] private GameObject _playerProfileCard;
        [SerializeField] private Image _playerAvatarImage;
        [SerializeField] private TMP_Text _playerUsernameText;
        [SerializeField] private TMP_Text _playerLevelText;
        [SerializeField] private GameObject _loginEntryButton;
        [SerializeField] private GameObject _registerEntryButton;
        [SerializeField] private GameObject _collectionEntryButton;
        [SerializeField] private Sprite[] _playerAvatarSprites;

        [Header("Profile Details")]
        [SerializeField] private GameObject _profileDetailsPanel;
        [SerializeField] private Image _profileDetailsAvatarImage;
        [SerializeField] private TMP_Text _profileDetailsUsernameText;
        [SerializeField] private TMP_Text _profileDetailsLevelText;
        [SerializeField] private TMP_Text _profileDetailsProgressText;
        [SerializeField] private TMP_Text _profileDetailsCollectionText;
        [SerializeField] private TMP_Text _profileDetailsAdventureText;
        [SerializeField] private TMP_Text _profileDetailsArenaText;
        [SerializeField] private GameObject _usernameClaimSection;
        [SerializeField] private TMP_InputField _usernameClaimInput;
        [SerializeField] private Button _usernameClaimButton;
        [SerializeField] private TMP_Text _usernameClaimMessageText;

        private static readonly string[] AvatarIds =
        {
            "avatar_01",
            "avatar_02",
            "avatar_03",
            "avatar_04",
            "avatar_05",
            "avatar_06",
            "avatar_07",
            "avatar_08"
        };

        private static readonly string[] AvatarDisplayNames =
        {
            "Warrior",
            "Archer",
            "Mage",
            "Paladin",
            "Druid",
            "Sorcerer",
            "Rogue",
            "Cleric"
        };

        private static readonly Color AvatarNormalColor = new Color32(30, 50, 62, 255);
        private static readonly Color AvatarHoverColor = new Color32(74, 114, 127, 255);
        private static readonly Color AvatarSelectedColor = new Color32(238, 190, 64, 255);

        private IAuthenticationService _authenticationService;
        private IAuthenticationSessionStore _authenticationSessionStore;
        private CancellationTokenSource _authCancellation;

        private void Awake()
        {
            var firebaseService = new FirebaseAuthenticationService(new UnityRestClient(), _firebaseApiKey, _databaseUrl);
            _authenticationService = firebaseService;
            _authenticationSessionStore = new ProtectedAuthenticationSessionStore();
            AppRuntimeSession.SetPlayerProgressionService(firebaseService);
        }

        private void Start()
        {
            if (!AppRuntimeSession.HasContent)
            {
                SceneManager.LoadScene(AppSceneNames.Intro);
                return;
            }

            HideAuthPanels();
            HideProfileDetails();
            RefreshAuthenticatedPlayerUi();
        }

        private void OnDestroy()
        {
            _authCancellation?.Cancel();
            _authCancellation?.Dispose();
        }

        public void NewGame()
        {
            AppRuntimeSession.SetSlotMode(false);
            SceneManager.LoadScene(AppSceneNames.LevelSelection);
        }

        public void ContinueGame()
        {
            AppRuntimeSession.SetSlotMode(true);
            SceneManager.LoadScene(AppSceneNames.LevelSelection);
        }

        public void PlayArena()
        {
            AppRuntimeSession.SetArenaMode();
            SceneManager.LoadScene(AppSceneNames.LevelSelection);
        }

        public void PlayEndless()
        {
            AppRuntimeSession.SetEndlessMode();
            SceneManager.LoadScene(AppSceneNames.LevelSelection);
        }

        public void ShowLoginPanel()
        {
            SetActive(_loginPanel, true);
            SetActive(_registerPanel, false);
            SetMessage(string.Empty);
        }

        public void ShowRegisterPanel()
        {
            SetActive(_loginPanel, false);
            SetActive(_registerPanel, true);
            _selectedAvatarId = string.Empty;
            UpdateAvatarSelectionVisuals();
            SetMessage(string.Empty);
        }

        public void HideAuthPanels()
        {
            SetActive(_loginPanel, false);
            SetActive(_registerPanel, false);
        }

        public void ShowProfileDetails()
        {
            var player = AppRuntimeSession.AuthenticatedPlayer;
            if (player == null)
            {
                return;
            }

            SetActive(_profileDetailsPanel, true);
            RefreshProfileDetails(player);
        }

        public void HideProfileDetails()
        {
            SetActive(_profileDetailsPanel, false);
        }

        public async void ClaimUsername()
        {
            var player = AppRuntimeSession.AuthenticatedPlayer;
            if (player == null || !string.IsNullOrWhiteSpace(player.Username))
            {
                RefreshAuthenticatedPlayerUi();
                return;
            }

            SetProfileClaimMessage("Checking availability...");
            if (_usernameClaimButton != null)
            {
                _usernameClaimButton.interactable = false;
            }

            BeginAuthRequest("Checking username...");
            try
            {
                var result = await _authenticationService.ClaimUsernameAsync(player, GetText(_usernameClaimInput), _authCancellation.Token);
                if (result != null && result.IsSuccess)
                {
                    AppRuntimeSession.SetAuthenticatedPlayer(result.Player);
                    RefreshAuthenticatedPlayerUi();
                    RefreshProfileDetails(result.Player);
                }

                SetProfileClaimMessage(result == null ? "Username could not be set." : result.Message);
            }
            catch (Exception exception)
            {
                SetProfileClaimMessage($"Username could not be set: {exception.Message}");
            }
            finally
            {
                if (_usernameClaimButton != null)
                {
                    _usernameClaimButton.interactable = string.IsNullOrWhiteSpace(AppRuntimeSession.AuthenticatedPlayer?.Username);
                }
            }
        }

        public void SelectAvatar(string avatarId)
        {
            var avatarIndex = Array.IndexOf(AvatarIds, avatarId);
            if (avatarIndex < 0)
            {
                SetMessage("That avatar is not available.");
                return;
            }

            _selectedAvatarId = avatarId;
            UpdateAvatarSelectionVisuals();
            SetMessage($"{AvatarDisplayNames[avatarIndex]} selected.");
        }

        public async void Register()
        {
            if (string.IsNullOrWhiteSpace(_selectedAvatarId))
            {
                SetMessage("Select an adventurer portrait.");
                return;
            }

            if (GetText(_registerPasswordInput) != GetText(_registerConfirmPasswordInput))
            {
                SetMessage("Passwords do not match.");
                return;
            }

            BeginAuthRequest("Creating account...");
            try
            {
                var result = await _authenticationService.RegisterAsync(
                    GetText(_registerUsernameInput),
                    _selectedAvatarId,
                    GetText(_registerEmailInput),
                    GetText(_registerPasswordInput),
                    _authCancellation.Token);
                CompleteAuthRequest(result);
            }
            catch (Exception exception)
            {
                SetMessage($"Register failed: {exception.Message}");
            }
        }

        public async void Login()
        {
            var identifier = GetText(_loginIdentifierInput);
            BeginAuthRequest("Logging in...");
            try
            {
                var result = identifier.Contains("@")
                    ? await _authenticationService.SignInWithEmailAsync(identifier, GetText(_loginPasswordInput), _authCancellation.Token)
                    : await _authenticationService.SignInWithUsernameAsync(identifier, GetText(_loginPasswordInput), _authCancellation.Token);
                CompleteAuthRequest(result);
            }
            catch (Exception exception)
            {
                SetMessage($"Login failed: {exception.Message}");
            }
        }

        private void BeginAuthRequest(string message)
        {
            _authCancellation?.Cancel();
            _authCancellation?.Dispose();
            _authCancellation = new CancellationTokenSource();
            _authCancellation.CancelAfter(TimeSpan.FromSeconds(10));
            SetMessage(message);
        }

        private void CompleteAuthRequest(AuthenticationResult result)
        {
            if (result != null && result.IsSuccess)
            {
                AppRuntimeSession.SetAuthenticatedPlayer(result.Player);
                _authenticationSessionStore.SaveRefreshToken(result.Player.RefreshToken);
                HideAuthPanels();
                RefreshAuthenticatedPlayerUi();
            }

            SetMessage(result == null ? "Authentication failed." : result.Message);
        }

        public void Logout()
        {
            _authenticationSessionStore?.Clear();
            AppRuntimeSession.SetAuthenticatedPlayer(null);
            HideAuthPanels();
            SetMessage("Logged out.");
            RefreshAuthenticatedPlayerUi();
        }

        private void SetMessage(string message)
        {
            if (_authMessageText != null)
            {
                _authMessageText.text = message;
            }
        }

        private void UpdateAvatarSelectionVisuals()
        {
            if (_avatarButtons == null)
            {
                return;
            }

            for (var index = 0; index < _avatarButtons.Length; index++)
            {
                var button = _avatarButtons[index];
                if (button == null)
                {
                    continue;
                }

                var isSelected = index < AvatarIds.Length && AvatarIds[index] == _selectedAvatarId;
                var colors = button.colors;
                colors.normalColor = isSelected ? AvatarSelectedColor : AvatarNormalColor;
                colors.highlightedColor = isSelected ? AvatarSelectedColor : AvatarHoverColor;
                colors.selectedColor = isSelected ? AvatarSelectedColor : AvatarHoverColor;
                colors.pressedColor = AvatarSelectedColor;
                button.colors = colors;
            }
        }

        private void RefreshAuthenticatedPlayerUi()
        {
            var player = AppRuntimeSession.AuthenticatedPlayer;
            var isAuthenticated = player != null;
            SetActive(_playerProfileCard, isAuthenticated);
            SetActive(_loginEntryButton, !isAuthenticated);
            SetActive(_registerEntryButton, !isAuthenticated);
            SetActive(_collectionEntryButton, isAuthenticated);

            if (!isAuthenticated)
            {
                HideProfileDetails();
                return;
            }

            var username = string.IsNullOrWhiteSpace(player.Username) ? "ADVENTURER" : player.Username;
            if (_playerUsernameText != null)
            {
                _playerUsernameText.text = username;
            }

            if (_playerLevelText != null)
            {
                _playerLevelText.text = $"LEVEL {player.PlayerLevel}";
            }

            var avatarIndex = Array.IndexOf(AvatarIds, player.AvatarId);
            if (_playerAvatarImage != null && _playerAvatarSprites != null && avatarIndex >= 0 && avatarIndex < _playerAvatarSprites.Length)
            {
                _playerAvatarImage.sprite = _playerAvatarSprites[avatarIndex];
            }
        }

        private void RefreshProfileDetails(AuthenticatedPlayer player)
        {
            var progression = player.Progression;
            var usernameMissing = string.IsNullOrWhiteSpace(player.Username);
            var username = usernameMissing ? "UNNAMED ADVENTURER" : player.Username.ToUpperInvariant();
            var currentLevelXp = progression.Experience % PlayerProgression.ExperiencePerLevel;
            var totalDefeats = progression.EnemyDefeats == null ? 0 : progression.EnemyDefeats.Sum(entry => entry == null ? 0 : entry.Defeats);
            var unlockedHeroes = progression.UnlockedTowerIds == null ? 0 : progression.UnlockedTowerIds.Length;
            var unlockedCards = progression.UnlockedCardIds == null ? 0 : progression.UnlockedCardIds.Length;
            var bestAdventureLevel = 0;
            var arenaMatches = 0;
            var arenaVictories = 0;
            var arenaEnemies = 0;
            var saveSlots = new LocalSaveSlotRepository();
            for (var slotIndex = 0; slotIndex < 3; slotIndex++)
            {
                var slot = saveSlots.Load(slotIndex);
                if (slot.IsEmpty)
                {
                    continue;
                }

                bestAdventureLevel = Math.Max(bestAdventureLevel, slot.HighestUnlockedLevelIndex + 1);
                arenaMatches += slot.ArenaMatchesPlayed;
                arenaVictories += slot.ArenaVictories;
                arenaEnemies += slot.ArenaEnemiesDefeated;
            }

            SetText(_profileDetailsUsernameText, username);
            SetText(_profileDetailsLevelText, $"LEVEL {player.PlayerLevel}");
            SetText(_profileDetailsProgressText, $"XP  {currentLevelXp:N0} / {PlayerProgression.ExperiencePerLevel:N0}\nSCRAP  {progression.Scrap:N0}    COINS  {progression.Coins:N0}");
            SetText(_profileDetailsCollectionText, $"HEROES UNLOCKED  {unlockedHeroes}\nENEMY CARDS  {unlockedCards}\nENEMIES DEFEATED  {totalDefeats:N0}");
            SetText(_profileDetailsAdventureText, bestAdventureLevel > 0 ? $"ADVENTURE\nHIGHEST LEVEL  {bestAdventureLevel}" : "ADVENTURE\nNO JOURNEY STARTED");
            SetText(_profileDetailsArenaText, $"CITADEL FALL ARENA\nMATCHES  {arenaMatches}    VICTORIES  {arenaVictories}\nENEMIES DEFEATED  {arenaEnemies:N0}");
            SetActive(_usernameClaimSection, usernameMissing);
            SetProfileClaimMessage(usernameMissing ? "Choose carefully. Your username is permanent and unique." : string.Empty);

            var avatarIndex = Array.IndexOf(AvatarIds, player.AvatarId);
            if (_profileDetailsAvatarImage != null && _playerAvatarSprites != null && avatarIndex >= 0 && avatarIndex < _playerAvatarSprites.Length)
            {
                _profileDetailsAvatarImage.sprite = _playerAvatarSprites[avatarIndex];
            }
            else if (_profileDetailsAvatarImage != null && _playerAvatarImage != null && _playerAvatarImage.sprite != null)
            {
                // Older profiles can predate avatarId persistence. Reuse the portrait already shown
                // on the compact profile card instead of presenting a blank white image.
                _profileDetailsAvatarImage.sprite = _playerAvatarImage.sprite;
            }
            else if (_profileDetailsAvatarImage != null && _playerAvatarSprites != null && _playerAvatarSprites.Length > 0)
            {
                _profileDetailsAvatarImage.sprite = _playerAvatarSprites[0];
            }

            if (_profileDetailsAvatarImage != null)
            {
                _profileDetailsAvatarImage.color = Color.white;
            }
        }

        private void SetProfileClaimMessage(string message)
        {
            SetText(_usernameClaimMessageText, message);
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static string GetText(TMP_InputField input)
        {
            return input == null ? string.Empty : input.text;
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }
    }
}
