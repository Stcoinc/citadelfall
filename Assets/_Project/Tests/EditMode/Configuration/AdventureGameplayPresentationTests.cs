using System.Collections.Generic;
using System.Reflection;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Tests.EditMode.Configuration
{
    public sealed class AdventureGameplayPresentationTests
    {
        [Test]
        public void TryBuildTower_LeavesDetailsClosedUntilOccupiedSocketIsSelectedAgain()
        {
            var socketObject = new GameObject("Socket", typeof(SpriteRenderer), typeof(TowerPlacementSocket));
            var prefab = new GameObject("Hero Prefab", typeof(TowerWeapon));
            var panelRoot = new GameObject("Details Root");
            var panelObject = new GameObject("Details Controller", typeof(TowerDetailsPanel));
            var rangeObject = new GameObject("Unit Range Indicator", typeof(LineRenderer), typeof(UnitRangeIndicator));

            try
            {
                var panel = panelObject.GetComponent<TowerDetailsPanel>();
                var socket = socketObject.GetComponent<TowerPlacementSocket>();
                if (panel == null || socket == null)
                {
                    Assert.Fail("Regression fixture could not create its gameplay components.");
                }

                SetPrivateField(panel, "_panelRoot", panelRoot);
                SetPrivateField(socket, "_detailsPanel", panel);
                var rangeIndicator = rangeObject.GetComponent<UnitRangeIndicator>();
                SetPrivateField(rangeIndicator, "_lineRenderer", rangeObject.GetComponent<LineRenderer>());
                rangeObject.transform.SetParent(socketObject.transform, false);
                SetPrivateField(socket, "_rangeIndicator", rangeIndicator);
                rangeObject.SetActive(false);
                panelRoot.SetActive(false);
                socketObject.transform.localScale = Vector3.one * 0.4f;

                var definition = CreateTowerDefinition();
                socket.Initialize(definition, prefab, new EnemyRegistry(), ignored => true);

                Assert.That(socket.TryBuildTower(), Is.True);
                Assert.That(socket.ActiveTowerTransform.lossyScale.x, Is.EqualTo(0.78f).Within(0.001f),
                    "Shrinking the authored socket base must not shrink the placed hero.");
                var placedWeapon = socket.ActiveTowerTransform.GetComponent<TowerWeapon>();
                socket.BeginDragPreview();
                Assert.That(placedWeapon.enabled, Is.False, "A dragged Adventure hero must stop attacking.");
                socket.EndDragPreview();
                Assert.That(placedWeapon.enabled, Is.True, "Returning the hero to its socket must resume attacks.");
                Assert.That(panelRoot.activeSelf, Is.False, "Placing a hero must not open its details panel.");

                socket.HandleSelection();
                Assert.That(panelRoot.activeSelf, Is.True, "A later press on the occupied socket should inspect it.");
                Assert.That(rangeObject.activeSelf, Is.True, "Selecting an occupied socket should show its range.");
                Assert.That(rangeIndicator.WorldRange, Is.EqualTo(2.8f).Within(0.001f));

                panel.Hide();
                Assert.That(rangeObject.activeSelf, Is.False, "Closing the details panel should hide the range.");
            }
            finally
            {
                Object.DestroyImmediate(socketObject);
                Object.DestroyImmediate(prefab);
                Object.DestroyImmediate(panelRoot);
                Object.DestroyImmediate(panelObject);
            }
        }

        [Test]
        public void BuildVictoryText_IncludesBattleRecordRewardsAndReadableTreasureName()
        {
            var level = new LevelDefinition(
                new StableId("level_forest_001"),
                "Whispering Vale",
                GameMode.ClassicPathDefense,
                100,
                40,
                6,
                new StableId("waves_forest_001"),
                160,
                230,
                3,
                "commander_nova_badge");
            var presenter = new AdventureMissionResultPresenter();

            var text = presenter.BuildVictoryText(level, 3, 3, 18, 36, true);

            Assert.That(text, Does.Contain("Whispering Vale"));
            Assert.That(text, Does.Contain("Waves cleared  3/3"));
            Assert.That(text, Does.Contain("Enemies defeated  18"));
            Assert.That(text, Does.Contain("160 Scrap"));
            Assert.That(text, Does.Contain("Commander Nova Badge"));
            Assert.That(text, Does.Not.Contain("commander_nova_badge"));
            Assert.That(text, Does.Contain("NEW ADVENTURE UNLOCKED"));
        }

        [Test]
        public void ShowBuildPreview_DisplaysLevelOneStatsButDisablesPlacedUnitActions()
        {
            var panelRoot = new GameObject("Details Root");
            var panelObject = new GameObject("Details Controller", typeof(TowerDetailsPanel));
            var nameObject = new GameObject("Name", typeof(TextMeshProUGUI));
            var levelObject = new GameObject("Level", typeof(TextMeshProUGUI));
            var actionObject = new GameObject("Action", typeof(TextMeshProUGUI));
            var upgradeButtonObject = new GameObject("Upgrade Button", typeof(Button));
            var mergeButtonObject = new GameObject("Merge Button", typeof(Button));
            var sellButtonObject = new GameObject("Sell Button", typeof(Button));
            var upgradeButtonTextObject = new GameObject("Upgrade Button Text", typeof(TextMeshProUGUI));
            var mergeButtonTextObject = new GameObject("Merge Button Text", typeof(TextMeshProUGUI));
            var sellButtonTextObject = new GameObject("Sell Button Text", typeof(TextMeshProUGUI));

            try
            {
                var panel = panelObject.GetComponent<TowerDetailsPanel>();
                SetPrivateField(panel, "_panelRoot", panelRoot);
                SetPrivateField(panel, "_nameText", nameObject.GetComponent<TextMeshProUGUI>());
                SetPrivateField(panel, "_levelText", levelObject.GetComponent<TextMeshProUGUI>());
                SetPrivateField(panel, "_actionMessageText", actionObject.GetComponent<TextMeshProUGUI>());
                SetPrivateField(panel, "_upgradeButton", upgradeButtonObject.GetComponent<Button>());
                SetPrivateField(panel, "_mergeButton", mergeButtonObject.GetComponent<Button>());
                SetPrivateField(panel, "_sellButton", sellButtonObject.GetComponent<Button>());
                SetPrivateField(panel, "_upgradeButtonText", upgradeButtonTextObject.GetComponent<TextMeshProUGUI>());
                SetPrivateField(panel, "_mergeButtonText", mergeButtonTextObject.GetComponent<TextMeshProUGUI>());
                SetPrivateField(panel, "_sellButtonText", sellButtonTextObject.GetComponent<TextMeshProUGUI>());

                panelRoot.SetActive(false);
                panel.ShowBuildPreview(CreateTowerDefinition(), 35);

                Assert.That(panelRoot.activeSelf, Is.True);
                Assert.That(nameObject.GetComponent<TextMeshProUGUI>().text, Is.EqualTo("Warrior"));
                Assert.That(levelObject.GetComponent<TextMeshProUGUI>().text, Is.EqualTo("Preview Level 1/7"));
                Assert.That(actionObject.GetComponent<TextMeshProUGUI>().text, Does.Contain("Cost 35 Scrap"));
                Assert.That(upgradeButtonObject.GetComponent<Button>().interactable, Is.False);
                Assert.That(mergeButtonObject.GetComponent<Button>().interactable, Is.False);
                Assert.That(sellButtonObject.GetComponent<Button>().interactable, Is.False);
                Assert.That(upgradeButtonTextObject.GetComponent<TextMeshProUGUI>().text, Does.Contain("PLACE FIRST"));
                Assert.That(mergeButtonTextObject.GetComponent<TextMeshProUGUI>().text, Does.Contain("PLACE FIRST"));
                Assert.That(sellButtonTextObject.GetComponent<TextMeshProUGUI>().text, Does.Contain("PLACE FIRST"));
            }
            finally
            {
                Object.DestroyImmediate(panelRoot);
                Object.DestroyImmediate(panelObject);
                Object.DestroyImmediate(nameObject);
                Object.DestroyImmediate(levelObject);
                Object.DestroyImmediate(actionObject);
                Object.DestroyImmediate(upgradeButtonObject);
                Object.DestroyImmediate(mergeButtonObject);
                Object.DestroyImmediate(sellButtonObject);
                Object.DestroyImmediate(upgradeButtonTextObject);
                Object.DestroyImmediate(mergeButtonTextObject);
                Object.DestroyImmediate(sellButtonTextObject);
            }
        }

        [Test]
        public void ArenaSelection_ShowsAndHidesConfiguredHeroRange()
        {
            var socketObject = new GameObject("Arena Socket", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(ArenaHeroSocket));
            var heroPrefab = new GameObject("Arena Hero Prefab", typeof(TowerWeapon));
            var rangeObject = new GameObject("Unit Range Indicator", typeof(LineRenderer), typeof(UnitRangeIndicator));

            try
            {
                var socket = socketObject.GetComponent<ArenaHeroSocket>();
                var rangeIndicator = rangeObject.GetComponent<UnitRangeIndicator>();
                SetPrivateField(rangeIndicator, "_lineRenderer", rangeObject.GetComponent<LineRenderer>());
                rangeObject.transform.SetParent(socketObject.transform, false);
                SetPrivateField(socket, "_rangeIndicator", rangeIndicator);
                rangeObject.SetActive(false);
                socketObject.transform.localScale = Vector3.one * 0.5f;

                socket.Initialize(heroPrefab, new EnemyRegistry());
                Assert.That(socket.Summon(CreateTowerDefinition()), Is.True);
                Assert.That(socket.ActiveHeroTransform.lossyScale.x, Is.EqualTo(0.86f).Within(0.001f),
                    "Shrinking the Arena platform must not shrink the summoned hero.");
                var summonedWeapon = socket.ActiveHeroTransform.GetComponent<TowerWeapon>();
                socket.BeginDragPreview();
                Assert.That(summonedWeapon.enabled, Is.False, "A dragged Arena hero must stop attacking.");

                socket.SetSelected(true);
                Assert.That(rangeObject.activeSelf, Is.True);
                Assert.That(rangeIndicator.WorldRange, Is.EqualTo(2.8f).Within(0.001f));

                socket.SetSelected(false);
                Assert.That(rangeObject.activeSelf, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(socketObject);
                Object.DestroyImmediate(heroPrefab);
            }
        }

        private static TowerDefinition CreateTowerDefinition()
        {
            return new TowerDefinition(
                new StableId("hero_warrior"),
                "Warrior",
                "Frontline defender",
                "behavior_steel_hero",
                TargetingMode.Closest,
                0,
                0,
                42f,
                2.8f,
                0.65f,
                7,
                78,
                "all",
                "steel",
                false,
                "prefab_hero_defender",
                "icon_hero_warrior",
                0,
                new List<TowerUpgradeDefinition>(),
                new List<TowerMergeDefinition>(),
                null,
                false);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
        }
    }
}
