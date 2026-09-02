using System;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using UnityEditor;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Editor
{
    public sealed class CitadelFallContentDashboard : EditorWindow
    {
        private const string Root = "Assets/_Project/Data/Authoring";
        private Vector2 _scroll;

        [MenuItem("Tools/Citadel Fall/Content Dashboard")]
        public static void Open()
        {
            var window = GetWindow<CitadelFallContentDashboard>("Citadel Fall Content");
            window.minSize = new Vector2(560f, 520f);
            window.Show();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("CITADEL FALL CONTENT", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Edit enemies, wave composition, and levels here. Scene geometry stays in the Battlefield asset; numeric content can then be exported to starter_content.json and published as a validated Firebase override.",
                MessageType.Info);

            DrawSection<EnemyDefinitionAsset>("Enemies", "Enemies", "NewEnemy.asset");
            DrawSection<WaveSetDefinitionAsset>("Wave Sets", "WaveSets", "NewWaveSet.asset");
            DrawSection<LevelDefinitionAsset>("Levels", "Levels", "NewLevel.asset");
            DrawSection<TowerDefinitionAsset>("Heroes", "Towers", "NewHero.asset");
            DrawSection<BattlefieldLayoutAsset>("Battlefields", "Battlefields", "NewBattlefield.asset");

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("CONTENT PIPELINE", EditorStyles.boldLabel);
            if (GUILayout.Button("Import starter_content.json into authoring assets", GUILayout.Height(34f)))
            {
                GameplayContentAuthoringTools.ImportStarterJsonToAuthoringAssets();
            }

            if (GUILayout.Button("Validate and export all authoring assets", GUILayout.Height(34f)))
            {
                GameplayContentAuthoringTools.ExportAllAuthoringToStarterJson();
            }

            EditorGUILayout.HelpBox(
                "Recommended order: Enemy -> Wave Set -> Level. Endless mode cycles the selected level's Wave Set and applies its tested per-round scaling every five waves.",
                MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawSection<T>(string label, string folderName, string defaultAssetName) where T : ScriptableObject
        {
            var folder = $"{Root}/{folderName}";
            var count = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder }).Length;
            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"{label} ({count})", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button($"Open {label}"))
                    {
                        SelectFolder(folder);
                    }

                    if (GUILayout.Button($"Create {Singular(label)}"))
                    {
                        SelectFolder(folder);
                        ProjectWindowUtil.CreateAsset(CreateInstance<T>(), defaultAssetName);
                    }
                }
            }
        }

        private static void SelectFolder(string folder)
        {
            var asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static string Singular(string label)
        {
            return label.EndsWith("ies", StringComparison.Ordinal)
                ? label.Substring(0, label.Length - 3) + "y"
                : label.TrimEnd('s');
        }
    }
}
