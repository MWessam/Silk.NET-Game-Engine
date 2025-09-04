## LunarEngine architecture review and refactoring plan (OpenGL-only for now)

### Repository snapshot
- Core runtime: `Core/` (`Application`, `BaseLayer`, `Singleton`, `TimeStep`, `WindowManager`)
- Editor: `Editor/` (layers: `EditorLayer`, `GizmosLayer`; systems: `HierarchySystem`, `InspectorSystem`, `SceneSystem`)
- Engine modules: `Engine/`
  - Assets: `Assets/` (`AssetManager`, `ShaderLibrary`, `TextureLibrary`, `ShaderAsset`, `TextureAsset`)
  - ECS: `ECS/` (Components, Systems, `ComponentFactoryManager`)
  - Renderer: `Renderer/` and `Renderer/OpenGLAPI/` (OpenGL wrappers & `Renderer`)
  - Physics: `PhysicsEngine` + `PhysicsSystem`
  - UI: `UI/` (ImGui integration)
  - Scenes: `Scenes/` (`ECSScene`, `SceneManager`, `TestEcsScene`)
- Events: `Events/` (generic `EventBus`, `WindowInitializedEvent`)
- Entry: `Engine/GameEngine/Program.cs` starts `Editor` application

### Current architecture summary
- Application and layers: `Application` owns a `LayerStack` and drives `OnUpdate` and `OnImguiRender` per layer. `Editor : Application` pushes editor and gizmo layers.
- Windowing/Input: Silk.NET window/input intended via `WindowManager` and `Input` singleton; callbacks wired in `Application.OnWindowLoad` (window creation not yet integrated here).
- Rendering: OpenGL-only. `Renderer` queues `RenderCommand`s and draws sprites/gizmos. Low-level GL wrappers: `BufferObject`, `VertexArrayObject`, `ShaderHandle`, `TextureHandle`, `FrameBuffer`.
- ECS/Scenes: Arch ECS used. `ECSScene` creates a `World`, constructs systems (Transform, SpriteRenderer, Camera, Shader, Physics, Input, Initialization), and runs them across `Awake/Start/Update/Tick/AfterUpdate`. `SceneManager` selects active scene.
- Editor/ImGui: Hexa.NET ImGui, editor viewport rendered to an FBO; hierarchy/inspector panels.
- Assets: Libraries for shaders/textures; GPU handles created lazily per asset.

### Design issues and gaps
1) Application/window bootstrap is incomplete
   - `Application` uses `_window` but never creates it. `WindowManager` exists but is not wired. No single composition root to construct window, GL, renderer, assets, scenes, editor.

2) Renderer lifecycle is not wired and accessed globally
   - `Renderer` has no `Instance`, yet code calls `Renderer.Instance`. No subscription to `WindowInitializedEvent`. Systems/scenes call straight into renderer/GL, increasing coupling.

3) Overuse of singletons and globals
   - `Singleton<T>` ctor can dispose and recreate instances unexpectedly. `Application.Instance`, `Input.Instance`, `Renderer.Instance` (assumed), global `EventBus<>`, static `Application.Viewport` reduce modularity/testability.

4) ECS scheduling is manual and tightly coupled
   - `ECSScene` manually orders systems. Some systems take GL/renderer directly (e.g., `SpriteRendererSystem(Renderer.Instance.Api, World)`), preventing headless testing and clean separation.

5) Component/hierarchy model issues
   - `Parent` stores a `Transform` instead of an `Entity` reference. `Transform` coexists with `Position/Rotation/Scale` dirty flags; define a single source of truth and proper parent-child propagation.

6) Editor-runtime coupling
   - Editor drives the same world; no explicit separation between editor world and runtime world/play session.

7) Asset/resource pipeline is minimal
   - Relative file paths in code, lazy GL handle creation inside assets, no virtual FS or hot reload, unclear ownership of GPU resources.

8) Scene management limitations
   - Fixed-size array (16), incomplete remove logic, no serialization/prefabs.

9) Event bus risks
   - Global static, no weak refs, no scoping per world, no thread-safety.

