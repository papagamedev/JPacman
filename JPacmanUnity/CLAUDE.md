# JPacman (Unity) — AI Assistant Guide

Unity port of JPacman, a Pac-Man clone originally written in C++ in 1998–99 (see `../README.md`; the original source lives in `../JPacman1999`, an axmol port in `../External/axmol`). This folder is the Unity project root; the git repo root is the parent folder (main branch: `master`).

## Versions

- Unity **6000.5.8f1** (6.5). `ProjectSettings/ProjectVersion.txt` is the source of truth.
- Entities **6.5** (now a core package), Entities Graphics, Burst, URP 17.5, Input System, uGUI + TextMesh Pro.
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
    Util/           blob builders (*ConfigData), Direction, map helpers, SystemStateExtensions
    Service/        BackendClient: posts/gets high scores from an Azure Functions backend
  ScriptableObjects/  config asset instances (levels, maps, rounds, fruits, menu dot shapes)
  Prefabs/, Textures/, Audio/
```

All code is in the single `Assembly-CSharp` assembly (no asmdefs), global namespace.

## Architecture

- **Pure ECS gameplay.** A singleton `Main` entity (baked by `MainAuthoring`) holds prefabs and `BlobAssetReference` configs (levels, maps, intro, menu). `Game` and `PowerupMode` components are added to the same entity once gameplay starts.
- **Game phases are tag components on the main entity** (`LevelStartPhaseTag`, `LevelPlayingPhaseTag`, `LevelDeadPhaseTag`, `LevelWinPhaseTag`, `LevelClearPhase`, `LevelResetLivePhaseTag`, `LevelGameOverPhaseTag`; also `IntroPhase`, `MenuPhase`). Each has a system that does `RequireForUpdate<Tag>` and uses `ISystemStartStop` for enter/exit logic. Changing phase = remove one tag, add the next, via an `EntityCommandBuffer`.
- **ECS → MonoBehaviour communication goes through dynamic buffers on the main entity** (`SoundEventBufferElement`, `SetScoreTextBufferElement`, `ShowUIBufferElement`, ...). `AudioSystem` and `UISystem` (managed `SystemBase`) drain them each frame and raise C# `Action` events that `AudioEvents` / `UIEvents` / `*MenuEvents` subscribe to via `XxxSystem.Instance`.
- **Rendering** uses companion `SpriteRenderer` components; `SpriteAnimationSystem`, `SpriteColorSystem` and `EnemyColorSystem` push ECS state into them on the main thread.
- Movement is grid-based in "map space" (`MapConfigData.WorldToMapPos` / `MapToWorldPos`); `MovableAspect.Move` handles cell edges, direction changes and tunnel teleports.

### `*Aspect` structs are NOT Entities aspects

`IAspect` and `Entities.ForEach` were removed in Entities 6.x. The former aspects were kept as plain `readonly struct`s holding `RefRW`/`RefRO`/`DynamicBuffer` fields, so their gameplay logic didn't change. Patterns:

- **In jobs**: `Execute` takes the `Entity` and `RefRW<T>`/`RefRO<T>` params, then calls `new XxxAspect(entity, ...).Method(...)`. Zero-size tags the aspect used to filter on are now `[WithAll(typeof(Tag))]` on the job.
- **Singleton/main-entity aspects** (`GameAspect`, `MainAspect`, `PowerupModeAspect`) have a `(ref SystemState state, Entity entity)` constructor: `new GameAspect(ref state, mainEntity)`. In a `SystemBase`, pass `ref CheckedStateRef`. `PlayerAspect.Get(ref state, entity)` works the same way.
- These constructors use `SystemStateExtensions.GetComponentRW/RO`, which completes dependencies first (like `SystemAPI` does). Use them only on the main thread.
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
- `execute_code` is handy for inspecting ECS state in Play mode (`World.DefaultGameObjectInjectionWorld.EntityManager`, `GetComponentObject<SpriteRenderer>` for companions).
- After script edits, refresh and check the console for errors. Burst and IL post-processing errors only show up in the editor.
- Don't hand-edit `.unity` / `.prefab` / `.asset` YAML unless it's trivial. Use the editor (or MCP). Never edit `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
- Every new asset or script needs its `.meta`. Let Unity generate it (refresh) and commit both.
- **Fallback compile without MCP**: Unity writes the compiler invocation to `Library/Bee/artifacts/<hash>.dag/Assembly-CSharp.rsp`. You can run `Editor/Data/DotNetSdk/dotnet.exe exec .../Roslyn/bincore/csc.dll -nostdlib -noconfig -shared @<rsp>`, with `-out`/`-refout` redirected outside `Library/`, to check C# and Entities source-gen errors while the editor is busy.
- There are no automated tests yet. To verify gameplay changes, enter Play mode in `Assets/Scenes/Game.unity`.
