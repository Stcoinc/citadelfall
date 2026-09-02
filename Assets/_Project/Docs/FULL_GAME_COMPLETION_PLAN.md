# Citadel Fall — Full Game Completion Ledger

## Final Product

Citadel Fall is an original, landscape-first fantasy merge-defense game. Citadel Fall Arena is the primary mode. Players equip five heroes, spend mana to summon random deck members onto a scene-authored 3 x 5 board, merge matching ranks, defend a stronghold from escalating fantasy enemies and bosses, and ultimately compete on equivalent deterministic boards.

The game may learn from the clarity and short-session appeal of Rush Royale, but it must not copy that game's protected names, characters, art, text, exact UI, balance, or economy.

## Three-Mode Product Architecture

Citadel Fall intentionally keeps three gameplay styles:

| Mode | Product role | Player decisions | Session structure |
| --- | --- | --- | --- |
| **Citadel Fall Arena** | Primary game and future versus foundation | Equip five heroes, spend mana, accept random summons, merge matching ranks, manage short-term pressure | Compact 3 x 5 board; three-to-five-minute deterministic match |
| **Adventure Defense** | Secondary offline campaign, onboarding, and content-authoring mode | Choose exact heroes, place them on authored sockets, upgrade, target, merge, and sell | Authored enemy path and mission waves; longer progression missions |
| **Endless Defense** | Score attack and future leaderboard mode | Directly place heroes at any time, upgrade, sell, and drag matching same-level heroes to merge | Manual waves; five waves per round; infinite tested pressure scaling and persistent personal best |

The project originally produced a reusable classic tower-defense engine and campaign before the Arena-first direction was approved. Adventure Defense preserves that useful work and provides offline value without changing Arena's clearer merge-defense rules. The modes share account identity, heroes, enemies, unlocks, currencies, saves, combat services, ScriptableObject/JSON content, Firebase-compatible configuration, prefabs, and the Intro/Main Menu/Level Selection shell. Each mode retains its own battlefield scene, resource rules, placement interaction, and HUD.

Arena receives new-feature priority. Adventure Defense must not delay Arena polish, collection/decks, deterministic local versus, or approved networking. Do not combine the two rule sets into one HUD or make players mistake Adventure for the primary product.

## Source of Truth

1. `Assets/AGENTS.md`, especially the 2026-08-05 Arena-first scope.
2. `Assets/_Project/Docs/CITADEL_FALL_ARENA_ROADMAP.md`.
3. This completion ledger.
4. `CONTENT_AUTHORING.md` for scene/ScriptableObject/JSON/Firebase ownership.

## Completion Definition

The game is complete for the current approved scope when all milestones below meet their acceptance criteria on the supported local build. Live online multiplayer remains a separate approval gate because selecting a networking provider requires explicit owner approval.

## Current Implemented Foundation

- Six fantasy heroes with authored definitions and original sprites.
- Five-hero deck selection and local save persistence.
- Scene-authored 3 x 5 Arena board, mana summons, increasing summon costs, same-rank merges, lives, waves, bosses, and match timer.
- Data-driven ScriptableObject to validated JSON to Firebase-compatible content pipeline.
- Offline startup, Firebase REST authentication, persistent auth restore, local save slots, account progression, durable currencies, and enemy-card milestones.
- Fantasy main menu, campaign maps, profile card, avatars, Enemy Chronicles, MedievalSharp TextMeshPro presentation, and fantasy build sockets.
- Deterministic summon sequence and Arena rule tests.
- Secondary Adventure Defense mode with ten authored campaign levels, direct hero placement, upgrades, merges, selling, wave rewards, and an emergency first-free Warrior rule.
- Endless Defense scene and Main Menu/profile routing, five-wave rounds, manual starts, drag-to-merge, scalable pressure, and persisted best score/highest round.

## Milestone A — Primary Arena Combat

- Dedicated Rat, Wolf, quick Goblin, strong Orc, elite, and two boss definitions.
- Correctly oriented fantasy enemy animation and a visible stronghold endpoint.
- Distinct hero combat identities, projectiles/VFX, targeting, and readable merge-rank power.
- Deterministic waves, boss cadence, win/lose/restart, rewards, and match summary.
- Tutorial that teaches summon, merge, mana, enemy leaks, and boss preparation.

Acceptance: a new player can complete a polished three-to-five-minute offline match using only fantasy presentation, and replaying a seed produces the same gameplay sequence.

## Milestone B — Collection, Decks, and Progression

- Collection screen for heroes and enemy cards.
- Five-slot deck builder filtered by unlocked heroes.
- Clear hero stats, rarity-free starting progression, mastery, and unlock rules.
- Account XP/level, match rewards, quests or milestones, and offline-first persistence.
- One primary soft currency plus mana inside matches; no competitive pay-to-win path.

Acceptance: progression survives restart, offline play remains available, and every equipped hero is represented consistently in deck, match, and results UI.

## Milestone C — Local Versus Simulation

- Two equivalent boards start from one content version and seed.
- Ordered commands for summon, merge, ability, and pressure actions.
- Pressure is earned by defending and always has an opportunity cost.
- Duplicate/out-of-order command rejection, deterministic replay, tiebreakers, and auditable result record.

Acceptance: repeated simulation and replay produce the same result and command digest.

## Milestone D — Online Approval Gate

- Owner selects a networking/relay provider.
- Lobby, matchmaking, ready state, reconnect, heartbeat, timeout, result submission, and anti-tamper checks are implemented against the approved provider.
- Firebase remains responsible for authentication, player data, decks, versioned configuration, and durable match records unless a scoped architecture change is approved.

Acceptance: two authenticated clients complete stable matches under expected latency and reconnect scenarios, using the same seed and configuration version.

## Milestone E — Release Quality

- Complete fantasy audio/VFX pass, onboarding, settings, safe area, accessibility, device performance, and mobile input validation.
- Production-deny Firebase rules and emulator denial tests.
- Android/iOS build verification, store presentation, privacy disclosures, and no secrets in the client.

Acceptance: no missing scripts or references, relevant automated tests pass, offline/network-failure behavior is verified, target device performance is acceptable, and release checklists are complete.

## Deferred Until Requested

- Clans or guilds.
- Global chat.
- Trading.
- Four-player real-time co-op.
- Paid random items or paid competitive power.
- A broad breaking rename of stable internal tower/enemy identifiers.
