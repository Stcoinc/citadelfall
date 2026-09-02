using System.IO;
using System.Linq;
using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using UnityEditor;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Editor
{
    public static class TowerAuthoringJsonExporter
    {
        private const string TowerAssetFolder = "Assets/_Project/Data/Authoring/Towers";
        private const string StarterContentJsonPath = "Assets/_Project/Data/Json/Defaults/starter_content.json";

        [MenuItem("Tower Defense/Content/Export Tower Authoring To Starter JSON")]
        public static void ExportTowerAuthoringToStarterJson()
        {
            var content = JsonUtility.FromJson<StarterContentDto>(File.ReadAllText(StarterContentJsonPath));
            var towers = AssetDatabase
                .FindAssets("t:TowerDefinitionAsset", new[] { TowerAssetFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<TowerDefinitionAsset>)
                .Where(asset => asset != null)
                .OrderBy(asset => asset.Id)
                .Select(asset => asset.ToDto())
                .ToArray();

            content.Towers = towers;
            File.WriteAllText(StarterContentJsonPath, JsonUtility.ToJson(content, true));
            AssetDatabase.ImportAsset(StarterContentJsonPath);
            Debug.Log($"Exported {towers.Length} tower authoring assets to {StarterContentJsonPath}.");
        }
    }
}
