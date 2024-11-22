using System.Drawing;
using LunarEngine.ECS.Systems;
using LunarEngine.UI;
using Serilog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.Engine.Graphics;

public static class GraphicsEngine
{
    public static event Action<IWindow>? OnWindowLoad;
    public static event Action<GL>? OnApiInitialized;
    public static event Action<double>? OnUpdateLoopTick;
    public static event Action<double>? OnRenderLoopTick; 
    public static event Action<Vector2D<int>>? OnViewportResized;
    public static event Action OnWindowClosed;
    private static FrameBuffer _renderTarget;
    private static event Action OnPostRenderLoopTick;
    private static SceneSystem _sceneSystem;
    public static IWindow WindowContext { get; private set; }
    public static GL Api { get; private set; }
    public static void Initialize()
    {
        InitializeWindow();
        InitializeEngineEvents();
    }
    public static void Start()
    {
        WindowContext.Run();
        WindowContext.Dispose();
    }
    private static void InitializeWindow()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Engine";
        WindowContext = Window.Create(options);
    }
    private static void InitializeEngineEvents()
    {
        WindowContext.FramebufferResize += OnViewportResize;
        WindowContext.Load += _OnWindowLoad;
        WindowContext.Update += OnUpdate;
        WindowContext.Render += OnRender;
        WindowContext.Closing += OnClose;
    }

    private static void OnClose()
    {
        OnWindowClosed?.Invoke();
    }

    private static void _OnWindowLoad()
    {
        try
        {
            Api = GL.GetApi(WindowContext);
            Api.ClearColor(Color.Black);
            Api.Enable(GLEnum.Blend);
            Api.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
            _renderTarget = FrameBuffer.CreateDefaultRenderFrameBuffer(Api);
            OnApiInitialized?.Invoke(Api);
            OnWindowLoad?.Invoke(WindowContext);
            _sceneSystem = new SceneSystem();
            _sceneSystem.Awake();
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
        _sceneSystem.PreRenderScene(deltaTime);
        _renderTarget.Resize(_sceneSystem.NewViewport);
        if (_renderTarget.DefaultRenderTarget)
        {
            Api.Clear(ClearBufferMask.ColorBufferBit);
        }
        else
        {
            _renderTarget.Bind();
            _renderTarget.Clear();
        }
        WindowContext.Title = $"Lunar Engine FPS: {(int)(1 / deltaTime)}";
        OnRenderLoopTick?.Invoke(deltaTime);
        _renderTarget.Unbind();
        Api.Clear(ClearBufferMask.ColorBufferBit);
        _sceneSystem.PostRender(deltaTime);
        UIEngine.Render();
    }
    private static void OnViewportResize(Vector2D<int> viewport)
    {
        _renderTarget.Resize(viewport);
        Api.Viewport(viewport);
        OnViewportResized?.Invoke(viewport);
    }
    internal static void SetRenderTarget(FrameBuffer sceneFrameBuffer)
    {
        _renderTarget = sceneFrameBuffer;
    }
    public static void ResetDefaultRenderTarget()
    {
        _renderTarget.Unbind();
    }
}