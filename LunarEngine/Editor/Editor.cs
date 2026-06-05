using Arch.Bus;
using Hexa.NET.ImGui;
using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Components;
using LunarEngine.Engine.Gizmos;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.InputEngine;
using LunarEngine.Scenes;
using LunarEngine.UI;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace LunarEngine.ECS.Systems;

public class Editor : Application
{
    public override void Initialize()
    {
        var device = Services.Get<IRenderDevice>();
        var imguiLayer = new ImGuiLayer("ImguiLayer", Window, device, InputContext);
        PushOverlay(imguiLayer);
        RegisterImGuiLayer(imguiLayer);

        PushLayer(new EditorLayer(SceneManager, Renderer, Input, AssetManager));
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
    private readonly Input _input;
    private readonly AssetManager _assetManager;

    public EditorLayer(SceneManager sceneManager, IRenderer renderer, Input input,
                       AssetManager assetManager)
        : base("Editor")
    {
        _sceneManager = sceneManager;
        _renderer = renderer;
        _input = input;
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
        
        _cameraInputHandler = new(_camera, _input);
        
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
    private Input _inputManager;
    private bool _canPan;
    private bool _canRotate;
    private EditorCamera _camera;
    private (Position, Rotation, Transform) _focusedEntityComponents;
    public EditorCameraInputHandler(EditorCamera camera, Input inputManager)
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
        _inputManager.AddMouseDownListener(MouseButton.Right, OnRightMouseDown);
        _inputManager.AddMouseDownListener(MouseButton.Middle, OnMiddleMouseHeld);
        _inputManager.AddMouseUpListener(MouseButton.Right, OnRightMouseUp);
        _inputManager.AddMouseUpListener(MouseButton.Middle, OnMiddleMouseUp);
        _inputManager.AddKeyDownListener(Key.F, OnFKeyPressed);
        _inputManager.OnMouseMoved += OnMouseMoved;
        _inputManager.OnMouseScrolled += OnMouseScrolled;
        _inputManager.OnKeyboardAxisInput += OnKeyboardAxisMoved;
    }

    private void OnFKeyPressed(Key obj)
    {
        _camera.LookAt(_focusedEntityComponents.Item1.Value, _focusedEntityComponents.Item3, _focusedEntityComponents.Item2.Value);
    }

    public void OnSceneLoseFocus()
    {
        OnMiddleMouseUp(MouseButton.Middle);
        OnRightMouseUp(MouseButton.Right);
        _inputManager.RemoveMouseDownListener(MouseButton.Right, OnRightMouseDown);
        _inputManager.RemoveMouseDownListener(MouseButton.Middle, OnMiddleMouseHeld);
        _inputManager.RemoveMouseUpListener(MouseButton.Right, OnRightMouseUp);
        _inputManager.RemoveMouseUpListener(MouseButton.Middle, OnMiddleMouseUp);
        _inputManager.RemoveKeyDownListener(Key.F, OnFKeyPressed);
        _inputManager.OnMouseMoved -= OnMouseMoved;
        _inputManager.OnMouseScrolled -= OnMouseScrolled;
    }

    private void OnMouseScrolled(float obj)
    {
        if (!_canRotate) return;
        _camera.MouseZoom(obj);
    }

    private void OnMiddleMouseUp(MouseButton obj)
    {
        _canPan = false;
    }

    private void OnMiddleMouseHeld(MouseButton obj)
    {
        _canPan = true;
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

    private void OnRightMouseUp(MouseButton obj)
    {
        _canRotate = false;
        _inputManager.LockAndHideCursor(false);
    }

    private void OnRightMouseDown(MouseButton obj)
    {
        _canRotate = true;
        _inputManager.LockAndHideCursor(true);
    }

    private void OnKeyboardAxisMoved(Vector2 inputAxis)
    {
        if (_canRotate)
        {
            _camera.KeyboardMove(inputAxis);
        }
    }
}
