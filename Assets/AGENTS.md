# Fantasy Adventure Tower Defense — AGENTS.md

> **Repository authority:** This file is the primary implementation contract for every human developer and AI coding agent working on the fantasy adventure tower-defense project.
>
> **Rule of precedence:** When generated code, comments, tickets, prompts, or assumptions conflict with this document, this document wins unless the project owner explicitly updates it.
>
> **Project owner:** ClubGamerZone
>
> **Final title:** Citadel Fall (owner approved 2026-08-31)
>
> **Engine:** Unity 6 LTS-compatible project
>
> **Primary platforms:** Android and iOS
>
> **Primary orientation:** Landscape
>
> **Core modes:** Citadel Fall Arena (primary), Adventure Defense campaign, Endless Defense score attack, and future online versus defense

---

## 0. Owner-Approved Fantasy Adventure Pivot — 2026-08-03

This section supersedes every conflicting fleet, spaceship, flagship, commander, module, and sci-fi theme requirement elsewhere in this document. Those older passages are retained temporarily as architectural and historical reference only; they are not the current product direction.

The game is a fantasy adventure tower-defense game. The player recruits and places fantasy heroes or defenders on authored build locations to protect a fantasy stronghold, sanctuary, settlement, or other mission objective from monsters and raiders.

### 0.1 Initial Defender Roster

The initial six defender archetypes are:

1. Mage — accessible ranged magic damage.
2. Warrior — durable close-range physical damage.
3. Paladin — defensive protector with support potential.
4. Archer — long-range physical damage and precise targeting.
5. Druid — nature magic, control, damage-over-time, or support potential.
6. Sorcerer — powerful burst or area magic with a higher-risk identity.

These are presented to players as characters, heroes, or defenders rather than mechanical gun turrets. The reusable engine may continue to use generic `Tower` terminology internally until a deliberate, tested migration is requested.

### 0.2 Initial Enemy Roster

The first enemy set begins with:

1. Rat — weak swarm enemy.
2. Wolf — mobile beast enemy.
3. Goblin — especially quick raider.
4. Orc — slow, strong, high-health bruiser.

Additional fantasy creatures may be added through the existing data-driven enemy, wave, prefab, animation, and balance systems.

### 0.3 Thematic Translation Rules

- Flagship, ship, or command bridge requirements translate to the fantasy objective, stronghold, sanctuary, or base core.
- Weapon modules translate to placed hero/defender archetypes and their abilities.
- Commander progression translates to party leader, champion, patron, or hero progression when that system is revisited.
- Enemy fleets and spacecraft translate to monsters, beasts, raiders, and fantasy bosses.
- Hull or ship health translates to objective/base health.
- Sci-fi visual direction is obsolete. New player-facing art, names, text, audio, VFX, and content must use a cohesive fantasy adventure direction.
- Existing sci-fi identifiers, JSON keys, class names, prefabs, and art are transitional technical debt. Do not perform a broad breaking rename without a scoped migration plan and tests.
- Offline-first behavior, data-driven content, authored scene/prefab rules, mobile readability, deterministic systems, validation, and security requirements remain authoritative.

### 0.4 Current Product Boundary

This pivot changes theme, roster, presentation, and future content direction. It does not by itself authorize unrelated architectural rewrites, a final game name, a new networking provider, or untested save/schema breakage.

### 0.5 Owner-Approved Arena-First Final Scope — 2026-08-05

The final product direction is an original Citadel Fall fantasy merge-defense game inspired by the understandable match structure of Rush Royale. Citadel Fall Arena is the primary product, not a secondary bonus mode. Older campaign and classic-path requirements remain useful as offline training, onboarding, content authoring, and optional adventure progression, but they must not delay the primary Arena loop.

The primary playable loop is:

1. Build and equip a deck of five unlocked fantasy heroes.
2. Enter a compact authored formation with fifteen build sockets.
3. Spend mana to summon a random equipped hero into an empty socket.
4. Merge matching heroes of the same rank into stronger defenders.
5. Defeat escalating Rat, Wolf, Goblin, Orc, elite, and boss waves before they breach the stronghold.
6. Use hero identities, abilities, deck synergy, mana timing, and pressure actions to outplay an equivalent opponent.
7. Finish a short match, receive fair progression rewards, refine the deck, and play again.

Product priorities, in order:

1. A polished, deterministic offline Arena match that is fun and fully reproducible from its seed.
2. Clear collection, deck-building, hero progression, enemy collection, onboarding, and match rewards.
3. A local two-board versus simulation with ordered low-frequency commands and replay verification.
4. Online two-player versus after the owner explicitly approves a networking provider.
5. Cooperative and event variants only after the primary versus loop is stable.

Originality and fairness rules:

- Do not copy Rush Royale names, characters, art, sounds, layouts, text, exact balance, progression tables, economy, or branded presentation.
- Use the game's own world, six starting hero classes, fantasy enemies, strongholds, visual language, and terminology.
- Preserve the offline-first contract and local fallback content.
- Competitive power must not be sold directly. Avoid excessive currencies, random paid items, manipulative timers, and menu clutter.
- The reusable classic tower-defense engine remains supported, but player-facing active scenes must use fantasy heroes and creatures rather than ships or mechanical turrets.

### 0.6 Why Citadel Fall Has Three Gameplay Styles

The three gameplay styles are deliberate product layers, not competing prototypes:

1. **Citadel Fall Arena — primary product.** A compact 3 x 5 merge-defense match built around a five-hero deck, random mana summons, same-rank merges, deterministic enemy pressure, bosses, short sessions, and future equivalent-board versus play.
2. **Adventure Defense — secondary mode.** A classic authored-path tower-defense experience where the player deliberately chooses heroes, places them on mission sockets, upgrades or sells them, and advances through campaign maps. It serves offline progression, onboarding, longer tactical missions, content testing, and players who prefer direct placement over random summons.

3. **Endless Defense — score-attack mode.** A direct-placement survival run on authored sockets. The player may place, upgrade, sell, and drag matching same-level heroes together at any time. Waves start manually, five waves form one round, pressure scales forever, and personal best score/round persist for a future leaderboard.

All modes share the same fantasy world, heroes, enemies, account, currencies, unlocks, content catalog, Firebase-compatible balance pipeline, prefabs, combat services, and application shell. They intentionally do not share every match rule or HUD. Arena uses mana, random deck summons, fifteen sockets, and merge ranks; Adventure Defense uses Scrap, direct hero selection, authored paths, mission rewards, upgrades, selling, and a smaller scene-defined socket layout; Endless reuses direct placement but replaces finite mission victory with five-wave rounds, escalating pressure, manual wave starts, and persistent score.

This separation exists because the project began as a reusable classic tower-defense engine before the owner approved an Arena-first final game. Reusing the completed campaign foundation adds offline value and a safe training/content-authoring surface without forcing Arena and Adventure into one confusing rule set. New feature priority remains Arena. Adventure work must reuse shared systems and must not delay Arena combat, collection/decks, deterministic versus simulation, or approved online play.

Player-facing naming rules:

- Use **Citadel Fall Arena** or **Arena** for the primary merge-defense mode.
- Use **Adventure Defense** or **Adventure** for the authored-path campaign mode.
- Do not label Adventure as the main game, `MVP`, `FullGame`, or `legacy campaign` in player-facing text.
- `Tower`, `MvpGameplayController`, and other older identifiers may remain internal compatibility names until a scoped migration is tested.

---

## 1. How AI Agents Must Use This File

Every agent must read this file before editing the repository.

After every implementation task, update `Assets/_Project/Docs/AGENT_MEMORY.md` and any relevant feature documentation. Record where the feature was added, how to find or author it in Unity, which libraries or CLI commands were used, and how Firebase or any other cloud service was accessed. Explicitly record when no external library or cloud change was required.

Before implementing a feature, the agent must:

1. Identify the requested feature and the systems it touches.
2. Inspect existing code, prefabs, ScriptableObjects, scenes, tests, and documentation.
3. Reuse existing abstractions when they are suitable.
4. Create a small implementation plan.
5. Make the smallest coherent change that fully satisfies the request.
6. Add or update tests.
7. Validate compilation, scenes, prefabs, serialization, and documentation.
8. Report changed files, important decisions, known limitations, and verification performed.

Agents must not redesign unrelated systems while implementing a focused task.

Agents must not claim that code compiles, tests pass, or a build succeeds unless they actually ran the relevant validation.

Agents must not silently replace a requested architecture with a preferred alternative. They may explain risks, but must follow this specification unless the owner authorizes a change.

---

# PART I — PRODUCT DEFINITION

