# LunarEngine — Agent Guide

## Quick start
```powershell
dotnet restore
dotnet build -c Debug
# Run from the project directory so resource paths resolve:
cd LunarEngine
dotnet run -c Debug
```
Build from repo root (where `LunarEngine.sln` sits). Run from `LunarEngine/` so `Resources/` paths resolve correctly. There is also a `LunarEngine/LunarEngine.sln` inside the project — use the root one.

## Project structure
Single `.csproj` at `LunarEngine/LunarEngine.csproj`. Targets `net9.0` (fallback from .NET 10.0 prerelease SDK — `global.json` temporarily rolled to `9.0.0` due to environment constraints). No test project.

Top-level dirs under `LunarEngine/`:
- `Core/` — `Application`, `BaseLayer`, `LayerStack`, `TimeStep`, `ServiceContainer`, `Logger`
- `Editor/` — `Editor` (app entry), `EditorLayer`, `GizmosLayer`, editor systems, plus `Component Inspectors/`
- `Engine/` — Assets, ECS, GameEngine, Gizmos, InputEngine, Physics, Renderer, Scenes, UI, AssetHandleCache, Debugging, Core
- `Events/` — Global `EventBus<T>`, events
- `Utilities/` — `VectorExtensions`

Empty placeholder dirs indicating planned but unimplemented features:
- `Engine/ECS/Scheduling/` — no ECS scheduler yet
- `Engine/Renderer/Abstractions/` — no `IRenderer`/`IRenderDevice` interfaces yet

## Entry point
`LunarEngine/Engine/GameEngine/Program.cs` → `new EngineHost().Run()` → `new Editor()` → `app.Run()`

`Application.Run()` calls `CreateWindow()` which creates a Silk.NET `IWindow` and subscribes to its events. **All service init happens in `OnWindowLoad`**: acquires GL context, creates `Input`, `Renderer`, `AssetManager`, `SceneManager`, then calls virtual `Initialize()`. `Editor.Initialize()` pushes `ImGuiLayer` as overlay, then `EditorLayer` and `GizmosLayer` as layers. Layers get `OnInitialize()` called after that.

## Architecture & key types
- **Layer stack**: `Application` owns a `LayerStack`. Layers iterate in order for `OnUpdate` then `OnImguiRender`. `ImGuiLayer` wraps layer rendering in `Begin`/`End`.
- **ECS**: [Arch](https://github.com/genemake/Arch) v1.2.8. Systems extend `ScriptableSystem` (partial class extending `BaseSystem<World, double>`). Source-generated queries via `[Query]` attributes (`Arch.System.SourceGenerator`). Each `ECSScene` owns a `World` and manually orders systems in `Awake()`, `Start()`, `Update()`, `Tick()` — no scheduler.
- **Renderer**: OpenGL-only. `Renderer` holds a `GL` instance. Systems call `_renderer.SubmitRenderCommand()`. ECS systems receive `GL` directly in constructors (e.g., `SpriteRendererSystem(GL, World, AssetManager, Renderer)`).
- **ImGui**: `Hexa.NET.ImGui` (not Silk.NET's ImGui extension).
- **Rendering flow**: `ECSScene.RenderScenes()` called by editor systems; finds primary `CameraComponent`, calls `renderer.BeginFrame(camera.ViewProjection)`, then `SpriteRendererSystem.Render()`, then `renderer.EndFrame()`.
- **Assets**: `ShaderLibrary` and `TextureLibrary` load from `Resources\` relative to the working directory. Resources are copied to the build output directory via the csproj. Default assets are created in `#region TEST` blocks.

## Conventions
- Components are **structs** implementing the empty `IComponent` marker interface (no base class). They are Arch-style value types, NOT the old `GameObject`-based `Component` class (that system is fully commented out in `Scene.cs` and `CustomBehaviour.cs`).
- ECS systems **must** be `partial class` — required by Arch source generators for `[Query]` methods.
- ECS system lifecycle on `ECSScene`: `Awake()` → `Start()` → `Update(dt)` / `Tick(dt)` → `AfterUpdate()` (plays back `CommandBuffer`). `RenderScenes()` is separate, called explicitly by editor systems.
- New scenes extend `ECSScene` and add entities via `World.Create<T1, T2, ...>(...)` in the constructor.
- Active scene: `SceneManager.ActiveScenes` (singular property, despite the name).
- Namespaces are inconsistent across the codebase:
  - Components: `LunarEngine.Components`, `LunarEngine.GameObjects`, `LunarEngine.Engine.ECS.Components`, `LunarEngine.ECS.Components`
  - Systems: `LunarEngine.GameEngine`, `LunarEngine.ECS.Systems`, `LunarEngine.Physics`, `LunarEngine.Engine.ECS.Systems`
  - Renderer: `LunarEngine.Engine.Graphics`
  - Core: `LunarEngine.GameEngine`

## Toolchain quirks
- `AllowUnsafeBlocks` enabled (required by Silk.NET interop — pointer usage in rendering).
- `ImplicitUsings` on; `Nullable` enabled.
- Shaders/textures use relative paths (`Resources\`) — must run from `LunarEngine/` directory or ensure `Resources/` is in the working directory.
- `Serilog` referenced but minimally used (mostly `Log.Error` in asset libraries).
- Arch source generators require `partial` class on query-bearing systems.
- Two `.sln` files exist — use the root `LunarEngine.sln`.

## Known design gaps (see `LunarEngine/ENGINE_DESIGN_REVIEW.md` for full detail)
- Renderer has no abstraction; ECS systems receive `GL` directly
- Heavy use of global singletons (`EventBus<T>` still static; `Singleton<T>` and `Gizmos.Instance` removed in Phase 2)
- Fixed-size scene array (max 16), incomplete remove logic in `SceneManager`
- `Parent` component stores a `Transform` instead of an `Entity` reference
- `Scene.cs`, `CustomBehaviour.cs`, `WindowManager.cs`, `TestScene.cs`, `PhysicsEngine.cs`, `ShaderSystem.cs`, `InputSystem.cs`, `ComponentFactoryManager.cs`, `PhysicsLayer.cs`, `RenderLayer.cs`, `SceneLayer.cs` removed in Phase 1 hygiene pass
- Editor and runtime share the same `World` — no isolation

## Commands cheat sheet
| Task | Command |
|------|---------|
| Build debug | `dotnet build -c Debug` |
| Run | `dotnet run -c Debug` |
| Build release | `dotnet build -c Release` |