using System;
using System.Linq;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Editor
{
    public static class EndlessModeSceneAuthoring
    {
        private const string GameplayScenePath = "Assets/_Project/Scenes/Gameplay/Gameplay.unity";
        private const string EndlessScenePath = "Assets/_Project/Scenes/Gameplay/Endless.unity";
        private const string MainMenuScenePath = "Assets/_Project/Scenes/App/MainMenu.unity";
        private static TMP_FontAsset _font;
        private static Sprite _buttonPlate;

        [MenuItem("Tools/Citadel Fall/Author Endless Mode")]
        public static void AuthorAll()
        {
            LoadAssets();
            AuthorEndlessScene();
            AuthorMainMenuEntry();
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Endless Defense scene, Main Menu entry, and build settings authored successfully.");
        }

        private static void AuthorEndlessScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(EndlessScenePath) == null &&
                !AssetDatabase.CopyAsset(GameplayScenePath, EndlessScenePath))
            {
                throw new InvalidOperationException("Gameplay.unity could not be copied to create Endless.unity.");
            }

            var scene = EditorSceneManager.OpenScene(EndlessScenePath, OpenSceneMode.Single);
            var gameplay = UnityEngine.Object.FindFirstObjectByType<MvpGameplayController>(FindObjectsInactive.Include);
            var sceneControllerObject = FindSceneObject("Gameplay Scene Controller");
            var hud = FindSceneObject("Gameplay HUD");
            var startButton = FindSceneObject("Start Waves Button")?.GetComponent<Button>();
            var pauseButton = FindSceneObject("Pause Button")?.GetComponent<Button>();
            if (gameplay == null || sceneControllerObject == null || hud == null || startButton == null)
            {
                throw new InvalidOperationException("The shared Gameplay scene is missing required authored objects.");
            }

            var campaignController = sceneControllerObject.GetComponent<GameplaySceneController>();
            if (campaignController != null)
            {
                UnityEngine.Object.DestroyImmediate(campaignController);
            }

            var endless = sceneControllerObject.GetComponent<EndlessModeController>() ??
                          sceneControllerObject.AddComponent<EndlessModeController>();
            var gameplaySerialized = new SerializedObject(gameplay);
            gameplaySerialized.FindProperty("_startAutomatically").boolValue = false;
            gameplaySerialized.FindProperty("_manualWaveStart").boolValue = true;
            gameplaySerialized.FindProperty("_autoBuildFirstTower").boolValue = false;
            gameplaySerialized.ApplyModifiedPropertiesWithoutUndo();

            DestroyNamedChild(hud.transform, "Endless Score Bar");
            DestroyNamedChild(hud.transform, "Endless Defeat Panel");
            var messagePanel = FindSceneObject("Mission Message Panel");
            if (messagePanel != null)
            {
                messagePanel.SetActive(false);
            }

            var scoreBar = CreateRect("Endless Score Bar", hud.transform, new Vector2(0f, -104f), new Vector2(650f, 50f));
            SetAnchors(scoreBar.GetComponent<RectTransform>(), new Vector2(0.5f, 1f));
            StylePanel(scoreBar, new Color32(4, 28, 46, 244), new Color32(225, 168, 48, 255), 2f);
            var roundText = CreateText("Endless Round Text", scoreBar.transform, "ROUND 1", new Vector2(-210f, 0f), new Vector2(190f, 38f), 22f);
            var scoreText = CreateText("Endless Score Text", scoreBar.transform, "SCORE 0", Vector2.zero, new Vector2(220f, 38f), 22f);
            var bestText = CreateText("Endless Best Text", scoreBar.transform, "BEST 0", new Vector2(210f, 0f), new Vector2(190f, 38f), 22f);

            var startLabel = startButton.GetComponentInChildren<TMP_Text>(true);
            if (startLabel != null)
            {
                startLabel.text = "START WAVE 1/5";
                startLabel.font = _font;
            }

            startButton.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(startButton.onClick, endless.StartNextWave);
            if (pauseButton != null)
            {
                pauseButton.onClick = new Button.ButtonClickedEvent();
                UnityEventTools.AddPersistentListener(pauseButton.onClick, endless.TogglePause);
            }

            var defeatPanel = CreateRect("Endless Defeat Panel", hud.transform, Vector2.zero, new Vector2(760f, 430f));
            StylePanel(defeatPanel, new Color32(3, 22, 37, 250), new Color32(230, 171, 49, 255), 4f);
            CreateText("Endless Defeat Heading", defeatPanel.transform, "THE STRONGHOLD HAS FALLEN", new Vector2(0f, 135f), new Vector2(680f, 70f), 34f, new Color32(255, 213, 100, 255));
            var defeatSummary = CreateText("Endless Defeat Summary", defeatPanel.transform, "FINAL SCORE  0\nROUND REACHED  1\nPERSONAL BEST  0", new Vector2(0f, 15f), new Vector2(620f, 150f), 25f);
            var returnButton = CreateButton("Endless Return Button", defeatPanel.transform, new Vector2(0f, -140f), new Vector2(330f, 80f), "RETURN TO MAIN MENU");
            UnityEventTools.AddPersistentListener(returnButton.onClick, endless.ReturnToMainMenu);
            defeatPanel.SetActive(false);
            defeatPanel.transform.SetAsLastSibling();

            var endlessSerialized = new SerializedObject(endless);
            SetReference(endlessSerialized, "_gameplayController", gameplay);
            SetReference(endlessSerialized, "_roundText", roundText);
            SetReference(endlessSerialized, "_scoreText", scoreText);
            SetReference(endlessSerialized, "_bestScoreText", bestText);
            SetReference(endlessSerialized, "_nextWaveText", startLabel);
            SetReference(endlessSerialized, "_nextWaveButton", startButton);
            SetReference(endlessSerialized, "_defeatPanel", defeatPanel);
            SetReference(endlessSerialized, "_defeatSummaryText", defeatSummary);
            endlessSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(gameplay);
            EditorUtility.SetDirty(endless);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AuthorMainMenuEntry()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            var controller = UnityEngine.Object.FindFirstObjectByType<MainMenuSceneController>(FindObjectsInactive.Include);
            var arenaButton = FindSceneObject("Arena Button")?.GetComponent<Button>();
            if (controller == null || arenaButton == null)
            {
                throw new InvalidOperationException("MainMenu.unity is missing its controller or Arena button.");
            }

            var existing = FindSceneObject("Endless Button");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            var arenaRect = arenaButton.GetComponent<RectTransform>();
            arenaRect.anchoredPosition = new Vector2(230f, -397f);
            arenaRect.sizeDelta = new Vector2(400f, 125f);
            SetButtonLabel(arenaButton, "CITADEL FALL ARENA", 25f);

            var endlessObject = UnityEngine.Object.Instantiate(arenaButton.gameObject, arenaButton.transform.parent);
            endlessObject.name = "Endless Button";
            var endlessRect = endlessObject.GetComponent<RectTransform>();
            endlessRect.anchoredPosition = new Vector2(-230f, -397f);
            endlessRect.sizeDelta = new Vector2(400f, 125f);
            var endlessButton = endlessObject.GetComponent<Button>();
            endlessButton.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(endlessButton.onClick, controller.PlayEndless);
            SetButtonLabel(endlessButton, "ENDLESS DEFENSE", 25f);

            EditorUtility.SetDirty(arenaButton);
            EditorUtility.SetDirty(endlessButton);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AddSceneToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(entry => entry.path != EndlessScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(EndlessScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        private static void LoadAssets()
        {
            var fontGuid = AssetDatabase.FindAssets("MedievalSharp-Regular SDF t:TMP_FontAsset").FirstOrDefault();
            _font = string.IsNullOrEmpty(fontGuid) ? null : AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(fontGuid));
            _buttonPlate = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/MainMenu/FantasyButtonPlate.png");
        }

        private static GameObject FindSceneObject(string objectName)
        {
            var activeScene = SceneManager.GetActiveScene();
            return Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(candidate => candidate.scene == activeScene && candidate.name == objectName);
        }

        private static void DestroyNamedChild(Transform root, string childName)
        {
            var target = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(child => child.name == childName);
            if (target != null)
            {
                UnityEngine.Object.DestroyImmediate(target.gameObject);
            }
        }

        private static GameObject CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var result = new GameObject(name, typeof(RectTransform));
            result.transform.SetParent(parent, false);
            var rect = result.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return result;
        }

        private static void SetAnchors(RectTransform rect, Vector2 anchor)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        }

        private static void StylePanel(GameObject target, Color background, Color border, float thickness)
        {
            target.AddComponent<Image>().color = background;
            var outline = target.AddComponent<Outline>();
            outline.effectColor = border;
            outline.effectDistance = new Vector2(thickness, -thickness);
        }

        private static TMP_Text CreateText(string name, Transform parent, string value, Vector2 position, Vector2 size, float fontSize, Color? color = null)
        {
            var root = CreateRect(name, parent, position, size);
            var text = root.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.font = _font;
            text.fontSize = fontSize;
            text.color = color ?? Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(14f, fontSize * 0.7f);
            text.fontSizeMax = fontSize;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, string label)
        {
            var root = CreateRect(name, parent, position, size);
            var image = root.AddComponent<Image>();
            image.sprite = _buttonPlate;
            image.type = _buttonPlate == null ? Image.Type.Simple : Image.Type.Sliced;
            var button = root.AddComponent<Button>();
            button.targetGraphic = image;
            SetButtonLabel(button, label, 23f);
            return button;
        }

        private static void SetButtonLabel(Button button, string label, float fontSize)
        {
            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text == null)
            {
                text = CreateText("Text", button.transform, label, Vector2.zero, button.GetComponent<RectTransform>().sizeDelta - new Vector2(30f, 20f), fontSize);
            }

            text.text = label;
            text.font = _font;
            text.fontSize = fontSize;
            text.enableAutoSizing = true;
            text.fontSizeMin = 16f;
            text.fontSizeMax = fontSize;
        }

        private static void SetReference(SerializedObject serialized, string propertyName, UnityEngine.Object value)
        {
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"Serialized property not found: {propertyName}");
            }

            property.objectReferenceValue = value;
        }
    }
}