## 2. Game Vision

Citadel Fall is a fast, replayable fantasy merge-defense game centered on Citadel Fall Arena, with a secondary authored-path Adventure Defense campaign. The player assembles a party of fantasy heroes, develops their collection, and protects the realm's strongholds from creatures and raiders.

The core fantasy is:

> Gather mages, warriors, paladins, archers, druids, sorcerers, and other heroes; develop their abilities; and defend the realm from increasingly dangerous creatures and raiders.

The game must be easy to understand in the first minute and deep enough to support long-term deck, collection, and build experimentation.

Arena targets short three-to-five-minute matches; Adventure Defense may use longer eight-to-twelve-minute missions; Endless Defense is open-ended. All contribute to long-term deck, hero, collection, and account progression.

The game must support complete offline single-player play. Internet features enhance the game but must not prevent the player from accessing owned offline content.

## 3. Product Pillars

### 3.1 Build Your Adventuring Defense

Fantasy defenders are the player's towers. Their class identity, placement, upgrades, abilities, and visual progression must be readable and meaningful.

### 3.2 Fast Tactical Decisions

Defenders attack automatically, but the player makes frequent meaningful decisions involving party composition, placement, upgrades, targeting priorities, abilities, resource allocation, and roguelite rewards.

### 3.3 Short Runs, Long Progression

Individual missions are short. Unlockable defenders, class paths, campaign regions, cosmetics, and mastery provide long-term goals.

### 3.4 Offline First

The campaign, tutorials, practice mode, unlocked defenders, and locally available progression must remain playable without connectivity.

### 3.5 Fair Multiplayer

Multiplayer must reward strategy and mastery. Competitive power must not be sold directly. Paid content should focus primarily on cosmetics and clearly separated expansion content.

### 3.6 Live Balance Without Mandatory APK Updates

Most numeric balance values and selected feature flags must be remotely configurable. The project will primarily use Firebase Authentication REST APIs and Firebase Realtime Database REST APIs for account and data services.

A versioned Realtime Database node named `gameSettings` will provide validated cloud overrides for values such as objective health, defender damage, wave timing, enemy health, rewards, and feature availability.

The game must always include safe local defaults so it can start and play offline even when Firebase is unavailable.

---

## 4. Target Audience

Primary audience:

- Mobile players who enjoy fantasy adventures, hero-based tower defense, roguelite choices, strategy, and short sessions.
- Players who want meaningful progression without requiring continuous online access.
- Players who enjoy asynchronous or lightweight competitive modes.

Secondary audience:

- Traditional tower-defense players.
- Fantasy RPG and adventure fans.
- Players who enjoy optimizing parties, class combinations, and defensive layouts.

The interface must remain readable on common phone screens and must not depend on tiny desktop-style controls.

---

## 5. Initial Scope and Scope Protection

The project must be built in releases. AI agents must protect the current milestone from uncontrolled scope growth.

### 5.1 MVP

The first playable MVP includes:

- One fantasy stronghold or mission objective presentation.
- Six initial defender classes: Mage, Warrior, Paladin, Archer, Druid, and Sorcerer.
- One adventure region containing five missions.
- One tutorial mission.
- Six defender types.
- At least four regular enemy types beginning with Rat, Wolf, quick Goblin, and strong Orc.
- One elite modifier system.
- Two bosses.
- Local save system.
- Offline campaign.
- Versioned remote game settings through Firebase Realtime Database REST.
- Firebase email/password or anonymous authentication through REST.
- Basic multiplayer lobby data model.
- One multiplayer mode: two-player versus defense.
- No mandatory advertising system in the architecture.

### 5.2 Vertical Slice

The vertical slice adds:

- Polished art and audio for one complete world.
- Ten missions.
- Ten tower or support types.
- Three commanders.
- Daily challenge seed.
- Match reconnect support.
- Analytics events.
- Production-ready Firebase security rules.
- Balance dashboard workflow or documented JSON publishing workflow.

### 5.3 Features Explicitly Deferred

Unless requested by the owner, do not implement these during the MVP:

- Guilds.
- Four-player real-time co-op.
- Open world navigation.
- Fully freeform ship construction.
- User-generated content.
- Global chat.
- Trading.
- Blockchain or NFT systems.
- Complex server-authoritative physics.
- Runtime asset generation.

---

# PART II — GAMEPLAY DESIGN

## 6. Core Gameplay Loop

### 6.1 Citadel Fall Arena — Primary Loop

1. Equip five unlocked heroes.
2. Start a deterministic match on the authored 3 x 5 formation.
3. Spend mana to summon a random equipped hero into an empty socket.
4. Merge matching heroes of the same rank.
5. Defeat escalating fantasy waves before enemies breach the stronghold.
6. Prepare for elite and boss pressure using deck synergy, mana timing, abilities, and future pressure actions.
7. Finish the short match, receive progression rewards, refine the deck, and play again.

### 6.2 Adventure Defense — Secondary Loop

1. Choose a campaign mission and save profile.
2. Enter its authored path-and-socket battlefield.
3. Select a specific available hero and spend Scrap to place it.
4. Upgrade, merge, target, or sell placed heroes as the mission permits.
5. Defeat configured waves before they exhaust stronghold health.
6. Receive mission rewards, unlock progression, and return to the adventure map.

### 6.3 Endless Defense — Score-Attack Loop

1. Choose a save profile and enter the authored Endless battlefield.
2. Directly place heroes on available sockets at any time.
3. Manually start the next wave when ready; five waves complete one round.
4. Upgrade, sell, or drag a hero onto an identical same-level hero to merge at any time.
5. Survive indefinitely while enemy health, damage, count, speed, and cadence scale through tested rules.
6. Persist best score and highest round for profile display and a future validated leaderboard.

## 7. Session Targets

- Arena tutorial: 3–5 minutes.
- Standard Arena match: 3–5 minutes.
- Adventure tutorial: 4–7 minutes.
- Standard Adventure mission: 8–12 minutes.
- Adventure boss mission: 10–15 minutes.
- Future online versus match: target 3–5 minutes, subject to playtesting.
- Endless mode: open-ended after the initial 10-minute milestone.

No standard mission should require a player to remain continuously active for more than approximately 15 minutes.

## 8. Battlefield Model

All modes use a readable 2D top-down or slightly angled presentation optimized for landscape mobile screens.

The battlefield contains:

- Fantasy stronghold or mission objective.
- Enemy approach path or pressure lane.
- Spawn gates.
- Environmental obstacles.
- Hero sockets: a 3 x 5 Arena formation or a scene-authored Adventure/Endless layout.
- Optional temporary tactical zones.

All modes must use authored placement sockets or an explicit validated grid. They must not use unconstrained physics-based construction. Owner-approved Adventure campaign battlefield configuration may reposition and activate a bounded pool of pre-authored path-point and build-socket GameObjects before mission initialization using validated versioned battlefield coordinates from local/Firebase-compatible content. It must never create the authored hierarchy, exceed the scene's serialized capacity, move cameras or UI, or mutate geometry during an active mission. Arena and Endless retain their dedicated scene-authored geometry unless the owner explicitly approves an equivalent scoped data contract.

Adventure boss levels are data-driven rather than inferred from a level number. A `WaveSetDefinitionAsset` exposes **Has Boss**; when enabled, that wave set must contain exactly one boss-tagged spawn group as the final group of the final wave, with count 1. Any current or future level may reference a boss-enabled wave set.

## 9. Ship Structure

A flagship consists of:

- Core hull.
- Command bridge.
- Module slots.
- Power capacity.
- Total hull health.
- Shield health where applicable.
- Armor or damage mitigation.
- Module inventory.
- Commander.
- Ship class modifiers.

### 9.1 Damage Layers

Recommended damage order:

1. Temporary barrier.
2. Shield.
3. Armor mitigation.
4. Hull or module health.

A configuration value must determine whether modules have independent health in a specific mode.

### 9.2 Destruction

The player loses when the command bridge or flagship core reaches zero health, unless the mission defines another failure condition.

Destroyed non-core modules may become disabled for the remainder of the wave and repairable during preparation.

## 10. Module Categories

### 10.1 Weapon Modules

- Laser Turret: rapid single-target energy damage.
- Missile Battery: slow area damage.
- Railgun: high damage with penetration.
- Flak Cannon: anti-fighter and anti-projectile specialization.
- Plasma Cannon: slow heavy energy damage.
- Arc Emitter: chains damage between nearby targets.

### 10.2 Defensive Modules

- Shield Generator.
- Point Defense.
- Armor Plating.
- Decoy Projector.
- Emergency Barrier.

