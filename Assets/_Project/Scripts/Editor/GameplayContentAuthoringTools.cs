using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClubGamerZone.TowerDefense.Application.Configuration;
using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using ClubGamerZone.TowerDefense.Infrastructure.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Editor
{
    public static class GameplayContentAuthoringTools
    {
        private const string AuthoringRoot = "Assets/_Project/Data/Authoring";
        private const string EnemyFolder = AuthoringRoot + "/Enemies";
        private const string WaveSetFolder = AuthoringRoot + "/WaveSets";
        private const string LevelFolder = AuthoringRoot + "/Levels";
        private const string LayoutFolder = AuthoringRoot + "/Battlefields";
        private const string TowerFolder = AuthoringRoot + "/Towers";
        private const string StarterContentJsonPath = "Assets/_Project/Data/Json/Defaults/starter_content.json";
        private const string EnemyPrefabPath = "Assets/_Project/Prefabs/Enemies/EnemyScoutPlaceholder.prefab";

        [MenuItem("Tower Defense/Content/Import Starter JSON To Authoring Assets")]
        public static void ImportStarterJsonToAuthoringAssets()
        {
            EnsureFolders();
            var dto = ReadStarterContent();
            var sharedEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);

            var enemyAssets = new Dictionary<string, EnemyDefinitionAsset>(StringComparer.Ordinal);
            foreach (var enemyDto in dto.Enemies ?? Array.Empty<EnemyDto>())
            {
                var asset = LoadOrCreate<EnemyDefinitionAsset>($"{EnemyFolder}/{enemyDto.Id}.asset");
                asset.Configure(enemyDto, sharedEnemyPrefab);
                EditorUtility.SetDirty(asset);
                enemyAssets[enemyDto.Id] = asset;
            }

            var waveSetAssets = new Dictionary<string, WaveSetDefinitionAsset>(StringComparer.Ordinal);
            foreach (var waveSetDto in dto.WaveSets ?? Array.Empty<WaveSetDto>())
            {
                var waves = (waveSetDto.Waves ?? Array.Empty<WaveDto>())
                    .Select(waveDto => BuildWave(waveDto, enemyAssets))
                    .ToArray();
                var asset = LoadOrCreate<WaveSetDefinitionAsset>($"{WaveSetFolder}/{waveSetDto.Id}.asset");
                asset.Configure(waveSetDto.Id, waveSetDto.HasBoss, waves);
                EditorUtility.SetDirty(asset);
                waveSetAssets[waveSetDto.Id] = asset;
            }

            var battlefieldAssets = new Dictionary<string, BattlefieldLayoutAsset>(StringComparer.Ordinal);
            foreach (var battlefieldDto in dto.Battlefields ?? Array.Empty<BattlefieldDto>())
            {
                var battlefield = LoadOrCreate<BattlefieldLayoutAsset>($"{LayoutFolder}/{battlefieldDto.Id}.asset");
                battlefield.Configure(battlefieldDto);
                EditorUtility.SetDirty(battlefield);
                battlefieldAssets[battlefieldDto.Id] = battlefield;
            }

            var levelAssets = new List<LevelDefinitionAsset>();
            foreach (var levelDto in dto.Levels ?? Array.Empty<LevelDto>())
            {
                waveSetAssets.TryGetValue(levelDto.WaveSetId, out var waveSet);
                var asset = LoadOrCreate<LevelDefinitionAsset>($"{LevelFolder}/{levelDto.Id}.asset");
                battlefieldAssets.TryGetValue(levelDto.BattlefieldId, out var battlefield);
                battlefield ??= asset.BattlefieldLayout;
                asset.Configure(levelDto, waveSet, battlefield);
                EditorUtility.SetDirty(asset);
                levelAssets.Add(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Imported {enemyAssets.Count} enemies, {waveSetAssets.Count} wave sets, {battlefieldAssets.Count} battlefields, and {levelAssets.Count} levels.");
        }

        [MenuItem("Tower Defense/Content/Export All Authoring To Starter JSON")]
        public static void ExportAllAuthoringToStarterJson()
        {
            var content = ReadStarterContent();
            content.Towers = LoadAssets<TowerDefinitionAsset>(TowerFolder).Select(asset => asset.ToDto()).ToArray();
            content.Enemies = LoadAssets<EnemyDefinitionAsset>(EnemyFolder).Select(asset => asset.ToDto()).ToArray();
            content.WaveSets = LoadAssets<WaveSetDefinitionAsset>(WaveSetFolder).Select(asset => asset.ToDto()).ToArray();
            content.Battlefields = LoadAssets<BattlefieldLayoutAsset>(LayoutFolder).Select(asset => asset.ToDto()).ToArray();
            content.Levels = LoadAssets<LevelDefinitionAsset>(LevelFolder).Select(asset => asset.ToDto()).ToArray();

            var json = JsonUtility.ToJson(content, true);
            var parser = new UnityContentJsonParser();
            var builder = new StarterContentCatalogBuilder(new StarterContentValidator(new ContentValidationLimits()));
            var result = builder.Build(parser.Parse(json));
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(
                    "Authoring export is invalid:\n" +
                    string.Join("\n", result.Validation.Issues.Select(issue => issue.ToString())));
            }

            File.WriteAllText(StarterContentJsonPath, json);
            AssetDatabase.ImportAsset(StarterContentJsonPath);
            Debug.Log($"Exported {content.Towers.Length} towers, {content.Enemies.Length} enemies, {content.WaveSets.Length} wave sets, {content.Battlefields.Length} battlefields, and {content.Levels.Length} levels to starter_content.json.");
        }

        private static WaveAuthoringData BuildWave(
            WaveDto waveDto,
            IReadOnlyDictionary<string, EnemyDefinitionAsset> enemyAssets)
        {
            var spawns = (waveDto.Spawns ?? Array.Empty<WaveSpawnDto>())
                .Select(spawnDto =>
                {
                    enemyAssets.TryGetValue(spawnDto.EnemyId, out var enemy);
                    var spawn = new WaveSpawnAuthoringData();
                    spawn.Configure(enemy, spawnDto.Count, spawnDto.IntervalSeconds);
                    return spawn;
                })
                .ToArray();
            var wave = new WaveAuthoringData();
            wave.Configure(waveDto.Id, waveDto.StartDelaySeconds, spawns);
            return wave;
        }

        private static StarterContentDto ReadStarterContent()
        {
            return JsonUtility.FromJson<StarterContentDto>(File.ReadAllText(StarterContentJsonPath))
                ?? throw new InvalidOperationException("Could not read starter_content.json.");
        }

        private static IEnumerable<T> LoadAssets<T>(string folder) where T : UnityEngine.Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null)
                .OrderBy(asset => asset.name, StringComparer.Ordinal);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolders()
        {
            EnsureFolder(AuthoringRoot, "Enemies");
            EnsureFolder(AuthoringRoot, "WaveSets");
            EnsureFolder(AuthoringRoot, "Levels");
            EnsureFolder(AuthoringRoot, "Battlefields");
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }

    [CustomEditor(typeof(BattlefieldLayoutAsset))]
    public sealed class BattlefieldLayoutAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("Capture From Active Gameplay Scene"))
            {
                CaptureFromScene();
            }

            if (GUILayout.Button("Apply To Active Gameplay Scene"))
            {
                ApplyToScene();
            }
        }

        private void OnSceneGUI()
        {
            var layout = (BattlefieldLayoutAsset)target;
            var changed = false;

            Handles.color = new Color(1f, 0.55f, 0.1f, 1f);
            for (var i = 0; i < layout.PathPointCount; i++)
            {
                var point = (Vector3)layout.GetPathPoint(i);
                if (i > 0)
                {
                    Handles.DrawLine(layout.GetPathPoint(i - 1), point, 3f);
                }

                EditorGUI.BeginChangeCheck();
                var moved = Handles.PositionHandle(point, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(layout, "Move enemy path point");
                    layout.SetPathPoint(i, moved);
                    changed = true;
                }

                Handles.Label(point + Vector3.up * 0.25f, $"Path {i + 1}");
            }

            Handles.color = new Color(0.1f, 0.9f, 1f, 1f);
            for (var i = 0; i < layout.BuildSocketCount; i++)
            {
                var point = (Vector3)layout.GetBuildSocketPosition(i);
                Handles.DrawWireDisc(point, Vector3.forward, 0.55f, 3f);
                EditorGUI.BeginChangeCheck();
                var moved = Handles.PositionHandle(point, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(layout, "Move build socket");
                    layout.SetBuildSocketPosition(i, moved);
                    changed = true;
                }

                Handles.Label(point + Vector3.up * 0.65f, $"Socket {i + 1}");
            }

            if (changed)
            {
                EditorUtility.SetDirty(layout);
                ApplyToScene();
            }
        }

        private void CaptureFromScene()
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<MvpGameplayController>();
            if (controller == null)
            {
                Debug.LogWarning("Open the Gameplay scene before capturing a battlefield layout.");
                return;
            }

            var layout = (BattlefieldLayoutAsset)target;
            Undo.RecordObject(layout, "Capture battlefield layout");
            controller.CaptureBattlefieldLayout(layout);
            EditorUtility.SetDirty(layout);
            SceneView.RepaintAll();
        }

        private void ApplyToScene()
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<MvpGameplayController>();
            if (controller == null)
            {
                Debug.LogWarning("Open the Gameplay scene before applying a battlefield layout.");
                return;
            }

            foreach (var root in controller.gameObject.scene.GetRootGameObjects())
            {
                Undo.RegisterFullObjectHierarchyUndo(root, "Apply battlefield layout");
            }
            controller.ApplyBattlefieldLayoutPreview((BattlefieldLayoutAsset)target);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            SceneView.RepaintAll();
        }
    }
}
