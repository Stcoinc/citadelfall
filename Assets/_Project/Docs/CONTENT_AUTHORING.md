# Citadel Fall Content Authoring

## Selected hero range preview

The `Range` value authored on each hero definition drives the visible coverage ring when that placed hero is selected. Adventure/Endless upgrades use their effective upgraded range. The presentation itself is scene-authored under every socket as `Unit Range Indicator`; repair it with `Tools > Citadel Fall > Authoring > Rebuild Unit Range Indicators`. See `UNIT_RANGE_INDICATORS.md` for hierarchy and lifecycle details.

## Socket base size versus hero size

Build-socket and Arena-platform transforms may be scaled in their scene Inspector to make the visible placement bases smaller or larger. Spawned heroes compensate for the parent socket's world scale, so changing the socket scale no longer changes the hero's displayed world size. Adventure/Endless heroes retain their `0.78` world scale; Arena heroes retain the configured rank presentation beginning at `0.86`. The selected-unit range indicator uses the same parent-scale compensation. This is implemented by `TransformWorldScaleUtility` and requires no runtime-created helper objects.

## Authoring Boundary

- Unity scenes own battlefield composition, path transforms, build-socket GameObjects, colliders, backgrounds, cameras, and presentation.
- ScriptableObjects provide editable local defaults for towers, enemies, waves, and levels.
- `starter_content.json` remains the validated runtime format and the payload published to Firebase Realtime Database.
- Firebase values are loaded before a mission and never mutate an active mission snapshot.

## Gameplay Modes

The content pipeline serves three deliberate modes:

| Mode | Scene geometry | Match rules |
| --- | --- | --- |
| **Citadel Fall Arena** (primary) | `CitadelFallArena.unity` owns the 3 x 5 formation, pressure path, stronghold, camera, and Arena HUD | `ArenaRules` controls deck size, mana, summon-cost growth, merge-rank limit, lives, boss cadence, duration, and rewards |
| **Adventure Defense** (secondary) | `Gameplay.unity` owns its authored enemy path, build sockets, stronghold, camera, and Adventure HUD | Level, wave, hero/tower, upgrade, merge, sell, Scrap, and reward data comes from the validated catalog |
| **Endless Defense** (score attack) | `Endless.unity` reuses an authored Adventure battlefield and sockets with its own score/round HUD | The chosen level supplies base enemies and wave templates; five waves form a round and tested Endless rules scale pressure indefinitely |

These modes share hero and enemy definitions where their mechanics allow it, but their geometry and UI are not interchangeable. Do not copy Arena's random-summon rules into Adventure or Adventure's direct build bar into Arena unless the product scope is explicitly changed.

Arena enemy pressure is also mode-specific. Configure `EnemySpeedMultiplier`, `EnemyHealthMultiplier`, `EnemyHealthPerWave`, `BossHealthMultiplier`, spawn intervals, and Wolf/Goblin/Orc introduction waves in `ArenaRules`. See `BALANCE.md` for the current baseline. Do not edit shared enemy movement speed merely to make Arena easier.

## Authoring Assets

Assets are stored under `Assets/_Project/Data/Authoring`:

- `Towers`: defender/tower statistics and progression.
- `Enemies`: health, movement, damage, reward, tag, weakness, and prefab identity.
- `WaveSets`: ordered waves and spawn groups referencing enemy assets.
- `Levels`: mission balance, wave-set reference, and rewards.
- `Battlefields`: per-level Adventure layouts containing stable background IDs plus validated path and socket positions; they also support copying positions to or from the open Gameplay scene.

Create new assets from `Assets > Create > Tower Defense > Content`.

### Editing Hero Names After Merging

Select a hero in `Data/Authoring/Towers`. Its custom Inspector contains **Player-Facing Level Names**, with a clearly labeled field for every level. Level 1 is normally the base class name; later entries may be simple numbered names such as `Sorcerer 2` or unique titles such as `Elder Sorcerer`. These values appear in the details panel and merge confirmation, are exported as `LevelDisplayNames`, and may be overridden through the same validated Firebase content version used for other hero statistics.

Use **Create Level Name Fields** when changing `Max Level`, or **Fill Missing Names From Base Name** to generate safe numbered defaults. Export authoring assets after editing names.