10) Physics is skeletal
   - `PhysicsEngine` mostly a shell; narrow features in `PhysicsSystem` only; no broadphase/spatial partitioning; fixed-step integration spread across editor loop.

11) Error handling and lifecycle
   - Many disposables with empty `Dispose`. Ownership rules unclear. Bug in `ShaderHandle` uses `_gl` before assignment on error path.

12) Logging/diagnostics
   - Mixed usage; timings/metrics absent.

13) Namespaces/project hygiene
   - Inconsistent namespaces, commented dead code, dev-relative resource paths.

### Refactoring objectives (OpenGL-only target)
- Introduce a composition root (EngineHost) that wires window → GL → renderer → assets → scene manager → editor layers.
- Replace global singletons with dependency injection or a scoped service container.
- Define a staged ECS scheduler (PreUpdate, FixedUpdate, Update, LateUpdate, RenderPrepare, RenderSubmit) with declarative ordering.
- Decouple systems from GL by emitting render data/commands; renderer consumes them.
- Fix hierarchy: `Parent` as entity relation; derive `Transform` from PRS and parents.
- Provide an `IRenderDevice/IRenderer` boundary with an OpenGL implementation now.
- Establish asset provider, handle ownership, and stable asset keys.
- Improve scene management and add serialization/prefabs.
- Scope events per world or via resources; ensure safe subscription.
- Clarify disposal, add GL debug checks, and strengthen logging/metrics.

### Refactoring checklist
- [ ] Core bootstrap: Add `EngineHost` (composition root). Create window, GL, input; construct and register renderer, assets, scene manager; start editor layers.
- [ ] Windowing integration: Ensure `Application` or `EngineHost` creates `_window` and wires `OnWindowLoad/Update/Resize`.
- [ ] Renderer boundary: Define `IRenderer` and `IRenderDevice`; implement `OpenGLRenderer` and `OpenGLRenderDevice`. Remove `Renderer.Instance` usages by injecting services.
- [ ] Fix `ShaderHandle` constructor bug (`_gl` usage before assignment); add error log output.
- [ ] ECS scheduler: Introduce stages and system registration with ordering; move system calls from `ECSScene` to scheduler.
- [ ] Rendering decoupling: Split render systems into prepare (build draw data) vs submit (renderer consumes buffers); eliminate direct GL in systems.
- [ ] Hierarchy/Transform: Replace `Parent` with entity reference; implement hierarchical transform update; make PRS the source of truth.
- [ ] Input state: Replace `Input.Instance` in systems with an `InputState` resource updated from window callbacks.
- [ ] Assets: Add `IAssetProvider` and stable asset IDs; renderer/device owns GPU handles; support reload.
- [ ] SceneManager: Use dynamic collection; implement remove/unload; add scene activation events.
- [ ] Serialization/Prefabs: Add JSON serialization for core components; implement prefab/variant instantiation pipeline.
- [ ] Editor separation: Distinct editor world or tagged editor-only systems/components; viewport-sized FBO; selection/highlight systems.
- [ ] Event bus: Scope per world or implement a typed, scoped message system; add weak subscriptions or explicit unsubscribe.
- [ ] Physics: Centralize fixed-step in scheduler; add simple broadphase and interpolation.
- [ ] Disposal/ownership: Implement `Dispose` across GPU resources with clear ownership; shutdown order verified.
- [ ] Logging/diagnostics: Standardize logger; add system timings, draw-call counters, and GL debug output in debug builds.
- [ ] Namespaces/hygiene: Normalize namespaces, remove dead code, externalize resource paths, add README/build notes.

### Prompts to drive each refactor (copy/paste)

1) Core bootstrap (EngineHost)
```
Act as a senior engine architect. In this C# Silk.NET engine, create an `EngineHost` (composition root) that:
- Creates the Silk `IWindow` and GL context
- Constructs and registers: `OpenGLRenderDevice`, `OpenGLRenderer : IRenderer`, `AssetManager`, `SceneManager`, input service
- Wires window callbacks to input and renderer
- Starts the `Editor` application with injected services and pushes editor layers
Acceptance criteria:
- `Program.Main` calls `EngineHost.Run()` and the editor opens with a scene loaded and rendering to a viewport
- No direct use of uninitialized `_window` in `Application`
```

