using System.Drawing;
using LunarEngine.GameObjects;
using Serilog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.Graphics;

public class GraphicsEngine
{
    public event Action<IWindow>? OnWindowInitialized;
    public event Action<GL>? OnApiInitialized;
    public event Action<double>? OnGraphicsRender;
    public event Action<double>? OnUpdateLoopTick;
    public event Action<Vector2D<int>>? OnViewportResized; 
    private IWindow _windowContext;
    private GL _gl;
    public static GraphicsEngine Create()
    {
        GraphicsEngine engine = new GraphicsEngine();
        engine.InitializeWindow();
        engine.InitializeEngineEvents();
        return engine;
    }
    public void Start()
    {
        _windowContext.Run();
        _windowContext.Dispose();
    }
    public void Render(Span<GameObject> renderers)
    {
        for (int i = 0; i < renderers.Length; ++i)
        {
            renderers[i].Render();
        }
    }
    private void InitializeWindow()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Engine";
        _windowContext = Window.Create(options);
    }
    private void InitializeEngineEvents()
    {
        _windowContext.FramebufferResize += OnViewportResize;
        _windowContext.Load += OnWindowLoad;
        _windowContext.Render += OnRender;
        _windowContext.Update += OnUpdate;
    }
    private void OnWindowLoad()
    {
        try
        {
            _gl = GL.GetApi(_windowContext);
            _gl.ClearColor(Color.Black);
            OnApiInitialized?.Invoke(_gl);
            OnWindowInitialized?.Invoke(_windowContext);

        }
        catch (Exception e)
        {
            Log.Error(e, $"Error initializing Graphics Engine.");
            throw;
        }
    }
    private void OnUpdate(double dt)
    {
        OnUpdateLoopTick?.Invoke(dt);
    }
    private void OnRender(double deltaTime = 0)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        OnGraphicsRender?.Invoke(deltaTime);
    }
    private void OnViewportResize(Vector2D<int> viewport)
    {
        _gl.Viewport(viewport);
        OnViewportResized?.Invoke(viewport);
    }
    private GraphicsEngine(){}
}