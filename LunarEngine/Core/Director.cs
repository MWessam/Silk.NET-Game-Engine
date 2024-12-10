#define RUN_IWINDOW

using System.Drawing;
using LunarEngine.Assets;
using LunarEngine.ECS.Systems;
using LunarEngine.Engine.Graphics;
using LunarEngine.InputEngine;
using LunarEngine.Physics;
using LunarEngine.Scenes;
using LunarEngine.UI;
using Serilog;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.GameEngine;

public class Director : Singleton<Director>, ISingletonObject, IDisposable
{
    #region DEPENDENCIES
    private Renderer _renderer;
    private Editor _editor;
    private ECSScene _scene;
    private Input _input;
    private AssetManager _assetManager;
    private float _accumulatedTime;
    #endregion
    
    #region Lifetime
    /// <summary>
    /// Initializes the director and all of its dependencies.
    /// </summary>
    /// <param name="api"></param>
    public void InitSingleton()
    {
        _renderer = Renderer.Instance;
        _assetManager = AssetManager.Instance;
        _editor = Editor.Instance;
    }
    /// <summary>
    /// Dispose all open resources.
    /// </summary>
    public void Dispose()
    {
        
    }
    #endregion


    public void LoadRenderer(IWindow window, IInputContext context, GL api)
    {
        _renderer.InitializeRenderer(api);
        UIEngine.Initialize(window, api, context);
    }

    /// <summary>
    /// Coordinates the main loop with all other parts of the engine.
    /// Updates the main loop of the scene,
    /// Invokes renderer commands,
    /// Simulate physics step.
    /// </summary>
    /// <param name="dt"></param>
    public void MainLoop(float dt)
    {
        UIEngine.Update(dt);
        // Renderer start
        _renderer.BeginFrame();
        // Stats
        Time.DeltaTime = dt;
        _accumulatedTime += dt;
        
        // Input
        _input.Update(dt);
        
        // Physics
        while (_accumulatedTime >= PhysicsEngine.FIXED_TIMESTAMP)
        {
            PhysicsEngine.TickPhysics(PhysicsEngine.FIXED_TIMESTAMP);
            _scene.Tick(PhysicsEngine.FIXED_TIMESTAMP);
            _accumulatedTime -= PhysicsEngine.FIXED_TIMESTAMP;
        }
        PhysicsEngine.InterpolatedTime = (_accumulatedTime / PhysicsEngine.FIXED_TIMESTAMP);
        
        // Scene dispatcher
        _scene.Update(dt);
        _editor.EditorLoop(_scene, dt);
        // Render
        _renderer.EndFrame();
    }

    public void InitializeAssetManager()
    {
        _assetManager.InitializeAssetManager();
        _assetManager.TextureLibrary.CreateTexture("water", @"..\..\..\Resources\water.png");
        _assetManager.TextureLibrary.CreateTexture("square", @"..\..\..\Resources\square.jpg");
        _assetManager.ShaderLibrary.CreateShader("wave", @"..\..\..\Resources\wave.vert", @"..\..\..\Resources\wave.frag");
        _assetManager.ShaderLibrary.CreateShader("wireframe_gizmo", @"..\..\..\Resources\Debug\wireframe_gizmo.vert", @"..\..\..\Resources\Debug\wireframe_gizmo.frag");
    }

    public void InitializeInput(IInputContext context)
    {
        _input = Input.Instance;
        // Assign keyboard events.
        foreach (var keyboard in context.Keyboards)
        {
            keyboard.KeyDown += _input.OnKeyDown;
            keyboard.KeyUp += _input.OnKeyUp;
        }
        // Assign mouse events
        foreach (var mouse in context.Mice)
        {
            mouse.MouseDown += _input.OnMouseDown;
            mouse.MouseUp += _input.OnMouseUp;
            mouse.MouseMove += _input.OnMouseMove;
            mouse.Scroll += _input.OnMouseScroll;
        }
    }
    public void LoadScene()
    {
        _scene = new TestEcsScene();
        _scene.Awake();
        _scene.Start();
        _editor.Initialize(_scene);
    }

    public void UpdateViewport(Vector2D<int> viewport)
    {
        _editor.OnViewportResize(viewport);
    }
}

public unsafe class Application
{
    public static Vector2D<int> Viewport;
    private Director _director;
    private GL _api;
    #if RUN_GLFW
    private static Glfw _glfw;
    private static WindowHandle* _window;
    public void Run()
    {
        _glfw = Glfw.GetApi();
        if (!_glfw.Init())
        {
            Log.Error("Failed to create GLFW window.");
            _glfw.Terminate();
            return;
        }
        _glfw.WindowHint(WindowHintInt.ContextVersionMajor, 4);
        _glfw.WindowHint(WindowHintInt.ContextVersionMinor, 6);
        _glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);
        _window = _glfw.CreateWindow(800, 600, "Lunar Engine", null, null);
        
        _glfw.MakeContextCurrent(_window);
        
        // Main loop
        while (!_glfw.WindowShouldClose(_window))
        {
            _glfw.PollEvents();
            _director.MainLoop();
            _glfw.SwapBuffers(_window);
        }
        
        _glfw.DestroyWindow(_window);
        _glfw.Terminate();
    }
    #endif
    #if RUN_IWINDOW
    public Vector2D<int> WindowSize
    {
        get
        {
            return _window.Size;
        }
    }
    private IWindow _window; 
    public void Run()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Engine";
        _window = Window.Create(options);
        
        _window.FramebufferResize += OnViewportResize;
        _window.Load += OnWindowLoad;
        _window.Update += OnUpdate;
        _window.Closing += OnClose;
        _window.Run();
    }
    private void OnClose()
    {
        _director.Dispose();
    }
    private void OnUpdate(double dt)
    {
        _director.MainLoop((float)dt);
    }
    private void OnWindowLoad()
    {
        Viewport = _window.Size;
        _api = GL.GetApi(_window);
        IInputContext context = _window.CreateInput();
        _api.ClearColor(Color.Black);
        _api.Enable(GLEnum.Blend);
        _api.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        _director = Director.Instance;
        _director.LoadRenderer(_window, context, _api);
        _director.InitializeAssetManager();
        _director.InitializeInput(context);
        _director.LoadScene();

    }
    private void OnViewportResize(Vector2D<int> viewport)
    {
        Viewport = viewport;
        _api.Viewport(viewport);
        _director.UpdateViewport(viewport);
    }
#endif
}