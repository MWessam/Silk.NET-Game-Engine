# LunarEngine — Target Architecture

## Guiding Principles

1. **Core engine as framework**: The engine provides reusable modules and APIs. It knows nothing about the editor, specific games, or ImGui.
2. **Module boundaries via namespaces**: Single `.csproj` with strict directory/namespace boundaries. Modules only depend on modules below them.
3. **Dependency injection via service container**: No global singletons. Services are registered by interface and resolved at startup.
4. **Thin ECS abstraction over Arch**: Wrap Arch's `World`, `Entity`, and query system behind engine interfaces so ECS internals are isolated.
5. **Full IRenderDevice + command-based IRenderer**: Low-level graphics abstraction (buffers, textures, shaders) behind `IRenderDevice`, high-level render commands behind `IRenderer`.
6. **Editor is an application on top**: Editor layers, gizmos, inspectors, and ImGui live above the engine. They consume engine APIs, never bypass them.

---

## Module Dependency Graph (top to bottom)

```
+--------------------------------------------------+
|                    Editor                         |
|  EditorLayers, Inspector, Hierarchy, SceneView    |
+--------------------------------------------------+
|                 Application                       |
|  Window bootstrap, main loop, layer stack         |
+--------+---------+----------+-----------+----------+
| Scenes |   UI    |  Gizmos  |   Input   | Physics |
| (manage| (ImGui  | (debug   | (keyboard | (2D phys |
| worlds)| backend)| drawing) |  /mouse)  | +broad-  |
|        |         |          |           |  phase)  |
+--------+---------+----------+-----------+----------+
|                     ECS                           |
|  IEntity, IWorld, IQuery, ISystem, Components     |
+------------------------+--------------------------+
|      Renderer          |        Assets            |
|  IRenderDevice         |  IAssetProvider          |
|  IRenderer             |  AssetManager            |
|  (OpenGL impl)         |  (loaders, caches)       |
+------------------------+--------------------------+
|                     Core                          |
|  ServiceContainer, Logger, EventBus<T>,           |
|  TimeStep, Math extensions, Debug                 |
+--------------------------------------------------+
|                   Platform                        |
|  IWindow, IInput, OS abstraction (Silk.NET)       |
+--------------------------------------------------+
```

**Dependency rule**: A module may only `using` modules directly below it (or same level if strictly necessary). Editor never appears in engine code. Platform never depends on anything.

---

## Directory and Namespace Map

All source lives under `LunarEngine/`. The directory tree mirrors namespaces:

```
LunarEngine/
|-- Core/                           # LunarEngine.Core
|   |-- ServiceContainer.cs
|   |-- ServiceDescriptor.cs
|   |-- Logger.cs
|   |-- TimeStep.cs
|   +-- DebugUtils.cs
|
|-- Platform/                       # LunarEngine.Platform
|   |-- IWindow.cs                  # Window abstraction
|   |-- IInputContext.cs            # Input abstraction
|   |-- SilkWindow.cs               # Silk.NET IWindow wrapper
|   +-- SilkInputContext.cs         # Silk.NET input wrapper
|
|-- Events/                         # LunarEngine.Events
|   |-- EventBus.cs                 # Scoped, typed pub/sub
|   |-- ViewportResizedEvent.cs
|   |-- SceneLoadedEvent.cs
|   +-- ...                         # Other event definitions
|
|-- ECS/                            # LunarEngine.ECS
|   |-- Abstractions/
|   |   |-- IEntity.cs              # IEntity, EntityReference
|   |   |-- IWorld.cs               # IWorld (create, query, destroy)
|   |   |-- ISystem.cs              # ISystem lifecycle
|   |   |-- CommandBuffer.cs        # Buffered entity commands
|   |   +-- SystemStage.cs          # Enum: PreUpdate, FixedUpdate, Update, ...
|   |-- Components/
|   |   |-- Transform.cs            # Position, Rotation, Scale, LocalToWorld, Parent
|   |   |-- SpriteRenderer.cs
|   |   |-- CameraComponent.cs
|   |   |-- Name.cs
|   |   |-- NeedsInitialization.cs  # IsInstantiating, IsDestroying markers
|   |   |-- Physics/
|   |   |   |-- RigidBody2D.cs
|   |   |   +-- BoxCollider2D.cs
|   |   +-- Input/
|   |       +-- InputState.cs       # ECS-consumable input data
|   |-- Systems/
|   |   |-- TransformSystem.cs
|   |   |-- SpriteRendererSystem.cs
|   |   |-- CameraSystem.cs
|   |   |-- PhysicsSystem.cs
|   |   |-- InitializationSystem.cs
|   |   +-- SystemScheduler.cs      # Orchestrates system stages
|   |-- World.cs                    # Arch World wrapper
|   +-- EntityFactory.cs            # Default entity creation
|
|-- Renderer/                       # LunarEngine.Renderer
|   |-- Abstractions/
|   |   |-- IRenderDevice.cs        # Low-level GPU operations
|   |   |-- IRenderer.cs            # High-level render commands
|   |   |-- IBuffer.cs
|   |   |-- ITexture.cs
|   |   |-- IShader.cs
|   |   |-- IFrameBuffer.cs
|   |   |-- IVertexArray.cs
|   |   +-- RenderCommand.cs        # SpriteDrawCommand, LineDrawCommand, etc.
|   |-- OpenGL/                     # LunarEngine.Renderer.OpenGL
|   |   |-- GLRenderDevice.cs       # IRenderDevice implementation
|   |   |-- GLRenderer.cs           # IRenderer implementation
|   |   |-- GLBuffer.cs
|   |   |-- GLTexture.cs
|   |   |-- GLShader.cs
|   |   |-- GLFrameBuffer.cs
|   |   |-- GLVertexArray.cs
|   |   +-- GLSprite.cs
|   |-- Sprite.cs
|   |-- Quad.cs
|   +-- Gizmos.cs                   # Debug drawing (no longer a singleton)
|
|-- Assets/                         # LunarEngine.Assets
|   |-- IAssetProvider.cs           # Resolve asset IDs to file paths
|   |-- AssetManager.cs            # Orchestrates loading
|   |-- AssetHandleCache.cs
|   |-- ShaderAsset.cs
|   |-- TextureAsset.cs
|   |-- ShaderLibrary.cs
|   |-- TextureLibrary.cs
|   +-- BaseAssetLibrary.cs
|
|-- Scenes/                         # LunarEngine.Scenes
|   |-- IScene.cs                   # Scene lifecycle interface
|   |-- ECSScene.cs                 # ECS-based scene implementation
|   |-- SceneManager.cs             # Dynamic scene collection
|   +-- TestScene.cs
|
|-- Physics/                        # LunarEngine.Physics
|   |-- PhysicsWorld.cs             # Physics step, broadphase, collision
|   +-- PhysicsSystem.cs            # ECS system delegating to PhysicsWorld
|
|-- Input/                          # LunarEngine.Input
|   |-- InputManager.cs            # OS to InputState bridge
|   +-- InputState.cs              # Read-only snapshot consumed by ECS
|
|-- UI/                             # LunarEngine.UI
|   |-- ImGuiController.cs
|   +-- ImGuiLayer.cs               # ImGui rendering backend
|
|-- Application/                    # LunarEngine.Application
|   |-- Application.cs              # Window + main loop + service container
|   |-- LayerStack.cs               # Ordered layer management
|   +-- BaseLayer.cs                # Abstract layer
|
|-- Editor/                         # LunarEngine.Editor
|   |-- Editor.cs                   # Editor : Application
|   |-- EditorLayer.cs
|   |-- GizmosLayer.cs
|   |-- Systems/
|   |   |-- HierarchySystem.cs
|   |   |-- InspectorSystem.cs
|   |   +-- SceneSystem.cs
|   +-- Inspectors/
|       |-- IComponentInspector.cs
|       |-- CameraInspector.cs
|       |-- NameInspector.cs
|       |-- PositionInspector.cs
|       |-- RigidBody2DInspector.cs
|       +-- SpriteRendererInspector.cs
|
|-- Utilities/                      # LunarEngine.Utilities
|   +-- VectorExtensions.cs
|
+-- Entry/                          # LunarEngine (root namespace)
    +-- Program.cs                   # Composition root
```

