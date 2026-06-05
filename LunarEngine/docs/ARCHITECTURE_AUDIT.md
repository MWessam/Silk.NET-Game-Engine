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

## 7. Input (Resolved in Phase 6)

| Target | Current |
|--------|---------|
| `InputManager` + read-only `InputState` snapshot | `InputManager` created in `Application.OnWindowLoad()` |
| Configurable axis mappings | WASD configured as "Movement" axis mapping in `Application` |
| Read from `ServiceContainer` | `EditorCameraInputHandler` receives `InputManager` via DI |

**Files**: `Input/InputManager.cs`, `Input/InputState.cs`, `Core/Application.cs`, `Editor/Editor.cs`.

**Conflict**: Resolved.

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

## Phase 4 Completion Notes (2026-06-05)

Renderer abstraction completed. Build + run verified.

**Changes made**:
1. **Created renderer abstraction interfaces** in `Renderer/Abstractions/`:
   - `IBuffer`, `IVertexArray`, `ITexture2D`, `IShader`, `IFrameBuffer`, `IRenderDevice`, `IRenderer`
   - `BufferLayout` and `ElementType` extracted to top-level types
2. **Created `GLRenderDevice`** in `Renderer/OpenGL/` — wraps raw `GL` and implements `IRenderDevice`. Exposes `.GL` property for `ImGuiController` which still needs raw GL.
3. **Made OpenGL types implement interfaces**:
   - `BufferObject<T>` → `IBuffer`
   - `VertexArrayObject<TVertex, TIndex>` → `IVertexArray`
   - `TextureHandle` → `ITexture2D` (added `NativeHandle` for ImGui)
   - `ShaderHandle` → `IShader`
   - `FrameBuffer` → `IFrameBuffer` (added `ColorTextureAttachment` inner class)
4. **Refactored `Sprite`, `Quad`, `Gizmos`** to use `IRenderDevice` and abstraction types instead of raw `GL`.
5. **Refactored `Renderer`** to implement `IRenderer`, use `IRenderDevice`, and expose `Quad` and `CreateSprite()` factory.
6. **Refactored `AssetHandleCache` and `AssetManager`** to use `IRenderDevice` instead of raw `GL`. Return `ITexture2D`/`IShader` from getters.
7. **Removed direct `GL` from ECS systems**:
   - `SpriteRendererSystem` no longer takes `GL` in constructor; uses `IRenderer` and `AssetManager`
   - `GizmosSystem` uses `IRenderer` instead of concrete `Renderer`
   - `ECSScene` uses `IRenderer`
8. **Refactored Editor code** to use `IRenderer` and `IRenderDevice`:
   - `Editor`, `EditorLayer`, `SceneSystem`, `GizmosLayer` all use `IRenderer`
   - `ImGuiLayer` takes `IRenderDevice` and casts to `GLRenderDevice` for `ImGuiController`
9. **Removed unused `using Silk.NET.OpenGL;`** from `ShaderLibrary`, `TextureLibrary`, `BaseAssetLibrary`, `UIEngine`, `SpriteRenderer` component, `ShaderAsset`, `TextureAsset`, `SpriteRendererInspector`.

**Remaining GL references** (expected, to be addressed in later phases):
- `ImGuiController` and `ImGuiTexture`/`ImGuiShader` in `Engine/UI/` — deeply tied to raw GL; will be abstracted when `IRenderDevice` is extended or replaced in Phase 6/9.
- `Application.GL` field — still acquired for `GLRenderDevice` creation; only internal to `Application`.
- OpenGL implementation details (`BufferObject`, `VertexArrayObject`, `ShaderHandle`, `TextureHandle`, `FrameBuffer`) still use `GL` internally, which is correct as they are OpenGL-specific implementations.

---

## Phase 6 Completion Notes (2026-06-05)

Input state resource abstraction completed. Build + run verified.

