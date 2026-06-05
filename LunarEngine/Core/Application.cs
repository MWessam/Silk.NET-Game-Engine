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
    protected IWindow Window = null!;
    protected IInputContext InputContext = null!;
    protected GL GL;
    private Renderer _renderer = null!;
    private Input _input = null!;
    private AssetManager _assetManager = null!;
    private SceneManager _sceneManager = null!;

    public Renderer Renderer => _renderer;
    public Input Input => _input;
    public AssetManager AssetManager => _assetManager;
    public SceneManager SceneManager => _sceneManager;

    public Vector2D<int> WindowSize => Window.Size;

    protected Application() { }

    public virtual void Initialize() { }

    public void Run()
    {
        _isRunning = true;
        CreateWindow();
        Window.FramebufferResize += OnViewportResize;
        Window.Update += OnUpdate;
        Window.Closing += OnClose;
        Window.Run();
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
        Window = Silk.NET.Windowing.Window.Create(options);
        Window.Load += OnWindowLoad;
    }

    private void OnWindowLoad()
    {
        GL = Silk.NET.OpenGL.GL.GetApi(Window);
        InputContext = Window.CreateInput();

        _input = new Input();
        _input.InputContext = InputContext;
        foreach (var keyboard in InputContext.Keyboards)
        {
            keyboard.KeyDown += _input.OnKeyDown;
            keyboard.KeyUp += _input.OnKeyUp;
        }
        foreach (var mouse in InputContext.Mice)
        {
            mouse.MouseDown += _input.OnMouseDown;
            mouse.MouseUp += _input.OnMouseUp;
            mouse.MouseMove += _input.OnMouseMove;
            mouse.Scroll += _input.OnMouseScroll;
        }

        _renderer = new Renderer(GL);
        _renderer.Initialize();

        _assetManager = new AssetManager();
        _assetManager.Initialize(GL);
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
        // TODO: Notify layers of viewport resize
    }
}
