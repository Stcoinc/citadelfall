using System.Linq;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UIImage = UnityEngine.UI.Image;

namespace ClubGamerZone.TowerDefense.Editor
{
    public static class GameplayHudSceneAuthoring
    {
        private static readonly Color Navy = new Color(0.015f, 0.055f, 0.12f, 0.94f);
        private static readonly Color NavyLight = new Color(0.03f, 0.12f, 0.23f, 0.94f);
        private static readonly Color Gold = new Color(0.96f, 0.72f, 0.18f, 1f);
        private static readonly Color Cyan = new Color(0.2f, 0.9f, 1f, 1f);

        private static TMP_FontAsset _font;

        [MenuItem("Tower Defense/UI/Rebuild Gameplay HUD")]
        public static void RebuildGameplayHud()
        {
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Fonts/MedievalSharp-Regular SDF.asset");
            var buttonPlate = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/_Project/Art/UI/MainMenu/FantasyButtonPlate.png");
            var cardFrame = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/_Project/Art/UI/LevelSelection/LevelNode.png");
            var turretSprites = AssetDatabase
                .LoadAllAssetsAtPath("Assets/_Project/Art/Sprites/Towers/TurretSheet.png")
                .OfType<Sprite>()
                .OrderBy(sprite => sprite.name)
                .ToArray();

            var hud = GameObject.Find("Gameplay HUD");
            var controller = Object.FindFirstObjectByType<MvpGameplayController>();
            if (hud == null || controller == null || _font == null || buttonPlate == null ||
                cardFrame == null || turretSprites.Length < 7)
            {
                Debug.LogError("Open Gameplay.unity and verify the HUD art/font assets before rebuilding the HUD.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(hud, "Rebuild Gameplay HUD");
            var scaler = hud.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                Undo.RecordObject(scaler, "Configure Gameplay HUD scaler");
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            DestroyChild(hud.transform, "Top Resource Bar");
            DestroyChild(hud.transform, "Mission Message Panel");
            SetActive(FindChild(hud.transform, "Status Text"), false);

            var values = BuildResourceBar(hud.transform);
            BuildMessagePanel(hud.transform);
            var frames = BuildTurretTray(hud.transform, cardFrame, turretSprites);
            StyleMajorButton(
                FindChild(hud.transform, "Start Waves Button"),
                buttonPlate,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-18f, 20f),
                new Vector2(250f, 78f),
                25f,
                "BEGIN WAVES");
            StyleMajorButton(
                FindChild(hud.transform, "Pause Button"),
                buttonPlate,
                Vector2.one,
                Vector2.one,
                Vector2.one,
                new Vector2(-18f, -18f),
                new Vector2(170f, 58f),
                21f,
                "PAUSE");

            StyleAllText(hud);
            StyleOverlayCanvas(buttonPlate);
            WireController(controller, values, frames);
            EditorSceneManager.MarkSceneDirty(hud.scene);
            EditorSceneManager.SaveScene(hud.scene);
            Debug.Log("Gameplay HUD rebuilt with resource counters and seven illustrated turret cards.");
        }

        private static TMP_Text[] BuildResourceBar(Transform hud)
        {
            var bar = CreatePanel(
                hud,
                "Top Resource Bar",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(18f, -18f),
                new Vector2(830f, 74f),
                Navy);
            AddOutline(bar, Gold, new Vector2(2f, -2f));

            var titles = new[] { "STRONGHOLD", "SCRAP", "COINS", "WAVE", "ENEMIES" };
            var startingValues = new[] { "20", "120", "500", "0/3", "0" };
            var values = new TMP_Text[titles.Length];
            for (var i = 0; i < titles.Length; i++)
            {
                var chip = CreatePanel(
                    bar.transform,
                    $"Status {titles[i]}",
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(10f + (i * 162f), 0f),
                    new Vector2(150f, 54f),
                    NavyLight);
                AddOutline(
                    chip,
                    i == 0 ? new Color(0.9f, 0.25f, 0.18f, 1f) : new Color(0.15f, 0.65f, 0.85f, 1f),
                    new Vector2(1.5f, -1.5f));
                CreateText(
                    chip.transform,
                    $"{titles[i]} Label",
                    titles[i],
                    13f,
                    Gold,
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -4f),
                    new Vector2(-8f, 20f));
                values[i] = CreateText(
                    chip.transform,
                    $"{titles[i]} Value",
                    startingValues[i],
                    25f,
                    Color.white,
                    Vector2.zero,
                    Vector2.one,
                    new Vector2(0.5f, 0.35f),
                    new Vector2(0f, -7f),
                    new Vector2(-8f, 32f));
            }

            return values;
        }

