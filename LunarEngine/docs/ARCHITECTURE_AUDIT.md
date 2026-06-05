# LunarEngine Architecture Audit — Current Code vs. Target Architecture

> **Date**: 2026-06-05  
> **Scope**: Full codebase review against `LunarEngine/docs/ARCHITECTURE.md`  
> **Status**: Conflicts identified; no code changes applied yet.

---

## 1. Dependency Injection

| Target | Current |
|--------|---------|
| `ServiceContainer` with interface registration | `Singleton<T>` base class |
| No `.Instance` accessors | `Gizmos.Instance`, `ComponentFactoryManager.Instance` |
| Services resolved from container | Services are public properties on `Application` |

**Files**: `Core/Singleton.cs`, `Engine/Renderer/Gizmos.cs:8`, `Engine/ECS/ComponentFactories/ComponentFactoryManager.cs:18`, `Core/Application.cs`.

**Conflict**: High.

---

## 2. Platform Abstraction

| Target | Current |
|--------|---------|
| `IWindow`, `IInputContext` in `Platform/` | `Platform/` created with `IWindow`, `IInputContext`, `SilkWindow`, `SilkInputContext` |
| `SilkWindow` / `SilkInputContext` wrappers | Wrappers created; `SilkInputContext` created in `OnWindowLoad` because `CreateInput()` requires initialized window |
| `Application` receives `IWindow` via DI | `Application` constructor creates `SilkWindow`, registers `IWindow` in `ServiceContainer` |

**Files**: `Platform/IWindow.cs`, `Platform/IInputContext.cs`, `Platform/SilkWindow.cs`, `Platform/SilkInputContext.cs`, `Core/Application.cs`.

**Conflict**: Resolved for Phase 3. `ImGuiController` still uses raw Silk.NET `IView`/`IInputContext` (will be addressed in Phase 4 with `IRenderDevice`).

---

## 3. ECS Abstraction

| Target | Current |
|--------|---------|
| `IWorld`, `ISystem`, `SystemStage`, `SystemScheduler` | `ScriptableSystem` extends `BaseSystem<World, double>` |
| Systems declare `Stage`/`Order` | `ECSScene` manually calls `Awake`/`Start`/`Update`/`Tick` in hardcoded order |
| `CommandBuffer` from `IWorld` | `new CommandBuffer()` every frame in multiple systems |

**Allocations per frame**:
- `TransformSystem` (`Engine/ECS/Systems/TransformSystem.cs:18`)
- `SpriteRendererSystem` (`Engine/ECS/Systems/SpriteRendererSystem.cs:36,43`)
- `PhysicsSystem` (`Engine/ECS/Systems/PhysicsSystem.cs:26,33`)
- `GizmosSystem` (`Engine/ECS/Systems/GizmosSystem.cs:21`)

**Files**: `Engine/ECS/Systems/LunarSystem.cs`, `Engine/Scenes/ECSScene.cs:48–99`.

**Conflict**: High.

---

## 4. Components — Namespaces & Design

| Target | Current |
|--------|---------|
| Unified under `LunarEngine.ECS.Components` | Scattered across `LunarEngine.Components`, `LunarEngine.GameObjects`, `LunarEngine.Engine.ECS.Components`, `LunarEngine.ECS.Components`, `LunarEngine.Physics` |
| `Parent` stores `EntityReference` | `Parent` stores a `Transform` copy (`Engine/ECS/Components/Transform.cs:80–82`) |
| `Transform` component removed; `LocalToWorld` derived | `Transform` component (`Matrix4x4`) still exists |
| `DirtyTransform` removed | `DirtyTransform` tag still exists |
| `LGShader`, `TagComponent`, `Input` removed | All still present in code |

**Files**: `Engine/ECS/Components/Transform.cs`, `Engine/ECS/Components/SpriteRenderer.cs`, `Engine/ECS/Components/Input.cs`, `Engine/ECS/Systems/ShaderSystem.cs`.

**Conflict**: High.

---

## 5. Renderer Abstraction

| Target | Current |
|--------|---------|
| `IRenderDevice` + `IRenderer` interfaces | No abstraction; `Renderer` exposes `public GL Api` |
| ECS systems submit `SpriteDrawCommand` | `SpriteRendererSystem` takes `GL` in constructor and creates `Sprite` objects directly |
| `Gizmos` as injected service | `Gizmos` is `Singleton<Gizmos>`; `Renderer.Initialize()` calls `Gizmos.Instance.InitializeGizmos(Api)` |

**Files**: `Engine/Renderer/Renderer.cs`, `Engine/ECS/Systems/SpriteRendererSystem.cs`, `Engine/Renderer/Gizmos.cs`.

**Conflict**: High.

---

## 6. Events