### 10.3 Support Modules

- Reactor.
- Repair Drone Bay.
- Targeting Computer.
- Gravity Well.
- Resource Collector.
- Fighter Hangar.

### 10.4 Structural Modules

- Hull Expansion.
- Wing Section.
- Utility Frame.
- Reinforced Bulkhead.

## 11. Module Upgrade Model

Each module has:

- Stable string identifier.
- Display name localization key.
- Optional player-facing name for each configured level or merge rank; missing entries fall back to the base display name plus the level number.
- Description localization key.
- Category.
- Base stats.
- Maximum in-run level.
- Optional permanent mastery.
- Upgrade cost curve.
- Visual prefab.
- Icon.
- Audio references.
- Targeting behavior.
- Power requirement.
- Tags.

In-run upgrades should be understandable and visually meaningful.

Example Laser Turret path:

- Level 1: baseline.
- Level 2: increased damage.
- Level 3 choice A: rapid-fire laser.
- Level 3 choice B: armor-piercing beam.
- Level 4: strengthens the selected branch.

Branch choices must be data-driven.

## 12. Resources

### 12.1 Scrap

Primary in-mission construction and upgrade currency.

### 12.2 Energy Capacity

A ship-build constraint, not necessarily a spendable currency. Reactors increase capacity. Active modules consume capacity.

### 12.3 Data

Long-term research currency earned from campaign milestones and challenges.

### 12.4 Alloy

Rare progression resource for ship class upgrades or special unlocks.

The MVP should avoid excessive currencies. Scrap and one permanent progression currency are sufficient initially.

## 13. Wave System

Each wave is represented by data and contains:

- Wave identifier.
- Delay before start.
- Spawn groups.
- Spawn path.
- Spawn timing.
- Enemy types.
- Enemy levels or stat multipliers.
- Elite modifiers.
- Completion conditions.
- Rewards.
- Optional dialogue or event triggers.

Agents must not hardcode campaign waves inside MonoBehaviours.

## 14. Enemy Archetypes

MVP enemy set:

1. Scout Fighter — fast, weak, high count.
2. Armored Fighter — slower and resistant to basic attacks.
3. Bomber — prioritizes ship modules.
4. Frigate — durable medium ship.
5. Missile Boat — attacks from range.
6. Support Drone — shields or repairs nearby enemies.

Future enemies:

- Stealth ship.
- Carrier.
- Teleporter.
- Shield breaker.
- Swarm splitter.
- Boarding vessel.

## 15. Elite Modifiers

Elite modifiers are reusable data assets. Examples:

- Reinforced: more hull health.
- Shielded: regenerating shield.
- Overcharged: faster attacks.
- Phase: periodic untargetable state.
- Regenerator: health regeneration.
- Volatile: area effect when destroyed.
- Commander: buffs nearby enemies.

Modifiers must be composable but limited by compatibility rules.

## 16. Boss Design

Bosses must alter normal decision-making rather than only having more health.

Boss phases may include:

- Changing approach lane.
- Temporarily disabling module categories.
- Launching fighter waves.
- Targeting exposed ship sections.
- Generating shields that require target priority changes.
- Telegraphing a powerful attack requiring a commander ability or defensive response.

Every boss must have readable telegraphs and avoid unavoidable damage.

## 17. Commanders

A commander defines:

- Passive ability.
- Active ability.
- Ultimate ability where applicable.
- Starting bonus.
- Preferred build archetype.
- Portrait and presentation assets.
- Unlock conditions.

MVP commander:

**Commander Nova**

- Passive: repair effects are stronger.
- Active: emergency repair pulse.
- Tactical identity: forgiving introductory commander.

## 18. Roguelite Run Upgrades

After selected waves, present three upgrade choices.

Upgrade categories:

- Weapon family bonuses.
- Defensive bonuses.
- Economy bonuses.
- Commander bonuses.
- Build-defining rare upgrades.

Rules:

- Choices must be deterministic from the run seed.
- Duplicate stacking limits are data-driven.
- Incompatible upgrades cannot appear together when prohibited.
- The system must support weighted rarity.
- Cloud settings may modify weights within safe bounds.

## 19. Targeting

Every weapon must use a targeting strategy interface.

Supported strategies:

- First along path.
- Last along path.
- Lowest health.
- Highest health.
- Closest.
- Fastest.
- Boss priority.
- Air or fighter priority.
- Manual marked target.

The player may change targeting priority for selected advanced modules if the UX supports it.

## 20. Combat Formula Guidance

All formulas must be centralized and testable.

Recommended conceptual formula:

`finalDamage = max(minimumDamage, baseDamage × additiveModifiers × multiplicativeModifiers × resistanceMultiplier × criticalMultiplier)`

Requirements:

- Order of operations must be documented in code.
- Damage types must use stable identifiers or enums.
- Floating-point edge cases must be covered by tests.
- Remote settings must have minimum and maximum validation bounds.
- Multiplayer simulations must use the same configuration snapshot.

## 21. Status Effects

Initial status effects:

- Slow.
- Stun.
- Armor Break.
- Marked.
- Damage Over Time.
- Shield Disruption.

Each status effect must define stacking, duration refresh, maximum stacks, immunity rules, and VFX behavior.

---

# PART III — GAME MODES

## 22. Offline Campaign

The campaign must work without internet after required game content is installed.

Features:

- Star system map.
- Mission unlock path.
- Difficulty selection where appropriate.
- Local progression.
- Star rating or objective rating.
- Boss missions.
- Optional challenge objectives.

Offline progress is written locally first. When authenticated and connected, eligible progress is synchronized according to the save conflict policy.

## 23. Endless Mode

Endless Defense begins from a selected adventurer profile and an authored hero-socket battlefield. It uses direct placement, five-wave rounds, manual starts, configured hero upgrades/merges, and deterministic rule-based pressure scaling.

Leaderboards require server validation or plausibility checks before accepting scores. The client must never be treated as authoritative for globally ranked results.

## 24. Versus Defense

MVP multiplayer mode:

- Two players enter separate but equivalent battlefields.
- Both receive the same base wave seed.
- Players earn a secondary pressure resource by defeating enemies efficiently.
- Pressure resource can be spent on predefined enemy reinforcements or modifiers sent to the opponent.
- Sending pressure must involve an opportunity cost.
- A player loses when the flagship core is destroyed.
- If both survive the final timed wave, tiebreakers are applied.

Recommended tiebreakers:

1. Remaining flagship health percentage.
2. Remaining module value.
3. Total leaked enemy threat.
4. Completion time or score.

The result record must include the configuration version and deterministic seed.

## 25. Multiplayer Networking Strategy

Firebase Realtime Database is suitable for authentication-linked data, matchmaking metadata, invitations, room state, low-frequency commands, match snapshots, asynchronous competition, and result records.

Firebase Realtime Database must not be assumed to be a low-latency authoritative action server for high-frequency combat synchronization.

The MVP versus design should minimize network frequency by synchronizing:

- Match seed.
- Ready state.
- Loadout hash.
- Configuration version.
- Player commands that affect the opponent.
- Periodic summary snapshots.
- Heartbeats.
- Final result claims.

Each player's local combat simulation remains deterministic where practical. Opponent-impact commands are timestamped and sequence-numbered.

If the project later requires action-heavy real-time co-op, a dedicated networking solution or authoritative server layer must be evaluated rather than forcing high-frequency state through Realtime Database.

## 26. Match State Machine

States:

- Created.
- Joining.
- WaitingForPlayers.
- Synchronizing.
- Countdown.
- Running.
- Reconnecting.
- Completed.
- Cancelled.
- Expired.

Every transition must be explicit and validated.

## 27. Disconnect and Reconnect

- Client writes a heartbeat at a conservative interval.
- A player may reconnect within a configured grace period.
- The match record stores the last accepted command sequence.
- Duplicate commands are ignored.
- Missing commands can be requested or reconstructed from event history when available.
- An abandoned match resolves according to documented rules.
- Local pause is not allowed during active competitive multiplayer.

---

# PART IV — TECHNICAL ARCHITECTURE

## 28. Architectural Principles

- Feature-oriented modular architecture.
- Data-driven content.
- Dependency inversion for services.
- Minimal coupling between gameplay, presentation, persistence, and network layers.
- Offline-first behavior.
- Testable domain logic.
- Prefab-driven Unity workflows.
- Explicit state machines.
- No hidden global mutable state.
- No service credentials inside the client.

## 29. Unity Technology Direction

Preferred components:

- Unity 6 LTS-compatible release.
- Universal Render Pipeline.
- Unity Input System.
- TextMeshPro.
- Addressables only when content delivery requirements justify it.
- Unity Test Framework.
- Assembly definitions.

