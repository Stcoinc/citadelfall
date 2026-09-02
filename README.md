# Citadel Fall

Citadel Fall is a Unity 6 fantasy tower-defense project built around an Arena-first merge-defense direction, with Adventure Defense and Endless Defense modes sharing the same content, progression, and combat foundation.

The player protects fantasy strongholds by placing and merging heroes such as Mage, Warrior, Paladin, Archer, Druid, and Sorcerer against escalating enemy waves. The project is offline-first and keeps gameplay content data-driven so enemy, hero, level, wave, and balance values can later be adjusted through validated Firebase Realtime Database content without requiring an app update.

## Current Game Modes

- **Citadel Fall Arena**: the primary mode. Players bring a five-hero deck into a 3 x 5 formation, spend mana to summon random equipped heroes, merge matching same-rank units, and survive deterministic short-match pressure.
- **Adventure Defense**: a campaign-style tower-defense mode with authored paths, sockets, direct hero selection, Scrap placement, upgrades, selling, mission rewards, and level-map progression.
- **Endless Defense**: a score-attack mode with direct placement, manual wave starts, five waves per round, scaling pressure, upgrade/merge support, and persistent best score/round data.

## Key Features

- Unity-authored scenes and prefabs for all visible gameplay/UI objects.
- No runtime construction of authored UI hierarchy; scripts bind and update serialized scene references.
- Fantasy hero roster, enemy roster, campaign maps, Arena board, Endless board, and profile flow.
- TextMeshPro UI using the MedievalSharp-Regular SDF font.
- Firebase REST architecture for authentication, progression, username claims, and future validated remote content.
- Local fallback content and save data so the game remains playable offline.
- ScriptableObject authoring workflow for heroes, enemies, waves, levels, Arena rules, and battlefield layouts.
- Enemy collection progress, profile panel, avatar selection, level selection, unit shop, victory and Game Over flows.

## Unity Version

Open the project with:

```text
Unity 6000.3.7f1
```

The version is recorded in `ProjectSettings/ProjectVersion.txt`.

## Project Structure

```text
Assets/AGENTS.md                         Primary project and AI-agent implementation contract
Assets/_Project/Art                      Game art, UI art, sprites, avatars, portraits
Assets/_Project/Data                     Authoring assets and local JSON defaults
Assets/_Project/Docs                     Design, authoring, balance, and implementation notes
Assets/_Project/Scenes/App               Intro, Main Menu, Level Selection
Assets/_Project/Scenes/Gameplay          Gameplay, Arena, Endless, preserved historical scenes
Assets/_Project/Scripts/Application      App-level services and gameplay application logic
Assets/_Project/Scripts/Domain           Content/domain models
Assets/_Project/Scripts/Features         Unity-facing gameplay, UI, scene, and editor code
Assets/_Project/Tests                    Edit Mode regression tests
firebase                                 Firebase rules and configuration notes
Packages                                 Unity package manifest and lock file
ProjectSettings                          Unity project settings
```

## Active Scene Flow

```text
Intro -> MainMenu -> LevelSelection -> Gameplay
Intro -> MainMenu -> LevelSelection -> CitadelFallArena
Intro -> MainMenu -> LevelSelection -> Endless
```

Enabled build scenes include Intro, MainMenu, LevelSelection, Gameplay, CitadelFallArena, and Endless. `MvpGameplay` and `FullGame` are preserved historical scenes and are not the current player-facing flow.

## Development Rules

- Keep all intended UI and authored gameplay objects visible and editable in Unity before Play Mode.
- Use Unity editor tooling, Unity MCP, or direct scene/prefab authoring for hierarchy changes.
- Runtime scripts may instantiate authored prefabs for live gameplay, but must not procedurally assemble UI.
- Preserve offline-first behavior and validated local defaults.
- Do not add Firebase service-account credentials or secrets to the Unity client.
- Update `Assets/_Project/Docs/AGENT_MEMORY.md` after implementation work so future sessions can find what changed and why.

## Validation Commands

From the repository root:

```powershell
dotnet build "Tower defense engine.sln" --no-restore
dotnet test "Project.Tests.EditMode.csproj" --no-build --verbosity minimal
```

Unity-generated solution and project files are ignored by Git, so regenerate them from Unity/Rider/Visual Studio if needed.

## Git Notes

This repository uses a Unity-focused `.gitignore` and stores the current binary art/font/audio-style assets directly in Git. Git LFS is intentionally not required for this project to avoid separate LFS storage and bandwidth costs.

Keep individual committed files below GitHub's 100 MB hard limit. If future assets grow beyond that, compress them, resize source art, split archives, or discuss an external asset-storage option before adding paid LFS usage.

## Repository

GitHub: https://github.com/Stcoinc/citadelfall