| Target | Current |
|--------|---------|
| Instance-based `EventBus` owned by `Application`/`IWorld` | Static generic `EventBus<T>` in `Events/EventBus.cs` |
| No Arch `[Event]` attributes | `EditorCameraInputHandler` uses `Arch.Bus.[Event]` (`Editor/Editor.cs:112,128`) |
| Explicit subscribe/unsubscribe | `SceneSystem` / `HierarchySystem` use `Arch.Bus.EventBus.Send()` |

**Files**: `Events/EventBus.cs`, `Editor/Editor.cs`, `Editor/SceneSystem.cs`, `Editor/HierarchySystem.cs`.

**Conflict**: Medium-High.

---

## 7. Input

| Target | Current |
|--------|---------|
| `InputManager` + read-only `InputState` snapshot | `Input` class created directly in `Application.OnWindowLoad()` |
| Configurable axis mappings | WASD hardcoded in `Input` constructor (`Engine/InputEngine/Input.cs:29–38`) |
| Read from `ServiceContainer` | `EditorCameraInputHandler` receives concrete `Input` object |

**Files**: `Engine/InputEngine/Input.cs`, `Core/Application.cs:111–124`, `Editor/Editor.cs`.

**Conflict**: High.

---

## 8. Assets

| Target | Current |
|--------|---------|
| `IAssetProvider.ResolvePath(AssetKey)` | No `IAssetProvider` or `AssetKey` |
| Configurable `FileSystemAssetProvider` | Hardcoded `@"..\..\..\Resources\"` in `ShaderLibrary` and `TextureLibrary` |
| `AssetManager` uses `IRenderDevice` | `AssetManager.Initialize(GL gl)` takes raw `GL` |
| Proper GPU disposal | `AssetManager.Dispose()` is empty; `AssetHandleCache.ClearCache()` is empty |

**Files**: `Engine/Assets/ShaderLibrary.cs:43–46`, `Engine/Assets/TextureLibrary.cs:42–45`, `Engine/Assets/AssetManager.cs:31–34`, `Engine/AssetHandleCache/AssetHandleCache.cs:18–20`.

**Conflict**: High.

---

## 9. Scenes

| Target | Current |
|--------|---------|
| `List<IScene>` | Fixed-size array `ECSScene?[] _scenes = new ECSScene[16]` |
| `IScene` interface | No `IScene` interface |
| `ActiveScene` (singular) | `ActiveScenes` (misleading plural) |
| Proper `RemoveScene` cleanup | `RemoveScene` sets slot to `null` with a TODO, no shift/cleanup |

**Files**: `Engine/Scenes/SceneManager.cs:9–36`.

**Conflict**: High.

---

## 10. Physics

| Target | Current |
|--------|---------|
| `PhysicsWorld` instance, `SystemStage.FixedUpdate` | `PhysicsEngine` static class with empty `TickPhysics()` |
| Broadphase spatial hash | O(n²) AABB collision in `PhysicsSystem.CheckCollisions()` |
| Interpolation built-in | Interpolation relies on `PhysicsEngine.InterpolatedTime` static field |

**Files**: `Engine/Physics/PhysicsEngine.cs`, `Engine/ECS/Systems/PhysicsSystem.cs:125–151`.

**Conflict**: High.

---

## 11. Editor / Runtime Separation

| Target | Current |
|--------|---------|
| Editor uses separate `IWorld` | Editor operates on the same `ECSScene.World` as the game |
| `EditorCamera` reads `InputState` from DI | `EditorCamera` uses static `Time.DeltaTime`; `EditorCameraInputHandler` uses concrete `Input` |
| Editor systems in editor scheduler | `HierarchySystem`, `InspectorSystem` are `ScriptableSystem` on the game world |

**Files**: `Editor/Editor.cs:55–63`, `Editor/Editor.cs:97–218`.

**Conflict**: High.

---

## 12. Time

| Target | Current |
|--------|---------|
| `TimeStep` passed through update chain | Static `Time.DeltaTime` set in `Application.OnUpdate()` |
| No global `Time` class | `Time` static class exists (`Core/Application.cs:15–18`) |

**Files**: `Core/Application.cs:146`, `Editor/Editor.cs:146,152`.

**Conflict**: Medium.

---

## 13. Dead Code Still Present

Files listed for removal in the architecture document that still exist:

- `Engine/Scenes/Scene.cs` — entirely commented out
- `Engine/ECS/Components/CustomBehaviour.cs` — entirely commented out
- `Core/WindowManager.cs` — entirely commented out
- `Events/WindowInitializedEvent.cs` — commented out
- `Events/ViewportResizedEvent.cs` — exists (empty file)
- `Core/PhysicsLayer.cs` — unused
- `Core/RenderLayer.cs` — unused
- `Core/SceneLayer.cs` — unused
- `Engine/Physics/PhysicsEngine.cs` — static no-op
- `Engine/Scenes/TestScene.cs` — commented out
- `Engine/ECS/Systems/ShaderSystem.cs` — empty system + unrelated structs
- `Engine/ECS/Systems/InputSystem.cs` — stub
- `Engine/ECS/ComponentFactories/ComponentFactoryManager.cs` — functionality moves to `IWorld.Create`
- `Core/Singleton.cs` — replaced by `ServiceContainer`

