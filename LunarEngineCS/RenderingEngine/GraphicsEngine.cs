using System.Drawing;
using System.Numerics;
using LunarEngineCS.GameObjects;
using LunarEngineCS.OpenGLAPI;
using Serilog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngineCS.RenderingEngine;

public class GraphicsEngine
{
    public event Action<IWindow> OnWindowInitialized;
    public event Action<GL> OnApiInitialized;
    public event Action<double> OnUpdateLoopTick; 
    private IWindow _windowContext;
    private List<ShaderHandle> _shaders = new();
    private List<IRenderer> _rendererObjects = new();
    private Camera _camera;
    private List<Scene> _scenes = new();
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

    public void AddScene(Scene scene)
    {
        _scenes.Add(scene);
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
        _windowContext.Load += InitializeEngine;
        _windowContext.Render += OnRender;
        _windowContext.Update += OnUpdate;
    }
    private void InitializeEngine()
    {
        try
        {
            _gl = GL.GetApi(_windowContext);
            _gl.ClearColor(Color.Black);
            OnApiInitialized?.Invoke(_gl);
        }
        catch (Exception e)
        {
            Log.Error(e, $"Error initializing Graphics Engine.");
            throw;
        }
    }
    private void OnUpdate(double obj)
    {
        OnUpdateLoopTick?.Invoke(obj);
    }
    private void OnRender(double deltaTime = 0)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        foreach (var scene in _scenes)
        {
            scene.UpdateDirtyShaderUniforms();
            foreach (var gameObject in scene.GameObjects)
            {
                gameObject.Render();
            }
        }
    }
    private void OnViewportResize(Vector2D<int> viewport)
    {
        _gl.Viewport(viewport);
        foreach (var scene in _scenes)
        {
            scene.ResetShadersVP();
        }
    }
    private GraphicsEngine(){}
}