2) Windowing integration
```
Integrate window creation with `Application` or `EngineHost` so that:
- `_window` is created before `Run()`
- `OnWindowLoad/Update/Resize/Closing` are hooked
- GL context is acquired on load and passed to renderer/device
Acceptance: engine runs without null `_window`; resize updates viewport and FBOs.
```

3) Renderer boundary (interfaces + OpenGL impl)
```
Define `IRenderDevice` (buffers, textures, framebuffers, shaders) and `IRenderer` (BeginFrame, Submit, EndFrame). Implement `OpenGLRenderDevice` and adapt existing GL wrappers behind it. Replace `Renderer.Instance` call-sites with constructor-injected `IRenderer`.
Acceptance: compile passes; systems no longer reference Silk.NET GL types; rendering still works via OpenGL backend.
```

4) Fix `ShaderHandle` bug
```
In `ShaderHandle`, assign `_gl = api` before link-status checks and use `api.GetProgramInfoLog` on failure. Add meaningful exception/log message with shader names/paths.
Acceptance: no null `_gl` usages; link errors report correctly.
```

5) ECS scheduler with stages
```
Introduce a scheduler with stages: PreUpdate, FixedUpdate, Update, LateUpdate, RenderPrepare, RenderSubmit. Register all systems with stage and ordering constraints (e.g., Transform after Physics). Move manual system calls out of `ECSScene` into the scheduler.
Acceptance: `ECSScene` delegates to scheduler; fixed-step handled centrally; order is explicit and testable.
```

6) Rendering decoupling (prepare vs submit)
```
Refactor `SpriteRendererSystem` into:
- Prepare: iterate ECS and build a SoA buffer of sprite draw data (transform, texture ID, color, UVs)
- Submit: `IRenderer` consumes this buffer to issue GL draws
Acceptance: no GL calls inside ECS systems; frame draws match previous visual output.
```

7) Hierarchy and transform model
```
Replace `Parent` component to reference an `Entity` (or `EntityReference`). Implement a `TransformHierarchySystem` that:
- Rebuilds world transforms from PRS and parent chains when dirty
- Supports reparenting by marking subtrees dirty
Acceptance: child transforms follow parents; PRS is the source of truth; `Transform` is derived.
```

8) Input state as resource
```
Create an `InputState` resource updated by window callbacks each frame (keys, mouse, scroll, delta). Make systems read `InputState` instead of `Input.Instance`. Remove global input access from systems.
Acceptance: input-driven systems work via resource; `Input.Instance` only adapts OS → resource.
```

9) Asset provider and handle ownership
```
Introduce `IAssetProvider` to resolve logical IDs to files. Ensure renderer/device creates and owns GPU handles; assets provide CPU-side data and metadata. Add a simple reload path.
Acceptance: asset paths removed from shader/texture constructors; device caches/reuses handles.
```

10) SceneManager improvements
```
Replace fixed-size array with a dynamic collection; add `RemoveScene`, `UnloadScene`, and activation events. Ensure IDs are stable or use GUIDs.
Acceptance: can add/remove/activate scenes dynamically without errors.
```

11) Serialization and prefabs
```
Add JSON serialization for common components (Name, Position, Rotation, Scale, SpriteRenderer, Camera, colliders, RB2D) using source generators. Implement prefab definitions (component sets) and instantiate via a command buffer.
Acceptance: can save/load a simple scene and instantiate a prefab into the world.
```

12) Editor/runtime separation
```
Create either a separate editor world or mark editor-only systems/components. Render the scene into a docked viewport FBO sized to the panel. Provide selection/highlight by tagging selected entities.
Acceptance: editor UI no longer pollutes runtime world; viewport resizes correctly.
```

13) Event bus scoping
```
Scope events per world or replace global `EventBus<>` with a typed, scoped message service. Implement weak references or explicit unsubscribe and basic thread-safety if needed.
Acceptance: no global cross-world event leaks; subscribers are cleaned up.
```