        private static void BuildMessagePanel(Transform hud)
        {
            var panel = CreatePanel(
                hud,
                "Mission Message Panel",
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -100f),
                new Vector2(680f, 42f),
                new Color(0.01f, 0.04f, 0.09f, 0.9f));
            AddOutline(panel, new Color(0.2f, 0.7f, 0.9f, 0.9f), new Vector2(1.5f, -1.5f));

            var help = FindChild(hud, "Help Text");
            if (help == null)
            {
                return;
            }

            Undo.SetTransformParent(help, panel.transform, "Move mission message");
            SetStretch(help, new Vector2(12f, 4f), new Vector2(-12f, -4f));
            StyleText(help.GetComponent<TMP_Text>(), 17f, Color.white);
            help.GetComponent<TMP_Text>().text = "Choose a turret, place it on a socket, then begin the assault.";
        }

        private static UIImage[] BuildTurretTray(Transform hud, Sprite cardFrame, Sprite[] turretSprites)
        {
            var tray = FindChild(hud, "Turret Build Bar");
            if (tray == null)
            {
                return new UIImage[0];
            }

            SetRect(
                tray,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(-115f, 18f),
                new Vector2(900f, 174f));
            var trayImage = tray.GetComponent<UIImage>() ?? Undo.AddComponent<UIImage>(tray.gameObject);
            trayImage.color = Navy;
            trayImage.raycastTarget = false;
            AddOrUpdateOutline(tray.gameObject, Gold, new Vector2(2f, -2f));

            var selected = FindChild(tray, "Selected Turret Text");
            if (selected != null)
            {
                SetRect(
                    selected,
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -8f),
                    new Vector2(-26f, 30f));
                StyleText(selected.GetComponent<TMP_Text>(), 18f, Color.white);
                selected.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
            }

            var frames = new UIImage[7];
            for (var i = 0; i < frames.Length; i++)
            {
                var buttonTransform = FindChild(tray, $"Turret Option {i + 1} Button");
                if (buttonTransform == null)
                {
                    continue;
                }

                SetRect(
                    buttonTransform,
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(-378f + (i * 126f), 12f),
                    new Vector2(112f, 125f));
                ConfigureTurretButton(buttonTransform);
                DestroyChild(buttonTransform, "Card Frame");
                DestroyChild(buttonTransform, "Turret Icon");

                frames[i] = CreateImage(
                    buttonTransform,
                    "Card Frame",
                    cardFrame,
                    Color.white,
                    Vector2.zero,
                    Vector2.one,
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    Vector2.zero);
                SetStretch(frames[i].rectTransform, new Vector2(-3f, -3f), new Vector2(3f, 3f));
                frames[i].raycastTarget = false;

                var icon = CreateImage(
                    buttonTransform,
                    "Turret Icon",
                    turretSprites[i],
                    Color.white,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -10f),
                    new Vector2(76f, 76f));
                icon.preserveAspect = true;
                icon.raycastTarget = false;

