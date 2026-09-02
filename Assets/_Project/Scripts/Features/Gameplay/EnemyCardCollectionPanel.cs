using ClubGamerZone.TowerDefense.Application.Authentication;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class EnemyCardCollectionPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TMP_Text _accountSummaryText;
        [SerializeField] private string[] _enemyIds;
        [SerializeField] private string[] _enemyDisplayNames;
        [SerializeField] private Image[] _portraitImages;
        [SerializeField] private TMP_Text[] _progressTexts;

        public void Show()
        {
            Refresh();
            SetActive(_panelRoot, true);
        }

        public void Hide()
        {
            SetActive(_panelRoot, false);
        }

        public void Refresh()
        {
            var progression = AppRuntimeSession.AuthenticatedPlayer?.Progression;
            if (progression == null)
            {
                SetText(_accountSummaryText, "LOG IN TO VIEW YOUR COLLECTION");
                return;
            }

            SetText(
                _accountSummaryText,
                $"LEVEL {progression.Level}   •   {progression.Experience:N0} XP   •   {progression.UnlockedCardIds.Length} CARDS");

            var count = Mathf.Min(
                _enemyIds == null ? 0 : _enemyIds.Length,
                Mathf.Min(_portraitImages == null ? 0 : _portraitImages.Length, _progressTexts == null ? 0 : _progressTexts.Length));
            for (var index = 0; index < count; index++)
            {
                var enemyId = _enemyIds[index];
                var defeats = progression.GetEnemyDefeats(enemyId);
                var tier = progression.GetUnlockedCardTier(enemyId);
                var displayName = _enemyDisplayNames != null && index < _enemyDisplayNames.Length
                    ? _enemyDisplayNames[index]
                    : enemyId;
                var nextMilestone = (tier + 1) * PlayerProgression.DefeatsPerCard;
                _portraitImages[index].color = tier > 0 ? Color.white : new Color32(55, 66, 72, 255);
                SetText(
                    _progressTexts[index],
                    tier > 0
                        ? $"{displayName.ToUpperInvariant()}  •  CARD TIER {tier}\n{defeats:N0} DEFEATED\nNEXT CARD: {nextMilestone:N0}"
                        : $"{displayName.ToUpperInvariant()}  •  LOCKED\n{defeats:N0} / {PlayerProgression.DefeatsPerCard:N0} DEFEATED");
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
