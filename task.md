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

## Phase 2: Core module extraction
- [ ] Create `ServiceContainer` and `ServiceDescriptor` in `Core/`
- [ ] Replace `Singleton<T>` usage with service registration
- [ ] Make `EventBus<T>` instance-based instead of static
- [ ] Move `TimeStep` and `Logger` to `Core/`

## Phase 2: Core module extraction
- [ ] Create `ServiceContainer` and `ServiceDescriptor` in `Core/`
- [ ] Replace `Singleton<T>` usage with service registration
- [ ] Make `EventBus<T>` instance-based instead of static
- [ ] Move `TimeStep` and `Logger` to `Core/`

## Phase 3: Platform abstraction
- [ ] Create `IWindow` and `IInputContext` interfaces in `Platform/`
- [ ] Create `SilkWindow` and `SilkInputContext` wrappers
- [ ] Refactor `Application` to depend on `IWindow`/`IInputContext`
- [ ] Wire `Application` constructor to register platform services

## Phase 4: Renderer abstraction
- [ ] Create `IRenderDevice`, `IRenderer`, `IBuffer`, `ITexture2D`, `IShader`, `IFrameBuffer`, `IVertexArray` interfaces
- [ ] Create OpenGL implementations in `Renderer/OpenGL/`
- [ ] Port `Sprite`, `Quad`, and `Renderer` internals behind interfaces
- [ ] Remove direct `GL` references from ECS systems
- [ ] Remove `Gizmos` singleton; make it a render service

## Phase 5: ECS abstraction layer
- [ ] Create `IEntity`, `IWorld`, `ISystem`, `SystemStage` interfaces
- [ ] Create `ECSWorld` wrapper
- [ ] Create `SystemScheduler`
- [ ] Convert all systems to implement `ISystem`
- [ ] Move `ECSScene` to use `SystemScheduler`
- [ ] Fix `Parent` component to use `EntityReference`

## Phase 6: Input state resource
- [ ] Create `InputState` struct and `InputManager` class in `Input/`
- [ ] Remove `Input` singleton class
- [ ] Make `InputManager` register callbacks on `IInputContext`
- [ ] Update systems to read `InputState` from `ServiceContainer`
- [ ] Remove hardcoded WASD from input class

## Phase 7: Asset provider
- [ ] Create `IAssetProvider` and `AssetKey` in `Assets/`
- [ ] Create `FileSystemAssetProvider`
- [ ] Remove hardcoded paths from `ShaderLibrary` and `TextureLibrary`
- [ ] Make `AssetManager` use `IRenderDevice`
- [ ] Implement `AssetManager.Dispose()` properly

## Phase 8: Scene manager
- [ ] Convert `SceneManager` to use `List<IScene>`
- [ ] Rename `ActiveScenes` to `ActiveScene`
- [ ] Implement proper `RemoveScene` with cleanup
- [ ] Create `IScene` interface and refactor `ECSScene`

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