**Changes made**:
1. **Created `Input/` directory** with `InputState.cs` and `InputManager.cs` under `LunarEngine.Input` namespace.
2. **`InputState`** — `readonly struct` with `IsKeyDown`, `IsKeyPressed`, `IsKeyReleased`, `MousePosition`, `MouseDelta`, `ScrollDelta`, and `GetAxis(string name)`.
3. **`AxisMapping`** — Configurable axis mapping class with `PositiveX`, `NegativeX`, `PositiveY`, `NegativeY` keys. `InputManager.AddAxis(name, mapping)` registers axes.
4. **`InputManager`** — Receives `LunarEngine.Platform.IInputContext` in constructor, registers keyboard/mouse callbacks, exposes `State` snapshot, and events (`KeyDown`, `KeyUp`, `MouseDown`, `MouseUp`, `MouseMoved`, `MouseScrolled`). `Update()` is called once per frame in `Application.OnUpdate()` to refresh the snapshot. `SetCursorLock(bool)` replaces the old `LockAndHideCursor`.
5. **Removed old `Input` class** — `Engine/InputEngine/Input.cs` deleted. `Engine/ECS/Components/Input.cs` (unused ECS component) also deleted.
6. **Updated `Application`** — Creates `InputManager`, registers "Movement" axis with WASD mappings, registers it in `ServiceContainer`, and calls `Update()` each frame.
7. **Updated `Editor` and `EditorCameraInputHandler`** — Now consumes `InputManager` instead of `Input`. `EditorCameraInputHandler` subscribes to `InputManager` events (`MouseDown`, `MouseUp`, `KeyDown`, `MouseMoved`, `MouseScrolled`). Keyboard axis movement is now read from `InputState.GetAxis("Movement")` in a new `Update(InputState)` method called from `EditorLayer.OnUpdate()`.
8. **No hardcoded WASD** — WASD mappings moved to the composition root (`Application.OnWindowLoad`) as a configurable axis mapping rather than being baked into the input class constructor.

---

## Phase 7 Completion Notes (2026-06-05)

Asset provider abstraction completed. Build + run verified.

**Changes made**:
1. **Created `IAssetProvider` and `AssetKey`** in `Assets/` under `LunarEngine.Assets` namespace. `AssetKey` is a `readonly struct` with `Category` and `Name`. `IAssetProvider` exposes `ResolvePath(AssetKey)`, `OpenStream(AssetKey)`, and `Exists(AssetKey)`.
2. **Created `FileSystemAssetProvider`** — Resolves `AssetKey` to paths under a configurable root directory (`Resources/`). Uses `key.Name` as the filename directly (category is ignored for filesystem resolution, but can be used by other providers).
3. **Added `IAssetProvider` support to `BaseAssetLibrary`** — Added `AssetProvider` property and `WithProvider(IAssetProvider)` builder method so libraries can resolve paths during default asset creation.
4. **Updated `ShaderLibrary`** — `BasicShader()` now uses `AssetProvider.ResolvePath(new AssetKey("shader", "shader.vert"))` and `AssetProvider.ResolvePath(new AssetKey("shader", "shader.frag"))` instead of hardcoded `Resources\shader.vert`/`Resources\shader.frag`. Fixed `DefaultAsset` getter to not self-add the asset (preventing duplicate-add errors when `Builder.Build()` also adds it).
5. **Updated `TextureLibrary`** — `BirbTexture()` now uses `AssetProvider.ResolvePath(new AssetKey("texture", "birb.jpg"))` instead of hardcoded `Resources\birb.jpg`. Fixed `DefaultAsset` getter to not self-add the asset.
6. **Updated `AssetManager`** — Constructor now takes `(IAssetProvider provider, IRenderDevice device)` and passes the provider to the library builders. `Initialize()` method removed. `Dispose()` calls `_handleCache?.ClearCache()` which properly disposes all GPU handles (already fixed in Phase 1, verified working).
7. **Updated `Application`** — Creates `FileSystemAssetProvider("Resources")`, registers it in `ServiceContainer`, and passes it to `AssetManager` constructor.

---

## Phase 9 Completion Notes (2026-06-05)

Physics consolidation completed. Build + run verified.

**Changes made**:
1. **Created `PhysicsWorld`** in `Engine/Physics/PhysicsWorld.cs` — non-static instance class owning `FixedTimeStep`, `Gravity`, and fixed-step time accumulation via `Step(World, deltaTime)`.
2. **Moved all physics simulation logic** from `PhysicsSystem` into `PhysicsWorld`: rigidbody integration (velocity Verlet), AABB position updates, collision detection, and collision resolution.
3. **Added spatial hash broadphase** — `PhysicsWorld` builds a uniform grid (`Dictionary<(int, int), List<Entity>>`) each fixed step. Dynamic colliders only check against entities in overlapping cells, replacing the previous O(n²) all-pairs check.
4. **Refactored `PhysicsSystem`** — changed `Stage` from `SystemStage.Update` to `SystemStage.FixedUpdate`. `Tick()` now delegates to `PhysicsWorld.Step()` after running initialization queries. Removed redundant `Update()` override.
5. **Fixed editor fixed-update invocation** — `EditorLayer.OnUpdate` now calls `_scene.Tick(timeStep)` so FixedUpdate systems actually execute. Previously `PhysicsSystem.Tick()` was dead code because the editor never called `Tick()` and `PhysicsSystem` was registered in the wrong stage.
6. **Removed `PhysicsSystem.GRAVITY` static field** — gravity is now configurable per `PhysicsWorld` instance.

