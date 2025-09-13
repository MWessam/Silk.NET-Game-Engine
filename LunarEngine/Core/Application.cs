using System.Drawing;
using System.Runtime.CompilerServices;
using LunarEngine.Assets;
using LunarEngine.Engine.AssetHandleCache;
using LunarEngine.Engine.Graphics;
using LunarEngine.Events;
using LunarEngine.InputEngine;
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
    public static Vector2D<int> Viewport;
    private LayerStack _layerStack = new();
    private ImGuiLayer _imGuiLayer;
    private Queue<Action> _mainThreadQueue = new();
    private object _mainThreadQueueLock = new();
    
    // Dependencies
    private IWindow _window;
    private Input _input;
    private Renderer _renderer;
    private AssetManager _assetManager;
    private AssetHandleCache _assetHandleCache;
    
    private bool _isRunning;
    public Vector2D<int> WindowSize
    {
        get
        {
            return _window.Size;
        }
    }
    public IWindow Window => _window;
    public Renderer Renderer => _renderer;
    public Input Input => _input;
    public AssetManager AssetManager => _assetManager;
    public AssetHandleCache AssetHandleCache => _assetHandleCache;
    public void CreateWindow(string title = "Lunar Editor", int width = 1280, int height = 720)
    {
        var options = WindowOptions.Default;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);
        _window = Silk.NET.Windowing.Window.Create(options);
        _window.Load += OnWindowLoad;
    }

    protected Application()
    {
    }
    public virtual void Initialize()
    {

    }

    public void Dispose()
    {
        
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
    private void OnClose()
    {
        _isRunning = false;
    }
    private void OnUpdate(double dt)
    {
        ExecuteMainThreadQueue();
        Time.DeltaTime = new TimeStep(dt);
        foreach (var layer in _layerStack)
        {
            layer.OnUpdate(Time.DeltaTime);
        }
        _imGuiLayer.Begin();
        foreach (var layer in _layerStack)
        {
            layer.OnImguiRender(Time.DeltaTime);
        }
        _imGuiLayer.End();
    }
    private void OnWindowLoad()
    {
        Viewport = _window.Size;
        var api = GL.GetApi(_window);
        var inputContext = _window.CreateInput();

        _input = new Input();
        _input.InputContext = inputContext;
        
        // Assign keyboard events
        foreach (var keyboard in inputContext.Keyboards)
        {
            keyboard.KeyDown += _input.OnKeyDown;
            keyboard.KeyUp += _input.OnKeyUp;
        }
        // Assign mouse events
        foreach (var mouse in inputContext.Mice)
        {
            mouse.MouseDown += _input.OnMouseDown;
            mouse.MouseUp += _input.OnMouseUp;
            mouse.MouseMove += _input.OnMouseMove;
            mouse.Scroll += _input.OnMouseScroll;
        }

        _renderer = new Renderer(api);
        _imGuiLayer = new ImGuiLayer("ImguiLayer", _window, api, inputContext, this);
        PushOverlay(_imGuiLayer);

        _assetManager = new AssetManager();
        _assetManager.Initialize();
        _assetHandleCache = new AssetHandleCache(_assetManager, api);

        foreach (var layer in _layerStack)
        {
            layer.OnInitialize();
        }
    }
    private void OnViewportResize(Vector2D<int> viewport)
    {
        Viewport = viewport;
        EventBus<ViewportResizedEvent>.Raise(new ViewportResizedEvent(viewport));
    }
}