# JPacman (Unity) — AI Assistant Guide

Unity port of JPacman, a Pac-Man clone originally written in C++ in 1998–99 (see `../README.md`; the original source lives in `../JPacman1999`, an axmol port in `../External/axmol`). This folder is the Unity project root; the git repo root is the parent folder (main branch: `master`).

## Versions

- Unity **6000.5.8f1** (6.5). `ProjectSettings/ProjectVersion.txt` is the source of truth.
- Entities **6.5** (now a core package), Entities Graphics, Burst, Input System, uGUI + TextMesh Pro.
- **URP 17.5 with the 2D Renderer** (`Assets/New Universal Render Pipeline Asset.asset`, assigned in Graphics settings). Built-in RP is deprecated in 6.5; don't switch back.
- MCP for Unity (`com.coplaydev.unity-mcp`, from OpenUPM) for editor automation; see "Working with the editor" below.

## Layout

```
Assets/
  Scenes/Game.unity            only scene in the build; hosts UI/audio MonoBehaviours
  Scenes/Game/GameEntities.unity  subscene baked into ECS (Main entity + prefabs)
  Scripts/
    Authoring/      MonoBehaviour + Baker pairs (MainAuthoring bakes all config into blobs)
    Components/     IComponentData, buffer elements, tags, and the *Aspect helper structs
    Systems/        ISystem / SystemBase systems and their IJobEntity jobs
    MonoBehaviours/ UI and audio (*Events.cs) driven by events from ECS
    ScriptableObjects/  config assets types (GameConfig, LevelConfig, MapConfig, ...)
    Util/           blob builders (*ConfigData), Direction, map helpers
    Service/        BackendClient: posts/gets high scores from an Azure Functions backend
  ScriptableObjects/  config asset instances (levels, maps, rounds, fruits, menu dot shapes)
  Prefabs/, Textures/, Audio/
```

All code is in the single `Assembly-CSharp` assembly (no asmdefs), global namespace.

## Architecture

- **Pure ECS gameplay.** A singleton `Main` entity (baked by `MainAuthoring`) holds prefabs and `BlobAssetReference` configs (levels, maps, intro, menu). `Game` and `PowerupMode` components are added to the same entity once gameplay starts.
- **Game phases are tag components on the main entity** (`LevelStartPhaseTag`, `LevelPlayingPhaseTag`, `LevelDeadPhaseTag`, `LevelWinPhaseTag`, `LevelClearPhase`, `LevelResetLivePhaseTag`, `LevelGameOverPhaseTag`; also `IntroPhase`, `MenuPhase`). Each has a system that does `RequireForUpdate<Tag>` and uses `ISystemStartStop` for enter/exit logic. Changing phase = remove one tag, add the next, via an `EntityCommandBuffer`.
- **ECS → MonoBehaviour communication goes through dynamic buffers on the main entity** (`SoundEventBufferElement`, `SetScoreTextBufferElement`, `ShowUIBufferElement`, ...). `AudioSystem` and `UISystem` (managed `SystemBase`) drain them each frame and raise C# `Action` events that `AudioEvents` / `UIEvents` / `*MenuEvents` subscribe to via `XxxSystem.Instance`.
- **Rendering is hybrid, not DOTS**: each sprite is a hidden companion GameObject with a `SpriteRenderer` (materials: built-in `Sprites-Default`). `SpriteAnimationSystem`, `SpriteColorSystem` and `EnemyColorSystem` push ECS state into them on the main thread every frame. Entities Graphics can't render `SpriteRenderer`s, so it's currently unused; it's kept deliberately for a planned move to DOTS rendering (quad meshes + atlas material with per-entity UV/colour material properties, animated from Burst jobs).
- **Menus** are roots under the `Hud` prefab, one per `UIEvents.ShowUIType` (Intro, Menu, Scores, Ingame, Paused, GameOver, Options, Info), each with a `MenuEvents` subclass. `Menu.asset` maps menu screens to background dot shapes. Append new `ShowUIType` values at the end: they're serialised as ints.
- **Levels and rounds**: `GameConfig.RoundConfigs` holds 3 rounds (Fácil/Normal/Difícil, each with its own map), and `MainAuthoring` flattens all their levels into one list (0–7, 8–17, 18–31). Winning advances to the next index, so play continues into the next round, as in the 1999 game. The Options "Ronda" setting picks the starting round (`UISystem.GetRoundFirstLevel`).
- **Player options** live in `GameSettings` (static, `PlayerPrefs`): `StartRound`, `MovingCollectibles` (copied into `Game.CollectiblesMoveDisabled` at game start; gates dot/power-up movement), `MusicOn`, `SoundOn` (applied as `AudioSource.mute` in `AudioEvents`). The original 1999 menu in `../JPacman1999/Src/JPacman/Menu.cpp` is the reference.
- **Input**: Input System only (no legacy `UnityEngine.Input`). `Assets/InputSystem_Actions.inputactions` is the project-wide actions asset (Project Settings > Input System Package; `InputSystem.actions`). The **Player** map has `Move` and `Pause` (Gamepad Start, Esc, P), read by `PlayerInputSystem`. `PausedMenuEvents` resumes on the same `Pause` action. The **UI** map drives the scene's `InputSystemUIInputModule`; `MenuEvents` hooks `UI/Cancel` (Esc, gamepad B) as "back". Edit bindings in the Input Actions editor. `PlayerInputSystem` only enables/disables the Player map, because the UI map is shared with the input module.
- Movement is grid-based in "map space" (`MapConfigData.WorldToMapPos` / `MapToWorldPos`); `MovableAspect.Move` handles cell edges, direction changes and tunnel teleports.

### `*Aspect` structs are NOT Entities aspects