## Editing A Battlefield Scene

### Adventure Defense

Every campaign level now references its own `BattlefieldLayoutAsset` through the level asset's **Battlefield Layout** field. The ten assigned assets are `battlefield_level_001` through `battlefield_level_010` under `Data/Authoring/Battlefields`. Each battlefield stores a stable background ID, enemy path coordinates, and build-socket coordinates. `starter_content.json` schema 2 exports these as `Battlefields`, while each `Level` stores its `BattlefieldId`.

The Gameplay scene authors the maximum reusable pool of path-point and socket GameObjects. Loading a campaign level repositions and activates only the configured objects; runtime code never creates the hierarchy. Endless keeps its separate scene-authored geometry and does not apply Adventure battlefield layouts.

To edit one level safely:

1. Open `Gameplay.unity` and ensure `MVP Gameplay Controller > Path Points` and **Tower Sockets** include every reusable scene object.
2. Select the target `battlefield_level_###` asset and click **Apply To Active Gameplay Scene**. This applies its saved path points, sockets, and configured background binding together so the geometry can be aligned against the correct artwork.
3. Move the existing path points and build sockets in the Scene view. Do not create runtime-only helpers.
4. Set the battlefield asset's **Background Id** to one of the serialized IDs under `MVP Gameplay Controller > Background Bindings`.
5. Click **Capture From Active Gameplay Scene** on that same battlefield asset.
6. Select the matching `level_classic_###` asset and set **Build Socket Count** to the number of leading captured socket positions that should be active.
7. Save the scene and run `Tower Defense > Content > Export All Authoring To Starter JSON`.

Adventure Play Mode does not read the authoring ScriptableObjects directly. `GameplaySceneController` starts the mission from `AppRuntimeSession.ActiveContentJson`, which was loaded during the Intro flow from the exported local JSON or an accepted Firebase override. After capturing geometry or changing **Build Socket Count**, export before testing, leave Play Mode, and restart from Intro so the session reloads the new content. If Firebase supplies an older active version, it continues to override the local JSON until that remote version is updated or the game falls back to local content.

To replace only a level background while preserving its saved path and sockets, open `Gameplay.unity`, select `MVP Gameplay Controller`, find the battlefield's existing ID under **Background Bindings**, and replace only that binding's **Sprite** reference. Then select the battlefield asset and click **Apply To Active Gameplay Scene** to preview the new background together with its saved geometry. Do not click **Capture From Active Gameplay Scene** unless the path or sockets were intentionally moved. For example, Level 2 uses `battlefield_level_002 > Background Id = background_forest_morning`; changing the `background_forest_morning` sprite and applying that battlefield preserves its stored coordinates.

Capture reads the controller arrays, not arbitrary hierarchy children. A duplicated socket must be added to **Tower Sockets** before capture or it will not be saved or initialized.

1. Open `Assets/_Project/Scenes/Gameplay/Gameplay.unity`.
2. Move the existing `Enemy Path/Path Point` transforms in route order.
3. Duplicate or move `Build Sockets/Build Socket` objects. Keep `SpriteRenderer`, `CircleCollider2D`, and `TowerPlacementSocket`.
4. Maintain the `_pathPoints` and `_towerSockets` arrays on `MVP Gameplay Controller`.

The Adventure HUD uses three mutually exclusive interaction states. Normal play shows the resource bar, guidance banner, direct-build hero tray, and action buttons. Tapping an occupied socket opens the compact right-side details drawer; the press that first places a hero must not open it. Mission completion hides the complete gameplay HUD before showing the separate full-screen victory modal. Keep the victory title, battle record, rewards, treasure, unlock notice, and Continue button inside `Victory Result Card`; do not place new result elements directly over the battlefield.
5. Optionally select a `BattlefieldLayoutAsset` and use **Capture From Active Gameplay Scene** as a reusable position template.

The scene arrays remain the capacity and object-authoring authority. A validated schema 2 battlefield may reposition or deactivate those existing Adventure objects before mission initialization, but cannot create new scene objects, exceed the arrays, or alter an active mission.

### Citadel Fall Arena

