using LunarEngine.Assets;
using LunarEngine.Engine.Graphics;
using LunarEngine.Events;
using LunarEngine.InputEngine;
using LunarEngine.Physics;
using LunarEngine.Scenes;
using LunarEngine.UI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.GameEngine;

public static class Time
{
    public static TimeStep DeltaTime { get; internal set; }
}

public class Application : IDisposable
{
    private LayerStack _layerStack = new();
    private ImGuiLayer? _imGuiLayer;
    private Queue<Action> _mainThreadQueue = new();
    private object _mainThreadQueueLock = new();
    private bool _isRunning;

    // Services
    private IWindow _window = null!;
    private IInputContext _inputContext = null!;
    private GL _gl;
    private Renderer _renderer = null!;
    private Input _input = null!;
    private AssetManager _assetManager = null!;
    private SceneManager _sceneManager = null!;

    public Renderer Renderer => _renderer;
    public Input Input => _input;
    public AssetManager AssetManager => _assetManager;
    public SceneManager SceneManager => _sceneManager;

    public Vector2D<int> WindowSize => _window.Size;

    protected Application() { }

    public virtual void Initialize() { }

    public void Run()
    {
        _isRunning = true;
        CreateWindow();
        _window.FramebufferResize += OnViewportResize;
        _window.Update += OnUpdate;
        _window.Closing += OnClose;
        _window.Run();
    }

    public void PushLayer(BaseLayer layer)
    {
        _layerStack.PushLayer(layer);
        layer.OnAttach();
    }

    public void PushOverlay(BaseLayer layer)
    {
        _layerStack.PushOverlay(layer);
        layer.OnAttach();
    }

    protected void RegisterImGuiLayer(ImGuiLayer layer)
    {
        _imGuiLayer = layer;
    }

    public void SubmitToMainThread(Action action)
    {
        lock (_mainThreadQueueLock)
        {
            _mainThreadQueue.Enqueue(action);
        }
    }

    public void ExecuteMainThreadQueue()
    {
        lock (_mainThreadQueueLock)
        {
            while (_mainThreadQueue.Count > 0)
            {
                var action = _mainThreadQueue.Dequeue();
                action?.Invoke();
            }
        }
    }

    public void Dispose() { }

    private void CreateWindow(string title = "Lunar Editor", int width = 1280, int height = 720)
    {
        var options = WindowOptions.Default;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);
        _window = Window.Create(options);
        _window.Load += OnWindowLoad;
    }

    private void OnWindowLoad()
    {
        var gl = GL.GetApi(_window);
        _inputContext = _window.CreateInput();

        _input = new Input();
        _input.InputContext = _inputContext;
        foreach (var keyboard in _inputContext.Keyboards)
        {
            keyboard.KeyDown += _input.OnKeyDown;
            keyboard.KeyUp += _input.OnKeyUp;
        }
        foreach (var mouse in _inputContext.Mice)
        {
            mouse.MouseDown += _input.OnMouseDown;
            mouse.MouseUp += _input.OnMouseUp;
            mouse.MouseMove += _input.OnMouseMove;
            mouse.Scroll += _input.OnMouseScroll;
        }

        _renderer = new Renderer(gl);
        _renderer.Initialize();

        _assetManager = new AssetManager();
        _assetManager.Initialize(gl);
        Gizmos.Instance.AssetManager = _assetManager;

        _sceneManager = new SceneManager();

        Initialize();

        foreach (var layer in _layerStack)
        {
            layer.OnInitialize();
        }
    }

    private void OnUpdate(double dt)
    {
        ExecuteMainThreadQueue();
        Time.DeltaTime = new TimeStep(dt);

        foreach (var layer in _layerStack)
        {
            layer.OnUpdate(Time.DeltaTime);
        }

        if (_imGuiLayer != null)
        {
            _imGuiLayer.Begin();
            foreach (var layer in _layerStack)
            {
                layer.OnImguiRender(Time.DeltaTime);
            }
            _imGuiLayer.End();
        }
    }

    private void OnClose()
    {
        _isRunning = false;
    }

    private void OnViewportResize(Vector2D<int> viewport)
    {
        _layerStack.InvokeEvent();
    }
}