Third-party packages must not be added without a documented reason and owner approval.

## 30. Runtime Object Creation Rule

The project strongly prefers authored scene objects and prefabs.

Rules:

- Do not construct UI hierarchies at runtime.
- Do not procedurally create manager GameObjects at runtime.
- Do not use runtime `new GameObject()` for normal gameplay content.
- Modules, enemies, projectiles, VFX, panels, and services must originate from prefabs or authored scene objects.
- Runtime spawning from prefabs is allowed when gameplay requires it.
- Spawned high-frequency objects must use pooling.
- Scene bootstrap objects must be present in the scene or loaded from an explicitly approved bootstrap prefab.

## 31. Suggested Repository Structure

```text
Assets/
  _Project/
    Art/
      Animations/
      Materials/
      Sprites/
      UI/
      VFX/
    Audio/
      Music/
      SFX/
    Data/
      Commanders/
      Config/
      Enemies/
      Missions/
      Modules/
      Upgrades/
      Waves/
    Prefabs/
      Core/
      Enemies/
      Modules/
      Projectiles/
      Ships/
      UI/
      VFX/
    Scenes/
      Bootstrap/
      Gameplay/
      Menus/
      Tests/
    Scripts/
      Application/
      Core/
      Domain/
      Features/
        Authentication/
        Combat/
        Configuration/
        Fleet/
        Multiplayer/
        Progression/
        Save/
        Waves/
      Infrastructure/
        Firebase/
        Http/
        Persistence/
      Presentation/
        Gameplay/
        Menus/
        UI/
    Settings/
    Tests/
      EditMode/
      PlayMode/
```

## 32. Assembly Definitions

At minimum, separate:

- Project.Core
- Project.Domain
- Project.Application
- Project.Infrastructure
- Project.Presentation
- Project.Tests.EditMode
- Project.Tests.PlayMode

Domain code must not reference Unity scene objects where avoidable.

Infrastructure may depend on domain interfaces, not the reverse.

## 33. Namespace Convention

```text
ClubGamerZone.FleetCommand.Core
ClubGamerZone.FleetCommand.Domain
ClubGamerZone.FleetCommand.Application
ClubGamerZone.FleetCommand.Features.<FeatureName>
ClubGamerZone.FleetCommand.Infrastructure.Firebase
ClubGamerZone.FleetCommand.Presentation
```

## 34. C# Coding Conventions

- Private fields use `_camelCase`.
- Inspector fields use `[SerializeField] private`.
- Do not expose mutable public fields.
- Public members use PascalCase.
- Interfaces begin with `I`.
- Async methods end with `Async`.
- Boolean names communicate a condition: `IsReady`, `HasTarget`, `CanUpgrade`.
- Avoid abbreviations except established terms such as UI, ID, HP, DPS, URL.
- Prefer immutable value objects and readonly data where practical.
- Use nullable reference annotations when supported by the project configuration.
- Do not use magic numbers or magic strings.
- Use explicit access modifiers.
- One primary type per file.
- File name matches primary type.

## 35. Forbidden Patterns

Do not use:

- `GameObject.Find`.
- `FindObjectOfType` or broad scene searches as architecture.
- Repeated `GetComponent` calls in hot paths.
- Service locator access scattered throughout gameplay.
- Mutable static global managers.
- `PlayerPrefs` as the primary save system.
- Hardcoded Firebase paths throughout the codebase.
- Service account keys in Unity.
- Admin credentials in the repository.
- Direct JSON parsing inside UI components.
- Network calls directly from buttons or view scripts.
- Infinite retries.
- Empty catch blocks.
- `async void` except Unity event entry points where unavoidable and safely wrapped.
- Per-frame LINQ in gameplay hot paths.
- Per-frame allocations in combat loops.

## 36. Update Loop Policy

Avoid `Update()` unless continuous frame behavior is required.

Prefer:

- Events.
- Coroutines for simple Unity-timed presentation work.
- Central tick services for large groups of simulation entities.
- Timers.
- Animation events when appropriate.
- Explicit state transitions.

Every `Update`, `FixedUpdate`, or `LateUpdate` must have a clear reason.

## 37. Dependency Injection

Use constructor injection for pure C# classes.

For MonoBehaviours, dependencies may be assigned through a composition root, serialized references, or a project-approved dependency-injection framework.

Do not introduce a DI package solely to avoid writing a small composition root.

## 38. Event System

Use strongly typed events.

Events should describe completed facts, such as:

- `WaveStarted`.
- `EnemyDestroyed`.
- `ModulePlaced`.
- `FlagshipDamaged`.
- `ConfigurationActivated`.

Avoid string-based event buses.

Unsubscribe listeners reliably.

## 39. Object Pooling

Pool:

- Projectiles.
- Common enemies.
- Damage numbers.
- Explosion VFX.
- Repeated audio emitters when necessary.

Pools must:

- Reset object state before reuse.
- Have bounded growth policies.
- Provide diagnostics in development builds.
- Avoid silently instantiating unlimited objects.

## 40. Scene Strategy

Recommended scenes:

- `Bootstrap` — composition root, persistent approved services.
- `MainMenu`.
- `FleetHub`.
- `Gameplay`.
- `MultiplayerLobby`.
- `Loading` where required.

Do not put every feature in one scene.

Scene transitions must use a centralized scene flow service.

---

# PART V — DATA-DRIVEN CONTENT

## 41. ScriptableObjects

Use ScriptableObjects for authored local defaults and content definitions.

Appropriate uses:

- Module definitions.
- Enemy definitions.
- Wave definitions.
- Mission definitions.
- Commander definitions.
- Upgrade definitions.
- Audio catalogs.
- Local default balance configuration.

Do not use ScriptableObjects as mutable runtime save data.

Runtime state must be cloned or represented by separate runtime models.

## 42. Stable IDs

Every remotely referenced content item must have a stable lowercase identifier.

Examples:

- `ship_cruiser_mk1`
- `module_laser_basic`
- `enemy_scout_fighter`
- `commander_nova`
- `mission_system01_001`

IDs must never depend on display names, asset names, or localization text.

Once released, IDs should not be changed. Use migrations or aliases if necessary.

## 43. Configuration Layers

Effective settings use layered precedence:

1. Hard safety constraints in code.
2. Local ScriptableObject defaults shipped with the build.
3. Last-known-good cached remote configuration.
4. Newly downloaded and validated remote configuration.
5. Match-locked configuration snapshot for active multiplayer.

A remote value may override a local value only if:

- The key is allowed.
- The schema version is supported.
- The value type is correct.
- The value is within hardcoded safety bounds.
- Required references exist.
- Cross-field validation passes.
- The configuration is active for the current build and environment.

## 44. Configuration Immutability During Gameplay

Do not apply balance changes in the middle of an active mission or multiplayer match.

At mission start:

- Resolve effective configuration.
- Create an immutable configuration snapshot.
- Store the configuration version in run or match metadata.
- Use that snapshot until the run ends.

New cloud settings become active at a safe boundary such as the next mission, next menu load, or next application start.

---

# PART VI — FIREBASE ARCHITECTURE

## 45. Firebase Role

The project primarily uses Firebase services through HTTPS REST APIs to reduce reliance on large client SDK integrations and to keep service boundaries explicit.

Planned use:

- Firebase Authentication REST API for account creation, sign-in, token refresh, and anonymous authentication where enabled.
- Firebase Realtime Database REST API for player cloud data, game settings, multiplayer room metadata, low-frequency match commands, and selected progression records.
- Optional Firebase Remote Config evaluation later for feature flags or audience targeting.
- Cloud Functions or another trusted backend for privileged validation, moderation, score verification, secure administrative writes, and anti-cheat-sensitive operations.

## 46. Important Security Boundary

The mobile client is untrusted.

Never place these in the Unity project:

- Firebase service-account JSON.
- Private keys.
- Admin SDK credentials.
- Database secrets.
- Any credential capable of bypassing Security Rules.

The Firebase Web API key used by client authentication identifies the Firebase project but is not an administrator secret. Security must still be enforced by Authentication, Realtime Database Security Rules, App Check where supported, validation, and trusted backend operations.

## 47. Authentication Requirements

Create an `IAuthenticationService` interface.

Required operations:

- Register with email and password.
- Sign in with email and password.
- Sign in anonymously if enabled.
- Refresh ID token.
- Sign out locally.
- Restore a valid cached session.
- Get current user identity.
- Notify session state changes.

Authentication state model:

```text
SignedOut
Authenticating
Authenticated
Refreshing
Expired
Error
```

