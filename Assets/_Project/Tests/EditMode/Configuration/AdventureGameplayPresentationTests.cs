using System.Collections.Generic;
using System.Reflection;
using ClubGamerZone.TowerDefense.Core;
using ClubGamerZone.TowerDefense.Domain.Content;
using ClubGamerZone.TowerDefense.Features.Gameplay;
using NUnit.Framework;
using UnityEngine;

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
                new List<TowerMergeDefinition>());
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
        }
    }
}