---

## Module Specifications

### 1. Core (`LunarEngine.Core`)

**Purpose**: Foundational types used by every other module. No engine logic, no OpenGL, no ECS.

**Public API**:

```csharp
// ServiceContainer -- lightweight DI container
public sealed class ServiceContainer
{
    void Register<TService>(TService instance) where TService : class;
    void RegisterLazy<TService>(Func<TService> factory) where TService : class;
    TService Get<TService>();
    bool TryGet<TService>(out TService service);
    void Reset(); // for shutdown/tests
}

public readonly struct TimeStep
{
    public float DeltaTime { get; }
    public float TotalTime { get; }
    public static TimeStep Zero { get; }
}

public static class Logger
{
    void Initialize(); // Serilog config
    void Shutdown();
}

public static class DebugUtils
{
    void Assert(bool condition, string message);
}

// Scoped event bus -- replaces global static
public sealed class EventBus<T> where T : struct
{
    void Subscribe(Action<T> handler);
    void Unsubscribe(Action<T> handler);
    void Publish(T evt);
}
```

**Key changes from current**:
- `Singleton<T>` removed. Services go through `ServiceContainer`.
- `EventBus<T>` is instance-based, not static. Each `IWorld` or `Application` owns its scope.
- `Time` static class replaced by `TimeStep` passed through the update chain.

---

### 2. Platform (`LunarEngine.Platform`)

**Purpose**: Abstract window, input, and OS services. Silk.NET is the only implementation.

**Public API**:

```csharp
public interface IWindow : IDisposable
{
    Vector2D<int> Size { get; }
    bool IsRunning { get; }
    event Action OnLoad;
    event Action<double> OnUpdate;
    event Action<Vector2D<int>> OnResize;
    event Action OnClosing;
    void Run();
    void Close();
}

public interface IInputContext : IDisposable
{
    IKeyboard Keyboard { get; }
    IMouse Mouse { get; }
}

// SilkWindow implements IWindow, delegates to Silk.NET IWindow
// SilkInputContext implements IInputContext, delegates to Silk.NET IInputContext
```

**Key changes from current**:
- `Application` no longer directly creates Silk.NET `IWindow`. It receives an `IWindow` via `ServiceContainer`.
- Input callbacks are attached to `IInputContext`, not `Application`.
- No more `Input.Instance` singleton.

---

### 3. ECS (`LunarEngine.ECS`)

**Purpose**: Thin abstraction over Arch ECS. Defines component contracts, system lifecycle, and scheduling.

**Public API**:

```csharp
public interface IComponent { } // Marker

public readonly struct EntityReference
{
    public int Id { get; }
    public IWorld World { get; }
    public bool IsValid { get; }
}

public interface IWorld
{
    EntityReference Create();
    EntityReference Create<T1>(in T1 c1) where T1 : struct, IComponent;
    EntityReference Create<T1, T2>(in T1 c1, in T2 c2)
        where T1 : struct, IComponent where T2 : struct, IComponent;
    // ... up to T8 or use params
    void Destroy(EntityReference entity);
    ref T Get<T>(EntityReference entity) where T : struct, IComponent;
    void Set<T>(EntityReference entity, in T component) where T : struct, IComponent;
    bool Has<T>(EntityReference entity) where T : struct, IComponent;
    void Query<T1>(Action<EntityReference, ref T1> callback) where T1 : struct, IComponent;
    void Query<T1, T2>(Action<EntityReference, ref T1, ref T2> callback)
        where T1 : struct, IComponent where T2 : struct, IComponent;
    // Higher-arity queries...
    CommandBuffer CreateCommandBuffer();
    void Playback(CommandBuffer buffer);
}

public enum SystemStage
{
    Awake,
    Start,
    PreUpdate,
    FixedUpdate,
    Update,
    LateUpdate,
    RenderPrepare,
    RenderSubmit
}

public interface ISystem
{
    int Order { get; } // Within a stage, lower runs first
    SystemStage Stage { get; }
    void Initialize(IWorld world, ServiceContainer services);
    void Update(double deltaTime);
}
```

**Arch wrapper implementation**:

```csharp
// World.cs wraps Arch.Core.World
internal sealed class ECSWorld : IWorld
{
    private readonly Arch.Core.World _archWorld;
    // Delegates all IWorld operations to _archWorld
    // Create/Destroy/Get/Set/Query map to Arch equivalents
}
```

**Component changes**:
- `Parent` changes from `struct Parent { Transform ParentEntity; }` to `struct Parent { EntityReference Entity; }`
- `Transform` component removed. Derived from `Position + Rotation + Scale + Parent` by `TransformSystem`.
- `LocalToWorld` replaces `Transform` as the computed world matrix component.
- `Input` struct removed. `InputState` resource is read via `ServiceContainer.Get<InputState>()`.
- All components move to `LunarEngine.ECS.Components` or sub-namespaces (`LunarEngine.ECS.Components.Physics`, etc.).

**SystemScheduler**:

```csharp
public sealed class SystemScheduler
{
    private readonly Dictionary<SystemStage, List<ISystem>> _systems;

    void Register(ISystem system);
    void RunStage(SystemStage stage, double deltaTime);
    void RunStage(SystemStage stage); // For zero-dt stages like Awake/Start
}
```

**Key changes from current**:
- No `ScriptableSystem` base class. Systems implement `ISystem`.
- `[Query]` source generators still work on systems (via partial class), but query methods are invoked through `IWorld.Query<T>()`.
- `ECSScene` no longer manually orders systems -- `SystemScheduler` handles staging.
- `CommandBuffer` is obtained from `IWorld`, not newed per frame.

---

### 4. Renderer (`LunarEngine.Renderer`)

**Purpose**: Two-level abstraction. `IRenderDevice` for low-level GPU ops. `IRenderer` for high-level frame-based rendering.

#### IRenderDevice (low-level)

```csharp
public interface IRenderDevice : IDisposable
{
    IBuffer CreateBuffer<T>(ReadOnlySpan<T> data, BufferType type, BufferUsage usage) where T : unmanaged;
    IVertexArray CreateVertexArray();
    IShader CreateShader(string vertexSource, string fragmentSource);
    ITexture2D CreateTexture2D(ReadOnlySpan<byte> pixels, uint width, uint height, TextureFormat format);
    IFrameBuffer CreateFrameBuffer(int width, int height);

    void SetViewport(int x, int y, uint width, uint height);
    void SetClearColor(float r, float g, float b, float a);
    void Clear(ClearMask mask);
    void DrawArrays(PrimitiveType type, int first, int count);
    void DrawElements(PrimitiveType type, int count);
    void DrawArraysInstanced(PrimitiveType type, int first, int count, int instanceCount);
    void DrawElementsInstanced(PrimitiveType type, int count, int instanceCount);
}

public interface IBuffer : IDisposable
{
    void Bind();
    void Unbind();
    void SetData<T>(ReadOnlySpan<T> data);
}

public interface IVertexArray : IDisposable
{
    void AddVertexBuffer(IBuffer buffer, BufferLayout layout);
    void SetIndexBuffer(IBuffer buffer);
    void Bind();
    void Unbind();
}

public interface IShader : IDisposable
{
    void Bind();
    void SetUniform(string name, in Matrix4x4 value);
    void SetUniform(string name, float value);
    /* ... other uniform overloads ... */
}

public interface ITexture2D : IDisposable
{
    void Bind(int unit);
    uint Width { get; }
    uint Height { get; }
}

public interface IFrameBuffer : IDisposable
{
    void Bind();
    void Unbind();
    void Resize(int width, int height);
    ITexture2D ColorAttachment { get; }
    Vector2D<int> Size { get; }
}
```

#### IRenderer (high-level, command-based)

```csharp
public interface IRenderer : IDisposable
{
    IRenderDevice Device { get; }
    void BeginFrame(Matrix4x4 viewProjection);
    void Submit(in SpriteDrawCommand command);
    void SubmitLine(Vector3 start, Vector3 end, Vector4 color);
    void SubmitQuad(Vector3 position, Vector2 size, Vector4 color);
    void EndFrame();
    void SetRenderTarget(IFrameBuffer? target); // null = default framebuffer
    void Initialize(IRenderDevice device);
}

public readonly struct SpriteDrawCommand
{
    public required Matrix4x4 Transform;
    public required Vector4 Color;
    public required int TextureId;    // Asset key / handle
    public required int ShaderId;     // Asset key / handle
    public float PPU;
}

public readonly struct LineDrawCommand
{
    public required Vector3 Start;
    public required Vector3 End;
    public required Vector4 Color;
}

public readonly struct QuadDrawCommand
{
    public required Vector3 Position;
    public required Vector2 Size;
    public required Vector4 Color;
}
```

