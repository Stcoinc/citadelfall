# Citadel Fall Unit Shop Audit

Audit date: 2026-09-01

## Verdict

The existing purchase transaction is implemented, but the current panel is **not yet a functional Citadel Fall Unit Shop**. It is a three-item legacy turret shop embedded in `LevelSelection.unity`. Its three purchases do not unlock anything shown in the active Adventure or Endless hero bars, and all six active heroes currently have `UnlockCostCoins = 0`, which makes them automatically available without a shop purchase.

## Where It Currently Lives

- Scene and UI: `Assets/_Project/Scenes/App/LevelSelection.unity`
- Controller: `Assets/_Project/Scripts/Features/Gameplay/LevelSelectionSceneController.cs`
- Local ownership and wallet: `SaveSlotData` through `LocalSaveSlotRepository`
- Account ownership and cloud persistence: `PlayerProgression.UnlockedTowerIds` through `AppRuntimeSession.SavePlayerProgressionAsync`
- Unit/tower definitions: `Assets/_Project/Data/Authoring/Towers`
- Runtime content: `Assets/_Project/Data/Json/Defaults/starter_content.json`

The Level Selection scene still serializes `TOWER SHOP`, `Tower Shop Panel`, and `Tower Shop Button`. `LevelSelectionSceneController.ShowTurretShop` opens the panel after a save profile has been selected.

## What Works in Code

1. The open and close buttons are serialized to `ShowTurretShop` and `HideTurretShop`.
2. Purchase buttons resolve their configured content IDs.
3. A purchase rejects missing content, already-owned content, and insufficient Coins.
4. A valid purchase subtracts Coins and immediately saves the selected local profile.
5. The purchased ID is added to both save-slot ownership and authenticated account progression.
6. Authenticated progression is queued to Firebase through the existing progression service.
7. Owned and price text refresh after a successful purchase.

## What Does Not Match the Current Game

- The configured products are `tower_sniper_basic`, `tower_poison_basic`, and `tower_artillery_basic`.
- Their player-facing names are Sniper Turret, Poison Turret, and Artillery Turret.
- Active Adventure and Endless bars use `hero_mage`, `hero_warrior`, `hero_paladin`, `hero_archer`, `hero_druid`, and `hero_sorcerer` instead.
- All six hero definitions currently have a zero Coin unlock cost, so the gameplay unlock rule treats every hero as already unlocked.
- The shop has only three text labels and three hard-coded purchase callbacks.
- It has no unit portraits, rarity/class presentation, stat preview, wallet header, filtering, scrolling, selected-unit details, or disabled/owned visual states.
- Shop purchase behavior has no dedicated automated transaction test; current validation covers compilation and neighboring save/navigation systems.

## Required Foundation Before the UI Pass

1. Define the starter-owned heroes and assign non-zero Coin costs to later unlockable units.
2. Replace the fixed three-ID array and three purchase methods with a data-driven unit catalog.
3. Rename player-facing `TOWER SHOP` text to `UNIT SHOP`; internal `TowerDefinition` terminology may remain for save and content compatibility.
4. Decide whether Unit Shop remains a Level Selection modal or becomes a Main Menu destination.
5. Create reusable unit-card UI showing portrait, name, class/damage identity, important stats, price, owned/equipped state, and purchase affordance.
6. Add purchase-service tests independent of scene UI, including offline save, authenticated save queue, insufficient funds, and duplicate purchase.

## Validation and Tooling

- Source, scene YAML, ScriptableObjects, JSON content, local save code, and Firebase progression call sites were inspected locally.
- Compilation command: `dotnet build "Tower defense engine.sln" --no-restore`
- Result: build succeeded with 0 warnings and 0 errors.
- Unity was inspected through Windows app control, but a full Play Mode transaction was not performed because user input was detected in the Unity window and automation stopped rather than interfering.
- No Firebase request, Google Cloud command, deployment, third-party library, or external cloud write was performed during this audit.