**Bugs fixed as part of this phase**:
- **Physics simulation was non-functional**: `PhysicsSystem.Stage` was `SystemStage.Update`, but `ECSScene.Update()` only called `RunStage(SystemStage.Update)`, and `SystemScheduler.RunFixedUpdate()` filters by `SystemStage.FixedUpdate`. Thus `PhysicsSystem.Tick()` (containing all physics logic) was never executed. Only initialization queries ran.

---

## Phase 9 Pre-phase Audit (2026-06-05)

**New drift discovered**:
1. **`PhysicsSystem.Stage` is incorrectly set to `SystemStage.Update`** instead of `SystemStage.FixedUpdate`. Since `ECSScene.Tick()` calls `SystemScheduler.RunFixedUpdate()`, and `RunFixedUpdate` filters by `SystemStage.FixedUpdate`, the `PhysicsSystem.Tick()` method (which contains all physics simulation logic: integration, AABB update, collision detection) is **never executed**. Only `Update()` (initialization) runs. This means the physics simulation has been non-functional.
2. **`EditorLayer.OnUpdate` does not call `_scene.Tick()`** (or `_scene.FixedUpdate()`), so even if the stage were correct, FixedUpdate systems would not be invoked from the editor loop.
3. **No `PhysicsWorld` instance exists** — all physics logic is inline in `PhysicsSystem`.

**Plan for Phase 9**:
- Create `PhysicsWorld` in `Engine/Physics/PhysicsWorld.cs`
- Move integration, AABB update, collision detection, and resolution into `PhysicsWorld`
- Add spatial hash broadphase to replace O(n²) collision
- Move fixed-step time accumulation into `PhysicsWorld.Step()`
- Change `PhysicsSystem.Stage` to `FixedUpdate`
- Have `PhysicsSystem.Tick()` delegate to `PhysicsWorld.Step()`
- Update `EditorLayer.OnUpdate` to call `_scene.Tick(timeStep)` so FixedUpdate runs

---

## Phase 10 Completion Notes (2026-06-05)

Editor/runtime separation completed. Build + run verified.

**Changes made**:
1. **Created editor `IWorld`** — `EditorLayer` now instantiates a dedicated `ECSWorld` (`_editorWorld`) for editor-only state, establishing structural dual-world isolation.
2. **Removed all `Arch.Bus` usage** — Replaced `Arch.Bus.EventBus.Send()` and `[Event]` attributes with scoped `EventBus<T>` from `LunarEngine.Events`:
   - `HierarchySystem` publishes `InspectorTargetSelectedEvent` via `EventBus<InspectorTargetSelectedEvent>`.
   - `InspectorSystem` subscribes to `InspectorTargetSelectedEvent` explicitly in its constructor.
   - `SceneSystem` publishes `SceneFocusEvent` via `EventBus<SceneFocusEvent>`.
   - `EditorCameraInputHandler` subscribes to both event buses explicitly in its constructor.
3. **Moved event structs to `LunarEngine.Events`** — Created `Events/InspectorTargetSelectedEvent.cs` and `Events/SceneFocusEvent.cs` as `readonly struct` definitions.
4. **Refactored `HierarchySystem` and `InspectorSystem`** — Converted from `ScriptableSystem` (Arch `BaseSystem<World, double>`) to plain C# classes that consume `IWorld` directly. This removes the Arch dependency from editor UI systems.
5. **Updated `EditorCamera`** — Removed static `Time.DeltaTime` dependency. `MousePan`, `MouseZoom`, `MouseRotate`, and `KeyboardMove` now accept `float dt` parameter explicitly.
6. **Refactored `EditorCameraInputHandler`** — Removed `[Event]` attributes and `Hook()` call. Input processing for mouse delta, scroll, and keyboard movement now happens in `Update(InputState, float)` reading from `InputState`. Mouse button events (`MouseDown`/`MouseUp`) remain subscribed via `InputManager` for cursor lock and pan/rotate toggles.
7. **Added `Query<T1>` to `IWorld`** — Added `QueryCallback<T1>` delegate and `IWorld.Query<T1>()` method so consumers can iterate entities without referencing `Arch.Core.World` directly.
8. **Added non-generic `Set` to `IWorld`** — Added `IWorld.Set(EntityReference, object)` to support runtime-type component updates (required by inspector generic reflection path).
9. **Removed all `using Arch.Bus;` statements** from `Editor.cs`, `HierarchySystem.cs`, `InspectorSystem.cs`, `SceneSystem.cs`, `CameraSystem.cs`, and `SpriteRendererSystem.cs`.