---

## 14. Confirmed Bugs

| # | Bug | Evidence |
|---|-----|----------|
| 1 | `InspectorSystem.AddComponentInspector<T>` inverted `TryAdd` logic | `Editor/InspectorSystem.cs:39` logs error on success and skips adding |
| 2 | `EditorCameraInputHandler.OnSceneLoseFocus` uses `+=` instead of `-=` | `Editor/Editor.cs:167–168` events never unsubscribe |
| 3 | Duplicate `_camera.LookAt` call | `Editor/Editor.cs:154–155` identical calls |
| 4 | `AssetManager.Dispose()` is empty | `Engine/Assets/AssetManager.cs:31–34` |
| 5 | `AssetHandleCache.ClearCache()` is empty | `Engine/AssetHandleCache/AssetHandleCache.cs:18–20` |
| 6 | `_canZoom` field never set to `true` | `Editor/Editor.cs:102` declared but never assigned |
| 7 | `SceneSystem` accesses `_sceneFrameBuffer._colorTexture` and `_size` | `Editor/SceneSystem.cs:66` breaks encapsulation |
| 8 | `Time.DeltaTime` set twice per frame | `Core/Application.cs:146` sets it; redundant sets also in layer updates |
| 9 | `SpriteRendererSystem.AdjustScale` sets random shader uniform every frame | `Engine/ECS/Systems/SpriteRendererSystem.cs:69` debug code in production |
| 10 | `CommandBuffer` newed every frame in multiple systems | Confirmed in `TransformSystem`, `SpriteRendererSystem`, `PhysicsSystem`, `GizmosSystem` |

---

## 15. Namespace Map Violations

| Current Namespace | Target Namespace | Example Files |
|-------------------|------------------|---------------|
| `LunarEngine.GameEngine` | `LunarEngine.Core` / `LunarEngine.Application` / `LunarEngine.ECS` | `Core/Application.cs`, `Core/Singleton.cs` |
| `LunarEngine.GameObjects` | `LunarEngine.ECS.Components` | `Engine/ECS/Components/SpriteRenderer.cs`, `Engine/ECS/Components/CameraComponent.cs` |
| `LunarEngine.Components` | `LunarEngine.ECS.Components` | `Engine/ECS/Components/Transform.cs` |
| `LunarEngine.Engine.ECS.Components` | `LunarEngine.ECS.Components` | `Engine/ECS/Components/IComponent.cs`, `Engine/ECS/Components/BoxCollider2D.cs` |
| `LunarEngine.Engine.ECS.Systems` | `LunarEngine.ECS.Systems` | (would apply after abstraction) |
| `LunarEngine.Physics` | `LunarEngine.ECS.Components.Physics` / `LunarEngine.ECS.Systems` | `Engine/ECS/Components/Rigidbody2D.cs`, `Engine/ECS/Systems/PhysicsSystem.cs` |
| `LunarEngine.Engine.Graphics` | `LunarEngine.Renderer` / `LunarEngine.Renderer.OpenGL` | `Engine/Renderer/Renderer.cs`, `Engine/Renderer/Gizmos.cs` |
| `LunarEngine.Engine.AssetHandleCache` | `LunarEngine.Assets` | `Engine/AssetHandleCache/AssetHandleCache.cs` |
| `LunarEngine.InputEngine` | `LunarEngine.Input` | `Engine/InputEngine/Input.cs` |
| `LunarEngine.ECS.Systems` (Editor) | `LunarEngine.Editor` / `LunarEngine.Editor.Systems` | `Editor/Editor.cs`, `Editor/InspectorSystem.cs`, `Editor/HierarchySystem.cs`, `Editor/SceneSystem.cs` |

---

## Phase 1 Completion Notes (2026-06-05)

All 10 bugs fixed, dead code removed, usings normalized, build + run verified.

