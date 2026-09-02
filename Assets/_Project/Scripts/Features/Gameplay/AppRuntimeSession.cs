using ClubGamerZone.TowerDefense.Domain.Content;
using ClubGamerZone.TowerDefense.Application.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    public enum AppPlayMode
    {
        Adventure = 0,
        Arena = 1,
        Endless = 2
    }

    public static class AppRuntimeSession
    {
        public static string ActiveContentJson { get; private set; } = string.Empty;

        public static ContentCatalog Catalog { get; private set; }

        public static SaveSlotData SelectedSlot { get; private set; }

        public static string SelectedLevelId { get; private set; } = string.Empty;

        public static bool SlotModeContinue { get; private set; }

        public static AppPlayMode PlayMode { get; private set; } = AppPlayMode.Adventure;

        public static bool IsArenaMode => PlayMode == AppPlayMode.Arena;

        public static bool IsEndlessMode => PlayMode == AppPlayMode.Endless;

        public static bool LoadedRemoteContent { get; private set; }

        public static AuthenticatedPlayer AuthenticatedPlayer { get; private set; }

        public static IPlayerProgressionService PlayerProgressionService { get; private set; }

        public static bool IsAuthenticated => AuthenticatedPlayer != null;

        public static bool HasContent => Catalog != null && !string.IsNullOrWhiteSpace(ActiveContentJson);

        public static void SetContent(string contentJson, ContentCatalog catalog, bool loadedRemoteContent)
        {
            ActiveContentJson = contentJson ?? string.Empty;
            Catalog = catalog;
            LoadedRemoteContent = loadedRemoteContent;
        }

        public static void SetSlotMode(bool continueMode)
        {
            PlayMode = AppPlayMode.Adventure;
            SlotModeContinue = continueMode;
            SelectedSlot = null;
            SelectedLevelId = string.Empty;
        }

        public static void SetArenaMode()
        {
            PlayMode = AppPlayMode.Arena;
            SlotModeContinue = false;
            SelectedSlot = null;
            SelectedLevelId = string.Empty;
        }

        public static void SetEndlessMode()
        {
            PlayMode = AppPlayMode.Endless;
            SlotModeContinue = false;
            SelectedSlot = null;
            SelectedLevelId = string.Empty;
        }

        public static void SetSelectedSlot(SaveSlotData slot)
        {
            SelectedSlot = slot;
            SelectedLevelId = string.Empty;
        }

        public static void SetSelectedLevel(string levelId)
        {
            SelectedLevelId = levelId ?? string.Empty;
        }

        public static void SetAuthenticatedPlayer(AuthenticatedPlayer player)
        {
            AuthenticatedPlayer = player;
        }

        public static void SetPlayerProgressionService(IPlayerProgressionService service)
        {
            PlayerProgressionService = service;
        }

        public static void RegisterEnemyDefeat(string enemyId)
        {
            AuthenticatedPlayer?.Progression?.RegisterEnemyDefeat(enemyId);
        }

        public static void UnlockTower(string towerId)
        {
            AuthenticatedPlayer?.Progression?.UnlockTower(towerId);
        }

        public static void SetPlayerCurrencies(int scrap, int coins)
        {
            AuthenticatedPlayer?.Progression?.SetCurrencies(scrap, coins);
        }

        public static Task SavePlayerProgressionAsync(CancellationToken cancellationToken)
        {
            return PlayerProgressionService == null || AuthenticatedPlayer == null
                ? Task.CompletedTask
                : PlayerProgressionService.SaveProgressionAsync(AuthenticatedPlayer, cancellationToken);
        }
    }
}