**Remaining design gaps**:
- `HierarchySystem` and `InspectorSystem` still query/manipulate the **game world** because they edit scene entities. Full dual-world isolation would require editor-world proxies or a command-based edit bridge; this is deferred to Phase 12 / future work.
- `GizmosSystem` is still registered on the game world (`GizmosLayer`). In the target architecture, gizmos rendering is an editor-layer concern.

---

## Phase 10 Pre-phase Audit (2026-06-05)

**New drift discovered**:
1. **`EditorCamera` uses static `Time.DeltaTime`** in `MousePan`, `MouseZoom`, `MouseRotate`, and `KeyboardMove` — violates the goal of removing global statics.
2. **`EditorCameraInputHandler` uses `Arch.Bus.[Event]`** — `OnInspectorTargetSelected` and `OnSceneFocusEvent` are decorated with `[Event]` and rely on `Hook()` for automatic subscription. This is the last Arch event bus consumer in the codebase.
3. **`HierarchySystem` and `InspectorSystem` extend `ScriptableSystem`** — editor UI systems are coupled to Arch `BaseSystem<World, double>` despite not being registered with any scheduler.
4. **`EventBus<T>` still requires `IEvent` constraint** — prevents usage with plain structs like `SceneFocusEvent`.

**Plan for Phase 10**:
- Remove `IEvent` constraint from `EventBus<T>`.
- Create `InspectorTargetSelectedEvent` and move `SceneFocusEvent` to `LunarEngine.Events`.
- Convert `HierarchySystem` and `InspectorSystem` to plain classes using `IWorld`.
- Replace Arch `[Event]` with explicit `EventBus<T>` subscriptions.
- Pass `TimeStep`/`float dt` through `EditorCamera` methods.
- Create an editor `ECSWorld` in `EditorLayer`.

---

## Phase 11 Completion Notes (2026-06-05)

Namespace consolidation completed. Build + run verified.

**Changes made**:
1. **Renamed all namespaces per the consolidation map**:
   - `LunarEngine.GameEngine` → `LunarEngine.Application` / `LunarEngine.Core` / `LunarEngine.ECS`
   - `LunarEngine.GameObjects` → `LunarEngine.ECS.Components` / `LunarEngine.ECS.Systems`
   - `LunarEngine.Components` → `LunarEngine.ECS.Components`
   - `LunarEngine.Engine.ECS.Components` → `LunarEngine.ECS.Components` / `LunarEngine.ECS.Components.Physics`
   - `LunarEngine.Engine.ECS.Systems` → `LunarEngine.ECS.Systems`
   - `LunarEngine.Physics` (components) → `LunarEngine.ECS.Components.Physics`
   - `LunarEngine.Physics` (systems) → `LunarEngine.ECS.Systems`
   - `LunarEngine.Engine.Graphics` → `LunarEngine.Renderer` / `LunarEngine.Renderer.OpenGL`
   - `LunarEngine.Engine.Gizmos` → `LunarEngine.Editor`
   - `LunarEngine.Engine.AssetHandleCache` → `LunarEngine.Assets`
   - `LunarEngine.Engine.Core` → `LunarEngine.Core`
   - `LunarEngine.Graphics.Debugging` → `LunarEngine.Core`
   - `Editor/*` → `LunarEngine.Editor` / `LunarEngine.Editor.Systems`
   - `Engine/UI/ImGuiController.cs` → added `namespace LunarEngine.UI;`
2. **Updated all `using` statements** across ~60 files to match the new namespaces.
3. **Handled namespace/type name collisions** where a namespace and a class share the same name (e.g., `LunarEngine.Renderer` namespace vs `Renderer` class, `LunarEngine.Application` namespace vs `Application` class, `LunarEngine.Editor` namespace vs `Editor` class). Used `using` aliases (`RenderEngine`, `EngineApp`) and fully qualified names to resolve ambiguities.

**Remaining design gaps**:
- `GizmosLayer` file still lives in `Engine/Gizmos/` directory instead of `Editor/` (cosmetic, does not affect compilation).
- `RotationInspector` and `BoxCollider2DInspector` are nested in `PositionInspector.cs` and `RigidBody2DInspector.cs` respectively rather than in separate files.

---

## Summary

The current codebase is a functional prototype that directly uses Silk.NET, Arch ECS, and raw OpenGL with minimal abstraction. The target architecture defines a fully modular, dependency-injected, interface-driven engine. **Conflicts are pervasive across every module**: Core, Platform, ECS, Renderer, Assets, Input, Scenes, Physics, and Editor all deviate substantially from the target design. The 12-phase implementation plan in `ARCHITECTURE.md` is well-justified given the breadth of changes required.