**Additional issues discovered and fixed during Phase 1**:
1. **Missing scene lifecycle calls in EditorLayer** — `_scene.Awake()`, `_scene.Start()`, `_scene.Update()` were never called in the editor path; SceneLayer was the intended caller but was dead code. Added explicit lifecycle calls to `EditorLayer`.
2. **Broken resource path resolution** — `dotnet run` working directory is the project root, but hardcoded `..\..\..\Resources\` paths assumed the exe's output directory. Changed paths to `Resources\` and added `CopyToOutputDirectory` in csproj; `dotnet run` must execute from `LunarEngine/` directory.
3. **Editor.cs accessed private Application fields** — `Window`, `GL`, `InputContext` were private fields on `Application`; `Editor` tried to access them as if they were protected properties. Made the fields `protected` on `Application` so `Editor` can access them.
4. **LayerStack.InvokeEvent did not exist** — `Application.OnViewportResize` called `_layerStack.InvokeEvent()` which was never defined. Replaced with a no-op + TODO comment.

---

## Phase 2 Completion Notes (2026-06-05)

Core module extraction completed. Build + run verified.

**Changes made**:
1. **Created `ServiceContainer` and `ServiceDescriptor`** in `Core/` with `Register<T>`, `RegisterLazy<T>`, `Get<T>`, `TryGet<T>`, and `Reset()` APIs.
2. **Removed `Singleton<T>` and `ISingletonObject`** — `Core/Singleton.cs` deleted. `Gizmos` was the only consumer; refactored to a regular class with constructor injection.
3. **Refactored `Gizmos`** — Now accepts `GL` and `AssetManager` in constructor. `Renderer` receives `Gizmos` via constructor instead of calling `Gizmos.Instance`.
4. **Updated `Application`** — Initializes `ServiceContainer` in constructor, registers `AssetManager`, `Gizmos`, `Renderer`, `Input`, `SceneManager`, `Window`, and `InputContext` in `OnWindowLoad()`.
5. **Made `EventBus<T>` instance-based** — Converted from `static class` to `sealed class` with instance-level `Subscribe`/`Unsubscribe`/`Publish`/`Clear` methods. (Note: no current code consumes this yet; Arch.EventBus is still used by editor systems and will be replaced in Phase 10.)
6. **Moved `Logger` to `Core/`** — `Engine/Debugging/Logger.cs` moved to `Core/Logger.cs` under `LunarEngine.Core` namespace. `Logger.Initialize()` is now called in `Application.OnWindowLoad()`. `DebugUtils` remains in `Engine/Debugging/` and continues using `Serilog.Log` directly.
7. **Temporary SDK downgrade** — `global.json` and `LunarEngine.csproj` rolled from `10.0.0`/`net10.0` to `9.0.0`/`net9.0` because the .NET 10 prerelease SDK is not installed in the current environment. `AGENTS.md` updated to reflect this.

---

## Phase 3 Completion Notes (2026-06-05)

Platform abstraction completed. Build + run verified.

**Changes made**:
1. **Created `Platform/` directory** with `IWindow`, `IInputContext`, `SilkWindow`, and `SilkInputContext`.
2. **`IWindow` abstraction** — `Size`, `FramebufferSize`, `Title`, `OnLoad`, `OnUpdate`, `OnResize`, `OnClosing`, `Run()`, `Close()`.
3. **`IInputContext` abstraction** — `Keyboards` and `Mice` (still returning Silk.NET `IKeyboard`/`IMouse` for now, to be abstracted further in Phase 6).
4. **`SilkWindow`** — Wraps `Silk.NET.Windowing.IWindow`, wires events through to `IWindow` events. Exposes `NativeWindow` internally for consumers still needing the raw Silk.NET window (e.g. `ImGuiController` until Phase 4).
5. **`SilkInputContext`** — Wraps `Silk.NET.Input.IInputContext`. Created in `OnWindowLoad()` because `CreateInput()` requires the window to be initialized. Exposes `NativeContext` internally for `ImGuiController`.
6. **Refactored `Application`** — Constructor creates `SilkWindow` and registers `IWindow` in `ServiceContainer`. Removed direct `Silk.NET.Windowing` dependency. `OnWindowLoad()` creates `SilkInputContext`, registers `IInputContext`, and gets `GL` via `SilkWindow.NativeWindow`.
7. **Updated `ImGuiLayer`** — Now accepts `LunarEngine.Platform.IWindow` and `IInputContext`. Casts to `SilkWindow`/`SilkInputContext` in `OnInitialize()` to pass underlying types to `ImGuiController`.
8. **Updated `Input`** — `InputContext` property now uses `LunarEngine.Platform.IInputContext`.
9. **Removed `IsRunning` from `IWindow`** — `Silk.NET.Windowing.IWindow` does not expose this property; it was unused in the codebase. Will revisit if needed in later phases.

---

## Summary

The current codebase is a functional prototype that directly uses Silk.NET, Arch ECS, and raw OpenGL with minimal abstraction. The target architecture defines a fully modular, dependency-injected, interface-driven engine. **Conflicts are pervasive across every module**: Core, Platform, ECS, Renderer, Assets, Input, Scenes, Physics, and Editor all deviate substantially from the target design. The 12-phase implementation plan in `ARCHITECTURE.md` is well-justified given the breadth of changes required.
