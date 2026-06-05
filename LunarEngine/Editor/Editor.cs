using Arch.Bus;
using Hexa.NET.ImGui;
using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Components;
using LunarEngine.Engine.Gizmos;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Input;
using LunarEngine.Scenes;
using LunarEngine.UI;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace LunarEngine.ECS.Systems;

using EntityReference = Arch.Core.EntityReference;
using World = Arch.Core.World;

public class Editor : Application
{
    public override void Initialize()
    {
        var device = Services.Get<IRenderDevice>();
        var imguiLayer = new ImGuiLayer("ImguiLayer", Window, device, InputContext);
        PushOverlay(imguiLayer);
        RegisterImGuiLayer(imguiLayer);

        PushLayer(new EditorLayer(SceneManager, Renderer, InputManager, AssetManager));
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
    
    private readonly SceneManager _sceneManager;
    private readonly IRenderer _renderer;
    private readonly InputManager _inputManager;
    private readonly AssetManager _assetManager;

    public EditorLayer(SceneManager sceneManager, IRenderer renderer, InputManager inputManager,
                       AssetManager assetManager)
        : base("Editor")
    {
        _sceneManager = sceneManager;
        _renderer = renderer;
        _inputManager = inputManager;
        _assetManager = assetManager;
    }

    public override void OnInitialize()
    {
        _sceneManager.AddScene(new TestEcsScene(_renderer, _assetManager));
        _scene = _sceneManager.ActiveScenes;
        _scene.Awake();
        _scene.Start();
        _hierarchySystem = new HierarchySystem(_scene.World);
        _inspectorSystem = new InspectorSystem(_scene.World);
        _sceneSystem = new SceneSystem(_renderer);
        _hierarchySystem.Awake();
        _inspectorSystem.Awake();
        _sceneSystem.Awake();
        
        _cameraInputHandler = new(_camera, _inputManager);
        
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
        _cameraInputHandler.Update(_inputManager.State);
        _scene.Update(timeStep);
    }

    public override void OnImguiRender(TimeStep timeStep)
    {
        _sceneSystem.Draw(_scene, _camera, timeStep);
        _hierarchySystem.Update(timeStep);
        _inspectorSystem.Update(timeStep);
    }
}

public partial class EditorCameraInputHandler
{
    private InputManager _inputManager;
    private bool _canPan;
    private bool _canRotate;
    private EditorCamera _camera;
    private (Position, Rotation, Transform) _focusedEntityComponents;
    public EditorCameraInputHandler(EditorCamera camera, InputManager inputManager)
    {
        Hook();
        _camera = camera;
        _inputManager = inputManager;
    }

    [Event(order: 0)]
    public void OnInspectorTargetSelected(InspectorTarget target)
    {
        var entityPos = target.EntityWorld.Get<Position, Transform>(target.Entity);
        _focusedEntityComponents.Item1 = entityPos.t0;
        _focusedEntityComponents.Item2 = new Rotation()
        {
            Value = Quaternion.Identity
        };
        _focusedEntityComponents.Item3 = entityPos.t1;
        if (target.EntityWorld.Has<Rotation>(target.Entity))
        {
            var entityRot = target.EntityWorld.Get<Rotation>(target.Entity);
            _focusedEntityComponents.Item2 = entityRot;
        }
    }
    [Event(order: 1)]
    public void OnSceneFocusEvent(SceneFocusEvent evt)
    {
        if (evt.IsFocused)
        {
            OnSceneFocused();
        }
        else
        {
            OnSceneLoseFocus();
        }
    }
    public void OnSceneFocused()
    {
        _inputManager.MouseDown += OnMouseDown;
        _inputManager.MouseUp += OnMouseUp;
        _inputManager.KeyDown += OnKeyDown;
        _inputManager.MouseMoved += OnMouseMoved;
        _inputManager.MouseScrolled += OnMouseScrolled;
    }

    private void OnKeyDown(Key key)
    {
        if (key == Key.F)
        {
            _camera.LookAt(_focusedEntityComponents.Item1.Value, _focusedEntityComponents.Item3, _focusedEntityComponents.Item2.Value);
        }
    }

    public void OnSceneLoseFocus()
    {
        OnMouseUp(MouseButton.Middle);
        OnMouseUp(MouseButton.Right);
        _inputManager.MouseDown -= OnMouseDown;
        _inputManager.MouseUp -= OnMouseUp;
        _inputManager.KeyDown -= OnKeyDown;
        _inputManager.MouseMoved -= OnMouseMoved;
        _inputManager.MouseScrolled -= OnMouseScrolled;
    }

    private void OnMouseScrolled(float delta)
    {
        if (!_canRotate) return;
        _camera.MouseZoom(delta);
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

    private void OnMouseMoved(Vector2 delta)
    {
        if (_canPan)
        {
            _camera.MousePan(delta);
        }

        if (_canRotate)
        {
            _camera.MouseRotate(delta);
        }
    }

    public void Update(InputState state)
    {
        if (_canRotate)
        {
            _camera.KeyboardMove(state.GetAxis("Movement"));
        }
    }
}
