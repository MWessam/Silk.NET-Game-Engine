using System.Diagnostics;
using LunarEngine.Assets;
using LunarEngine.Graphics;
using LunarEngine.InputEngine;
using LunarEngine.Scenes;
using LunarEngine.Physics;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.GameEngine;

public class GameEngine
{
    private Input _input;
    private SceneManager _sceneManager;
    private bool _isRunning;
    private object _physicsLock = 0;
    private Thread _physicsLoop;
    private double _accumulatedTime;
    public static GameEngine CreateGameEngine()
    {
        GameEngine engine = new GameEngine();
        engine.Initialize();
        return engine;
    }
    internal void StartEngine()
    {
        GraphicsEngine.Start();
    }
    private void Initialize()
    {
        _input = InputEngine.Input.Create();
        _sceneManager = new SceneManager();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/log.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        CreateWindow();
    }
    private void CreateWindow()
    {
        GraphicsEngine.Initialize();
        GraphicsEngine.OnUpdateLoopTick += GameLoop;
        GraphicsEngine.OnApiInitialized += OnApiInitialized;
        GraphicsEngine.OnWindowInitialized += OnEngineStart;
        GraphicsEngine.OnGraphicsRender += OnRenderReady;
        GraphicsEngine.OnViewportResized += OnViewportResized;
        GraphicsEngine.OnWindowClosed += OnClose;
    }

    private void OnClose()
    {
        _isRunning = false;
    }

    private void OnViewportResized(Vector2D<int> obj)
    {
        _sceneManager.OnViewportResized();
    }
    private void OnRenderReady(double obj)
    {
        _sceneManager.RenderScenes();
    }
    private void OnEngineStart(IWindow window)
    {
        _isRunning = true;
        IInputContext context = window.CreateInput();
        foreach (var keyboard in context.Keyboards)
        {
            keyboard.KeyDown += _input.OnKeyDown;
            keyboard.KeyUp += _input.OnKeyUp;
        }

        foreach (var mouse in context.Mice)
        {
            mouse.MouseDown += _input.OnMouseDown;
            mouse.MouseUp += _input.OnMouseUp;
            mouse.MouseMove += _input.OnMouseMove;
            mouse.Scroll += _input.OnMouseScroll;
        }
        _sceneManager.AwakeScenes();
        _sceneManager.StartScenes();
    }

    private void OnApiInitialized(GL gl)
    {
        AssetManager.InitializeAssetManager();
        LoadScene();
    }
    private void LoadScene()
    {
        var scene = new TestScene();
        _sceneManager.AddScene(scene);
        
    }
    private void GameLoop(double dt)
    {
        Time.DeltaTime = (float)dt;
        _accumulatedTime += dt;
        _input.Update(dt);
        _sceneManager.UpdateScenes(dt);
        while (_accumulatedTime >= PhysicsEngine.FIXED_TIMESTAMP)
        {
            PhysicsEngine.TickPhysics();
            _sceneManager.TickScenes(PhysicsEngine.FIXED_TIMESTAMP);
            _accumulatedTime -= PhysicsEngine.FIXED_TIMESTAMP;
        }
        PhysicsEngine.Interpolate((float)(_accumulatedTime / PhysicsEngine.FIXED_TIMESTAMP));
        PhysicsEngine.UpdateBufferedObjects();
    }

    // private void PhysicsLoop()
    // {
    //     var stopwatch = new Stopwatch();
    //     stopwatch.Start();
    //     var currentTime = stopwatch.Elapsed.TotalSeconds;
    //     var lastTime = currentTime;
    //     double deltaTime = 0;
    //     while (_isRunning)
    //     {
    //         currentTime = stopwatch.Elapsed.TotalSeconds;
    //         deltaTime += currentTime - lastTime;
    //         lastTime = currentTime;
    //         lock (_physicsLock)
    //         {
    //             
    //
    //         }
    //     }
    // }
}

public static class Time
{
    public static float DeltaTime { get; internal set; }
}