**OpenGL implementation**: `GLRenderDevice` and `GLRenderer` in `Renderer/OpenGL/` sub-namespace. These wrap the existing `BufferObject<T>`, `VertexArrayObject`, `ShaderHandle`, `TextureHandle`, `FrameBuffer`, `Sprite`, `Quad`, and `Gizmos`.

**Key changes from current**:
- ECS systems never reference `GL` or any Silk.NET type.
- Systems call `renderer.Submit(new SpriteDrawCommand { ... })` instead of directly creating `Sprite` objects.
- `Gizmos` is no longer a singleton. It becomes a service injected into editor layers.
- `Renderer` internal render queue sorts/sprues commands into sorted batches, then uses `IRenderDevice` to draw.

---

### 5. Assets (`LunarEngine.Assets`)

**Purpose**: Load, cache, and provide CPU-side asset data. GPU handles are owned by `IRenderDevice` / `AssetHandleCache`.

**Public API**:

```csharp
public interface IAssetProvider
{
    string ResolvePath(AssetKey key);        // logical ID to file path
    Stream OpenStream(AssetKey key);          // open file for reading
    bool Exists(AssetKey key);
}

public readonly struct AssetKey
{
    public string Category { get; } // "shader", "texture", "mesh", etc.
    public string Name { get; }     // "basic", "birb", etc.
}

public sealed class AssetManager
{
    AssetManager(IAssetProvider provider, IRenderDevice device);

    ShaderHandle LoadShader(AssetKey key);
    TextureHandle LoadTexture(AssetKey key);
    void UnloadShader(AssetKey key);
    void UnloadTexture(AssetKey key);
    void Dispose(); // dispose all GPU handles
}
```

**Key changes from current**:
- No hardcoded relative paths. `IAssetProvider.ResolvePath()` maps logical keys to actual paths.
- Default `FileSystemAssetProvider` reads from a configurable root directory.
- GPU handles created via `IRenderDevice` (not directly via `GL`).
- `AssetHandleCache` still exists but uses `IRenderDevice` instead of raw GL.
- `AssetManager.Dispose()` actually disposes all GPU resources.

---

### 6. Scenes (`LunarEngine.Scenes`)

**Purpose**: Manage scene lifecycle -- creation, destruction, activation, serialization (future).

**Public API**:

```csharp
public interface IScene
{
    string Name { get; }
    IWorld World { get; }
    SystemScheduler Scheduler { get; }
    bool IsActive { get; set; }

    void Awake();
    void Start();
    void Update(double deltaTime);
    void FixedUpdate(double fixedDeltaTime);
    void Render(IRenderer renderer);
    void Dispose();
}

public sealed class SceneManager
{
    private readonly List<IScene> _scenes = new();
    private readonly ServiceContainer _services;

    SceneManager(ServiceContainer services);

    IScene CreateScene(string name);                    // Creates ECSScene with default systems
    IScene CreateScene(string name, Action<IWorld> configure); // Custom scene setup
    bool RemoveScene(IScene scene);
    IScene? ActiveScene { get; set; }
    IReadOnlyList<IScene> Scenes { get; }
}
```

**ECSScene changes**:
- No longer owns `Renderer`, `AssetManager`, or `GL`. These come from `ServiceContainer`.
- No longer manually calls system methods. Delegates to `SystemScheduler`.
- `RenderScenes()` becomes `Render(IRenderer renderer)` -- receives renderer by parameter.

**Key changes from current**:
- Fixed-size array replaced with `List<IScene>`.
- `ActiveScenes` (misleading plural) becomes `ActiveScene` (singular).
- Scene receives services via DI, not by storing constructor parameters.
- `RemoveScene` properly unloads and removes from list.

---

### 7. Physics (`LunarEngine.Physics`)

**Purpose**: 2D physics simulation with broadphase and fixed-step integration.

**Public API**:

```csharp
public sealed class PhysicsWorld
{
    float FixedTimeStep { get; set; } = 1f / 60f;
    Vector2 Gravity { get; set; } = new(0, -9.81f);

    void Step(IWorld world, double deltaTime);
    // Internally: accumulate time, run fixed steps, provide interpolation data
}

// PhysicsSystem is an ISystem that delegates to PhysicsWorld
public sealed class PhysicsSystem : ISystem
{
    public SystemStage Stage => SystemStage.FixedUpdate;
    public int Order => 0;
    // Owns PhysicsWorld, runs during FixedUpdate stage
}
```

**Key changes from current**:
- `PhysicsEngine` static class removed. Replaced by `PhysicsWorld` instance.
- `PhysicsLayer` removed (scheduling handled by `SystemScheduler`).
- O(n^2) collision replaced with simple spatial hash / uniform grid broadphase.
- Interpolation between physics steps built in.

---

### 8. Input (`LunarEngine.Input`)

**Purpose**: Bridge OS input events to an `InputState` struct consumed by systems.

**Public API**:

```csharp
public sealed class InputManager
{
    InputManager(IInputContext context);
    InputState State { get; } // Read-only snapshot for the current frame

    event Action<Key> KeyDown;
    event Action<Key> KeyUp;
    event Action<Vector2> MouseMoved;
    event Action<float> MouseScrolled;

    void Update(); // Called once per frame to update State
}

public readonly struct InputState
{
    public bool IsKeyDown(Key key);
    public bool IsKeyPressed(Key key); // down this frame, up last frame
    public bool IsKeyReleased(Key key); // up this frame, down last frame
    public Vector2 MousePosition { get; }
    public Vector2 MouseDelta { get; }
    public float ScrollDelta { get; }
    public Vector2 GetAxis(string name); // WASD, arrow keys, etc.
}
```

**Key changes from current**:
- No more `Input.Instance` singleton.
- No hardcoded WASD in constructor. Axis mappings are configurable.
- `InputState` is a read-only snapshot that systems read via `ServiceContainer.Get<InputState>()` or through `IWorld` resources.
- `InputManager` is created by composition root, receives `IInputContext`.

---

### 9. UI / ImGui (`LunarEngine.UI`)

**Purpose**: ImGui rendering backend. Does not include any editor logic.

**Public API**:

```csharp
public sealed class ImGuiController : IDisposable
{
    ImGuiController(IRenderDevice device, IWindow window, IInputContext input);
    void Initialize();
    void Update(TimeStep dt);
    void Render();
    void Resize(int width, int height);
}
```

**Key changes from current**:
- `ImGuiController` receives `IRenderDevice` instead of raw `GL`.
- `ImGuiLayer` remains a `BaseLayer` that wraps Begin/End.
- No editor logic in this module.

---

### 10. Application (`LunarEngine.Application`)

**Purpose**: Window bootstrap, main loop, service container setup, layer stack.

**Public API**:

```csharp
public class Application : IDisposable
{
    protected ServiceContainer Services { get; }
    protected LayerStack Layers { get; }
    protected IWindow Window { get; }
    protected IRenderer Renderer { get; }
    protected AssetManager Assets { get; }
    protected SceneManager Scenes { get; }
    protected InputManager Input { get; }

    Application(); // Sets up defaults
    virtual void Initialize(); // Override to register services and push layers
    void Run(); // Main loop
    void PushLayer(BaseLayer layer);
    void PushOverlay(BaseLayer overlay);
    void SubmitToMainThread(Action action);
    void Dispose();
}

public abstract class BaseLayer
{
    string Name { get; }
    virtual void OnAttach();
    virtual void OnDetach();
    virtual void OnInitialize();
    virtual void OnUpdate(TimeStep dt);
    virtual void OnImguiRender(TimeStep dt);
}

public sealed class LayerStack : IEnumerable<BaseLayer>
{
    void PushLayer(BaseLayer layer);
    void PushOverlay(BaseLayer overlay);
    void PopLayer(BaseLayer layer);
    void PopOverlay(BaseLayer overlay);
    IEnumerator<BaseLayer> GetEnumerator();
}
```

**Key changes from current**:
- `Application` no longer directly creates `GL`, `Renderer`, `AssetManager`, etc. These are registered in `ServiceContainer` during `Initialize()` or by the composition root.
- Services are resolved from `ServiceContainer`, not created in `OnWindowLoad()`.
- `Time.DeltaTime` static removed. `TimeStep` is passed through the layer update chain.
- `Gizmos.Instance.AssetManager` removed. Gizmos receives its dependencies through DI.

---

### 11. Editor (`LunarEngine.Editor`)

**Purpose**: Application layer on top of the engine. All editor-specific UI, hierarchy, inspector, and scene viewport live here.

**Public API**:

```csharp
public sealed class Editor : Application
{
    override void Initialize(); // Push ImGuiLayer, EditorLayer, GizmosLayer
}

public sealed class EditorLayer : BaseLayer
{
    EditorLayer(ServiceContainer services);

    override void OnInitialize(); // Create editor systems, register inspectors
    override void OnUpdate(TimeStep dt);
    override void OnImguiRender(TimeStep dt);
}

// Editor-only ECS systems that live in Editor/Systems/
// These systems have [Query] attributes and run on the editor's World
```

**Key changes from current**:
- Editor uses a completely separate `IWorld` from the game runtime world (dual-world isolation).
- Editor systems (Hierarchy, Inspector, SceneView) are `ISystem` implementations registered with the editor's scheduler.
- `EditorCamera` is no longer a static-aware class -- it reads `InputState` from the service container.
- Event communication between editor systems uses a scoped `EventBus<T>` owned by `Application.Services`.
- No `EditorCameraInputHandler` with Arch `[Event]` attributes -- replaced by direct `InputState` reads.

---

## Composition Root (`Program.cs`)

The composition root wires everything together:

```csharp
public class Program
{
    static void Main(string[] args)
    {
        var app = new Editor();
        app.Run();
    }
}

// Inside Editor.Initialize():
protected override void Initialize()
{
    // Services are already registered by Application() constructor:
    // - IWindow -> SilkWindow
    // - IInputContext -> SilkInputContext
    // - IRenderDevice -> GLRenderDevice
    // - IRenderer -> GLRenderer
    // - AssetManager
    // - SceneManager
    // - InputManager
    // - PhysicsWorld

    var imGuiLayer = new ImGuiLayer(Services);
    var editorLayer = new EditorLayer(Services);
    var gizmosLayer = new GizmosLayer(Services);

    PushOverlay(imGuiLayer);
    PushLayer(editorLayer);
    PushLayer(gizmosLayer);
}
```

`Application()` constructor registers all core services:

