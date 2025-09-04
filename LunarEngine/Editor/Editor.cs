using System.Numerics;
using Arch.Buffer;
using Arch.Bus;
using Hexa.NET.ImGui;
using LunarEngine.Components;
using LunarEngine.Events;
using LunarEngine.Engine.Gizmos;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.InputEngine;
using LunarEngine.Physics;
using LunarEngine.Scenes;
using LunarEngine.UI;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace LunarEngine.ECS.Systems;

public class Editor : Application
{
    private SceneManager _sceneManager;
    public override void InitSingleton()
    {
        _sceneManager = new();
        base.InitSingleton();
        PushLayer(new EditorLayer(_sceneManager));
        PushLayer(new GizmosLayer(_sceneManager));
    }
}

public class EditorLayer : BaseLayer
{
    private SceneSystem _sceneSystem;
    private HierarchySystem _hierarchySystem;
    private InspectorSystem _inspectorSystem;
    private ECSScene _scene;
    private float _accumulatedTime;
    private EditorCamera _camera = new();
    private EditorCameraInputHandler _cameraInputHandler;
    private FrameBuffer _sceneFrameBuffer;
    
    private SceneManager _sceneManager;

    public EditorLayer(SceneManager sceneManager) : base("Editor")
    {
        _sceneManager = sceneManager;
    }

    public override void OnAttach()
    {
        _sceneManager.AddScene(new TestEcsScene());
        _scene = _sceneManager.ActiveScenes;
        _hierarchySystem = new HierarchySystem(_scene.World);
        _inspectorSystem = new InspectorSystem(_scene.World);
        _sceneSystem = new SceneSystem();
        _hierarchySystem.Awake();
        _inspectorSystem.Awake();
        _sceneSystem.Awake();
        
        _sceneFrameBuffer = new FrameBuffer(Renderer.Instance.Api, new Vector2D<int>(800, 600));
        EventBus<ViewportResizedEvent>.Register(OnViewportResized);
        _cameraInputHandler = new (_camera, Input.Instance);
    }

    public override void OnDetach()
    {
        EventBus<ViewportResizedEvent>.Deregister(OnViewportResized);
        _sceneFrameBuffer?.Dispose();
    }

    public override void OnUpdate(TimeStep timeStep)
    {
        
        Time.DeltaTime = timeStep;
        _accumulatedTime += timeStep;
        
        // Input
        Input.Instance.Update(timeStep);

        
        // Physics
        while (_accumulatedTime >= PhysicsEngine.FIXED_TIMESTAMP)
        {
            PhysicsEngine.TickPhysics(PhysicsEngine.FIXED_TIMESTAMP);
            _scene.Tick(PhysicsEngine.FIXED_TIMESTAMP);
            _accumulatedTime -= PhysicsEngine.FIXED_TIMESTAMP;
        }
        PhysicsEngine.InterpolatedTime = (_accumulatedTime / PhysicsEngine.FIXED_TIMESTAMP);
        
        // Scene dispatcher
        _scene.Update(timeStep);
        _camera.Update();

    }

    public override void OnImguiRender(TimeStep timeStep)
    {
        Renderer.Instance.Clear();
        _sceneSystem.Draw(_scene, _camera, timeStep);
        _hierarchySystem.Update(timeStep);
        _inspectorSystem.Update(timeStep);
    }

    private void HandleEditorCameraInput()
    {
    }
}

public partial class EditorCameraInputHandler
{
    private Input _inputManager;
    private bool _canPan;
    private bool _canRotate;
    private bool _canZoom;
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
        _inputManager.OnMouseMoved += OnMouseMoved;
        _inputManager.OnMouseScrolled += OnMouseScrolled;
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

    private void OnViewportResized(ViewportResizedEvent evt)
    {
        // Resize editor scene framebuffer to match window for now
        _sceneFrameBuffer.Bind();
        _sceneFrameBuffer.Resize(evt.Size);
        _sceneFrameBuffer.Unbind();
    }
}