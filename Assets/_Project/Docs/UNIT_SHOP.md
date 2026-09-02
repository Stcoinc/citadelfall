# Citadel Fall Unit Shop

Implemented: 2026-09-01

## Player Experience

The campaign-map shop now opens as a full-screen fantasy Unit Shop inspired by the owner's three-card reference layout. It uses a dark citadel armory background, a large navy-and-gold frame, Coin and Gem wallet badges, three portrait cards, class-colored accents, concise combat stats, price/ownership states, green purchase buttons, and a red Close button. All runtime text uses the existing MedievalSharp TextMeshPro font supplied by the Level Selection scene.

The first page contains:

1. Paladin — starter-owned.
2. Druid — starter-owned.
3. Sorcerer — costs 750 Coins.

The five original starter heroes remain sufficient for the default five-unit Arena deck. Sorcerer is now the first paid hero. Locked Sorcerer choices are dimmed in Arena deck selection and direct players to the Unit Shop.

## Implementation Locations

- Authored shop hierarchy: `Assets/_Project/Scenes/App/LevelSelection.unity` under `Selection Canvas > Unit Shop Panel`
- Serialized shop/card binding components: `Assets/_Project/Scripts/Features/Gameplay/UnitShopView.cs`
- Shop purchase, wallet, local persistence, and account persistence: `Assets/_Project/Scripts/Features/Gameplay/LevelSelectionSceneController.cs`
- Reusable editor authoring command: `Assets/_Project/Scripts/Editor/UnitShopSceneAuthoring.cs`
- Arena ownership enforcement: `Assets/_Project/Scripts/Features/Gameplay/CitadelFallArenaController.cs`
- Sorcerer authoring cost: `Assets/_Project/Data/Authoring/Towers/hero_sorcerer.asset`
- Runtime content version and cost: `Assets/_Project/Data/Json/Defaults/starter_content.json` (`2026.09.01.001`)
- Unit Shop art referenced directly by the scene: `Assets/_Project/Resources/UnitShop`
- Content validation assertions: `Assets/_Project/Tests/EditMode/Configuration/StarterContentValidatorTests.cs`

The old shop hierarchy has been replaced in `LevelSelection.unity`. Every background, frame, wallet, card, portrait, label, purchase button, message, and Close control is now a serialized GameObject visible before Play Mode. The controller references `UnitShopView`, and each card references its portrait, accent, text, and button through serialized Inspector fields. Purchase and Close callbacks are persistent scene `Button.onClick` events. Internal `TowerDefinition`, `PurchasedTowerIds`, `BuyShopTower*`, and `ShowTurretShop` identifiers remain unchanged to protect saves and UnityEvent compatibility.

To rebuild the authored hierarchy after deliberate layout changes, open `LevelSelection.unity` and run `Tools > Citadel Fall > Authoring > Rebuild Unit Shop UI`. This is Editor-only authoring: it saves real scene GameObjects and never executes in a player build.

## Art and Image Generation

The built-in image generator created the background used at:

`Assets/_Project/Resources/UnitShop/UnitShopBackground.png`

Final prompt:

> Create a polished medieval-fantasy shop backdrop for Citadel Fall: a shadowed royal armory and adventurers' guild hall inside a stone citadel, faint blue magical crystals on the left, warm forge embers on the right, subtle banners and shelves at the far edges, with a deliberately dark and low-detail central area for readable Unity UI. High-end hand-painted fantasy game style using navy blue, antique gold, sapphire, and restrained ember-red. Background art only; no UI, text, cards, people, characters, weapons, coins, gems, logos, watermarks, sci-fi, planets, firearms, or modern technology.

Existing project art was reused for the frame, button states, and Paladin/Druid/Sorcerer portraits. The imported assets live under `Assets/_Project/Resources/UnitShop` and are serialized directly on scene Images and card components; runtime resource loading is not used by the shop view.

## Purchase and Persistence Flow

1. The selected save profile provides the wallet.
2. Starter units display `STARTER UNIT / OWNED` and cannot be purchased again.
3. A paid unit is disabled and displays `NOT ENOUGH` when Coins are insufficient.
4. A valid purchase subtracts Coins, marks currencies initialized, stores the unit ID in the local save slot, unlocks it in authenticated progression, saves `PlayerPrefs`, and queues the existing Firebase progression save.
5. The wallet and card immediately refresh to the owned state.

No Firebase request, Google Cloud command, deployment, or external library was added while implementing this UI. The feature reuses the existing `AppRuntimeSession.SavePlayerProgressionAsync` path.

## Validation

- Command: `dotnet build "Tower defense engine.sln" --no-restore`
- Unity MCP verified the saved hierarchy, controller/card references, authored hero IDs, and scene persistence.
- Unity Play Mode was opened through Windows app control at Full HD reference resolution.
- Verified Campaign → save slot → Unit Shop, live wallet/stats binding, three portraits, MedievalSharp text, owned states, insufficient-funds disabled state, and the persistent Close button returning to the level map.
- The generated background and all copied sprites imported successfully through Unity.