`IAspect` and `Entities.ForEach` were removed in Entities 6.x. The former aspects were kept as plain `readonly struct`s holding `RefRW`/`RefRO`/`DynamicBuffer` fields, so their gameplay logic didn't change. Patterns:

- **In jobs**: `Execute` takes the `Entity` and `RefRW<T>`/`RefRO<T>` params, then calls `new XxxAspect(entity, ...).Method(...)`. Zero-size tags the aspect used to filter on are now `[WithAll(typeof(Tag))]` on the job.
- **Main-entity aspects** (`GameAspect`, `PowerupModeAspect`) are built in system code from `SystemAPI.GetComponentRW<T>(mainEntity)` / `SystemAPI.GetBuffer<T>(mainEntity)`. Always fetch refs via `SystemAPI` inside the system: the source generator caches the lookups in `OnCreate` and completes dependencies. Creating lookups/queries/type handles at runtime from helper code (`state.GetComponentLookup`, `state.GetEntityQuery`) logs a "creates a Lookup object during OnUpdate" warning in `ISystem`s.
- `MainAspect` (only used by the `SystemBase` `AudioSystem`/`UISystem`) takes `(ref SystemState state, Entity entity)` and only reads buffers via `EntityManager.GetBuffer`. In a `SystemBase`, pass `ref CheckedStateRef`.
- New code: prefer plain `SystemAPI.Query<RefRW<..>, RefRO<..>>()` / `IJobEntity` with ref params. Don't reintroduce `IAspect` or `Entities.ForEach`.
- If a system schedules a job and then iterates the same components with `SystemAPI.Query` in the same update, call `CompleteDependency()` in between (see `SpriteAnimationSystem`). Without it the query throws a job-safety exception every frame and the rest of `OnUpdate` silently never runs. `Entities.ForEach(...).Run()` used to do this for you.

## Conventions

- Private fields `m_camelCase`, constants `kPascalCase`, public fields `PascalCase`. No namespaces.
- `ISystem` structs with `[BurstCompile]` for gameplay; `SystemBase` only where managed objects are involved (UI, audio, input, sprites).
- Parallel jobs write structural changes through `EntityCommandBuffer.ParallelWriter` with `[EntityIndexInQuery] int sortKey`.
- Source files use **CRLF** line endings (git `core.autocrlf=true`). Keep them when editing. Note that Git Bash `sed -i` strips CRs.
- Cheats (`CHEATS_ENABLED`) exist only in the editor and development builds (see `Game.cs`).

## Working with the editor

- **MCP for Unity**: `.mcp.json` points Claude Code at `http://127.0.0.1:8080/mcp`. In Unity, open *Window > MCP for Unity* and start the server/bridge before starting the Claude session. Useful tools: `refresh_unity` (recompile), `read_console` (compile errors and logs), `manage_scene`, `manage_gameobject`, `run_tests`.
- Both this project and ProtoQuest can be connected to the same MCP server. If tools complain about multiple instances, call `set_active_instance` with `JPacmanUnity@<hash>`.
- `Application.runInBackground` is off, so the player loop doesn't tick while the editor is unfocused (e.g. while you're driving it over MCP). For Play-mode debugging, set `Application.runInBackground = true` via `execute_code` after entering Play mode. It's a runtime-only flag.
- MCP screenshots render through the Main Camera, so the Screen Space - Overlay HUD is missing. To capture menus, temporarily switch the `Hud` canvas in Play mode to `RenderMode.ScreenSpaceCamera` with `worldCamera = Camera.main` and `planeDistance = 1`. It reverts when Play mode exits.
- `execute_code` (CodeDom, C# 6) can't resolve a bare `Object`, so write `UnityEngine.Object.FindAnyObjectByType<T>()`. It also dislikes chained property assignments.
- To simulate controller or keyboard input in Play mode, add virtual devices with `InputSystem.AddDevice<Gamepad>("TestGamepad")`, then queue press and release in separate `execute_code` calls (they need frames in between): `InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.Start))` followed by `new GamepadState()`. For a keyboard, use `new KeyboardState(Key.Escape)`.
- `execute_code` is handy for inspecting ECS state in Play mode (`World.DefaultGameObjectInjectionWorld.EntityManager`, `GetComponentObject<SpriteRenderer>` for companions).
- After script edits, refresh and check the console for errors. Burst and IL post-processing errors only show up in the editor.
- Don't hand-edit `.unity` / `.prefab` / `.asset` YAML unless it's trivial. Use the editor (or MCP). Never edit `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
- Every new asset or script needs its `.meta`. Let Unity generate it (refresh) and commit both.
- **Fallback compile without MCP**: Unity writes the compiler invocation to `Library/Bee/artifacts/<hash>.dag/Assembly-CSharp.rsp`. You can run `Editor/Data/DotNetSdk/dotnet.exe exec .../Roslyn/bincore/csc.dll -nostdlib -noconfig -shared @<rsp>`, with `-out`/`-refout` redirected outside `Library/`, to check C# and Entities source-gen errors while the editor is busy.
- There are no automated tests yet. To verify gameplay changes, enter Play mode in `Assets/Scenes/Game.unity`. To drive the flow over MCP, wait for each phase before triggering the next (the subscene loads a few frames in; `IntroPhase` → `MenuPhase` takes a fade): `UISystem.Instance.OnIntroSkip()` while `IntroPhase` exists, then `UISystem.Instance.OnMenuPlay(startRound, movingCollectibles)` once `MenuPhase` exists (or invoke the menu buttons, e.g. `FindAnyObjectByType<MainMenuEvents>().m_playButton.onClick.Invoke()`). Triggering Play before the menu exists leaves menu and game running together.
- Removing packages while the editor is open can leave a stale folder in `Library/PackageCache` that hangs asset import workers; restart the editor if that happens.
