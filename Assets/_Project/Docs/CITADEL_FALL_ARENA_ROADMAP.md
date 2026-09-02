# Citadel Fall: Arena Direction

## Decision

Citadel Fall is now **Arena-first** by owner direction. Citadel Fall Arena is the primary game mode. The authored offline campaign remains valuable as training, onboarding, and optional adventure content, but it no longer defines the final product's main loop. Arena takes inspiration from the clarity and short-session structure of merge tower-defense games, while using Citadel Fall's original heroes, enemies, progression, art direction, terminology, and fair economy.

Existing campaign maps, authentication, player progression, Enemy Chronicles, and the Firebase content pipeline remain reusable. New work prioritizes the Arena match, collection/deck loop, deterministic versus simulation, and then approved online play.

The game stays landscape-first. Choosing a live multiplayer provider is intentionally deferred until the offline deterministic prototype is fun and stable; that choice requires owner approval.

## Why Arena and Adventure Defense Both Exist

The project first established a reusable classic tower-defense engine with authored paths, selectable defenders, upgrades, selling, and campaign progression. The later owner-approved direction made the compact merge-defense Arena the final product's primary loop. Removing the campaign would discard a working offline mode, onboarding surface, content-testing path, and useful engine capability; forcing both modes into one ruleset would make the Arena harder to understand.

Therefore:

- **Arena** owns five-hero decks, random mana summons, the 3 x 5 formation, merge ranks, deterministic short matches, and future equivalent-board versus play.
- **Adventure Defense** owns direct hero choice, Scrap placement, authored mission paths/sockets, upgrades, selling, longer waves, and campaign-map progression.
- Both reuse the application shell, account/profile, unlocks, durable currencies, enemies, heroes, combat services, content pipeline, and Firebase-compatible settings.
- Arena is the feature priority. Adventure is maintained as a secondary offline mode and creator-engine proof, not developed as a competing main product.

## Player experience

An Arena match is designed for a three-to-five-minute session:

1. The player enters with a deck of five unlocked hero defenders.
2. The battlefield presents a compact 3 x 5 formation of fifteen sockets.
3. Mana summons a random hero from the equipped deck into an empty socket.
4. Each summon increases the next summon cost for that match.
5. Two copies of the same hero at the same merge rank can merge into one stronger copy.
6. Enemy waves grow in pressure, with a boss approximately every 90 seconds.
7. The stronghold begins with mode-specific health or lives. A match ends on stronghold defeat or the configured time/result condition.
8. When the stronghold reaches zero in any mode, show an authored Game Over panel with Restart and Main Menu actions.

The first six hero families are Mage, Warrior, Paladin, Archer, Druid, and Sorcerer. The first enemy families are Rat, Wolf, quick Goblin, and strong Orc. Existing tower/enemy IDs continue working while this content is migrated safely.

## What belongs where

| Concern | Authority |
| --- | --- |
| Arena formation, Adventure sockets/paths, cameras, per-mode HUD composition, VFX | Unity scenes and prefabs |
| Friendly authoring of heroes, enemies, waves, levels | ScriptableObject authoring assets |
| Runtime stats, Arena rules, balance versions | Validated JSON, local fallback, Firebase Realtime Database |
| Selected deck, unlocked heroes, currencies, collection, campaign progress | Player save/progression data |
| Match seed, commands, results | Deterministic match model; network transport later |

Firebase paths should follow the existing versioned settings contract. `ArenaRules` is part of `starterContent`, so a balance version can change mana, board, boss, merge, or duration rules without changing scene geometry.

## Delivery phases

### Phase 1 — Arena foundation (started)

- Add validated `ArenaRules` to the runtime content catalog.
- Add a five-hero deck model and save-slot selection field.
- Add deterministic seeded summon selection.
- Add pure summon-cost and merge eligibility rules.
- Add automated tests and a local `arena_standard` configuration.

Exit: content loads from the existing JSON/Firebase pipeline and all Arena foundation tests pass.

### Phase 2 — Offline playable board (playable slice started)

- Author a dedicated `CitadelFallArena.unity` scene; do not generate runtime UI.
- Place fifteen scene-authored fantasy sockets in a readable 3 x 5 board.
- Add mana, summon button, next-cost feedback, stronghold lives, timer, wave, and boss HUD.
- Spawn the equipped five-hero deck using existing hero/tower prefabs.
- Support drag/tap merge with persistent, clear selection feedback. A hero's weapon is suspended from drag start until it returns to its socket; a consumed merge source never attacks while moving, while the stationary merge destination remains active.
- Run one complete local match against deterministic waves.

Exit: a player can finish, win, lose, restart from Game Over, return to Main Menu from Game Over, and reproduce a match with the same seed.

### Phase 3 — Collection and deck building

- Add a scene-authored deck builder with five slots.
- Filter choices to unlocked hero defenders and persist the chosen deck.
- Replace transitional sci-fi tower content with the six initial fantasy hero families.
- Connect hero upgrades to account progression while keeping match rules understandable.