```csharp
public Application()
{
    Services = new ServiceContainer();

    // Platform
    var window = new SilkWindow(width: 1280, height: 720);
    Services.Register<IWindow>(window);
    var input = new SilkInputContext(window);
    Services.Register<IInputContext>(input);

    // Platform must be initialized before renderer
    window.Initialize(); // Creates GL context

    // Renderer
    var device = new GLRenderDevice(window.GLContext);
    Services.Register<IRenderDevice>(device);
    var renderer = new GLRenderer(device);
    Services.Register<IRenderer>(renderer);

    // Input
    var inputManager = new InputManager(input);
    Services.Register<InputManager>(inputManager);

    // Assets
    var assetProvider = new FileSystemAssetProvider("Resources/");
    Services.Register<IAssetProvider>(assetProvider);
    var assets = new AssetManager(assetProvider, device);
    Services.Register<AssetManager>(assets);

    // Scenes
    var scenes = new SceneManager(Services);
    Services.Register<SceneManager>(scenes);

    // Physics
    var physics = new PhysicsWorld();
    Services.Register<PhysicsWorld>(physics);

    Layers = new LayerStack();
}
```

---

## Event System Design

Replace the current dual-event-system (custom static `EventBus<T>` + Arch `EventBus`) with a scoped, typed event bus:

```csharp
// LunarEngine.Events
public sealed class EventBus : IDisposable
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<T>(Action<T> handler) where T : struct;
    public void Unsubscribe<T>(Action<T> handler) where T : struct;
    public void Publish<T>(T evt) where T : struct;

    public void Dispose(); // Clears all subscriptions
}
```

**Key design decisions**:
- Instance-based, not static. Owned by `Application.Services` or by individual `IWorld` instances.
- Type-safe: each event type `T` gets its own handler list.
- No `[Event]` attribute reflection. Explicit subscribe/unsubscribe.
- Event definitions live in `LunarEngine.Events/` as plain structs:
  ```csharp
  public readonly struct ViewportResizedEvent { public Vector2D<int> Size; }
  public readonly struct SceneLoadedEvent { public IScene Scene; }
  public readonly struct InspectorTargetSelectedEvent { public EntityReference Entity; }
  public readonly struct SceneFocusEvent { public bool IsFocused; }
  ```

---

## Component Redesign

All components use `LunarEngine.ECS.Components` namespace (or sub-namespaces for physics/input).

```csharp
// Transform components -- PRS is source of truth, LocalToWorld is derived
namespace LunarEngine.ECS.Components;

public struct Position : IComponent { public Vector3 Value; }
public struct Rotation : IComponent { public Quaternion Value; }
public struct Scale : IComponent { public Vector3 Value; }
public struct LocalToWorld : IComponent { public Matrix4x4 Value; public bool IsDirty; }
public struct Parent : IComponent { public EntityReference Entity; } // Entity reference, not Transform copy

// Rendering
public struct SpriteRenderer : IComponent { public AssetKey Texture; public AssetKey Shader; public Vector4 Color; public float PPU; }

// Camera
public struct CameraComponent : IComponent { public Camera Camera; public bool IsPrimary; }

// Physics (sub-namespace)
namespace LunarEngine.ECS.Components.Physics;
public struct RigidBody2D : IComponent { public float Mass; public float GravityScale; public Vector2 Velocity; public EBodyType BodyType; }
public struct BoxCollider2D : IComponent { public Vector2 Position; public float Width; public float Height; }

// Tags
public struct IsInstantiating : IComponent { }
public struct IsDestroying : IComponent { }
public struct Name : IComponent { public string Value; }
```

**Removed components**:
- `Transform` (replaced by `LocalToWorld`, computed by `TransformSystem`)
- `DirtyTransform` (replaced by `IsDirty` flag on `LocalToWorld`)
- `LGShader` (shader is now an `AssetKey` on `SpriteRenderer`)
- `Input` (replaced by `InputState` service)
- `TagComponent` (unused, from dead `ShaderSystem`)
- `CustomBehaviour` (entirely commented out, removed)
- `ViewProjectionEvent`, `AssignShaderEvent` (become render commands)

---

## System Redesign

All systems implement `ISystem` and declare their stage and order:

```csharp
public sealed class TransformSystem : ISystem
{
    public SystemStage Stage => SystemStage.Update;
    public int Order => 0; // Runs first in Update

    public void Initialize(IWorld world, ServiceContainer services) { ... }
    public void Update(double deltaTime) { ... }

    // Query methods use IWorld.Query<T1, T2, ...>()
    // Still uses [Query] source generator from Arch internally
}

public sealed class SpriteRenderPrepareSystem : ISystem
{
    public SystemStage Stage => SystemStage.RenderPrepare;
    public int Order => 0;

    public void Initialize(IWorld world, ServiceContainer services)
    {
        _renderer = services.Get<IRenderer>();
    }

    public void Update(double deltaTime)
    {
        // Iterate sprites, build SpriteDrawCommands, submit to renderer
        // NO direct GL calls
    }
}

public sealed class PhysicsSystem : ISystem
{
    public SystemStage Stage => SystemStage.FixedUpdate;
    public int Order => 0;

    public void Initialize(IWorld world, ServiceContainer services)
    {
        _physics = services.Get<PhysicsWorld>();
    }

    public void Update(double deltaTime)
    {
        _physics.Step(_world, deltaTime);
    }
}
```

**System execution order by stage**:

| Stage | Systems (in order) | Purpose |
|-------|---------------------|---------|
| Awake | InitializationSystem | Remove IsInstantiating tags |
| Start | (none currently) | One-time setup |
| PreUpdate | InputUpdateSystem | Capture InputState |
| FixedUpdate | PhysicsSystem | AABB collision, velocity integration |
| Update | TransformSystem then CameraSystem | Compute transforms, update cameras |
| LateUpdate | (none currently) | Post-transform logic |
| RenderPrepare | SpriteRenderPrepareSystem then GizmosPrepareSystem | Build draw commands |
| RenderSubmit | (renderer.EndFrame) | Execute GPU draws |