14) Physics consolidation
```
Move fixed-step logic into the scheduler's `FixedUpdate`. Implement a simple broadphase (uniform grid) and interpolation from previous/current transforms.
Acceptance: physics runs deterministically with fixed dt; visual interpolation is smooth.
```

15) Disposal and ownership rules
```
Define ownership and implement `Dispose` for all GPU resources. Ensure shutdown order: renderer drains commands → disposes resources → destroys GL context. Add GL debug output in debug builds.
Acceptance: clean shutdown without GL errors; no leaked resources.
```

16) Logging and diagnostics
```
Standardize logging (e.g., Serilog). Add per-system timings, frame time, draw-call counts, and GL error checks in debug builds.
Acceptance: metrics visible in logs/overlay; easy to spot regressions.
```

17) Namespaces and hygiene
```
Normalize namespaces (e.g., `Lunar.Engine.*`), remove commented dead code, move resource paths to config, and add a README with run instructions.
Acceptance: consistent namespaces; no dead code; reproducible resource loading.
```

### Quick wins to start now
- Fix `ShaderHandle` bug and add link error logging.
- Centralize window creation and GL initialization in a composition root.
- Introduce `IRenderer`/`IRenderDevice` interfaces and adapt `Renderer` behind them (OpenGL implementation).
- Replace `Parent` with entity reference and add a simple transform hierarchy update.

## Step-by-step refactor guide (do-it-yourself plan)

Follow these phases in order. Keep each phase building and running before proceeding. Prefer creating a feature branch per phase.

### Preparation
- Create a new branch: `git checkout -b refactor/engine-architecture`.
- Ensure `dotnet restore` and `dotnet build -c Debug` succeed on the baseline.
- Note: clean up any obvious red herrings (e.g., `LGTexture` type in `Engine/Renderer/Sprite.cs` should be aligned with your current texture handle type or removed).

### Phase 1: Immediate fixes and hygiene
- Files:
  - `Engine/Renderer/OpenGLAPI/ShaderHandle.cs`
  - `Engine/Renderer/Renderer.cs`
- Tasks:
  - In `ShaderHandle`, assign `_gl = api;` before link checks and use `_gl.GetProgramInfoLog` in error messages.
  - Add helpful exception messages including shader paths.
  - Ensure `Renderer` is either injected everywhere or you temporarily expose `Renderer.Instance` with clear lifecycle.
- Acceptance:
  - Build succeeds; runtime shader link errors show detailed logs.

### Phase 2: Composition root and window lifecycle
- Files:
  - Add `Engine/GameEngine/EngineHost.cs` or reuse `Engine/GameEngine/GameEngine.cs` as host.
  - `Engine/GameEngine/Program.cs`
  - `Core/Application.cs`
  - `Events/WindowInitializedEvent.cs`
- Tasks:
  - Create `EngineHost` that:
    - Creates the Silk `IWindow` (via a `CreateWindow` method on `Application` or directly) with title and size.
    - Subscribes window events: load → acquire GL via `GL.GetApi(window)`; update, resize, closing.
    - Raises `WindowInitializedEvent` after GL is acquired.
    - Instantiates the editor application and starts the loop.
  - Update `Program.Main` to call `new EngineHost().Run()`.
  - In `Application`, add `CreateWindow(string title, int width, int height)` and wire `OnWindowLoad/OnUpdate/OnViewportResize/OnClose`.
  - Ensure `OnViewportResize` updates `Application.Viewport` and calls `_api.Viewport(viewport)`.
- Acceptance:
  - Engine runs without null `_window`.
  - Window resizing updates the GL viewport.

### Phase 3: Viewport resize propagation to editor viewport
- Files:
  - Add `Events/ViewportResizedEvent.cs`.
  - `Core/Application.cs` (raise event in `OnViewportResize`).
  - `Editor/Editor.cs` (subscribe in `OnAttach`, unsubscribe in `OnDetach`).
