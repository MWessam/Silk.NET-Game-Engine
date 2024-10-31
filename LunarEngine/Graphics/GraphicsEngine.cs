using System.Drawing;
using LunarEngine.GameObjects;
using Serilog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.Graphics;

public static class GraphicsEngine
{
    public static event Action<IWindow>? OnWindowInitialized;
    public static event Action<GL>? OnApiInitialized;
    public static event Action<double>? OnGraphicsRender;
    public static event Action<double>? OnUpdateLoopTick;
    public static event Action<Vector2D<int>>? OnViewportResized;
    public static event Action OnWindowClosed;
    private static IWindow _windowContext;
    public static GL Api { get; private set; }

    public static void Initialize()
    {
        InitializeWindow();
        InitializeEngineEvents();
    }
    public static void Start()
    {
        _windowContext.Run();
        _windowContext.Dispose();
    }
    public static void Render(Span<GameObject> renderers)
    {
        for (int i = 0; i < renderers.Length; ++i)
        {
            renderers[i].Render();
        }
    }
    private static void InitializeWindow()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Engine";
        _windowContext = Window.Create(options);
    }
    private static void InitializeEngineEvents()
    {
        _windowContext.FramebufferResize += OnViewportResize;
        _windowContext.Load += OnWindowLoad;
        _windowContext.Render += OnRender;
        _windowContext.Update += OnUpdate;
        _windowContext.Closing += OnClose;
    }

    private static void OnClose()
    {
        OnWindowClosed?.Invoke();
    }

    private static void OnWindowLoad()
    {
        try
        {
            Api = GL.GetApi(_windowContext);
            Api.ClearColor(Color.Black);
            OnApiInitialized?.Invoke(Api);
            OnWindowInitialized?.Invoke(_windowContext);

        }
        catch (Exception e)
        {
            Log.Error(e, $"Error initializing Graphics Engine.");
            throw;
        }
    }
    private static void OnUpdate(double dt)
    {
        OnUpdateLoopTick?.Invoke(dt);
    }
    private static void OnRender(double deltaTime = 0)
    {
        Api.Clear(ClearBufferMask.ColorBufferBit);
        _windowContext.Title = $"Lunar Engine FPS: {(int)(1 / deltaTime)}";
        OnGraphicsRender?.Invoke(deltaTime);
    }
    private static void OnViewportResize(Vector2D<int> viewport)
    {
        Api.Viewport(viewport);
        OnViewportResized?.Invoke(viewport);
    }
}