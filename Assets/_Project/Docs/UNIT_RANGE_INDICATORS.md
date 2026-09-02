# Unit Range Indicators

## Player behavior

- Selecting an occupied hero socket displays a cyan-to-gold world-space ring centered on that hero.
- The ring radius uses the hero's effective configured `Range`. Adventure and Endless therefore reflect level-up range changes; Arena reflects the selected hero definition's range.
- Selecting another hero moves the visualization to that socket.
- Closing the details panel, selecting an enemy, beginning a hero drag, deselecting an Arena hero, selling, merging, or clearing the socket hides the previous ring.
- The ring has no collider and does not intercept pointer input.

## Authored Unity hierarchy

Every supported socket contains an inactive, editable child named `Unit Range Indicator`. The child owns:

- `LineRenderer`
- `UnitRangeIndicator`
- the shared `Assets/_Project/Art/Materials/UnitRangeIndicator.mat` material

The child and serialized socket reference are authored before Play Mode. Runtime code only updates the authored line positions and shows or hides the child; it does not construct GameObjects or components.

Authored scenes:

- `Assets/_Project/Scenes/Gameplay/Gameplay.unity`
- `Assets/_Project/Scenes/Gameplay/Endless.unity`
- `Assets/_Project/Scenes/Gameplay/CitadelFallArena.unity`
- preserved compatibility scenes `MvpGameplay.unity` and `FullGame.unity`

## Rebuild workflow

In Unity, run:

`Tools > Citadel Fall > Authoring > Rebuild Unit Range Indicators`

The editor command is implemented by `Assets/_Project/Scripts/Editor/UnitRangeIndicatorSceneAuthoring.cs`. It creates or repairs the visible child hierarchy, shared material, `LineRenderer` styling, and serialized socket references, then saves every supported scene.

## Code ownership

- `UnitRangeIndicator.cs`: converts a world range into a scale-compensated 96-point circle and handles visibility.
- `TowerDetailsPanel.cs`: owns Adventure/Endless selection transitions and ensures only its selected hero displays coverage.
- `TowerPlacementSocket.cs`: exposes the authored indicator and hides it when a hero is cleared.
- `ArenaHeroSocket.cs`: shows/hides the authored indicator with Arena selection.

## Data and cloud boundary

Range values still come from `TowerDefinitionAsset` content and the exported Firebase-compatible catalog. This feature did not change schemas, starter content, Firebase rules, or cloud data, and no Firebase/cloud operation or external library was required.
