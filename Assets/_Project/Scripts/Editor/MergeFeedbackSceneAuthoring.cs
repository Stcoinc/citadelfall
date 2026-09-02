#if UNITY_EDITOR
using System;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClubGamerZone.TowerDefense.EditorTools
{
    public static class MergeFeedbackSceneAuthoring
    {
        private static readonly string[] PointerScenePaths =
        {
            "Assets/_Project/Scenes/Gameplay/MvpGameplay.unity",
            "Assets/_Project/Scenes/Gameplay/Gameplay.unity",
            "Assets/_Project/Scenes/Gameplay/FullGame.unity",
            "Assets/_Project/Scenes/Gameplay/Endless.unity"
        };

        private const string ArenaScenePath = "Assets/_Project/Scenes/Gameplay/CitadelFallArena.unity";

        [MenuItem("Tools/Citadel Fall/Authoring/Repair Merge Feedback Components")]
        public static void RepairAllScenes()
        {
            var originalPath = SceneManager.GetActiveScene().path;
            try
            {
                foreach (var scenePath in PointerScenePaths)
                {
                    RepairPointerScene(scenePath);
                }

                RepairArenaScene();
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(originalPath))
                {
                    EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);
                }
            }

            Debug.Log("Authored AudioSource and MergeInteractionFeedback references in all Citadel Fall gameplay scenes.");
        }

        private static void RepairPointerScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var target = FindInScene(scene, "MVP Pointer Placement Input");
            if (target == null)
            {
                throw new InvalidOperationException($"MVP Pointer Placement Input was not found in {scenePath}.");
            }

            var feedback = EnsureFeedback(target);
            var pointerInput = target.GetComponent<MvpPointerPlacementInput>();
            if (pointerInput == null)
            {
                throw new InvalidOperationException($"MvpPointerPlacementInput was not found on {target.name} in {scenePath}.");
            }

            var serializedInput = new SerializedObject(pointerInput);
            serializedInput.FindProperty("_mergeFeedback").objectReferenceValue = feedback;
            serializedInput.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pointerInput);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RepairArenaScene()
        {
            var scene = EditorSceneManager.OpenScene(ArenaScenePath, OpenSceneMode.Single);
            var target = FindInScene(scene, "Citadel Fall Arena Controller");
            if (target == null)
            {
                throw new InvalidOperationException("Citadel Fall Arena Controller was not found.");
            }

            var feedback = EnsureFeedback(target);
            var controller = target.GetComponent<CitadelFallArenaController>();
            if (controller == null)
            {
                throw new InvalidOperationException("CitadelFallArenaController component was not found.");
            }

            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("_mergeFeedback").objectReferenceValue = feedback;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static MergeInteractionFeedback EnsureFeedback(GameObject target)
        {
            var audioSource = target.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = Undo.AddComponent<AudioSource>(target);
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            EditorUtility.SetDirty(audioSource);

            var feedback = target.GetComponent<MergeInteractionFeedback>();
            if (feedback == null)
            {
                feedback = Undo.AddComponent<MergeInteractionFeedback>(target);
            }

            var serializedFeedback = new SerializedObject(feedback);
            serializedFeedback.FindProperty("_audioSource").objectReferenceValue = audioSource;
            serializedFeedback.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(feedback);
            return feedback;
        }

        private static GameObject FindInScene(Scene scene, string objectName)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (string.Equals(transform.name, objectName, StringComparison.Ordinal))
                    {
                        return transform.gameObject;
                    }
                }
            }

            return null;
        }
    }
}
#endif
