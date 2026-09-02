#if UNITY_EDITOR
using System;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClubGamerZone.TowerDefense.EditorTools
{
    public static class UnitRangeIndicatorSceneAuthoring
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/_Project/Scenes/Gameplay/MvpGameplay.unity",
            "Assets/_Project/Scenes/Gameplay/Gameplay.unity",
            "Assets/_Project/Scenes/Gameplay/FullGame.unity",
            "Assets/_Project/Scenes/Gameplay/Endless.unity"
        };

        private const string ArenaScenePath = "Assets/_Project/Scenes/Gameplay/CitadelFallArena.unity";

        private const string MaterialPath = "Assets/_Project/Art/Materials/UnitRangeIndicator.mat";

        [MenuItem("Tools/Citadel Fall/Authoring/Rebuild Unit Range Indicators")]
        public static void RebuildAllScenes()
        {
            var material = GetOrCreateMaterial();
            var originalPath = SceneManager.GetActiveScene().path;

            try
            {
                foreach (var scenePath in ScenePaths)
                {
                    RebuildScene(scenePath, material);
                }

                RebuildArenaScene(material);
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(originalPath))
                {
                    EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Authored selectable unit range indicators in all Adventure and Endless gameplay scenes.");
        }

        private static void RebuildScene(string scenePath, Material material)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var sockets = FindComponentsInScene<TowerPlacementSocket>(scene);

            if (sockets.Length == 0)
            {
                throw new InvalidOperationException($"No TowerPlacementSocket components were found in {scenePath}.");
            }

            foreach (var socket in sockets)
            {
                AuthorIndicator(socket.transform, socket, "_rangeIndicator", material);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RebuildArenaScene(Material material)
        {
            var scene = EditorSceneManager.OpenScene(ArenaScenePath, OpenSceneMode.Single);
            var sockets = FindComponentsInScene<ArenaHeroSocket>(scene);

            if (sockets.Length == 0)
            {
                throw new InvalidOperationException($"No ArenaHeroSocket components were found in {ArenaScenePath}.");
            }

            foreach (var socket in sockets)
            {
                AuthorIndicator(socket.transform, socket, "_rangeIndicator", material);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AuthorIndicator(
            Transform parent,
            Component owner,
            string ownerPropertyName,
            Material material)
        {
            var existing = parent.Find("Unit Range Indicator");
            var indicatorObject = existing == null
                ? new GameObject("Unit Range Indicator")
                : existing.gameObject;

            if (existing == null)
            {
                Undo.RegisterCreatedObjectUndo(indicatorObject, "Create Unit Range Indicator");
                indicatorObject.transform.SetParent(parent, false);
            }

            indicatorObject.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            indicatorObject.transform.localRotation = Quaternion.identity;
            indicatorObject.transform.localScale = Vector3.one;

            var line = indicatorObject.GetComponent<LineRenderer>();
            if (line == null)
            {
                line = Undo.AddComponent<LineRenderer>(indicatorObject);
            }

            ConfigureLine(line, material);

            var indicator = indicatorObject.GetComponent<UnitRangeIndicator>();
            if (indicator == null)
            {
                indicator = Undo.AddComponent<UnitRangeIndicator>(indicatorObject);
            }

            var serializedIndicator = new SerializedObject(indicator);
            serializedIndicator.FindProperty("_lineRenderer").objectReferenceValue = line;
            serializedIndicator.ApplyModifiedPropertiesWithoutUndo();

            var serializedOwner = new SerializedObject(owner);
            serializedOwner.FindProperty(ownerPropertyName).objectReferenceValue = indicator;
            serializedOwner.ApplyModifiedPropertiesWithoutUndo();

            indicatorObject.SetActive(false);
            EditorUtility.SetDirty(line);
            EditorUtility.SetDirty(indicator);
            EditorUtility.SetDirty(owner);
        }

        private static void ConfigureLine(LineRenderer line, Material material)
        {
            line.sharedMaterial = material;
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 96;
            line.startWidth = 0.075f;
            line.endWidth = 0.075f;
            line.numCornerVertices = 3;
            line.numCapVertices = 3;
            line.startColor = new Color(0.18f, 0.82f, 1f, 0.9f);
            line.endColor = new Color(1f, 0.75f, 0.18f, 0.9f);
            line.sortingOrder = 5;
            line.textureMode = LineTextureMode.Stretch;
        }

        private static Material GetOrCreateMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null)
            {
                return material;
            }

            const string materialsFolder = "Assets/_Project/Art/Materials";
            if (!AssetDatabase.IsValidFolder(materialsFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Art", "Materials");
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                throw new InvalidOperationException("Sprites/Default shader was not found.");
            }

            material = new Material(shader)
            {
                name = "Unit Range Indicator"
            };
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        private static T[] FindComponentsInScene<T>(Scene scene) where T : Component
        {
            var results = new System.Collections.Generic.List<T>();
            foreach (var root in scene.GetRootGameObjects())
            {
                results.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return results.ToArray();
        }
    }
}
#endif
