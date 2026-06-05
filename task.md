# LunarEngine Refactor Task List

## Phase 1: Hygiene and bug fixes (completed)
- [x] Fix bug 1: `InspectorSystem.AddComponentInspector<T>` inverted TryAdd logic
- [x] Fix bug 2: `EditorCameraInputHandler.OnSceneLoseFocus` uses `+=` instead of `-=`
- [x] Fix bug 3: Duplicate `_camera.LookAt` call
- [x] Fix bug 4: `AssetManager.Dispose()` is empty
- [x] Fix bug 5: `AssetHandleCache.ClearCache()` is empty
- [x] Fix bug 6: `_canZoom` field never set to true (dead field)
- [x] Fix bug 7: `SceneSystem` accesses `_sceneFrameBuffer._colorTexture` and `_size`
- [x] Fix bug 8: `Time.DeltaTime` set twice per frame (remove from SceneLayer)
- [x] Fix bug 9: `SpriteRendererSystem.AdjustScale` sets random shader uniform every frame
- [x] Fix bug 10: `CommandBuffer` newed every frame in multiple systems
- [x] Remove dead code files with no references
  - [x] `Engine/Scenes/Scene.cs`
  - [x] `Engine/ECS/Components/CustomBehaviour.cs`
  - [x] `Core/WindowManager.cs`
  - [x] `Events/WindowInitializedEvent.cs`
  - [x] `Events/ViewportResizedEvent.cs`
  - [x] `Core/PhysicsLayer.cs`
  - [x] `Core/RenderLayer.cs`
  - [x] `Core/SceneLayer.cs`
  - [x] `Engine/Scenes/TestScene.cs`
- [x] Remove dead code files with references (and fix references)
  - [x] `Engine/Physics/PhysicsEngine.cs` + remove reference in `PhysicsSystem`
  - [x] `Engine/ECS/Systems/ShaderSystem.cs` + remove references in `ECSScene`
  - [x] `Engine/ECS/Systems/InputSystem.cs` + remove references in `ECSScene`
  - [x] `Engine/ECS/ComponentFactories/ComponentFactoryManager.cs` + replace usage in `InspectorSystem`
- [x] Normalize all `using` statements to match current namespaces
- [x] Verify `dotnet build -c Debug` and `dotnet run -c Debug`
  - Note: `dotnet run -c Debug` must be executed from `LunarEngine/` directory so resource paths resolve correctly.

## Phase 2: Core module extraction (completed)
- [x] Create `ServiceContainer` and `ServiceDescriptor` in `Core/`
- [x] Replace `Singleton<T>` usage with service registration
- [x] Make `EventBus<T>` instance-based instead of static
- [x] Move `TimeStep` and `Logger` to `Core/`

## Phase 3: Platform abstraction (completed)
- [x] Create `IWindow` and `IInputContext` interfaces in `Platform/`
- [x] Create `SilkWindow` and `SilkInputContext` wrappers
- [x] Refactor `Application` to depend on `IWindow`/`IInputContext`
- [x] Wire `Application` constructor to register platform services

## Phase 4: Renderer abstraction (completed)
- [x] Create `IRenderDevice`, `IRenderer`, `IBuffer`, `ITexture2D`, `IShader`, `IFrameBuffer`, `IVertexArray` interfaces
- [x] Create OpenGL implementations in `Renderer/OpenGL/`
- [x] Port `Sprite`, `Quad`, and `Renderer` internals behind interfaces
- [x] Remove direct `GL` references from ECS systems
- [x] Remove `Gizmos` singleton; make it a render service

## Phase 5: ECS abstraction layer (completed)
- [x] Create `IEntity`, `IWorld`, `ISystem`, `SystemStage` interfaces
- [x] Create `ECSWorld` wrapper
- [x] Create `SystemScheduler`
- [x] Convert all systems to implement `ISystem`
- [x] Move `ECSScene` to use `SystemScheduler`
- [x] Fix `Parent` component to use `EntityReference`

## Phase 6: Input state resource (completed)
- [x] Create `InputState` struct and `InputManager` class in `Input/`
- [x] Remove `Input` singleton class
- [x] Make `InputManager` register callbacks on `IInputContext`
- [x] Update systems to read `InputState` from `ServiceContainer`
- [x] Remove hardcoded WASD from input class

## Phase 7: Asset provider (completed)
- [x] Create `IAssetProvider` and `AssetKey` in `Assets/`
- [x] Create `FileSystemAssetProvider`
- [x] Remove hardcoded paths from `ShaderLibrary` and `TextureLibrary`
- [x] Make `AssetManager` use `IAssetProvider` and `IRenderDevice`
- [x] Implement `AssetManager.Dispose()` properly

## Phase 8: Scene manager (completed)
- [x] Convert `SceneManager` to use `List<IScene>`
- [x] Rename `ActiveScenes` to `ActiveScene`
- [x] Implement proper `RemoveScene` with cleanup
- [x] Create `IScene` interface and refactor `ECSScene`

## Phase 9: Physics consolidation
- [ ] Create `PhysicsWorld` class (no longer static)
- [ ] Move fixed-step accumulation into `SystemScheduler.FixedUpdate`
- [ ] Add simple spatial hash broadphase
- [ ] Remove `PhysicsLayer` and `PhysicsEngine` static class

## Phase 10: Editor/runtime separation
- [ ] Editor creates its own `IWorld`
- [ ] Game scene uses separate `IWorld`
- [ ] `EditorCamera` reads from `InputState` service
- [ ] Remove Arch `[Event]` bus usage; replace with scoped `EventBus`
- [ ] Inspector events use typed event structs

## Phase 11: Namespace consolidation
- [ ] Rename all namespaces per the map
- [ ] Update all `using` statements
- [ ] Verify full build and runtime

## Phase 12: Disposal and lifecycle
- [ ] Implement `Dispose()` on all GPU resources
- [ ] Define disposal order in `Application.Dispose()`
- [ ] Add GL debug output in Debug builds
- [ ] Add frame timing metrics