Store tokens using platform-appropriate secure storage where available. Do not store passwords.

The service must track:

- User ID.
- ID token.
- Refresh token.
- Token expiration time.
- Provider type.
- Whether the account is anonymous.

Refresh the token before expiration or after an authorized request returns an authentication error.

Prevent simultaneous refresh operations with a single-flight mechanism.

## 48. HTTP Client Abstraction

All Firebase REST traffic must use a reusable HTTP abstraction.

Suggested interfaces:

```csharp
public interface IHttpClient
{
    Task<HttpResult<TResponse>> SendAsync<TResponse>(
        HttpRequest request,
        CancellationToken cancellationToken);
}
```

Requirements:

- HTTPS only.
- Request timeout.
- Cancellation support.
- Typed results.
- Structured error mapping.
- Limited retry policy.
- Exponential backoff with jitter for transient failures.
- No retry for validation failures or most authentication failures.
- Sanitized development logging.
- Never log tokens, passwords, full authorization URLs, or private player data.

## 49. Realtime Database REST Conventions

Centralize all paths in repository classes or path builders.

Do not concatenate arbitrary user input into paths.

All REST database paths end in `.json`.

Use authenticated user ID tokens for protected client requests.

Use:

- `GET` for reads.
- `PUT` for full replacement of a known node.
- `PATCH` for partial updates.
- `POST` only when Firebase-generated push IDs are intentionally required.
- `DELETE` for deletion.

Use ETags and conditional requests for records that require optimistic concurrency, such as selected save metadata or room claims.

## 50. Database Root Layout

```json
{
  "gameSettings": {},
  "users": {},
  "publicProfiles": {},
  "matchmaking": {},
  "matches": {},
  "leaderboards": {},
  "serverOperations": {},
  "admin": {}
}
```

The actual database should remain shallow and query-oriented. Avoid deeply nesting unbounded collections.

## 51. Game Settings Database Node

The canonical node name is:

```text
/gameSettings
```

Recommended structure:

```json
{
  "gameSettings": {
    "production": {
      "active": {
        "schemaVersion": 1,
        "configVersion": "2026.07.31.001",
        "minimumClientVersion": "0.1.0",
        "maximumClientVersion": "9.9.9",
        "publishedAtUtc": "2026-07-31T05:00:00Z",
        "contentHash": "sha256-placeholder",
        "maintenance": {
          "enabled": false,
          "messageKey": "maintenance_default"
        },
        "features": {
          "multiplayerEnabled": true,
          "anonymousAuthEnabled": true,
          "dailyChallengeEnabled": false
        },
        "economy": {
          "startingScrap": 100,
          "waveScrapMultiplier": 1.0,
          "sellRefundPercent": 0.7
        },
        "ships": {
          "ship_cruiser_mk1": {
            "maxHull": 1000,
            "baseArmor": 10,
            "basePowerCapacity": 12,
            "startingModuleSlots": 6
          }
        },
        "modules": {
          "module_laser_basic": {
            "damage": 12,
            "attackIntervalSeconds": 0.45,
            "range": 5.5,
            "buildCost": 50,
            "powerCost": 2
          }
        },
        "enemies": {
          "enemy_scout_fighter": {
            "maxHull": 50,
            "movementSpeed": 3.2,
            "contactDamage": 10,
            "rewardScrap": 5
          }
        },
        "waves": {
          "globalHealthMultiplier": 1.0,
          "globalDamageMultiplier": 1.0,
          "spawnIntervalMultiplier": 1.0
        },
        "multiplayer": {
          "heartbeatSeconds": 10,
          "disconnectGraceSeconds": 45,
          "matchExpirationMinutes": 30,
          "commandBatchSeconds": 0.5
        }
      },
      "history": {}
    },
    "staging": {
      "active": {}
    },
    "development": {
      "active": {}
    }
  }
}
```

Do not permit the game client to write to `gameSettings`.

## 52. Game Settings Publishing Model

Configuration publication must be atomic from the client's perspective.

Recommended workflow:

1. Administrator creates a candidate configuration outside the client.
2. Candidate is validated against JSON schema and project-specific rules.
3. Candidate receives a unique `configVersion`.
4. Candidate is written to a versioned history node.
5. The active pointer or active object is updated in one controlled operation.
6. Clients fetch the active configuration.
7. Clients validate and cache it.
8. New missions use the new immutable snapshot.

Safer optional structure:

```json
{
  "gameSettings": {
    "production": {
      "activeVersion": "2026.07.31.001",
      "versions": {
        "2026.07.31.001": {}
      }
    }
  }
}
```

This separates the pointer from immutable versions and allows rollback.

## 53. Settings That May Be Remote

Allowed examples:

- Ship numeric stats.
- Enemy numeric stats.
- Weapon numeric stats.
- Upgrade costs.
- Reward amounts.
- Wave timing multipliers.
- Drop weights.
- Feature flags.
- Maintenance messaging keys.
- Event start and end timestamps.
- Match grace periods.
- Safe UI labels through localization keys.

## 54. Settings That Must Not Be Remote Executable Behavior

Do not download or execute arbitrary code.

Do not remotely define:

- C# type names to instantiate without an allowlist.
- Assembly names.
- Reflection calls.
- Arbitrary URLs for executable content.
- Arbitrary file paths.
- Commands interpreted as code.
- Security-rule changes from the client.
- Authentication endpoints.
- Trusted certificate behavior.

Remote configuration selects among behavior already shipped in the application.

## 55. Hard Safety Bounds

The build must enforce hardcoded absolute limits, even if Firebase contains invalid or malicious values.

Examples:

- Ship hull: 1–10,000,000.
- Damage: 0–1,000,000.
- Attack interval: 0.05–60 seconds.
- Movement speed: 0–100 units per second.
- Reward amount: 0–1,000,000.
- Percentage: defined valid range, normally 0–1 or 0–100, never ambiguous.
- Spawn count per group: 0–1,000.
- Heartbeat interval: 5–60 seconds.
- Disconnect grace: 10–300 seconds.

Exact limits must live in code and tests, not only in Firebase.

## 56. Remote Configuration Fetch Flow

At application startup:

1. Load shipped local defaults.
2. Load last-known-good cached remote configuration.
3. Build an immediately usable effective configuration.
4. Start the game without blocking unnecessarily.
5. If online, fetch `/gameSettings/{environment}/active.json`.
6. Validate HTTP result.
7. Parse into DTOs.
8. Validate schema version.
9. Validate client version compatibility.
10. Validate all values and references.
11. Validate content hash if implemented.
12. Persist as the new last-known-good configuration.
13. Announce `ConfigurationAvailable`.
14. Activate only at a safe boundary.

If any step fails, continue using the last-known-good or local defaults.

## 57. Configuration Failure Behavior

The game must remain usable when:

- Device is offline.
- DNS fails.
- Firebase times out.
- Authentication is unavailable.
- JSON is malformed.
- Required fields are missing.
- Configuration schema is newer than the client supports.
- Values exceed safety bounds.
- A referenced module ID does not exist.

The player should not see technical error text. Development builds should provide detailed diagnostics without secrets.

## 58. Configuration Cache

Store:

- Raw validated JSON or normalized serialized configuration.
- Configuration version.
- Schema version.
- Fetch time.
- Environment.
- Integrity hash where implemented.

Use an atomic file write:

1. Write temporary file.
2. Flush and validate.
3. Replace previous cache.

Never overwrite the last-known-good cache with invalid data.

## 59. Environment Separation

Supported environments:

- Development.
- Staging.
- Production.

Environment endpoints and Firebase project identifiers must be selected by build configuration, not by a normal player-facing setting.

Production builds must not be able to switch to development databases through UI or remote settings.

## 60. Player Data Layout

Recommended user node:

```json
{
  "users": {
    "{uid}": {
      "profile": {
        "createdAtUtc": "...",
        "lastSeenAtUtc": "...",
        "displayName": "..."
      },
      "progression": {
        "accountLevel": 1,
        "experience": 0,
        "unlockedCommanders": {
          "commander_nova": true
        },
        "unlockedShips": {
          "ship_cruiser_mk1": true
        }
      },
      "inventory": {},
      "campaign": {},
      "settings": {},
      "cloudSave": {
        "revision": 1,
        "updatedAtUtc": "...",
        "deviceIdHash": "...",
        "payload": {}
      }
    }
  }
}
```

Do not expose private user data through `publicProfiles`.

## 61. Local Save and Cloud Save

Local save is authoritative for immediate offline play.

Cloud save is a synchronized backup and cross-device source, subject to conflict resolution.

