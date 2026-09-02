#if UNITY_EDITOR
using System;
using System.Linq;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.EditorTools
{
    public static class UnitShopSceneAuthoring
    {
        private const string ScenePath = "Assets/_Project/Scenes/App/LevelSelection.unity";
        private static readonly Color DeepNavy = new Color32(2, 12, 25, 245);
        private static readonly Color Gold = new Color32(231, 183, 72, 255);
        private static readonly Color Ivory = new Color32(246, 239, 218, 255);
        private static readonly Color Muted = new Color32(194, 205, 215, 255);

        private static TMP_FontAsset _font;
        private static Sprite _greenButton;
        private static Sprite _redButton;

        [MenuItem("Tools/Citadel Fall/Authoring/Rebuild Unit Shop UI")]
        public static void Rebuild()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var controller = UnityEngine.Object.FindFirstObjectByType<LevelSelectionSceneController>(FindObjectsInactive.Include);
            var canvas = GameObject.Find("Selection Canvas");
            if (controller == null || canvas == null)
            {
                throw new InvalidOperationException("LevelSelection controller or Selection Canvas was not found.");
            }

            var root = canvas.transform.Find("Tower Shop Panel")?.gameObject ??
                       canvas.transform.Find("Unit Shop Panel")?.gameObject;
            if (root == null)
            {
                throw new InvalidOperationException("The LevelSelection shop panel was not found.");
            }

            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/MedievalSharp-Regular SDF.asset");
            var backgroundSprite = LoadSprite("Assets/_Project/Resources/UnitShop/UnitShopBackground.png");
            var frameSprite = LoadSprite("Assets/_Project/Resources/UnitShop/UnitShopPanelFrame.png");
            var paladinPortrait = LoadSprite("Assets/_Project/Resources/UnitShop/PaladinPortrait.png");
            var druidPortrait = LoadSprite("Assets/_Project/Resources/UnitShop/DruidPortrait.png");
            var sorcererPortrait = LoadSprite("Assets/_Project/Resources/UnitShop/SorcererPortrait.png");
            var buttonSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Resources/UnitShop/UnitShopButtons.png")
                .OfType<Sprite>()
                .ToArray();
            _greenButton = buttonSprites.FirstOrDefault(sprite => sprite.name.EndsWith("_3", StringComparison.Ordinal));
            _redButton = buttonSprites.FirstOrDefault(sprite => sprite.name.EndsWith("_5", StringComparison.Ordinal));

            if (_font == null || backgroundSprite == null || frameSprite == null ||
                paladinPortrait == null || druidPortrait == null || sorcererPortrait == null)
            {
                throw new InvalidOperationException("One or more Unit Shop authored assets could not be loaded.");
            }

            Undo.RegisterCompleteObjectUndo(root, "Rebuild Unit Shop hierarchy");
            while (root.transform.childCount > 0)
            {
                Undo.DestroyObjectImmediate(root.transform.GetChild(0).gameObject);
            }

            root.name = "Unit Shop Panel";
            Stretch(root.GetComponent<RectTransform>());
            DestroyComponent<Image>(root);
            DestroyComponent<UnitShopView>(root);

            var background = CreateImage(root.transform, "Unit Shop Background", backgroundSprite, Color.white);
            Stretch(background.rectTransform);
            background.raycastTarget = false;

            var shade = CreateImage(root.transform, "Backdrop Shade", null, new Color(0f, 0.025f, 0.075f, 0.42f));
            Stretch(shade.rectTransform);
            shade.raycastTarget = true;

            var frame = CreateImage(root.transform, "Ornate Unit Shop Frame", frameSprite, Color.white);
            SetRect(frame.rectTransform, Vector2.zero, new Vector2(1540f, 940f));
            frame.type = Image.Type.Sliced;
            frame.raycastTarget = false;

            CreateText(frame.transform, "Unit Shop Title", "UNIT SHOP", 58f, Gold, new Vector2(0f, 220f), new Vector2(700f, 72f), FontStyles.Bold);

            var coinsText = CreateWallet(root.transform, "Coins Wallet", "COINS  0", new Vector2(-760f, 475f), new Color32(111, 75, 12, 245));
            var gemsText = CreateWallet(root.transform, "Gems Wallet", "GEMS  0", new Vector2(760f, 475f), new Color32(8, 73, 111, 245));

            var cards = new[]
            {
                CreateCard(frame.transform, "hero_paladin", "PALADIN", "Armored holy guardian who protects the front line.", "HOLY  •  DEFENDER", "STARTER UNIT", "OWNED", paladinPortrait, new Color32(243, 201, 77, 255), new Vector2(-430f, -85f), controller.BuyShopTower0),
                CreateCard(frame.transform, "hero_druid", "DRUID", "Nature keeper who controls enemies and supports allies.", "NATURE  •  CONTROL", "STARTER UNIT", "OWNED", druidPortrait, new Color32(76, 190, 91, 255), new Vector2(0f, -85f), controller.BuyShopTower1),
                CreateCard(frame.transform, "hero_sorcerer", "SORCERER", "Arcane spellcaster with powerful burst and area damage.", "ARCANE  •  BURST", "COINS  750", "BUY", sorcererPortrait, new Color32(151, 84, 205, 255), new Vector2(430f, -85f), controller.BuyShopTower2)
            };

            var message = CreateText(frame.transform, "Unit Shop Message", string.Empty, 22f, Ivory, new Vector2(0f, -385f), new Vector2(900f, 40f));
            var closeButton = CreateButton(frame.transform, "Close Button", "CLOSE", _redButton, new Vector2(0f, -445f), new Vector2(330f, 82f), out _);
            UnityEventTools.AddPersistentListener(closeButton.onClick, controller.HideTurretShop);

            var view = Undo.AddComponent<UnitShopView>(root);
            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("_coinsText").objectReferenceValue = coinsText;
            serializedView.FindProperty("_gemsText").objectReferenceValue = gemsText;
            serializedView.FindProperty("_messageText").objectReferenceValue = message;
            var cardArray = serializedView.FindProperty("_cards");
            cardArray.arraySize = cards.Length;
            for (var i = 0; i < cards.Length; i++)
            {
                cardArray.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            }
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("_towerShopPanel").objectReferenceValue = root;
            serializedController.FindProperty("_towerShopMessageText").objectReferenceValue = message;
            serializedController.FindProperty("_unitShopView").objectReferenceValue = view;
            serializedController.FindProperty("_towerShopLabels").arraySize = 0;
            var productIds = serializedController.FindProperty("_purchasableTowerIds");
            productIds.arraySize = 3;
            productIds.GetArrayElementAtIndex(0).stringValue = "hero_paladin";
            productIds.GetArrayElementAtIndex(1).stringValue = "hero_druid";
            productIds.GetArrayElementAtIndex(2).stringValue = "hero_sorcerer";
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            var shopEntry = canvas.transform.Find("Level Panel/Tower Shop Button") ??
                            canvas.transform.Find("Level Panel/Unit Shop Button");
            if (shopEntry != null)
            {
                Undo.RecordObject(shopEntry.gameObject, "Rename Unit Shop entry");
                shopEntry.name = "Unit Shop Button";
                var entryLabel = shopEntry.GetComponentInChildren<TMP_Text>(true);
                if (entryLabel != null)
                {
                    Undo.RecordObject(entryLabel, "Rename Unit Shop label");
                    entryLabel.name = "Unit Shop Button Label";
                    entryLabel.text = "UNIT SHOP";
                    EditorUtility.SetDirty(entryLabel);
                }
            }

            root.SetActive(true);
            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = root;
            Debug.Log("Authored Unit Shop hierarchy saved to LevelSelection.unity.", root);
        }

        private static UnitShopCardView CreateCard(
            Transform parent,
            string unitId,
            string title,
            string description,
            string stats,
            string price,
            string action,
            Sprite portraitSprite,
            Color accentColor,
            Vector2 position,
            UnityAction purchaseAction)
        {
            var card = CreateImage(parent, title + " Card", null, DeepNavy);
            SetRect(card.rectTransform, position, new Vector2(390f, 590f));
            card.raycastTarget = true;
            var outline = Undo.AddComponent<Outline>(card.gameObject);
            outline.effectColor = Gold;
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            var portrait = CreateImage(card.transform, title + " Portrait", portraitSprite, Color.white);
            SetRect(portrait.rectTransform, new Vector2(0f, 135f), new Vector2(350f, 285f));
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;

            var accent = CreateImage(card.transform, title + " Class Accent", null, accentColor);
            SetRect(accent.rectTransform, new Vector2(0f, -18f), new Vector2(350f, 5f));
            accent.raycastTarget = false;

            var nameText = CreateText(card.transform, title + " Name", title, 38f, Ivory, new Vector2(0f, -57f), new Vector2(350f, 55f), FontStyles.Bold);
            var descriptionText = CreateText(card.transform, title + " Description", description, 22f, Muted, new Vector2(0f, -125f), new Vector2(340f, 82f));
            var statsText = CreateText(card.transform, title + " Stats", stats, 19f, Gold, new Vector2(0f, -181f), new Vector2(340f, 34f));
            var priceText = CreateText(card.transform, title + " Price", price, 30f, Ivory, new Vector2(0f, -230f), new Vector2(300f, 52f), FontStyles.Bold);
            var purchaseButton = CreateButton(card.transform, title + " Purchase Button", action, _greenButton, new Vector2(0f, -283f), new Vector2(300f, 72f), out var buttonLabel);
            UnityEventTools.AddPersistentListener(purchaseButton.onClick, purchaseAction);

            var cardView = Undo.AddComponent<UnitShopCardView>(card.gameObject);
            var serializedCard = new SerializedObject(cardView);
            serializedCard.FindProperty("_unitId").stringValue = unitId;
            serializedCard.FindProperty("_portraitSprite").objectReferenceValue = portraitSprite;
            serializedCard.FindProperty("_accentColor").colorValue = accentColor;
            serializedCard.FindProperty("_portrait").objectReferenceValue = portrait;
            serializedCard.FindProperty("_accent").objectReferenceValue = accent;
            serializedCard.FindProperty("_nameText").objectReferenceValue = nameText;
            serializedCard.FindProperty("_descriptionText").objectReferenceValue = descriptionText;
            serializedCard.FindProperty("_statsText").objectReferenceValue = statsText;
            serializedCard.FindProperty("_priceText").objectReferenceValue = priceText;
            serializedCard.FindProperty("_buttonLabel").objectReferenceValue = buttonLabel;
            serializedCard.FindProperty("_purchaseButton").objectReferenceValue = purchaseButton;
            serializedCard.ApplyModifiedPropertiesWithoutUndo();
            return cardView;
        }

        private static TMP_Text CreateWallet(Transform parent, string name, string value, Vector2 position, Color color)
        {
            var wallet = CreateImage(parent, name, null, color);
            SetRect(wallet.rectTransform, position, new Vector2(290f, 70f));
            wallet.raycastTarget = false;
            var outline = Undo.AddComponent<Outline>(wallet.gameObject);
            outline.effectColor = Gold;
            outline.effectDistance = new Vector2(2f, -2f);
            return CreateText(wallet.transform, name + " Text", value, 31f, Ivory, Vector2.zero, new Vector2(270f, 58f), FontStyles.Bold);
        }

        private static Button CreateButton(Transform parent, string name, string label, Sprite sprite, Vector2 position, Vector2 size, out TMP_Text labelText)
        {
            var image = CreateImage(parent, name, sprite, sprite == null ? new Color32(19, 113, 38, 255) : Color.white);
            image.raycastTarget = true;
            if (sprite != null)
            {
                image.type = Image.Type.Sliced;
            }
            SetRect(image.rectTransform, position, size);

            var button = Undo.AddComponent<Button>(image.gameObject);
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.95f, 0.78f, 1f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.disabledColor = new Color(0.34f, 0.36f, 0.39f, 0.86f);
            button.colors = colors;
            labelText = CreateText(image.transform, name + " Label", label, 32f, Ivory, Vector2.zero, size - new Vector2(36f, 16f), FontStyles.Bold);
            return button;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite, Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
            gameObject.transform.SetParent(parent, false);
            var image = gameObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(Transform parent, string name, string value, float fontSize, Color color, Vector2 position, Vector2 size, FontStyles style = FontStyles.Normal)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
            gameObject.transform.SetParent(parent, false);
            var text = gameObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.font = _font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            SetRect(text.rectTransform, position, size);
            return text;
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void DestroyComponent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component != null)
            {
                Undo.DestroyObjectImmediate(component);
            }
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }
}
#endif