Exit: the chosen deck persists across app restarts and every equipped hero is represented by real Citadel Fall art/content.

### Phase 4 — Versus simulation before networking

- Run two equivalent boards locally from one shared seed.
- Add a fair pressure resource earned by defense, not bought during a match.
- Define low-frequency deterministic commands: summon, merge, ability, pressure action.
- Record config version, seed, ordered commands, and result for replay/debugging.

Exit: two local simulated players remain synchronized and a recorded match replays to the same result.

### Phase 5 — Online multiplayer

- Select a networking/relay provider with explicit owner approval.
- Add lobby, matchmaking, reconnect, timeout, anti-tamper validation, and result submission.
- Keep Firebase for identity, progression, decks, configuration, and durable results unless the approved network architecture requires a scoped change.

Exit: stable two-player matches under expected latency and reconnect conditions.

## Product guardrails

- Do not copy Rush Royale names, art, characters, UI assets, or exact economy.
- Keep the battle HUD focused: one primary resource, five deck heroes, board, wave/boss, and stronghold state.
- Avoid currency overload and avoid pay-to-win stat advantages in competitive play.
- Paid content may focus on cosmetics, presentation, and fair progression choices.
- Every gameplay-affecting value must be versioned, validated, and have a local fallback.
- Multiplayer work must not make the campaign require a network connection.

## Current implementation checkpoint

The first code slice adds `arena_standard` with a 3 x 5 board, five-hero deck, 40 starting mana, 10 base summon cost, +10 per summon, maximum merge rank 7, three stronghold lives, 90-second boss cadence, and 300-second match duration. These are starting playtest values, not final balance.

The early pressure baseline now introduces enemies progressively: waves 1–2 use Rat only, wave 3 unlocks Wolf, wave 5 unlocks Goblin, and wave 7 unlocks Orc. Arena applies a 0.50 movement-speed multiplier, 0.65 opening health multiplier, 8% per-wave health growth, and a 2.40-second opening spawn interval with a 1.00-second floor. These values are validated `ArenaRules` fields so Firebase can rebalance future matches without weakening shared Adventure enemy definitions.

The first Phase 2 slice is now present in `CitadelFallArena.unity`: fifteen authored sockets, five deck labels, mana and increasing summon cost, deterministic random placement, tap-to-select merging, visible merge ranks, escalating pressure waves, boss cadence, stronghold lives, match timer, and menu navigation. It reuses the current tower/enemy prefabs while fantasy hero content replaces the transitional definitions.

The deck-builder slice is now implemented inside the Arena start flow. Players select five of six fantasy defenders—Mage, Warrior, Paladin, Archer, Druid, and Sorcerer—and the selection is persisted when an active save slot exists. Original transparent hero sprites are used on the choice cards, equipped deck tray, and battlefield. Arena summoning exclusively selects `hero_*` definitions. Active Adventure Defense also presents the fantasy heroes; transitional turret definitions remain compatibility data and disabled-scene history rather than the active player-facing roster.

The fantasy enemy-presentation slice is now implemented. Arena selects only Rat, Wolf, quick Goblin, strong Orc, Runestone Troll, and Thorn Warden content. Each has a Unity-ready 8 x 5 directional sheet with eight-frame Right, Left, Up, Down, and Death rows; the shared enemy prefab holds 240 named frames. Creatures select rows from their actual path direction rather than rotating the sprite around bends. A scene-authored Citadel Fall stronghold marks the route endpoint.

Hero combat readability and the first deterministic match-completion slice are now implemented. Arcane, steel, holy, piercing, nature, and shadow attacks have distinct projectile/impact palettes and scales while numeric stats remain data-driven. Hero selection, socket placement, and enemy pressure use separate seed-derived random streams, so player summons cannot alter the shared enemy sequence. The scene-authored result panel reports wave, defeats, summons, merges, and leaks, with replay and menu actions. A first-match message tutorial teaches mana summons, rising cost, matching-rank merges, and stronghold defense, then persists completion when a save slot is active.

Arena now reuses the permanent application shell instead of bypassing it: `Intro -> MainMenu -> LevelSelection/profile -> CitadelFallArena`. The same Level Selection scene continues to own New Game/Continue and adventure-map selection. Arena victories and defeats grant remotely configurable coin rewards, persist match/victory/defeat totals locally, and mirror currencies plus enemy progression to authenticated Firebase profiles.

All gameplay modes now have an authored stronghold-defeat presentation. Adventure Defense receives the shared `MvpGameplayController.Defeat` event and opens `Gameplay.unity > Full Game Canvas > Game Over Panel`, with Restart and Main Menu buttons. Endless Defense keeps its scene-authored `Endless Defeat Panel` and adds an authored Restart button beside Main Menu. Arena already uses its authored result panel for both victory and stronghold defeat.

The next implementation slice is deeper combat and fair progression: explicit boss mechanics/telegraphs, deterministic tick-based wave events and replay records, and a collection/deck hub centered on the reused Level Selection flow.
