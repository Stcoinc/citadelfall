# Firebase Settings Notes

The enabled `IntroSceneController` loads remote balance/content from Firebase Realtime Database through REST before routing to Main Menu. The disabled historical `FullGame.unity` flow retains equivalent compatibility code but is not the active application entry point.

Expected development paths:

```text
/gameSettings/development/activeVersion
/gameSettings/development/versions/{activeVersion}/starterContent
```

For the first upload, set `activeVersion` to a stable version string such as:

```json
"2026.08.03.001"
```

Then upload the full contents of:

```text
Assets/_Project/Data/Json/Defaults/starter_content.json
```

to:

```text
/gameSettings/development/versions/2026.08.03.001/starterContent
```

If Firebase is offline, empty, blocked by rules, or slower than the boot timeout, the game uses the local `starter_content.json` defaults.

The same versioned `starterContent` snapshot supplies both gameplay styles:

- Citadel Fall Arena reads hero/enemy content plus `ArenaRules` for deck size, mana, summon growth, merge limits, lives, cadence, duration, and rewards.
- Adventure Defense reads hero/enemy/wave/level content for direct placement, authored mission waves, upgrades, selling, and mission rewards.

Scene geometry remains local and authored. Firebase must not move Arena sockets, Adventure paths/sockets, cameras, or UI during runtime, and downloaded values must not mutate an active match snapshot.

Arena enemy difficulty is controlled independently through validated `ArenaRules` fields for movement/health multipliers, health growth, boss health, spawn cadence, and Wolf/Goblin/Orc introduction waves. See `Assets/_Project/Docs/BALANCE.md` before publishing changes. Missing fields from an older compatible payload resolve to safe local Arena defaults.

## Permanent usernames

Player usernames are normalized to lowercase and must match `^[a-zA-Z0-9_]{3,20}$`. The client reserves `/usernames/{normalizedUsername}` with a conditional REST `PUT` using `if-match: null_etag`; Firebase returns HTTP 412 when another player wins the claim. A player profile with an existing non-empty username is never offered the claim action again.

Production Realtime Database rules must also enforce the invariant. The username index may only be created by its authenticated owner, an existing index may never change owners, and `players/{uid}/profile/username` may only move from absent/empty to the username whose index belongs to that UID. Keep all unrelated nodes default-deny. Do not publish production rules without owner approval and emulator denial tests.

A development rules file now lives at `firebase/database.rules.json`, referenced by the repository-root `firebase.json`. Review it against the target Firebase project and run emulator denial tests before any production deployment. An empty `/usernames` collection does not need to be created manually: Realtime Database creates `/usernames/{normalizedUsername}` atomically on the first successful claim.

The current username-login compatibility record also contains the account email so the REST client can translate a username into Firebase email/password authentication. Consequently, rules for username lookup require a deliberate privacy review before release; do not make any broader player profile or progression node public.