Every save includes:

- Save schema version.
- Revision.
- Updated timestamp.
- Device installation identifier hash.
- Progress summary.
- Integrity metadata where appropriate.

Conflict policy:

- Never silently discard meaningful progress.
- Compare revisions and timestamps.
- Merge monotonic unlocks where safe.
- For conflicting currencies or consumables, require server-validated strategy or choose one complete save rather than unsafe arithmetic merging.
- Present a conflict choice to the player when automatic resolution is unsafe.

## 62. Multiplayer Database Layout

Example:

```json
{
  "matchmaking": {
    "queues": {
      "versus_default": {
        "{uid}": {
          "rating": 1000,
          "region": "sa",
          "clientVersion": "0.1.0",
          "configVersion": "2026.07.31.001",
          "queuedAtUtc": "..."
        }
      }
    }
  },
  "matches": {
    "{matchId}": {
      "metadata": {
        "mode": "versus_defense",
        "state": "Running",
        "seed": 123456789,
        "configVersion": "2026.07.31.001",
        "createdAtUtc": "...",
        "expiresAtUtc": "..."
      },
      "players": {
        "{uidA}": {
          "ready": true,
          "loadoutHash": "...",
          "lastHeartbeatUtc": "...",
          "lastCommandSequence": 12
        },
        "{uidB}": {}
      },
      "commands": {
        "{uidA}": {
          "0000000012": {
            "type": "send_wave_modifier",
            "payload": {
              "modifierId": "reinforced"
            },
            "clientTick": 402,
            "createdAtUtc": "..."
          }
        }
      },
      "snapshots": {},
      "result": {}
    }
  }
}
```

Command payload types must use strict allowlists and validation.

## 63. Security Rules Requirements

Default deny.

Rules must ensure:

- Only authenticated users access protected player data.
- A user reads and writes only permitted portions of their own node.
- Clients cannot write game settings.
- Clients cannot grant themselves currency, unlocks, ranking, or premium items without trusted validation.
- Match participants can access only the required match data.
- Match writes validate membership, state, sequence, allowed fields, and types.
- Admin nodes are never client-readable unless explicitly designed as public.
- Query indexes are declared where required.

Illustrative starting point only:

```json
{
  "rules": {
    ".read": false,
    ".write": false,

    "gameSettings": {
      "$environment": {
        "active": {
          ".read": true,
          ".write": false
        }
      }
    },

    "users": {
      "$uid": {
        ".read": "auth != null && auth.uid === $uid",
        ".write": "auth != null && auth.uid === $uid",
        ".validate": "newData.hasChildren(['profile', 'progression'])"
      }
    },

    "matches": {
      "$matchId": {
        ".read": "auth != null && data.child('players').child(auth.uid).exists()",
        "players": {
          "$uid": {
            ".write": "auth != null && auth.uid === $uid"
          }
        },
        "commands": {
          "$uid": {
            "$sequence": {
              ".write": "auth != null && auth.uid === $uid && root.child('matches').child($matchId).child('players').child(auth.uid).exists()"
            }
          }
        }
      }
    }
  }
}
```

This sample is not production complete. Production rules must validate every writable field and must prevent users from modifying server-controlled progression or match results.

## 64. Administrative Writes

Game settings, verified rewards, bans, ranked results, and privileged account changes must be written from a trusted environment such as:

- Firebase Console for controlled early development.
- Cloud Functions.
- Cloud Run.
- Secure internal admin tool.
- CI/CD deployment using protected credentials.

Never perform privileged writes from the Unity client.

## 65. Optional Remote Config

Firebase Remote Config is specifically designed to change app behavior and appearance without requiring an app update and may later be used for:

- Feature flags.
- Gradual rollouts.
- Audience-specific values.
- A/B testing.
- Maintenance banners.

The MVP may use Realtime Database `gameSettings` as requested because it offers structured JSON and simple REST access. Keep `IGameConfigurationProvider` independent so Remote Config can be added or substituted later without rewriting gameplay.

---

# PART VII — CONFIGURATION IMPLEMENTATION CONTRACT

## 66. Required Interfaces

```csharp
public interface IGameConfigurationProvider
{
    GameConfiguration Current { get; }
    string CurrentVersion { get; }
    Task<ConfigurationFetchResult> FetchLatestAsync(
        CancellationToken cancellationToken);
    bool TryActivatePendingConfiguration();
}

public interface IConfigurationValidator
{
    ConfigurationValidationResult Validate(
        RemoteGameConfigurationDto configuration,
        ClientBuildInfo clientBuildInfo);
}

public interface IConfigurationCache
{
    bool TryLoad(out CachedConfiguration configuration);
    Task SaveAsync(
        CachedConfiguration configuration,
        CancellationToken cancellationToken);
}
```

## 67. DTO Separation

Use separate types for:

- Remote JSON DTOs.
- Validated domain configuration.
- ScriptableObject local defaults.
- Cached configuration envelope.

Do not expose raw remote DTOs directly to combat systems.

Map and normalize remote data into immutable domain objects.

## 68. Partial Overrides

The preferred cloud model supports partial overrides.

Example:

- Local defaults define every required field.
- Remote JSON may override selected approved fields.
- Missing remote values retain local defaults.

However, for multiplayer, all clients must resolve the same final configuration. Store a normalized versioned snapshot or ensure the same shipped default baseline is required by client version compatibility.

## 69. Versioning

Use semantic or date-based config versions.

Suggested format:

```text
YYYY.MM.DD.sequence
```

Example:

```text
2026.07.31.001
```

Every configuration also has an integer `schemaVersion`.

- `configVersion` identifies balance content.
- `schemaVersion` identifies JSON structure.
- `clientVersion` identifies the app build.

## 70. Migration

When local save or configuration schemas change:

- Add an explicit migration.
- Keep migrations ordered.
- Test migration from every publicly supported prior schema.
- Never delete an old migration simply because current developer saves are new.
- Back up the local save before migration.

## 71. Feature Flags

Feature flags must have:

- Stable ID.
- Local default.
- Remote override.
- Optional minimum client version.
- Optional start and end time.
- Clear fallback behavior.

A disabled feature must not leave inaccessible navigation or corrupt saved data.

---

# PART VIII — PRESENTATION AND MOBILE UX

## 72. UX Principles

- Landscape-first.
- Clear touch targets.
- Readable at common phone resolutions.
- Important combat information visible without opening menus.
- Minimal modal interruption during active waves.
- Fast restart and replay.
- Consistent back-button behavior on Android.
- Safe-area support.
- Accessibility settings remembered locally.

## 73. Required Screens

- Splash or bootstrap loading.
- Authentication or guest entry.
- Main menu.
- Fleet hub.
- Campaign map.
- Mission details.
- Ship loadout.
- Commander selection.
- Gameplay HUD.
- Pause.
- Victory.
- Defeat.
- Research.
- Settings.
- Multiplayer lobby.
- Matchmaking.
- Reconnect state.
- Cloud-save conflict dialog.
- Maintenance or unsupported-version notice.

## 74. Gameplay HUD

Must show:

- Flagship health.
- Shield where applicable.
- Scrap.
- Power capacity and usage.
- Wave number.
- Enemy count or wave progress.
- Commander ability cooldown.
- Speed controls in offline mode.
- Build button.
- Pause in offline mode.

## 75. Build Interaction

Recommended mobile flow:

1. Tap a valid socket.
2. Show compact module selection tray.
3. Tap module.
4. Show range and placement preview.
5. Confirm placement.

Do not rely on precise drag-and-drop as the only method.

## 76. Feedback

Every action needs readable feedback:

- Placement confirmation.
- Invalid placement reason.
- Upgrade effect.
- Damage type response.
- Shield hit.
- Armor hit.
- Critical hit.
- Module disabled.
- Boss telegraph.
- Ability ready.
- Network reconnect state.

---

# PART IX — ART AND AUDIO DIRECTION

## 77. Visual Direction

Use stylized sci-fi rather than photorealism.

Goals:

- Strong silhouettes.
- High contrast between friendly, enemy, neutral, and interactable elements.
- Modular ship pieces that visibly connect.
- Effects that are exciting but do not hide gameplay.
- Readable projectiles and telegraphs.
- Consistent faction palettes.

## 78. Asset Requirements

Every gameplay asset must document:

- Stable ID.
- Dimensions or world scale.
- Pivot.
- Pixels per unit where relevant.
- Compression.
- Filtering.
- Sorting layer.
- Collider expectations.
- Animation states.
- Pooling behavior.

## 79. UI Assets