                var label = FindChild(buttonTransform, "Label");
                if (label != null)
                {
                    label.SetAsLastSibling();
                    SetRect(
                        label,
                        new Vector2(0f, 0f),
                        new Vector2(1f, 0f),
                        new Vector2(0.5f, 0f),
                        new Vector2(0f, 3f),
                        new Vector2(-10f, 43f));
                    StyleText(label.GetComponent<TMP_Text>(), 13f, Color.white);
                    label.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
                }
            }

            return frames;
        }

        private static void ConfigureTurretButton(RectTransform buttonTransform)
        {
            var image = buttonTransform.GetComponent<UIImage>();
            var button = buttonTransform.GetComponent<Button>();
            if (image != null)
            {
                image.sprite = null;
                image.color = new Color(0.02f, 0.09f, 0.17f, 0.96f);
            }

            if (button == null)
            {
                return;
            }

            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.7f, 0.95f, 1f, 1f);
            colors.pressedColor = new Color(0.35f, 0.72f, 0.9f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.3f, 0.35f, 0.4f, 0.65f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        private static void StyleMajorButton(
            RectTransform rect,
            Sprite sprite,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 position,
            Vector2 size,
            float fontSize,
            string label)
        {
            if (rect == null)
            {
                return;
            }

            SetRect(rect, anchorMin, anchorMax, pivot, position, size);
            var image = rect.GetComponent<UIImage>();
            if (image != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
            }

            var text = rect.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                StyleText(text, fontSize, Color.white);
                text.fontStyle = FontStyles.Bold;
                text.text = label;
            }
        }

        private static void StyleAllText(GameObject hud)
        {
            foreach (var text in hud.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font = _font;
            }
        }

        private static void StyleOverlayCanvas(Sprite buttonPlate)
        {
            var canvas = GameObject.Find("Full Game Canvas");
            if (canvas == null)
            {
                return;
            }

            foreach (var text in canvas.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font = _font;
            }

            foreach (var button in canvas.GetComponentsInChildren<Button>(true))
            {
                var image = button.GetComponent<UIImage>();
                if (image != null)
                {
                    image.sprite = buttonPlate;
                    image.color = Color.white;
                }
            }
        }

        private static void WireController(MvpGameplayController controller, TMP_Text[] values, UIImage[] frames)
        {
            var serialized = new SerializedObject(controller);
            Assign(serialized, "_baseHealthValueText", values[0]);
            Assign(serialized, "_scrapValueText", values[1]);
            Assign(serialized, "_coinsValueText", values[2]);
            Assign(serialized, "_waveValueText", values[3]);
            Assign(serialized, "_enemiesValueText", values[4]);

            var framesProperty = serialized.FindProperty("_towerOptionFrames");
            framesProperty.arraySize = frames.Length;
            for (var i = 0; i < frames.Length; i++)
            {
                framesProperty.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            serialized.FindProperty("_selectedTowerFrameColor").colorValue = Cyan;
            serialized.FindProperty("_unselectedTowerFrameColor").colorValue = Color.white;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static void Assign(SerializedObject serialized, string name, Object value)
        {
            var property = serialized.FindProperty(name);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static GameObject CreatePanel(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UIImage));
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent, false);
            SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax, pivot, position, size);
            var image = go.GetComponent<UIImage>();
            image.color = color;
            image.raycastTarget = false;
            return go;
        }

        private static UIImage CreateImage(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 position,
            Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UIImage));
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent, false);
            SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax, pivot, position, size);
            var image = go.GetComponent<UIImage>();
            image.sprite = sprite;
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(
            Transform parent,
            string name,
            string value,
            float fontSize,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 position,
            Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent, false);
            SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax, pivot, position, size);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            StyleText(text, fontSize, color);
            return text;
        }

        private static void StyleText(TMP_Text text, float fontSize, Color color)
        {
            if (text == null)
            {
                return;
            }

            text.font = _font;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(8f, fontSize * 0.65f);
            text.fontSizeMax = fontSize;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
        }

        private static RectTransform FindChild(Transform parent, string name)
        {
            return parent.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(rect => rect.name == name);
        }

        private static void DestroyChild(Transform parent, string name)
        {
            var child = FindChild(parent, name);
            if (child != null)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }

        private static void SetActive(RectTransform rect, bool active)
        {
            if (rect != null)
            {
                rect.gameObject.SetActive(active);
            }
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 position,
            Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private static void SetStretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
        }

        private static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            var outline = Undo.AddComponent<Outline>(target);
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static void AddOrUpdateOutline(GameObject target, Color color, Vector2 distance)
        {
            var outline = target.GetComponent<Outline>() ?? Undo.AddComponent<Outline>(target);
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }
    }
}