1. Open `Assets/_Project/Scenes/Gameplay/CitadelFallArena.unity`.
2. Keep the fifteen `ArenaHeroSocket` objects as the scene-authored 3 x 5 formation unless an approved layout change updates both the scene and validated Arena rules.
3. Edit the Arena enemy path and stronghold endpoint in the scene, preserving controller references and directional enemy movement.
4. Configure numeric Arena balance through the `ArenaRules` authoring/runtime content rather than hardcoding it into the scene.
5. Validate deterministic summon, socket, and enemy streams after changing rules or formation behavior.

## Editing Enemies, Waves, And Levels

Open `Tools > Citadel Fall > Content Dashboard` for one place to create or locate Enemy, Wave Set, Level, Hero, and Battlefield assets and to run the import/export pipeline.

### Drag-to-Merge Feedback

Players merge heroes by dragging an occupied socket onto another occupied socket containing the same hero at the same rank or level. Arena routes pointer input through `CitadelFallArenaController`; Adventure and Endless route it through `MvpPointerPlacementInput`. The dragged hero follows mouse or touch input after an 18-pixel threshold. A valid merge plays a gold burst, scale punch, and merge chime. An invalid drop plays an error sound and eases the hero back to its original socket without opening it as a new placement.

For accessibility, drag-and-drop is not the only merge path. Arena retains tap-one-then-tap-matching-hero merging, while Adventure and Endless retain the details-panel merge action.

`MergeInteractionFeedback` and its required 2D `AudioSource` are authored components, never added during `Awake`. Adventure/Endless scenes serialize both components on `MVP Pointer Placement Input`; Arena serializes them on `Citadel Fall Arena Controller`. The input/controller **Merge Feedback** field and feedback **Audio Source** field must both be assigned in the Inspector. Run `Tools > Citadel Fall > Authoring > Repair Merge Feedback Components` to repair and save all five supported gameplay scenes. Generated fallback sounds remain available when optional **Merge Clip** and **Invalid Clip** assets are empty. No external package or cloud service is required.

1. Edit enemy assets in `Data/Authoring/Enemies`.
2. Edit wave assets in `Data/Authoring/WaveSets`; each spawn references an enemy asset.
3. Edit level assets in `Data/Authoring/Levels`; each level references a wave-set asset.
4. Run `Tower Defense > Content > Export All Authoring To Starter JSON`.

Endless currently uses `level_classic_001` as its base battlefield and cycles that level's configured wave templates. Edit those Enemy, Wave Set, and Level assets normally; per-round scaling is centralized in `EndlessModeRules` and should be changed with tests rather than scene edits.

The exporter replaces the tower, enemy, wave-set, and level arrays, preserves/exports supported Arena configuration, validates the complete catalog, and writes only when validation succeeds. Current authoring contains 15 tower/hero definitions, 7 enemies, 5 wave sets, 10 levels, 1 battlefield template, and `arena_standard` rules.

## Firebase Publishing

Upload the exported `Assets/_Project/Data/Json/Defaults/starter_content.json` to:

`/gameSettings/development/versions/{version}/starterContent`

Then set `/gameSettings/development/activeVersion` to that version. The Intro scene uses the remote content when available and the exported local JSON as the offline fallback.

Use `Tower Defense > Content > Import Starter JSON To Authoring Assets` only when intentionally rebuilding the ScriptableObjects from JSON.

Schema 3 Firebase content can configure each level's `BattlefieldId`, the referenced battlefield's path/socket coordinates and `BackgroundId`, its `WaveSetId`, and each wave set's `HasBoss` declaration. Coordinates are validated and capped before entering the runtime catalog. Background IDs may select only sprites already serialized in the shipped Gameplay scene; JSON alone cannot download a new Unity Sprite.

Boss placement is not calculated from a level number. Open the level's assigned `WaveSetDefinitionAsset` under `Data/Authoring/WaveSets` and enable **Has Boss**. Then add exactly one enemy whose `Enemy Tag` begins with `boss` as the final spawn group of the final wave, with **Count** set to 1. Validation rejects a checked wave set with no final boss and an unchecked wave set that contains a boss. Any future level can become a boss level by referencing a correctly configured boss wave set. In the current campaign, Map 1 Level 5 uses `waves_classic_005` with the Runestone Troll and Map 2 Level 5/global Level 10 uses `waves_classic_010` with the Thorn Warden.
