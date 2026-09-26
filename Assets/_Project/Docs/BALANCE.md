# Citadel Fall Balance

## Endless Defense Baseline

- Five manually started waves form each round.
- Enemy health grows 12% per round, contact damage 8%, and speed 2% up to a 1.5x cap.
- Each configured spawn group gains 20% of its base count per round (rounded up).
- Spawn intervals shrink by 3% per round with a 0.25-second floor.
- Defeats score `10 + threat * 5 + 2 * (round - 1)`; completing a round adds `round * 100`.
- Players may place and upgrade between or during waves. Dragging a hero onto an identical same-level hero uses the configured merge recipe.

These rules are implemented in `EndlessModeRules` and covered by EditMode tests. Enemy identities and base wave composition remain editable ScriptableObjects and Firebase-compatible JSON.

## Adventure hero progression

### Placement costs and roles

Adventure and Endless direct-placement costs now scale with battlefield value instead of every hero costing 50 Scrap.

| Hero | Build cost | Role and cost reason |
| --- | ---: | --- |
| Mage | 35 Scrap | Affordable ranged splash caster and easiest paid follow-up unit |
| Archer | 45 Scrap | Fast, very long-range precision damage without splash |
| Druid | 55 Scrap | Splash damage that prioritizes weakened groups |
| Warrior | 60 Scrap | High close-range single-target damage; the first Warrior is still free while the board is empty |
| Paladin | 75 Scrap | Durable-feeling heavy splash attacker that prioritizes the healthiest enemy |
| Sorcerer | 100 Scrap | Highest base power, boss-priority targeting, long range, and heavy splash damage |

Level 1 begins with 120 Scrap, so the guaranteed free Warrior prevents an economy soft-lock and leaves enough resources for meaningful follow-up choices. Later placements use the configured Warrior cost normally. Unaffordable heroes remain visible, disabled, and labeled with their required Scrap.

All six starting heroes now have complete level 1-7 Adventure progression. A player can either upgrade one placed hero with Scrap or merge two identical heroes of the same level with Coins; both routes advance exactly one level and use the same resulting combat statistics.

| From level | Upgrade cost | Merge cost | Damage multiplier | Range multiplier | Attack interval multiplier |
| --- | ---: | ---: | ---: | ---: | ---: |
| 1 | 60 Scrap | 30 Coins | 1.25x | 1.03x | 0.96x |
| 2 | 90 Scrap | 55 Coins | 1.55x | 1.06x | 0.92x |
| 3 | 130 Scrap | 90 Coins | 1.90x | 1.09x | 0.88x |
| 4 | 180 Scrap | 140 Coins | 2.30x | 1.12x | 0.84x |
| 5 | 240 Scrap | 200 Coins | 2.80x | 1.15x | 0.80x |
| 6 | 320 Scrap | 275 Coins | 3.40x | 1.18x | 0.76x |

The button may say `MAX` only when the placed hero is actually level 7. If a future content entry declares a higher maximum level without supplying a recipe, the UI says `NOT SET` instead of incorrectly claiming it is maxed.

## Citadel Fall Arena Early-Match Baseline

Arena uses its own validated pressure multipliers so Adventure enemy definitions remain reusable. These values belong to `arena_standard` in `starter_content.json` and may be published through the versioned Firebase settings pipeline.

### Enemy introduction

| Arena waves | Available regular enemies | Purpose |
| --- | --- | --- |
| 1–2 | Rat | Teach summoning and establish mana income with one simple enemy |
| 3–4 | Rat, Wolf | Introduce the first durable enemy after the board is established |
| 5–6 | Rat, Wolf, Goblin | Add faster pressure after the player has had time to summon and merge |
| 7+ | Rat, Wolf, Goblin, Orc | Use the full regular roster; bosses still follow the configured cadence |

This is an expanding deterministic pool, not a forced replacement. A newly introduced enemy becomes eligible from its configured wave onward while earlier enemies remain valid.

### Pressure values

| Setting | Baseline | Effect |
| --- | ---: | --- |
| `EnemySpeedMultiplier` | 0.50 | Arena enemies move at half their reusable base speed |
| `EnemyHealthMultiplier` | 0.65 | Regular and boss base health starts at 65% in Arena |
| `EnemyHealthPerWave` | 0.08 | Health grows 8% of the adjusted base per wave after wave 1 |
| `BossHealthMultiplier` | 1.35 | Boss-only multiplier after normal Arena scaling |
| `BaseSpawnIntervalSeconds` | 2.40 | Gentle opening spawn cadence |
| `SpawnIntervalReductionPerWave` | 0.05 | Pressure increases gradually each wave |
| `MinimumSpawnIntervalSeconds` | 1.00 | Prevents an unbounded spawn-rate escalation |
| `WolfIntroductionWave` | 3 | First Wolf eligibility |
| `GoblinIntroductionWave` | 5 | First Goblin eligibility |
| `OrcIntroductionWave` | 7 | First Orc eligibility |

Opening reference values:

- Wave 1 Rat: 20.8 health, 2.1 movement speed.
- Wave 2 Rat: approximately 22.46 health, 2.1 movement speed.
- Wave 3 Wolf: approximately 135.72 health, 0.75 movement speed when selected.
- Wave 5 Goblin: approximately 42.9 health, 1.6 movement speed when selected.
- Wave 7 Orc: approximately 134.68 health, 0.85 movement speed when selected.

## Balance Guardrails

- A fresh default deck must reliably survive wave 2 without requiring an early lucky merge.
- Waves 1–2 must never select Wolf, Goblin, Orc, elite, or boss content outside the separate configured boss cadence.
- Enemy movement speed changes for Arena must use `ArenaRules`; do not weaken shared Adventure definitions to fix Arena pressure.
- Preserve deterministic selection: balance changes may alter the eligible pool or numeric rules but must not couple enemy randomness to hero summons or socket selection.
- Apply remote balance only when starting a new match; never mutate an active match snapshot.
- Re-test new-player completion, mana income, leak rate, and boss time-to-kill after changing hero damage, summon cost, enemy rewards, or these pressure fields.

## Adventure map bosses

- Adventure maps contain five levels each.
- Boss status is controlled by the assigned wave set's serialized **Has Boss** checkbox; it is never inferred from the level number.
- The current design chooses the fifth level of each map as a boss level: global Levels 5 and 10 in the current ten-level campaign.
- Map 1 Level 5 uses `waves_classic_005`; its last wave must end with exactly one Runestone Troll (`enemy_boss`).
- Map 2 Level 5 (global Level 10) uses `waves_classic_010`; its last wave must end with exactly one Thorn Warden (`enemy_boss_warden`).
- Levels 1-4 and 6-9 currently use their own non-boss wave sets. Any future level may use a boss-enabled wave set. A boss must remain the only boss spawn group, as the final spawn group of the final wave, with count 1.