- Tasks:
  - Define `ViewportResizedEvent` carrying `Vector2D<int> Size`.
  - In `Application.OnViewportResize`, raise `EventBus<ViewportResizedEvent>.Raise(new(size))`.
  - In `EditorLayer`, subscribe to this event and resize the scene `FrameBuffer` accordingly.
- Acceptance:
  - Resizing the window resizes the editor viewport FBO and the content scales accordingly.

### Phase 4: Renderer abstraction boundary (OpenGL-only implementation now)
- Files:
  - Add `Engine/Renderer/Abstractions/IRenderDevice.cs`
  - Add `Engine/Renderer/Abstractions/IRenderer.cs`
  - Move/rename current GL wrappers under `Engine/Renderer/OpenGL/` if desired.
- Interfaces (example):
  - `IRenderDevice`:
    ```csharp
    public interface IRenderDevice : IDisposable {
      IBuffer CreateBuffer<T>(ReadOnlySpan<T> data, BufferUsage usage) where T : unmanaged;
      ITexture2D CreateTexture2D(ReadOnlySpan<byte> pixels, uint width, uint height, TextureFormat fmt);
      IShaderProgram CreateShader(string vertexPath, string fragmentPath);
      IFrameBuffer CreateFrameBuffer(int width, int height);
      void SetViewport(int x, int y, int w, int h);
      void Clear(Color4 color, ClearMask mask);
      void DrawIndexed(int indexCount);
    }
    ```
  - `IRenderer`:
    ```csharp
    public interface IRenderer : IDisposable {
      void BeginFrame(Matrix4x4 viewProjection);
      void Submit(in SpriteDrawData draw);
      void EndFrame();
      void SetRenderTarget(IFrameBuffer fbo);
    }
    ```
- Tasks:
  - Implement `OpenGLRenderDevice : IRenderDevice` by adapting `BufferObject`, `VertexArrayObject`, `TextureHandle`, `FrameBuffer`, `ShaderHandle` behind interface types.
  - Adapt `Renderer` to `OpenGLRenderer : IRenderer` that uses `IRenderDevice`.
  - Keep API surface minimal to reduce migration friction.
- Acceptance:
  - Build succeeds; renderer still draws using the OpenGL implementation but ECS code doesn’t reference Silk.NET types directly.

### Phase 5: Render data path (decouple systems from GL)
- Files:
  - `Engine/ECS/Systems/SpriteRendererSystem.cs`
  - Add `Engine/Renderer/DrawData/SpriteDrawData.cs`
- Tasks:
  - Define a POD `SpriteDrawData` type (transform, texture ID/handle ref, color, UVs).
  - Make `SpriteRendererSystem` build a list or native buffer of `SpriteDrawData` each frame and expose it via a resource or `CommandBuffer`.
  - Modify `IRenderer` consumption to iterate this buffer and issue draws.
- Acceptance:
  - No GL calls in ECS systems; same visual output.

### Phase 6: ECS scheduler with stages
- Files:
  - Add `Engine/ECS/Scheduling/Scheduler.cs` and related types.
  - `Engine/Scenes/ECSScene.cs`
- Tasks:
  - Implement a scheduler with stages: `PreUpdate`, `FixedUpdate`, `Update`, `LateUpdate`, `RenderPrepare`, `RenderSubmit`.
  - Register existing systems with explicit stage and ordering (e.g., Transform after Physics, RenderPrepare after Transform).
  - Replace manual `ECSScene` calls with `scheduler.Run(stage, dt)` invocations.
- Acceptance:
  - Clear, declarative order; fixed-step handled centrally.

### Phase 7: Transform and hierarchy model
- Files:
  - `Engine/ECS/Components/Transform.cs`
  - Add `Engine/ECS/Components/Parent.cs` as `struct Parent { public Entity Value; }`
  - `Engine/ECS/Systems/TransformSystem.cs`
- Tasks:
  - Replace `Parent` storing `Transform` with `Entity` parent reference.
  - Establish PRS (`Position`, `Rotation`, `Scale`) as sources of truth; derive `Transform`.
  - Implement hierarchical propagation: when PRS or parent changes, mark subtree dirty and recompute world transform.
- Acceptance:
  - Children follow parents; PRS edits reflect in `Transform`.