---

## Service Registration Flow

```
Program.Main()
  +-- new Editor()
       +-- base Application()
            |-- new ServiceContainer()
            |-- Register IWindow (SilkWindow)
            |-- Register IInputContext (SilkInputContext)
            |-- window.Initialize()  <-- creates GL context
            |-- Register IRenderDevice (GLRenderDevice)
            |-- Register IRenderer (GLRenderer)
            |-- Register InputManager
            |-- Register IAssetProvider (FileSystemAssetProvider)
            |-- Register AssetManager
            |-- Register SceneManager
            |-- Register PhysicsWorld
            |-- Register EventBus
            +-- Editor.Initialize()
                 |-- new ImGuiLayer(services)
                 |-- new EditorLayer(services)
                 +-- new GizmosLayer(services)
```

---

## Bug Fixes to Include in Refactoring

These bugs should be fixed as part of the architectural migration:

1. **`InspectorSystem.AddComponentInspector<T>` inverted TryAdd logic** -- log error on success, skip adding
2. **`EditorCameraInputHandler.OnSceneLoseFocus` uses `+=` instead of `-=`** -- events never unsubscribe
3. **Duplicate `_camera.LookAt` call** -- called twice identically
4. **`AssetManager.Dispose()` is empty** -- GPU handle leak
5. **`AssetHandleCache.ClearCache()` is empty** -- GPU handle leak
6. **`_canZoom` field never set to true** -- dead field
7. **`SceneSystem` accesses `_sceneFrameBuffer._colorTexture` and `_size`** -- breaking encapsulation
8. **`Time.DeltaTime` set twice per frame** -- redundant sets in `Application` and `SceneLayer`
9. **`SpriteRendererSystem.AdjustScale` sets random shader uniform every frame** -- debug code in production
10. **`CommandBuffer` newed every frame** in multiple systems -- GC pressure

---

## Dead Code to Remove

- `Scene.cs` (entirely commented out)
- `CustomBehaviour.cs` (entirely commented out)
- `WindowManager.cs` (entirely commented out)
- `WindowInitializedEvent.cs` (commented out)
- `ViewportResizedEvent.cs` (empty file)
- `PhysicsLayer.cs` (unused, scheduling moves to SystemScheduler)
- `RenderLayer.cs` (unused, rendering driven by IRenderer)
- `SceneLayer.cs` (unused, replaced by ECSScene lifecycle)
- `PhysicsEngine.cs` (static no-op, replaced by PhysicsWorld)
- `TestScene.cs` (commented out)
- `ShaderSystem.cs` (empty system + unrelated event/tag structs)
- `InputSystem.cs` (stub with empty UpdateInput)
- `ComponentFactoryManager.cs` and `EntityFactory` (functionality moves to `IWorld.Create`)
- `Singleton<T>` (replaced by ServiceContainer)

---

## Namespace Consolidation Map

| Current namespace | Target namespace |
|---|---|
| `LunarEngine.GameEngine` (misc) | Split into `LunarEngine.Core`, `LunarEngine.Application`, `LunarEngine.ECS` |
| `LunarEngine.GameObjects` | `LunarEngine.ECS.Components` |
| `LunarEngine.Components` | `LunarEngine.ECS.Components` |
| `LunarEngine.Engine.ECS.Components` | `LunarEngine.ECS.Components` |
| `LunarEngine.ECS.Components` | `LunarEngine.ECS.Components` |
| `LunarEngine.Engine.ECS.Systems` | `LunarEngine.ECS.Systems` |
| `LunarEngine.ECS.Systems` | `LunarEngine.ECS.Systems` |
| `LunarEngine.GameEngine` (systems) | `LunarEngine.ECS.Systems` |
| `LunarEngine.Physics` (components) | `LunarEngine.ECS.Components.Physics` |
| `LunarEngine.Physics` (systems) | `LunarEngine.ECS.Systems` |
| `LunarEngine.GameEngine` (Time, Debug) | `LunarEngine.Core` |
| `LunarEngine.Engine.Graphics` | `LunarEngine.Renderer` / `LunarEngine.Renderer.OpenGL` |
| `LunarEngine.Engine.Gizmos` | `LunarEngine.Renderer` (Gizmos as a service) |
| `LunarEngine.Engine.GameEngine` | `LunarEngine.Application` |
| `LunarEngine.Engine.Assets` | `LunarEngine.Assets` |
| `LunarEngine.Engine.Scenes` | `LunarEngine.Scenes` |
| `LunarEngine.Engine.InputEngine` | `LunarEngine.Input` |
| `LunarEngine.Engine.Core` | `LunarEngine.Core` |
| `LunarEngine.Engine.Debugger` | `LunarEngine.Core` |
| `LunarEngine.Engine.ECS.ComponentFactories` | Removed (use IWorld.Create) |
| `LunarEngine.Engine.AssetHandleCache` | `LunarEngine.Assets` |
| `LunarEngine.Events` | `LunarEngine.Events` (unchanged) |
| `LunarEngine.Utilities` | `LunarEngine.Utilities` (unchanged) |

---

## Implementation Phases

Each phase must build and run before proceeding to the next.

