using System.Drawing;
using System.Runtime.CompilerServices;
using LunarEngine.Assets;
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
    private static Application s_instance;
    private GL _api;
    private LayerStack _layerStack = new();
    private ImGuiLayer _imGuiLayer;
    private Queue<Action> _mainThreadQueue = new();
    private object _mainThreadQueueLock = new();
    private IWindow _window;
    private IInputContext _inputContext;
    private bool _isRunning;
    public Vector2D<int> WindowSize
    {
        get
        {
            return _window.Size;
        }
    }
    public IWindow Window => _window;
    public GL Api => _api;
    public IInputContext InputContext => _inputContext;
    public static Application Instance => s_instance;

    public void CreateWindow(string title = "Lunar Editor", int width = 1280, int height = 720)
    {
        var options = WindowOptions.Default;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);
        _window = Window.Create(options);
        _window.Load += OnWindowLoad;
    }

    protected Application()
    {
        s_instance ??= this;
    }
    public virtual void InitSingleton()
    {
        _imGuiLayer = new ImGuiLayer("ImguiLayer");
        PushOverlay(_imGuiLayer);
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
        _api = GL.GetApi(_window);
        _inputContext = _window.CreateInput();
        
        var input = Input.Instance;
        var context = InputContext;
        input.InputContext = context;
        
        // Assign keyboard events
        foreach (var keyboard in context.Keyboards)
        {
            keyboard.KeyDown += input.OnKeyDown;
            keyboard.KeyUp += input.OnKeyUp;
        }
        // Assign mouse events
        foreach (var mouse in context.Mice)
        {
            mouse.MouseDown += input.OnMouseDown;
            mouse.MouseUp += input.OnMouseUp;
            mouse.MouseMove += input.OnMouseMove;
            mouse.Scroll += input.OnMouseScroll;
        }
        
        // Notify systems that GL and window are initialized first
        EventBus<WindowInitializedEvent>.Raise(new WindowInitializedEvent(_window, _api));

        InitSingleton();
    }
    private void OnViewportResize(Vector2D<int> viewport)
    {
        Viewport = viewport;
        _api.Viewport(viewport);
        EventBus<ViewportResizedEvent>.Raise(new ViewportResizedEvent(viewport));
    }
}