- Use 9-sliced panels where appropriate.
- Keep icons readable at small sizes.
- Provide selected, disabled, pressed, and highlighted states.
- Do not bake translatable text into sprites.
- Avoid unnecessarily large textures.

## 80. Audio

Audio categories:

- Music.
- UI.
- Weapons.
- Impacts.
- Ship damage.
- Abilities.
- Enemies.
- Ambience.
- Voice.

Audio settings:

- Master.
- Music.
- SFX.
- Voice where present.

Repeated weapon sounds require variation, cooldowns, or concurrency limits.

---

# PART X — PERFORMANCE

## 81. Performance Targets

Initial targets on supported mid-range mobile devices:

- Stable 60 FPS preferred.
- 30 FPS fallback mode where needed.
- Minimal combat garbage collection.
- Controlled texture memory.
- Fast loading into a mission.
- No unbounded collections.
- No unbounded logs.
- No uncontrolled network polling.

Exact budgets must be established with target devices during the vertical slice.

## 82. Mobile Optimization Rules

- Pool frequently spawned objects.
- Cache references.
- Batch compatible sprites and materials.
- Avoid excessive transparent overdraw.
- Limit active particles.
- Use atlas strategies where beneficial.
- Avoid expensive physics when simple movement is sufficient.
- Use simplified colliders.
- Profile on devices, not only in the editor.

## 83. Network Efficiency

- Fetch game settings once per appropriate session boundary, not every frame or scene.
- Cache responses.
- Use `PATCH` for small updates.
- Batch low-frequency commands.
- Avoid downloading entire large user trees.
- Query bounded nodes.
- Add indexes for query fields.
- Use heartbeat intervals conservatively.

---

# PART XI — TESTING AND QUALITY

## 84. Definition of Done

A feature is done only when:

- Requirements are implemented.
- Code compiles without new warnings.
- Relevant automated tests pass.
- Prefabs and scenes contain no missing scripts.
- Serialization survives editor reload.
- Offline behavior is verified.
- Network failure behavior is verified where relevant.
- UI supports safe areas and target resolutions where relevant.
- Documentation is updated.
- No secrets or sensitive logs are added.
- Acceptance criteria are met.

## 85. Required Test Categories

### Edit Mode

- Damage calculations.
- Upgrade calculations.
- Configuration merging.
- Configuration validation.
- Schema version handling.
- Save migrations.
- Deterministic random selection.
- Multiplayer command validation.
- Conflict resolution.

### Play Mode

- Scene startup.
- Module placement.
- Wave completion.
- Victory and defeat.
- Object pool reset.
- UI navigation.
- Offline startup.
- Cached configuration activation.

### Integration

- Auth success and failure using test environment.
- Token refresh.
- RTDB read and write under test rules.
- Security-rule denial tests.
- Settings download and invalid-setting fallback.
- Match creation and join flow.

## 86. Firebase Emulator Use

Where practical, use Firebase Local Emulator Suite for development and automated integration testing.

Tests must not write to production databases.

CI secrets and service credentials, when needed for trusted deployment tests, must be stored in protected CI secret storage.

## 87. Configuration Test Matrix

Test at least:

- No internet and no cache.
- No internet with valid cache.
- Valid newer config.
- Same config version.
- Malformed JSON.
- Unsupported schema.
- Client below minimum version.
- Unknown content ID.
- Value below minimum.
- Value above maximum.
- Partial overrides.
- Staging and production isolation.
- Config changes while mission is active.

## 88. Security Testing

Attempt to verify that a normal client cannot:

- Write game settings.
- Read another user's private data.
- Write another user's data.
- Award itself protected currency.
- Change verified match results.
- Join a match without membership.
- Write commands for another user.
- Reuse or reorder commands improperly.
- Access admin nodes.

---

# PART XII — ANALYTICS AND PRIVACY

## 89. Analytics Events

Analytics must be purposeful and privacy-conscious.

Candidate events:

- Tutorial started and completed.
- Mission started and completed.
- Mission failure wave.
- Module selected.
- Upgrade selected.
- Commander selected.
- Matchmaking started.
- Match completed.
- Configuration fetch success or failure category.
- Cloud-save conflict.

Do not log:

- Passwords.
- Tokens.
- Full email addresses in gameplay analytics.
- Raw chat content.
- Sensitive device data not required for operation.

## 90. Consent and Age-Appropriate Design

The project must follow applicable platform and privacy requirements for its target audiences and regions.

Do not add manipulative purchasing patterns, misleading countdowns, or gambling-like paid reward systems.

---

# PART XIII — ECONOMY AND MONETIZATION

## 91. Monetization Principles

- No pay-to-win competitive power.
- Purchases must be clear.
- Offline campaign must remain meaningfully playable.
- Cosmetics are preferred.
- Expansion content may be sold transparently.
- Ads, if ever added, should be optional and not interrupt active gameplay.

## 92. Economy Protection

Client-side currency is not trusted for competitive or paid-value operations.

Protected transactions require trusted validation.

Do not let a general `/users/{uid}` write rule allow the client to set premium balances or verified rewards.

Separate client-writable preferences from server-controlled economy data.

---

# PART XIV — DEVELOPMENT ROADMAP

## 93. Milestone 0 — Repository Foundation

Deliverables:

- Unity project created.
- Folder and assembly structure.
- Bootstrap scene.
- Logging and environment configuration.
- Local configuration defaults.
- HTTP abstraction.
- Authentication interfaces.
- Realtime Database interfaces.
- Local save foundation.
- Test assemblies.

Exit criteria:

- Project compiles.
- Bootstrap enters main menu.
- Offline local configuration loads.
- Test suite runs.

## 94. Milestone 1 — Combat Prototype

Deliverables:

- One ship.
- Placement sockets.
- Laser module.
- One enemy.
- One wave.
- Damage and death.
- Scrap rewards.
- Win and lose conditions.

Exit criteria:

- Full five-minute local loop.
- No cloud dependency.
- Core calculations tested.

## 95. Milestone 2 — Offline MVP

Deliverables:

- Six modules.
- Six enemies.
- Five missions.
- Commander ability.
- Roguelite upgrades.
- Save progression.
- Main menus and HUD.

## 96. Milestone 3 — Firebase Integration

Deliverables:

- Auth REST implementation.
- Token storage and refresh.
- Realtime Database REST repository.
- `/gameSettings` download.
- Validation and caching.
- Development and staging environments.
- Security rules and emulator tests.
- Cloud-save prototype.

Exit criteria:

- Game plays offline without Firebase.
- Authorized user data is isolated.
- Ship or enemy balance can be changed remotely and takes effect on the next mission without a new APK.
- Invalid remote settings do not break startup or gameplay.

## 97. Milestone 4 — Multiplayer Prototype

Deliverables:

- Match creation.
- Join flow.
- Shared seed.
- Ready state.
- Pressure command exchange.
- Heartbeat.
- Disconnect detection.
- Result screen.

## 98. Milestone 5 — Vertical Slice

Deliverables:

- Production-quality first world.
- Boss.
- Audio and VFX polish.
- Onboarding.
- Performance pass.
- Device testing.
- Security review.
- Store-ready presentation.

---

# PART XV — AI AGENT IMPLEMENTATION RULES

## 99. General Rules

Every AI agent must:

- Read existing implementations before adding new ones.
- Preserve established conventions.
- Prefer simple, explicit code.
- Keep systems configurable.
- Add tests for logic.
- Handle cancellation in async operations.
- Handle offline and timeout cases.
- Avoid introducing hidden dependencies.
- Document public contracts.
- Preserve backward compatibility for released save data.

## 100. Agent Rules for Firebase Work

Before changing Firebase integration, inspect:

- Authentication service.
- Token model.
- HTTP client.
- Environment settings.
- Path builder.
- DTOs.
- Security rules.
- Emulator tests.
- Logging redaction.

An agent must never:

- Add service-account credentials to Unity.
- Open database rules broadly for convenience.
- Set `.read` or `.write` to `true` at the root.
- Log ID tokens or refresh tokens.
- let arbitrary Firebase JSON directly control instantiated C# types.
- Apply a remote balance update during an active match.
- make online configuration mandatory for offline startup.

## 101. Agent Rules for New Modules

When creating a new module, include:

- Stable ID.
- Localized name and description keys.
- ScriptableObject definition.
- Runtime behavior implementing approved interfaces.
- Prefab.
- Icon placeholder or documented art requirement.
- Build cost.
- Power cost.
- Base stats.
- Upgrade path.
- Targeting strategy.
- Pooling requirements.
- Audio and VFX hooks.
- Configuration override DTO support if remotely balanced.
- Validation bounds.
- Edit Mode tests.
- Play Mode smoke test where feasible.

