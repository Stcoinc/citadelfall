using System;
using System.Linq;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace ClubGamerZone.TowerDefense.Editor
{
    public static class MainMenuProfileSceneAuthoring
    {
        private const string MenuPath = "Tools/Citadel Fall/Author Main Menu Profile";
        private static TMP_FontAsset _font;
        private static Sprite _buttonPlate;

        [MenuItem(MenuPath)]
        public static void Author()
        {
            var scene = SceneManager.GetActiveScene();
            var canvas = FindSceneObject("Main Menu Canvas");
            var controller = FindSceneObject("Main Menu Controller")?.GetComponent<MainMenuSceneController>();
            var avatarFrame = FindSceneObject("Player Avatar Frame");
            if (canvas == null || controller == null || avatarFrame == null)
            {
                throw new InvalidOperationException("Open MainMenu.unity before authoring the profile panel.");
            }

            var fontGuid = AssetDatabase.FindAssets("MedievalSharp-Regular SDF t:TMP_FontAsset").FirstOrDefault();
            _font = string.IsNullOrEmpty(fontGuid)
                ? null
                : AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(fontGuid));
            _buttonPlate = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/MainMenu/FantasyButtonPlate.png");

            var oldPanel = FindSceneObject("Profile Details Panel");
            if (oldPanel != null)
            {
                UnityEngine.Object.DestroyImmediate(oldPanel);
            }

            var avatarButton = avatarFrame.GetComponent<Button>() ?? avatarFrame.AddComponent<Button>();
            var avatarFrameImage = avatarFrame.GetComponent<Image>();
            avatarFrameImage.raycastTarget = true;
            avatarButton.targetGraphic = avatarFrameImage;
            avatarButton.colors = CreateButtonColors();
            avatarButton.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(avatarButton.onClick, controller.ShowProfileDetails);

            var modal = CreateRect("Profile Details Panel", canvas.transform, Vector2.zero, new Vector2(1920f, 1080f));
            var dim = modal.AddComponent<Image>();
            dim.color = new Color32(1, 10, 18, 220);
            dim.raycastTarget = true;

            var card = CreateRect("Profile Details Card", modal.transform, Vector2.zero, new Vector2(1120f, 760f));
            StylePanel(card, new Color32(5, 32, 51, 252), new Color32(226, 166, 43, 255), 4f);
            CreateText("Profile Heading", card.transform, "ADVENTURER PROFILE", new Vector2(0f, 326f), new Vector2(800f, 70f), 38f, TextAlignmentOptions.Center, new Color32(255, 221, 116, 255));
            CreateText("Permanent Name Caption", card.transform, "YOUR CITADEL FALL LEGACY", new Vector2(0f, 281f), new Vector2(600f, 38f), 20f, TextAlignmentOptions.Center, new Color32(96, 204, 232, 255));

            var detailsAvatarFrame = CreateRect("Profile Details Avatar Frame", card.transform, new Vector2(-405f, 155f), new Vector2(220f, 220f));
            StylePanel(detailsAvatarFrame, new Color32(28, 56, 70, 255), new Color32(226, 166, 43, 255), 4f);
            var avatarImage = CreateRect("Profile Details Avatar", detailsAvatarFrame.transform, Vector2.zero, new Vector2(198f, 198f)).AddComponent<Image>();
            avatarImage.preserveAspect = true;
            var usernameText = CreateText("Profile Details Username", card.transform, "UNNAMED ADVENTURER", new Vector2(-405f, 14f), new Vector2(300f, 62f), 27f, TextAlignmentOptions.Center, Color.white);
            usernameText.enableAutoSizing = true;
            usernameText.fontSizeMin = 16f;
            usernameText.fontSizeMax = 27f;
            var levelText = CreateText("Profile Details Level", card.transform, "LEVEL 1", new Vector2(-405f, -32f), new Vector2(280f, 42f), 22f, TextAlignmentOptions.Center, new Color32(255, 221, 116, 255));

            var progressPanel = CreateInfoPanel("Progress Panel", card.transform, new Vector2(-115f, 155f), new Vector2(390f, 180f), "PROGRESSION");
            var progressText = CreateText("Profile Details Progress", progressPanel.transform, string.Empty, new Vector2(0f, -22f), new Vector2(350f, 105f), 21f, TextAlignmentOptions.Center, Color.white);
            var collectionPanel = CreateInfoPanel("Collection Panel", card.transform, new Vector2(315f, 155f), new Vector2(390f, 180f), "COLLECTION");
            var collectionText = CreateText("Profile Details Collection", collectionPanel.transform, string.Empty, new Vector2(0f, -25f), new Vector2(350f, 112f), 20f, TextAlignmentOptions.Center, Color.white);
            var adventurePanel = CreateInfoPanel("Adventure Panel", card.transform, new Vector2(-115f, -70f), new Vector2(390f, 170f), "ADVENTURE DEFENSE");
            var adventureText = CreateText("Profile Details Adventure", adventurePanel.transform, string.Empty, new Vector2(0f, -23f), new Vector2(350f, 100f), 20f, TextAlignmentOptions.Center, Color.white);
            var arenaPanel = CreateInfoPanel("Arena Panel", card.transform, new Vector2(315f, -70f), new Vector2(390f, 170f), "CITADEL FALL ARENA");
            var arenaText = CreateText("Profile Details Arena", arenaPanel.transform, string.Empty, new Vector2(0f, -25f), new Vector2(360f, 110f), 19f, TextAlignmentOptions.Center, Color.white);

            var claimSection = CreateRect("Username Claim Section", card.transform, new Vector2(0f, -235f), new Vector2(900f, 120f));
            StylePanel(claimSection, new Color32(11, 49, 65, 245), new Color32(50, 149, 182, 255), 2f);
            CreateText("Username Claim Heading", claimSection.transform, "CHOOSE YOUR PERMANENT USERNAME", new Vector2(0f, 39f), new Vector2(700f, 30f), 18f, TextAlignmentOptions.Center, new Color32(255, 221, 116, 255));
            var usernameInput = CreateInput("Username Claim Input", claimSection.transform, new Vector2(-145f, 2f), new Vector2(510f, 54f));
            var claimButton = CreateButton("Username Claim Button", claimSection.transform, new Vector2(305f, 2f), new Vector2(230f, 58f), "CLAIM NAME", 20f, controller.ClaimUsername);
            var claimMessage = CreateText("Username Claim Message", claimSection.transform, "Choose carefully. Your username is permanent and unique.", new Vector2(0f, -43f), new Vector2(820f, 28f), 15f, TextAlignmentOptions.Center, new Color32(180, 220, 230, 255));
            CreateButton("Profile Details Close Button", card.transform, new Vector2(0f, -336f), new Vector2(270f, 62f), "CLOSE", 23f, controller.HideProfileDetails);

            var serializedController = new SerializedObject(controller);
            SetReference(serializedController, "_profileDetailsPanel", modal);
            SetReference(serializedController, "_profileDetailsAvatarImage", avatarImage);
            SetReference(serializedController, "_profileDetailsUsernameText", usernameText);
            SetReference(serializedController, "_profileDetailsLevelText", levelText);
            SetReference(serializedController, "_profileDetailsProgressText", progressText);
            SetReference(serializedController, "_profileDetailsCollectionText", collectionText);
            SetReference(serializedController, "_profileDetailsAdventureText", adventureText);
            SetReference(serializedController, "_profileDetailsArenaText", arenaText);
            SetReference(serializedController, "_usernameClaimSection", claimSection);
            SetReference(serializedController, "_usernameClaimInput", usernameInput);
            SetReference(serializedController, "_usernameClaimButton", claimButton);
            SetReference(serializedController, "_usernameClaimMessageText", claimMessage);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            modal.transform.SetAsLastSibling();
            modal.SetActive(false);
            EditorUtility.SetDirty(avatarFrameImage);
            EditorUtility.SetDirty(avatarButton);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject FindSceneObject(string objectName)
        {
            var activeScene = SceneManager.GetActiveScene();
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(candidate => candidate.scene == activeScene && candidate.name == objectName);
        }

        private static GameObject CreateInfoPanel(string name, Transform parent, Vector2 position, Vector2 size, string heading)
        {
            var panel = CreateRect(name, parent, position, size);
            StylePanel(panel, new Color32(8, 39, 59, 235), new Color32(204, 151, 44, 210), 2f);
            CreateText($"{name} Heading", panel.transform, heading, new Vector2(0f, 56f), new Vector2(size.x - 30f, 34f), 18f, TextAlignmentOptions.Center, new Color32(255, 213, 93, 255));
            return panel;
        }

        private static void StylePanel(GameObject target, Color background, Color border, float thickness)
        {
            target.AddComponent<Image>().color = background;
            var outline = target.AddComponent<Outline>();
            outline.effectColor = border;
            outline.effectDistance = new Vector2(thickness, -thickness);
        }

        private static TMP_InputField CreateInput(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var root = CreateRect(name, parent, position, size);
            var image = root.AddComponent<Image>();
            image.color = new Color32(236, 239, 225, 255);
            var input = root.AddComponent<TMP_InputField>();
            input.targetGraphic = image;
            input.characterLimit = 20;
            input.contentType = TMP_InputField.ContentType.Standard;
            var text = CreateText("Text", root.transform, string.Empty, Vector2.zero, size - new Vector2(28f, 8f), 22f, TextAlignmentOptions.MidlineLeft, new Color32(17, 32, 43, 255));
            text.textWrappingMode = TextWrappingModes.NoWrap;
            var placeholder = CreateText("Placeholder", root.transform, "3-20 letters, numbers, or _", Vector2.zero, size - new Vector2(28f, 8f), 18f, TextAlignmentOptions.MidlineLeft, new Color32(87, 102, 110, 180));
            placeholder.fontStyle = FontStyles.Italic;
            input.textViewport = root.GetComponent<RectTransform>();
            input.textComponent = text;
            input.placeholder = placeholder;
            return input;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, string label, float fontSize, UnityAction action)
        {
            var root = CreateRect(name, parent, position, size);
            var image = root.AddComponent<Image>();
            image.sprite = _buttonPlate;
            image.type = _buttonPlate == null ? Image.Type.Simple : Image.Type.Sliced;
            var button = root.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = CreateButtonColors();
            UnityEventTools.AddPersistentListener(button.onClick, action);
            CreateText("Text", root.transform, label, Vector2.zero, size - new Vector2(20f, 10f), fontSize, TextAlignmentOptions.Center, Color.white);
            return button;
        }

        private static TMP_Text CreateText(string name, Transform parent, string value, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment, Color color)
        {
            var root = CreateRect(name, parent, position, size);
            var text = root.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.font = _font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
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

        private static ColorBlock CreateButtonColors()
        {
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color32(255, 230, 160, 255);
            colors.pressedColor = new Color32(190, 220, 235, 255);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color32(90, 90, 90, 150);
            return colors;
        }

        private static void SetReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"Serialized property not found: {propertyName}");
            }

            property.objectReferenceValue = value;
        }
    }
}