### Phase 8: Input state resource
- Files:
  - Add `Engine/InputEngine/InputState.cs` (resource struct/class)
  - `Core/Application.cs` (update `InputState` from callbacks)
  - `Engine/ECS/Systems/InputSystem.cs` (read `InputState` resource)
- Tasks:
  - Convert window callbacks to write into `InputState` (keys, buttons, mouse, scroll, delta).
  - Refactor systems to read `InputState` via ECS world resources instead of `Input.Instance`.
- Acceptance:
  - Same input behavior, no direct global singleton reads in systems.

### Phase 9: Asset provider and GPU ownership
- Files:
  - Add `Engine/Assets/IAssetProvider.cs`
  - Adjust `ShaderLibrary`, `TextureLibrary`, `AssetManager`
  - `IRenderDevice` manages GPU handle lifetime
- Tasks:
  - Use logical asset IDs; resolve to files via provider.
  - Assets produce CPU-side data and metadata; device creates and caches GPU resources.
  - Add a simple reload path for development.
- Acceptance:
  - No hard-coded relative paths in code; handles owned by device/renderer.

### Phase 10: SceneManager improvements
- Files:
  - `Engine/Scenes/SceneManager.cs`
- Tasks:
  - Replace fixed-size array with a dynamic collection.
  - Implement `RemoveScene`, `UnloadScene`, and activation events.
  - Choose stable IDs or GUIDs for scenes.
- Acceptance:
  - Add/remove/activate scenes at runtime safely.

### Phase 11: Serialization and prefabs
- Files:
  - Add `Engine/Serialization/SceneSerializer.cs`
  - Add `Engine/Prefabs/Prefab.cs`
- Tasks:
  - JSON serialization for common components using source generators or a lightweight serializer.
  - Prefab definitions as component sets; instantiate via command buffers.
- Acceptance:
  - Save/load a sample scene; instantiate a prefab into the world.

### Phase 12: Editor/runtime separation
- Files:
  - `Editor/` and possibly `Runtime/`
- Tasks:
  - Either maintain a separate editor world or tag editor-only systems/components.
  - Render scene into a docked viewport FBO sized to the panel.
  - Selection/highlight via tag components and queries.
- Acceptance:
  - Editor UI doesn’t pollute runtime world; viewport scales with panel.

### Phase 13: Event bus scoping
- Files:
  - `Events/EventBus.cs`
- Tasks:
  - Scope event buses per world/context or replace with a typed message service.
  - Support weak subscriptions or explicit unsubscribe; add basic thread-safety if needed.
- Acceptance:
  - No global cross-world leaks; clean shutdown without dangling handlers.

### Phase 14: Physics consolidation
- Files:
  - `Engine/Physics/PhysicsEngine.cs`, `Engine/ECS/Systems/PhysicsSystem.cs`
- Tasks:
  - Move fixed-step into scheduler’s `FixedUpdate`.
  - Implement a simple broadphase (uniform grid) and interpolation.
- Acceptance:
  - Deterministic fixed-step; smooth interpolation visuals.

### Phase 15: Disposal, logging, diagnostics
- Files:
  - All GPU resource types; logging config; optional overlay
- Tasks:
  - Implement `Dispose` and define ownership and shutdown order.
  - Standardize on Serilog; add timings and draw-call counters; enable GL debug in Debug builds.
- Acceptance:
  - Clean shutdown, no leaks; metrics visible.

### Phase 16: Namespaces and hygiene
- Files:
  - Solution-wide renames and cleanup
- Tasks:
  - Normalize namespaces to `Lunar.Engine.*` (or chosen scheme).
  - Remove commented dead code; move resource paths to config.
  - Add README with run/build instructions.
- Acceptance:
  - Consistent structure; no dev-relative paths in code.

## Run instructions (for Cursor)
- Terminal in repo root:
  - `dotnet restore`
  - `dotnet build -c Debug`
  - `dotnet run -c Debug`
- Create a Cursor Task:
  - Command: `dotnet run -c Debug`
  - Working directory: repo root (`LunarEngine`)


