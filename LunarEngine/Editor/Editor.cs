using Hexa.NET.ImGui;
using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.ECS;
using LunarEngine.ECS.Components;
using LunarEngine.Core;
using LunarEngine.Renderer;
using LunarEngine.Events;
using EngineApp = LunarEngine.Application.Application;
using LunarEngine.Application;
using LunarEngine.Input;
using LunarEngine.Scenes;
using LunarEngine.UI;
using LunarEngine.Editor.Systems;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace LunarEngine.Editor;

public class Editor : EngineApp
{
    public override void Initialize()
    {
        var device = Services.Get<IRenderDevice>();
        var imguiLayer = new ImGuiLayer("ImguiLayer", Window, device, InputContext);
        PushOverlay(imguiLayer);
        RegisterImGuiLayer(imguiLayer);

        PushLayer(new EditorLayer(SceneManager, Renderer, InputManager, AssetManager, Services));
        PushLayer(new GizmosLayer(SceneManager, Renderer));
    }
}

public class EditorLayer : BaseLayer
{
    private SceneSystem _sceneSystem = null!;
    private HierarchySystem _hierarchySystem = null!;
    private InspectorSystem _inspectorSystem = null!;
    private ECSScene _scene = null!;
    private EditorCamera _camera = new();
    private EditorCameraInputHandler _cameraInputHandler = null!;
    private ECSWorld _editorWorld = null!;

    private readonly SceneManager _sceneManager;
    private readonly IRenderer _renderer;
    private readonly InputManager _inputManager;
    private readonly AssetManager _assetManager;
    private readonly ServiceContainer _services;

    public EditorLayer(SceneManager sceneManager, IRenderer renderer, InputManager inputManager,
                       AssetManager assetManager, ServiceContainer services)
        : base("Editor")
    {
        _sceneManager = sceneManager;
        _renderer = renderer;
        _inputManager = inputManager;
        _assetManager = assetManager;
        _services = services;
    }

    public override void OnInitialize()
    {
        _editorWorld = new ECSWorld();

        _sceneManager.AddScene(new TestEcsScene(_services));
        _scene = (ECSScene)_sceneManager.ActiveScene!;
        _scene.Awake();
        _scene.Start();

        var inspectorEventBus = new EventBus<InspectorTargetSelectedEvent>();
        var sceneFocusEventBus = new EventBus<SceneFocusEvent>();

        var gameWorld = _sceneManager.ActiveScene!.World;
        _hierarchySystem = new HierarchySystem(gameWorld, inspectorEventBus);
        _inspectorSystem = new InspectorSystem(gameWorld, inspectorEventBus);
        _sceneSystem = new SceneSystem(_renderer, sceneFocusEventBus);
        _cameraInputHandler = new EditorCameraInputHandler(_camera, _inputManager, inspectorEventBus, sceneFocusEventBus);

        _hierarchySystem.Awake();
        _inspectorSystem.Awake();
        _sceneSystem.Awake();

        List<IComponentInspector> componentInspectors =
            [
                new CameraInspector(),
                new NameInspector(),
                new PositionInspector(),
                new RigidBody2DInspector(),
                new RotationInspector(),
                new ScaleInspector(),
                new SpriteRendererInspector(_assetManager),
                new BoxCollider2DInspector()
            ];
        foreach (var inspector in componentInspectors)
        {
            _inspectorSystem.AddComponentInspector(inspector.ComponentType, inspector);
        }
    }

    public override void OnUpdate(TimeStep timeStep)
    {
        _camera.Update();
        _cameraInputHandler.Update(_inputManager.State, (float)timeStep);
        _scene.Update(timeStep);
        _scene.Tick(timeStep);
    }

    public override void OnImguiRender(TimeStep timeStep)
    {
        _sceneSystem.Draw(_scene, _camera, timeStep);
        _hierarchySystem.Update(timeStep);
        _inspectorSystem.Update(timeStep);
    }
}

public class EditorCameraInputHandler
{
    private readonly InputManager _inputManager;
    private bool _canPan;
    private bool _canRotate;
    private readonly EditorCamera _camera;
    private (Position, Rotation, Transform) _focusedEntityComponents;

    public EditorCameraInputHandler(EditorCamera camera, InputManager inputManager,
                                   EventBus<InspectorTargetSelectedEvent> inspectorBus,
                                   EventBus<SceneFocusEvent> focusBus)
    {
        _camera = camera;
        _inputManager = inputManager;
        inspectorBus.Subscribe(OnInspectorTargetSelected);
        focusBus.Subscribe(OnSceneFocusChanged);
    }

    private void OnInspectorTargetSelected(InspectorTargetSelectedEvent evt)
    {
        var components = evt.World.Get<Position, Transform>(evt.Entity);
        _focusedEntityComponents.Item1 = components.t0;
        _focusedEntityComponents.Item3 = components.t1;
        _focusedEntityComponents.Item2 = evt.World.Has<Rotation>(evt.Entity)
            ? evt.World.Get<Rotation>(evt.Entity)
            : new Rotation { Value = Quaternion.Identity };
    }

    private void OnSceneFocusChanged(SceneFocusEvent evt)
    {
        if (evt.IsFocused)
        {
            _inputManager.MouseDown += OnMouseDown;
            _inputManager.MouseUp += OnMouseUp;
        }
        else
        {
            OnMouseUp(MouseButton.Middle);
            OnMouseUp(MouseButton.Right);
            _inputManager.MouseDown -= OnMouseDown;
            _inputManager.MouseUp -= OnMouseUp;
        }
    }

    public void Update(InputState state, float deltaTime)
    {
        if (state.IsKeyPressed(Key.F))
        {
            _camera.LookAt(_focusedEntityComponents.Item1.Value, _focusedEntityComponents.Item3, _focusedEntityComponents.Item2.Value);
        }

        if (state.MouseDelta != Vector2.Zero)
        {
            if (_canPan) _camera.MousePan(state.MouseDelta, deltaTime);
            if (_canRotate) _camera.MouseRotate(state.MouseDelta, deltaTime);
        }

        if (state.ScrollDelta != 0)
        {
            if (_canRotate) _camera.MouseZoom(state.ScrollDelta, deltaTime);
        }

        if (_canRotate)
        {
            _camera.KeyboardMove(state.GetAxis("Movement"), deltaTime);
        }
    }

    private void OnMouseDown(MouseButton button)
    {
        if (button == MouseButton.Right)
        {
            _canRotate = true;
            _inputManager.SetCursorLock(true);
        }
        else if (button == MouseButton.Middle)
        {
            _canPan = true;
        }
    }

    private void OnMouseUp(MouseButton button)
    {
        if (button == MouseButton.Right)
        {
            _canRotate = false;
            _inputManager.SetCursorLock(false);
        }
        else if (button == MouseButton.Middle)
        {
            _canPan = false;
        }
    }
}
