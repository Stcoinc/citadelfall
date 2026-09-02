using System.Collections.Generic;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Editor
{
    public static class CitadelFallArenaSceneAuthoring
    {
        private const string ScenePath = "Assets/_Project/Scenes/Gameplay/CitadelFallArena.unity";
        private static TMP_FontAsset _font;
        private static Sprite _panelSprite;
        private static Sprite[] _heroSprites;

        [MenuItem("Tower Defense/Arena/Rebuild Citadel Fall Arena Scene")]
        public static void Rebuild()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _font = FindFont();
            _panelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            _heroSprites = LoadHeroSprites();

            var camera = CreateCamera();
            CreateBackground();
            CreateFormationPanel();
            var path = CreatePath();
            var sockets = CreateSockets();
            var controller = new GameObject("Citadel Fall Arena Controller", typeof(CitadelFallArenaController))
                .GetComponent<CitadelFallArenaController>();
            CreateHud(controller, camera, out var hud);
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            WireController(controller, camera, path, sockets, hud);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Rebuilt the scene-authored Citadel Fall Arena battlefield and HUD.");
        }

        private static Camera CreateCamera()
        {
            var go = new GameObject("Arena Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);
            var camera = go.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.25f;
            camera.backgroundColor = new Color32(9, 22, 35, 255);
            return camera;
        }

        private static void CreateBackground()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/_Project/Art/UI/MainMenu/MainMenuAdventureBackground.png");
            var go = new GameObject("Arena Landscape", typeof(SpriteRenderer));
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -20;
            renderer.color = new Color(0.42f, 0.52f, 0.58f, 1f);
            if (sprite != null)
            {
                go.transform.localScale = new Vector3(
                    22.3f / sprite.bounds.size.x,
                    12.5f / sprite.bounds.size.y,
                    1f);
            }
        }

        private static void CreateFormationPanel()
        {
            var go = new GameObject("Formation Board", typeof(SpriteRenderer));
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = _panelSprite;
            renderer.color = new Color(0.025f, 0.09f, 0.14f, 0.84f);
            renderer.sortingOrder = -5;
            go.transform.position = new Vector3(0f, -0.3f, 0f);
            if (_panelSprite != null)
            {
                go.transform.localScale = new Vector3(
                    13.1f / _panelSprite.bounds.size.x,
                    7.1f / _panelSprite.bounds.size.y,
                    1f);
            }
        }

        private static Transform[] CreatePath()
        {
            var positions = new[]
            {
                new Vector3(-10.2f, 3.7f), new Vector3(-7.2f, 3.7f),
                new Vector3(-4.4f, 3.3f), new Vector3(-1.6f, 3.75f),
                new Vector3(1.4f, 3.35f), new Vector3(4.3f, 3.75f),
                new Vector3(7.2f, 3.45f), new Vector3(10.1f, 3.45f)
            };
            var root = new GameObject("Enemy Path").transform;
            var points = new Transform[positions.Length];
            for (var i = 0; i < positions.Length; i++)
            {
                points[i] = new GameObject($"Path Point {i + 1}").transform;
                points[i].SetParent(root);
                points[i].position = positions[i];
            }

            var line = root.gameObject.AddComponent<LineRenderer>();
            line.positionCount = positions.Length;
            line.SetPositions(positions);
            line.startWidth = 0.28f;
            line.endWidth = 0.28f;
            line.numCapVertices = 5;
            line.numCornerVertices = 5;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = new Color(0.82f, 0.56f, 0.2f, 0.65f);
            line.endColor = new Color(0.34f, 0.76f, 0.84f, 0.65f);
            line.sortingOrder = -3;
            return points;
        }

        private static ArenaHeroSocket[] CreateSockets()
        {
            var root = new GameObject("Hero Formation 3x5").transform;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/_Project/Art/Sprites/Battlefield/FantasyBuildSocket.png");
            var sockets = new List<ArenaHeroSocket>();
            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 5; column++)
                {
                    var go = new GameObject(
                        $"Arena Socket {row + 1}-{column + 1}",
                        typeof(SpriteRenderer),
                        typeof(CircleCollider2D),
                        typeof(ArenaHeroSocket));
                    go.transform.SetParent(root);
                    go.transform.position = new Vector3(-4.8f + (column * 2.4f), 1.72f - (row * 1.9f));
                    go.transform.localScale = Vector3.one * 0.78f;
                    var renderer = go.GetComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sortingOrder = 1;
                    go.GetComponent<CircleCollider2D>().radius = 0.85f;

                    var rankCanvas = new GameObject("Rank Canvas", typeof(Canvas)).GetComponent<Canvas>();
                    rankCanvas.transform.SetParent(go.transform, false);
                    rankCanvas.renderMode = RenderMode.WorldSpace;
                    rankCanvas.sortingOrder = 8;
                    var rankRect = rankCanvas.GetComponent<RectTransform>();
                    rankRect.sizeDelta = new Vector2(180f, 55f);
                    rankRect.localScale = Vector3.one * 0.008f;
                    rankRect.localPosition = new Vector3(0.58f, -0.58f, -0.2f);
                    var rank = CreateText("Rank", rankCanvas.transform, string.Empty, 30, TextAlignmentOptions.Center, Color.white);
                    Stretch(rank.rectTransform);

                    var socket = go.GetComponent<ArenaHeroSocket>();
                    var serialized = new SerializedObject(socket);
                    serialized.FindProperty("_platformRenderer").objectReferenceValue = renderer;
                    serialized.FindProperty("_rankText").objectReferenceValue = rank;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    sockets.Add(socket);
                }
            }

            return sockets.ToArray();
        }

        private static void CreateHud(CitadelFallArenaController controller, Camera camera, out HudReferences hud)
        {
            var canvasObject = new GameObject("Arena HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            var top = CreatePanel("Top Bar", canvas.transform, new Color(0.015f, 0.06f, 0.11f, 0.94f));
            SetRect(top.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 82f), new Vector2(0.5f, 1f));
            hud = new HudReferences
            {
                Mana = CreateCounter(top.transform, "MANA", 110f),
                Cost = CreateCounter(top.transform, "NEXT SUMMON", 350f),
                Lives = CreateCounter(top.transform, "STRONGHOLD", 620f),
                Wave = CreateCounter(top.transform, "WAVE", 865f),
                Timer = CreateCounter(top.transform, "TIME", 1080f)
            };

            var title = CreateText("Arena Title", canvas.transform, "CITADEL FALL ARENA", 34, TextAlignmentOptions.Center, new Color32(255, 215, 95, 255));
            SetRect(title.rectTransform, Vector2.one, Vector2.one, new Vector2(-640f, -108f), new Vector2(520f, 54f), new Vector2(0.5f, 1f));
            title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -108f);

            var messagePanel = CreatePanel("Message Panel", canvas.transform, new Color(0.02f, 0.08f, 0.14f, 0.92f));
            SetRect(messagePanel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 176f), new Vector2(760f, 50f), new Vector2(0.5f, 0f));
            hud.Message = CreateText("Message", messagePanel.transform, string.Empty, 21, TextAlignmentOptions.Center, Color.white);
            Stretch(hud.Message.rectTransform);

            var deck = CreatePanel("Five Hero Deck", canvas.transform, new Color(0.01f, 0.045f, 0.08f, 0.97f));
            SetRect(deck.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-130f, 67f), new Vector2(840f, 112f), new Vector2(0.5f, 0f));
            hud.DeckLabels = new TMP_Text[5];
            hud.DeckImages = new UnityEngine.UI.Image[5];
            for (var i = 0; i < 5; i++)
            {
                var card = CreatePanel($"Hero Card {i + 1}", deck.transform, new Color(0.04f, 0.16f, 0.24f, 1f));
                SetRect(card.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(90f + (i * 164f), 0f), new Vector2(150f, 90f), new Vector2(0.5f, 0.5f));
                hud.DeckImages[i] = CreatePanel("Hero Portrait", card.transform, Color.white);
                hud.DeckImages[i].preserveAspect = true;
                SetRect(hud.DeckImages[i].rectTransform, new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.98f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
                hud.DeckLabels[i] = CreateText("Hero Label", card.transform, "HERO", 15, TextAlignmentOptions.Center, new Color32(240, 214, 144, 255));
                SetRect(hud.DeckLabels[i].rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, -34f), new Vector2(0f, -68f), new Vector2(0.5f, 0.5f));
            }

            hud.Summon = CreateButton("Summon Button", canvas.transform, "SUMMON", new Vector2(1130f, 84f), new Vector2(250f, 112f));
            UnityEventTools.AddPersistentListener(hud.Summon.onClick, controller.SummonPressed);
            CreateDeckBuilder(canvas.transform, controller, out hud.DeckBuilder, out hud.DeckChoiceFrames, out hud.DeckChoiceLabels);
            hud.Start = CreateButton("Start Arena Button", hud.DeckBuilder.transform, "START ARENA", new Vector2(500f, 54f), new Vector2(300f, 64f));
            UnityEventTools.AddPersistentListener(hud.Start.onClick, controller.StartMatchPressed);
            var menu = CreateButton("Return Button", canvas.transform, "MENU", new Vector2(1170f, 655f), new Vector2(150f, 48f));
            UnityEventTools.AddPersistentListener(menu.onClick, controller.ReturnToMenuPressed);
        }

        private static void WireController(CitadelFallArenaController controller, Camera camera, Transform[] path, ArenaHeroSocket[] sockets, HudReferences hud)
        {
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_starterContentJson").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Project/Data/Json/Defaults/starter_content.json");
            SetArray(serialized.FindProperty("_sockets"), sockets);
            SetArray(serialized.FindProperty("_pathPoints"), path);
            serialized.FindProperty("_heroPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Modules/TowerLaserPlaceholder.prefab");
            serialized.FindProperty("_enemyPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/EnemyScoutPlaceholder.prefab");
            serialized.FindProperty("_worldCamera").objectReferenceValue = camera;
            serialized.FindProperty("_manaText").objectReferenceValue = hud.Mana;
            serialized.FindProperty("_summonCostText").objectReferenceValue = hud.Cost;
            serialized.FindProperty("_livesText").objectReferenceValue = hud.Lives;
            serialized.FindProperty("_waveText").objectReferenceValue = hud.Wave;
            serialized.FindProperty("_timerText").objectReferenceValue = hud.Timer;
            serialized.FindProperty("_messageText").objectReferenceValue = hud.Message;
            SetArray(serialized.FindProperty("_deckLabels"), hud.DeckLabels);
            SetArray(serialized.FindProperty("_deckImages"), hud.DeckImages);
            SetArray(serialized.FindProperty("_heroSprites"), _heroSprites);
            serialized.FindProperty("_deckBuilderPanel").objectReferenceValue = hud.DeckBuilder.gameObject;
            SetArray(serialized.FindProperty("_deckChoiceFrames"), hud.DeckChoiceFrames);
            SetArray(serialized.FindProperty("_deckChoiceLabels"), hud.DeckChoiceLabels);
            serialized.FindProperty("_summonButton").objectReferenceValue = hud.Summon;
            serialized.FindProperty("_startButton").objectReferenceValue = hud.Start;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TMP_Text CreateCounter(Transform parent, string label, float x)
        {
            var group = new GameObject($"{label} Counter", typeof(RectTransform));
            group.transform.SetParent(parent, false);
            SetRect((RectTransform)group.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -41f), new Vector2(220f, 70f), new Vector2(0.5f, 0.5f));
            var heading = CreateText("Label", group.transform, label, 15, TextAlignmentOptions.Top, new Color32(116, 211, 225, 255));
            Stretch(heading.rectTransform);
            var value = CreateText("Value", group.transform, "0", 28, TextAlignmentOptions.Bottom, Color.white);
            Stretch(value.rectTransform);
            return value;
        }

        private static void CreateDeckBuilder(
            Transform parent,
            CitadelFallArenaController controller,
            out UnityEngine.UI.Image panel,
            out UnityEngine.UI.Image[] frames,
            out TMP_Text[] labels)
        {
            var ids = new[] { "hero_mage", "hero_warrior", "hero_paladin", "hero_archer", "hero_druid", "hero_sorcerer" };
            var names = new[] { "MAGE", "WARRIOR", "PALADIN", "ARCHER", "DRUID", "SORCERER" };
            panel = CreatePanel("Deck Builder Panel", parent, new Color(0.015f, 0.06f, 0.11f, 0.97f));
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 350f), new Vector2(0.5f, 0.5f));
            var title = CreateText("Deck Builder Title", panel.transform, "CHOOSE FIVE HEROES", 28, TextAlignmentOptions.Center, new Color32(255, 215, 95, 255));
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -36f), new Vector2(0f, 48f), new Vector2(0.5f, 1f));
            frames = new UnityEngine.UI.Image[ids.Length];
            labels = new TMP_Text[ids.Length];
            for (var i = 0; i < ids.Length; i++)
            {
                var frame = CreatePanel($"{names[i]} Choice", panel.transform, new Color32(28, 76, 107, 255));
                SetRect(frame.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(92f + (i * 163f), -165f), new Vector2(145f, 185f), new Vector2(0.5f, 0.5f));
                var button = frame.gameObject.AddComponent<Button>();
                button.targetGraphic = frame;
                UnityEventTools.AddStringPersistentListener(button.onClick, controller.ToggleDeckHero, ids[i]);
                var portrait = CreatePanel("Portrait", frame.transform, Color.white);
                portrait.sprite = i < _heroSprites.Length ? _heroSprites[i] : null;
                portrait.preserveAspect = true;
                portrait.raycastTarget = false;
                SetRect(portrait.rectTransform, new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.98f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
                labels[i] = CreateText("Name", frame.transform, names[i], 16, TextAlignmentOptions.Center, Color.white);
                SetRect(labels[i].rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, -72f), new Vector2(0f, -144f), new Vector2(0.5f, 0.5f));
                frames[i] = frame;
            }
        }

        private static UnityEngine.UI.Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = _panelSprite;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = color;
            return image;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size)
        {
            var image = CreatePanel(name, parent, new Color32(18, 66, 107, 255));
            SetRect(image.rectTransform, Vector2.zero, Vector2.zero, position, size, new Vector2(0.5f, 0.5f));
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var text = CreateText("Label", image.transform, label, 25, TextAlignmentOptions.Center, Color.white);
            Stretch(text.rectTransform);
            return button;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, float size, TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = _font;
            label.fontSize = size;
            label.text = text;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private static TMP_FontAsset FindFont()
        {
            var guids = AssetDatabase.FindAssets("MedievalSharp-Regular SDF t:TMP_FontAsset");
            return guids.Length == 0
                ? TMP_Settings.defaultFontAsset
                : AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static Sprite[] LoadHeroSprites()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Sprites/Heroes/HeroDefendersSheet.png");
            var names = new[] { "hero_mage", "hero_warrior", "hero_paladin", "hero_archer", "hero_druid", "hero_sorcerer" };
            var sprites = new Sprite[names.Length];
            foreach (var asset in assets)
            {
                if (!(asset is Sprite sprite))
                {
                    continue;
                }

                var index = System.Array.IndexOf(names, sprite.name);
                if (index >= 0)
                {
                    sprites[index] = sprite;
                }
            }

            return sprites;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetArray<T>(SerializedProperty property, T[] values) where T : Object
        {
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void AddToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(scene => scene.path == ScenePath))
            {
                scenes.Insert(Mathf.Min(4, scenes.Count), new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        private sealed class HudReferences
        {
            public TMP_Text Mana;
            public TMP_Text Cost;
            public TMP_Text Lives;
            public TMP_Text Wave;
            public TMP_Text Timer;
            public TMP_Text Message;
            public TMP_Text[] DeckLabels;
            public UnityEngine.UI.Image[] DeckImages;
            public UnityEngine.UI.Image DeckBuilder;
            public UnityEngine.UI.Image[] DeckChoiceFrames;
            public TMP_Text[] DeckChoiceLabels;
            public Button Summon;
            public Button Start;
        }
    }
}
