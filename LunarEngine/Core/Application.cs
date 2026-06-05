using LunarEngine.Assets;
using LunarEngine.Core;
using LunarEngine.Engine.Graphics;
using LunarEngine.Events;
using LunarEngine.Input;
using LunarEngine.Physics;
using LunarEngine.Platform;
using LunarEngine.Scenes;
using LunarEngine.UI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

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
    protected ServiceContainer Services { get; }
    protected IWindow Window = null!;
    protected LunarEngine.Platform.IInputContext InputContext = null!;
    protected GL GL;
    private Renderer _renderer = null!;
    private InputManager _inputManager = null!;
    private AssetManager _assetManager = null!;
    private SceneManager _sceneManager = null!;
    private Gizmos _gizmos = null!;
    private GLRenderDevice _renderDevice = null!;

    public Renderer Renderer => _renderer;
    public InputManager InputManager => _inputManager;
    public AssetManager AssetManager => _assetManager;
    public SceneManager SceneManager => _sceneManager;

    public Vector2D<int> WindowSize => Window.Size;

    protected Application()
    {
        Services = new ServiceContainer();

        var silkWindow = new SilkWindow("Lunar Editor", 1280, 720);
        Window = silkWindow;

        Services.Register<IWindow>(Window);
    }

    public virtual void Initialize() { }

    public void Run()
    {
        _isRunning = true;
        Window.OnLoad += OnWindowLoad;
        Window.OnResize += OnViewportResize;
        Window.OnUpdate += OnUpdate;
        Window.OnClosing += OnClose;
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

    private void OnWindowLoad()
    {
        Logger.Initialize();

        var silkWindow = (SilkWindow)Window;
        GL = Silk.NET.OpenGL.GL.GetApi(silkWindow.NativeWindow);

        InputContext = new SilkInputContext(silkWindow);
        Services.Register<LunarEngine.Platform.IInputContext>(InputContext);

        _inputManager = new InputManager(InputContext);
        _inputManager.AddAxis("Movement", new AxisMapping
        {
            PositiveX = Key.D,
            NegativeX = Key.A,
            PositiveY = Key.W,
            NegativeY = Key.S
        });

        _renderDevice = new GLRenderDevice(GL);
        Services.Register<IRenderDevice>(_renderDevice);

        var assetProvider = new FileSystemAssetProvider("Resources");
        Services.Register<IAssetProvider>(assetProvider);

        _assetManager = new AssetManager(assetProvider, _renderDevice);
        Services.Register(_assetManager);

        _gizmos = new Gizmos(_renderDevice, _assetManager);
        Services.Register(_gizmos);

        _renderer = new Renderer(_renderDevice, _gizmos);
        _renderer.Initialize();
        Services.Register<IRenderer>(_renderer);

        Services.Register(_inputManager);

        _sceneManager = new SceneManager(Services);
        Services.Register(_sceneManager);

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
        _inputManager.Update();

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
