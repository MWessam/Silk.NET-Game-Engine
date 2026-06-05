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
- `Platform/` — `IWindow`, `IInputContext`, `SilkWindow`, `SilkInputContext`
- `Utilities/` — `VectorExtensions`

Empty placeholder dirs indicating planned but unimplemented features:
- `Engine/ECS/Scheduling/` — no ECS scheduler yet
- `Engine/Renderer/Abstractions/` — no `IRenderer`/`IRenderDevice` interfaces yet

## Entry point
`LunarEngine/Engine/GameEngine/Program.cs` → `new EngineHost().Run()` → `new Editor()` → `app.Run()`

`Application` constructor creates a `SilkWindow` and registers `IWindow` in `ServiceContainer`. `Application.Run()` wires to the `IWindow` events and calls `Run()`. **All service init happens in `OnWindowLoad`**: acquires GL context via `SilkWindow.NativeWindow`, creates `SilkInputContext`, then `Input`, `Renderer`, `AssetManager`, `SceneManager`, then calls virtual `Initialize()`. `Editor.Initialize()` pushes `ImGuiLayer` as overlay, then `EditorLayer` and `GizmosLayer` as layers. Layers get `OnInitialize()` called after that.

## Architecture & key types
- **Layer stack**: `Application` owns a `LayerStack`. Layers iterate in order for `OnUpdate` then `OnImguiRender`. `ImGuiLayer` wraps layer rendering in `Begin`/`End`.
- **ECS**: [Arch](https://github.com/genemake/Arch) v1.2.8. Systems extend `ScriptableSystem` (partial class extending `BaseSystem<World, double>`). Source-generated queries via `[Query]` attributes (`Arch.System.SourceGenerator`). Each `ECSScene` owns a `World` and delegates lifecycle to `SystemScheduler` (`Awake`, `Start`, `Update`, `FixedUpdate`, `RenderPrepare`).
- **Renderer**: OpenGL-only via `GLRenderDevice`. `IRenderer` exposes `SubmitRenderCommand()`. ECS systems use `IRenderer` for command submission; no direct `GL` references in systems.
- **ImGui**: `Hexa.NET.ImGui` (not Silk.NET's ImGui extension).
- **Rendering flow**: `ECSScene.RenderScenes()` called by editor systems; finds primary `CameraComponent`, calls `renderer.BeginFrame(camera.ViewProjection)`, then `SpriteRendererSystem.Render()`, then `renderer.EndFrame()`.
- **Assets**: `ShaderLibrary` and `TextureLibrary` load from `Resources\` relative to the working directory. Resources are copied to the build output directory via the csproj. Default assets are created in `#region TEST` blocks.

## Conventions
- Components are **structs** implementing the empty `IComponent` marker interface (no base class). They are Arch-style value types, NOT the old `GameObject`-based `Component` class (that system is fully commented out in `Scene.cs` and `CustomBehaviour.cs`).
- ECS systems **must** be `partial class` — required by Arch source generators for `[Query]` methods.
- ECS system lifecycle on `ECSScene`: `Awake()` → `Start()` → `Update(dt)` / `FixedUpdate(dt)` → `AfterUpdate()` (plays back `CommandBuffer`). `RenderScenes()` is separate, called explicitly by editor systems.
- New scenes extend `ECSScene` and add entities via `World.Create<T1, T2, ...>(...)` in the constructor. `ECSScene` receives its dependencies through `ServiceContainer`.
- Active scene: `SceneManager.ActiveScene` (returns `IScene?`).
- Namespaces are consolidated per the target architecture (completed in Phase 11):
  - Components: `LunarEngine.ECS.Components` / `LunarEngine.ECS.Components.Physics`
  - Systems: `LunarEngine.ECS.Systems`
  - Renderer: `LunarEngine.Renderer` / `LunarEngine.Renderer.OpenGL`
  - Core: `LunarEngine.Core`
  - Application: `LunarEngine.Application`
  - Editor: `LunarEngine.Editor` / `LunarEngine.Editor.Systems`

## Toolchain quirks
- `AllowUnsafeBlocks` enabled (required by Silk.NET interop — pointer usage in rendering).
- `ImplicitUsings` on; `Nullable` enabled.
- Shaders/textures use relative paths (`Resources\`) — must run from `LunarEngine/` directory or ensure `Resources/` is in the working directory.
- `Serilog` referenced but minimally used (mostly `Log.Error` in asset libraries).
- Arch source generators require `partial` class on query-bearing systems.
- Two `.sln` files exist — use the root `LunarEngine.sln`.

## Known design gaps (see `LunarEngine/ENGINE_DESIGN_REVIEW.md` for full detail)
- `Parent` component stores a `Transform` instead of an `Entity` reference
- `Scene.cs`, `CustomBehaviour.cs`, `WindowManager.cs`, `TestScene.cs`, `PhysicsEngine.cs`, `ShaderSystem.cs`, `InputSystem.cs`, `ComponentFactoryManager.cs`, `PhysicsLayer.cs`, `RenderLayer.cs`, `SceneLayer.cs` removed in Phase 1 hygiene pass
- Editor and runtime share the same `World` — no isolation

## Commands cheat sheet
| Task | Command |
|------|---------|
| Build debug | `dotnet build -c Debug` |
| Run | `dotnet run -c Debug` |
| Build release | `dotnet build -c Release` |