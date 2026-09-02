using System.Globalization;
using System.Text;
using ClubGamerZone.TowerDefense.Domain.Content;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    public sealed class AdventureMissionResultPresenter
    {
        public string BuildVictoryText(
            LevelDefinition level,
            int completedWaves,
            int totalWaves,
            int enemiesDefeated,
            int remainingBaseHealth,
            bool unlockedNextAdventure)
        {
            if (level == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            builder.AppendLine(level.DisplayNameKey);
            builder.AppendLine();
            builder.AppendLine("BATTLE RECORD");
            builder.Append("Waves cleared  ");
            builder.Append(completedWaves);
            builder.Append('/');
            builder.AppendLine(totalWaves.ToString(CultureInfo.InvariantCulture));
            builder.Append("Enemies defeated  ");
            builder.AppendLine(enemiesDefeated.ToString(CultureInfo.InvariantCulture));
            builder.Append("Stronghold health  ");
            builder.AppendLine(remainingBaseHealth.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine();
            builder.AppendLine("MISSION REWARDS");
            builder.Append(level.RewardScrap);
            builder.Append(" Scrap     ");
            builder.Append(level.RewardCoins);
            builder.Append(" Coins     ");
            builder.Append(level.RewardGems);
            builder.AppendLine(" Gems");

            if (!string.IsNullOrWhiteSpace(level.RewardItemId))
            {
                builder.Append("Treasure  ");
                builder.AppendLine(FormatStableId(level.RewardItemId));
            }

            if (unlockedNextAdventure)
            {
                builder.AppendLine();
                builder.Append("NEW ADVENTURE UNLOCKED");
            }

            return builder.ToString().TrimEnd();
        }

        public string FormatStableId(string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                return string.Empty;
            }

            var words = stableId.Replace('-', ' ').Replace('_', ' ').Split(' ');
            var builder = new StringBuilder();
            var textInfo = CultureInfo.InvariantCulture.TextInfo;

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(textInfo.ToTitleCase(word.ToLowerInvariant()));
            }

            return builder.ToString();
        }
    }
}