## 102. Agent Rules for New Enemies

Include:

- Stable ID.
- Definition asset.
- Prefab.
- Movement behavior.
- Targeting behavior.
- Stats.
- Reward.
- Threat value.
- Pooling support.
- Death cleanup.
- Elite compatibility.
- Remote setting support.
- Tests.

## 103. Agent Rules for New Firebase Settings

For every new remotely configurable field:

1. Add a local default.
2. Add remote DTO field.
3. Add mapping logic.
4. Add hardcoded bounds.
5. Add validation.
6. Add merge behavior.
7. Add tests for missing, valid, low, high, and malformed values.
8. Document whether it can change between missions.
9. Document multiplayer compatibility.
10. Update example JSON and schema.

## 104. Agent Rules for UI

- Use existing UI architecture.
- Never create UI GameObjects, scene objects, layout containers, visual controls, decorative objects, or authored gameplay objects from runtime scripts.
- Every authored object must be visible and editable in the Unity Editor hierarchy or in a prefab before Play Mode. Create and modify these objects through Unity MCP, Unity CLI/editor automation, or direct scene/prefab authoring.
- Runtime scripts may reference, configure, show, hide, animate, pool, or instantiate an explicitly authored prefab when gameplay requires spawning. They must not procedurally construct the UI hierarchy or use code as a substitute for scene/prefab authoring.
- Use prefabs and authored hierarchy. Inspector references and serialized configuration must remain visible to the project owner.
- A feature is not complete if its intended scene/prefab presentation exists only after entering Play Mode.
- Keep business logic outside views.
- Use localization keys.
- Support safe area.
- Avoid hardcoded resolution assumptions.
- Provide loading, success, empty, offline, and error states.

## 105. Agent Rules for Bug Fixes

- Reproduce or identify the cause.
- Add a failing regression test when practical.
- Fix the root cause.
- Avoid broad rewrites.
- Verify adjacent behavior.
- Document any data migration or compatibility impact.

## 106. Agent Completion Report Format

Every coding task response should include:

```text
Summary
- What was implemented.

Files changed
- Path: purpose.

Validation
- Compilation performed or not performed.
- Tests run and results.
- Manual checks.

Important decisions
- Architectural or data decisions.

Limitations / follow-up
- Only genuine remaining limitations.
```

---

# PART XVI — ACCEPTANCE CRITERIA FOR CORE SYSTEMS

## 107. Remote Game Settings Acceptance Criteria

- The build contains complete local default settings.
- The game starts offline with no prior cache.
- The game starts offline with a valid cache.
- The client fetches active settings through RTDB REST when connected.
- Authentication is used when rules require it.
- A valid cloud damage change affects newly started missions.
- An active mission is not changed mid-run.
- Invalid values are rejected.
- Unsupported schemas are rejected.
- Last-known-good configuration remains intact after a failed fetch.
- The active configuration version is visible in development diagnostics.
- Multiplayer stores and verifies a configuration version.
- Production clients cannot write settings.

## 108. Authentication Acceptance Criteria

- User can register where enabled.
- User can sign in.
- Anonymous sign-in works where enabled.
- Session can be restored.
- Expired ID token can be refreshed.
- Failed refresh signs the user out safely when recovery is impossible.
- Passwords and tokens are not logged.
- Offline single-player remains available when authentication fails.

## 109. Offline Acceptance Criteria

- Campaign can launch without connectivity.
- Local progression can be saved.
- Remote settings failure does not block gameplay.
- Multiplayer entry clearly reports that connectivity is required.
- Cloud synchronization resumes safely later.

## 110. Multiplayer Acceptance Criteria

- Two authenticated users can enter a match.
- Both use the same seed and configuration version.
- Commands have sequence numbers.
- Duplicate commands are ignored.
- Unauthorized users cannot read or write match data.
- Disconnect is detected.
- Reconnect within grace period works for supported scenarios.
- Final result includes sufficient audit metadata.

---

# PART XVII — INITIAL BALANCE PLACEHOLDERS

These are development defaults, not final balance.

## 111. Starter Ship

```yaml
id: ship_cruiser_mk1
maxHull: 1000
baseArmor: 10
basePowerCapacity: 12
startingScrap: 100
startingModuleSlots: 6
```

## 112. Starter Laser

```yaml
id: module_laser_basic
damage: 12
attackIntervalSeconds: 0.45
range: 5.5
buildCost: 50
powerCost: 2
projectileSpeed: 18
```

## 113. Starter Enemy

```yaml
id: enemy_scout_fighter
maxHull: 50
movementSpeed: 3.2
contactDamage: 10
rewardScrap: 5
threatValue: 1
```

All values must remain remotely overridable only within validated bounds.

---

# PART XVIII — FIREBASE DELIVERY CHECKLIST

## 114. Firebase Console Setup

- Create separate Firebase projects or carefully separated environments for development, staging, and production.
- Enable approved Authentication providers.
- Create Realtime Database in the appropriate region.
- Deploy default-deny Security Rules.
- Add indexes for known queries.
- Create `gameSettings` nodes.
- Publish initial versioned settings.
- Configure authorized domains where relevant.
- Configure App Check if compatible with the chosen REST integration and platform plan.
- Set budget alerts and usage monitoring.
- Do not distribute admin credentials.

## 115. Initial Game Settings Workflow

For the first development iteration:

1. Ship `DefaultGameConfiguration` as a ScriptableObject.
2. Export a matching JSON template.
3. Upload a validated copy to `/gameSettings/development/versions/{version}`.
4. Set `/gameSettings/development/activeVersion`.
5. Fetch active version on startup.
6. Cache validated configuration.
7. Start missions with an immutable snapshot.
8. Change `module_laser_basic.damage` in Firebase.
9. Confirm the next mission uses the new damage without rebuilding the APK.
10. Confirm an invalid value falls back safely.

## 116. Recommended Supporting Files

The repository should eventually include:

```text
/AGENTS.md
/docs/ARCHITECTURE.md
/docs/FIREBASE_SETUP.md
/docs/GAME_DESIGN.md
/docs/BALANCE.md
/firebase/database.rules.json
/firebase/database.indexes.json
/firebase/game-settings.schema.json
/firebase/game-settings.example.json
```

AGENTS.md remains the primary authority. Supporting files may contain expanded implementation details but must not contradict it.

---

# PART XIX — DECISIONS REQUIRING OWNER APPROVAL

Agents must not decide these silently:

- Final game name.
- Final art style.
- Portrait versus landscape change.
- Adding a paid currency.
- Adding advertising.
- Adding a third-party SDK.
- Replacing Firebase.
- Selecting a real-time networking provider.
- Introducing global chat.
- Changing multiplayer from deterministic command synchronization to full state streaming.
- Changing the no-runtime-UI rule.
- Changing minimum supported OS versions.
- Publishing production Security Rules.

---

# PART XX — FINAL PROJECT COMMANDMENTS

1. The game must remain playable offline.
2. Local defaults must always exist.
3. Remote settings are validated overrides, not trusted code.
4. Active missions use immutable configuration snapshots.
5. Multiplayer participants use the same seed and configuration version.
6. The mobile client is never trusted with privileged authority.
7. Firebase service-account credentials never enter the Unity client.
8. Security Rules default to deny.
9. GameObjects and UI are authored visibly through scenes and prefabs using Unity MCP, Unity CLI/editor automation, or direct editor authoring; runtime scripts never construct authored hierarchies, and runtime spawning uses explicitly authored prefabs and pools.
10. Gameplay content is data-driven.
11. Save schemas and remote schemas are versioned.
12. Agents add tests and report real validation results.
13. No pay-to-win competitive design.
14. No unrelated rewrites during focused tasks.
15. Working, maintainable MVP scope is more important than speculative complexity.

---

# Official Technical References

- Firebase Authentication documentation: https://firebase.google.com/docs/auth
- Firebase Authentication REST API: https://firebase.google.com/docs/reference/rest/auth
- Firebase Realtime Database documentation: https://firebase.google.com/docs/database
- Realtime Database REST authentication: https://firebase.google.com/docs/database/rest/auth
- Realtime Database REST data writes: https://firebase.google.com/docs/database/rest/save-data
- Realtime Database Security Rules: https://firebase.google.com/docs/database/security
- Security Rules conditions: https://firebase.google.com/docs/database/security/rules-conditions
- Realtime Database indexing: https://firebase.google.com/docs/database/security/indexing-data
- Firebase Remote Config: https://firebase.google.com/docs/remote-config

---

**End of AGENTS.md**
