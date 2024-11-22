using LunarEngine.Assets;
using LunarEngine.ECS.Systems;
using LunarEngine.Engine.Graphics;
using LunarEngine.Graphics;
using LunarEngine.InputEngine;
using LunarEngine.Physics;
using LunarEngine.Scenes;
using LunarEngine.UI;
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
    private double _accumulatedTime;
    private ImGuiController _imGuiController;
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
        _input = Input.Create();
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
        GraphicsEngine.OnRenderLoopTick += RenderLoop;
        GraphicsEngine.OnApiInitialized += OnApiInitialized;
        GraphicsEngine.OnWindowLoad += OnEngineStart;
        GraphicsEngine.OnViewportResized += OnViewportResized;
        GraphicsEngine.OnWindowClosed += OnClose;
    }

    private void OnClose()
    {
        _isRunning = false;
    }

    private void OnViewportResized(Vector2D<int> obj)
    {
        // _sceneManager.OnViewportResized();
    }
    private void OnEngineStart(IWindow window)
    {
        _isRunning = true;
        IInputContext context = window.CreateInput();
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
        UIEngine.Initialize(window, GraphicsEngine.Api, context);
        var activeSceneWorld = _sceneManager.ActiveScenes.World;
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
        var scene = new TestEcsScene();
        _sceneManager.AddScene(scene);
        
    }
    private void GameLoop(double dt)
    {
        UIEngine.Update((float)dt);
        Time.DeltaTime = dt;
        _accumulatedTime += dt;
        _input.Update(dt);
        _sceneManager.ActiveScenes.Update(dt);
        while (_accumulatedTime >= PhysicsEngine.FIXED_TIMESTAMP)
        {
            PhysicsEngine.TickPhysics(PhysicsEngine.FIXED_TIMESTAMP);
            _sceneManager.TickScenes(PhysicsEngine.FIXED_TIMESTAMP);
            _accumulatedTime -= PhysicsEngine.FIXED_TIMESTAMP;
        }

        PhysicsEngine.InterpolatedTime = (_accumulatedTime / PhysicsEngine.FIXED_TIMESTAMP);
        _sceneManager.ActiveScenes.AfterUpdate();
    }

    private void RenderLoop(double dt)
    {
        _sceneManager.ActiveScenes.RenderScenes(dt);
    }
}

public static class Time
{
    public static double DeltaTime { get; internal set; }
}