### Phase 1: Hygiene and bug fixes (no architecture changes)
- Fix all 10 bugs listed above
- Remove all dead code files
- Normalize all `using` statements to match current namespaces (don't rename yet)
- Verify `dotnet build -c Debug` and `dotnet run -c Debug` still work

### Phase 2: Core module extraction
- Create `ServiceContainer` and `ServiceDescriptor` in `Core/`
- Replace `Singleton<T>` usage with service registration
- Make `EventBus<T>` instance-based instead of static
- Move `TimeStep` and `Logger` to `Core/`

### Phase 3: Platform abstraction
- Create `IWindow` and `IInputContext` interfaces in `Platform/`
- Create `SilkWindow` and `SilkInputContext` wrappers
- Refactor `Application` to depend on `IWindow`/`IInputContext` instead of Silk.NET directly
- Wire `Application` constructor to register platform services

### Phase 4: Renderer abstraction
- Create `IRenderDevice`, `IRenderer`, `IBuffer`, `ITexture2D`, `IShader`, `IFrameBuffer`, `IVertexArray` interfaces in `Renderer/Abstractions/`
- Create `GLRenderDevice`, `GLRenderer`, `GLBuffer`, `GLTexture`, `GLShader`, `GLFrameBuffer`, `GLVertexArray` implementations in `Renderer/OpenGL/`
- Port `Sprite`, `Quad`, and `Renderer` internals behind these interfaces
- Remove direct `GL` references from all ECS systems -- they use `IRenderer.Submit()` instead
- Remove `Gizmos` singleton; make it a render service

### Phase 5: ECS abstraction layer
- Create `IEntity`, `IWorld`, `ISystem`, `SystemStage` interfaces in `ECS/Abstractions/`
- Create `ECSWorld` wrapper that delegates to Arch's `World`
- Create `SystemScheduler` with stage-based execution
- Convert all systems to implement `ISystem`
- Move `ECSScene` to use `SystemScheduler` instead of manual calls
- Fix `Parent` component to use `EntityReference`

### Phase 6: Input state resource
- Create `InputState` struct and `InputManager` class in `Input/`
- Remove `Input` singleton class
- Make `InputManager` register keyboard/mouse callbacks on `IInputContext`
- Update systems to read `InputState` from `ServiceContainer`
- Remove hardcoded WASD from input class

### Phase 7: Asset provider
- Create `IAssetProvider` and `AssetKey` in `Assets/`
- Create `FileSystemAssetProvider` that reads from configurable root
- Remove hardcoded paths from `ShaderLibrary` and `TextureLibrary`
- Make `AssetManager` use `IRenderDevice` instead of raw `GL` for GPU handle creation
- Implement `AssetManager.Dispose()` properly

### Phase 8: Scene manager
- Convert `SceneManager` to use `List<IScene>` instead of fixed array
- Rename `ActiveScenes` to `ActiveScene`
- Implement proper `RemoveScene` with cleanup
- Create `IScene` interface and refactor `ECSScene` to implement it

### Phase 9: Physics consolidation
- Create `PhysicsWorld` class (no longer static)
- Move fixed-step accumulation into `SystemScheduler.FixedUpdate` stage
- Add simple spatial hash broadphase to collision detection
- Remove `PhysicsLayer` and `PhysicsEngine` static class

### Phase 10: Editor/runtime separation
- Editor creates its own `IWorld` for editor-only systems
- Game scene uses a separate `IWorld`
- `EditorCamera` reads from `InputState` service, not static `Input`
- Remove Arch `[Event]` bus usage; replace with scoped `EventBus` from `Core/`
- Inspector events use typed event structs

### Phase 11: Namespace consolidation
- Rename all namespaces per the map above
- Update all `using` statements
- Verify full build and runtime

### Phase 12: Disposal and lifecycle
- Implement `Dispose()` on all GPU resources (`ShaderHandle`, `TextureHandle`, `FrameBuffer`, `BufferObject`)
- Define disposal order in `Application.Dispose()`: layers, scenes, assets, renderer, window
- Add GL debug output in Debug builds
- Add frame timing metrics

---

## Summary of Key Architectural Changes

| Aspect | Current | Target |
|--------|---------|--------|
| DI pattern | Global singletons (`Singleton<T>`, `.Instance`) | `ServiceContainer` with interface-based registration |
| Window/input | Direct Silk.NET in Application | `IWindow` / `IInputContext` abstractions |
| ECS | Direct Arch usage with `ScriptableSystem` | `IWorld` / `ISystem` / `SystemScheduler` wrappers |
| Components | Spread across 5 namespaces | Unified under `LunarEngine.ECS.Components` |
| Parent component | Stores `Transform` copy | Stores `EntityReference` |
| Transform | `Transform` component + PRS with dirty flags | PRS is source of truth, `LocalToWorld` is derived |
| Renderer | Direct `GL` usage in systems | `IRenderDevice` + `IRenderer` interfaces, command-based submission |
| Events | Dual system (static `EventBus<T>` + Arch `[Event]`) | Scoped instance-based `EventBus` |
| Input | `Input.Instance` singleton with hardcoded WASD | `InputManager` to `InputState` service |
| Assets | Hardcoded relative paths, `AssetHandleCache` leaks | `IAssetProvider` + `AssetKey`, proper GPU disposal |
| Scenes | Fixed-size array, incomplete removal | `List<IScene>`, proper lifecycle |
| Physics | Static no-op + inline ECS system | `PhysicsWorld` instance, scheduled via `SystemStage.FixedUpdate` |
| Editor | Shares game `World`, global event bus | Separate editor `IWorld`, scoped events |
| Namespace | 5+ namespaces for components, 4 for systems | One per module (e.g., `LunarEngine.ECS.Components`) |