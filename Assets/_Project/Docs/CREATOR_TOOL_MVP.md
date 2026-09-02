# Tower Defense Creator MVP

## Purpose

Build a playable tower defense MVP that is also the first version of a Unity Asset Store-style creation tool. The MVP must prove that a non-developer can define game content through safe data files and authored templates, while the runtime stays clean, testable, and reusable.

## Product Shape

- First deliverable: a real playable tower defense MVP.
- Long-term product: a creator engine that can be sold or reused to create many tower defense games.
- Future product: a no-Unity creator interface that writes the same validated JSON content.
- Current game use: the reusable engine powers both primary Citadel Fall Arena merge-defense and secondary Adventure Defense campaign play.

## First Playable Target

The first 10-minute success is:

1. Choose a classic tower defense template.
2. Load a JSON content pack.
3. Play one level with one path, build sockets, two towers, two enemies, three waves, win and lose conditions.
4. Validate the JSON before gameplay starts and show actionable creator-facing errors in the editor.

## Supported Templates

### Arena Merge Defense

- The player equips a fixed-size hero deck.
- Mana summons a random equipped hero into an empty authored grid socket.
- Matching heroes of the same rank merge into stronger defenders.
- Deterministic seeds and separated random streams support replay and future equivalent-board versus play.
- This is Citadel Fall's primary product mode.

### Classic Path Defense

- Enemies follow authored paths.
- Towers are placed on sockets or grid cells.
- Player loses lives or base health when enemies reach the goal.

### Fantasy Adventure Defense

- Fantasy heroes or defenders are placed on authored sockets or build positions.
- The player protects a stronghold, sanctuary, settlement, or mission objective.
- Monsters and raiders approach along authored paths or lanes.
- This should reuse the same tower, enemy, wave, and mission definitions where possible.
- This is Citadel Fall's secondary offline campaign, onboarding, and content-authoring mode.

The templates share validated content and combat abstractions, but they do not share all match rules or HUDs. Template-specific rules must remain explicit rather than hidden behind one overloaded controller.

## Content Source

JSON is the primary creator-facing format.

Runtime flow:

```text
JSON content pack -> DTO parse -> validation -> immutable runtime catalog -> gameplay systems
```

JSON must never directly instantiate arbitrary C# types, load arbitrary paths, or execute behavior. It may select from shipped allowlisted prefabs and behaviors by stable ID.

## Initial JSON Files

- `game_settings.json`
- `towers.json`
- `enemies.json`
- `levels.json`
- `waves.json`
- `upgrades.json`

The first implementation may use one combined `starter_content.json` while the schema stabilizes.

## Architecture Rules

- SOLID architecture.
- One primary class per file.
- No nested domain classes.
- Private fields use `_camelCase`.
- Public members use PascalCase only when needed.
- Domain logic does not depend on Unity scene objects.
- Runtime GameObjects and UI are authored as scenes or prefabs.
- Scene objects and UI must be wired in authored scenes or prefabs, not created by setup scripts.
- Editor automation must not regenerate scene hierarchies unless the owner explicitly asks for a disposable prototype scene.
- Runtime systems consume validated data and serialized scene/prefab references.

## MVP Systems

- Content DTOs and parser.
- Content validator with actionable errors.
- Runtime catalog builder.
- Damage formula service.
- Tower targeting interface.
- Enemy wave definition model.
- Level definition model.
- Local JSON loading from bundled files.
- Basic classic TD simulation.
- Fantasy hero-defense compatibility in the generic tower/enemy data shape.
- Minimal multiplayer-ready match metadata: seed, content version, mode, player slots.

## Multiplayer MVP Direction

Multiplayer is required, but the first implementation should avoid high-frequency state sync.

Initial target:

- Versus defense.
- Same content version.
- Same deterministic seed.
- Low-frequency player commands.
- Match metadata prepared for Firebase later.

Do not block the local playable MVP on Firebase.

## Platform Direction

- Target mobile, PC, and WebGL.
- Firebase integrations should use REST-friendly abstractions so Authentication and Realtime Database can run through Unity-supported web requests across all target platforms.
- Gameplay input must use Unity's new Input System, not the old Input Manager.

## Placeholder Assets

Placeholder art is acceptable.

Needed early:

- Basic path tile.
- Build socket marker.
- Mage defender.
- Warrior defender.
- Paladin defender.
- Archer defender.
- Druid defender.
- Sorcerer defender.
- Rat enemy.
- Wolf enemy.
- Quick goblin enemy.
- Strong orc enemy.
- Projectile.
- Hit effect.
- Fantasy stronghold, sanctuary, or objective core.
- Minimal HUD icons.

## Acceptance Criteria For First Foundation

- Project has `_Project` folder structure.
- Assembly definitions separate runtime and tests.
- JSON starter content exists.
- DTOs parse starter content.
- Validator catches missing IDs, invalid numbers, and missing references.
- Domain runtime catalog can be built from validated content.
- EditMode tests cover valid content and common invalid content.
- Project compiles without errors.
