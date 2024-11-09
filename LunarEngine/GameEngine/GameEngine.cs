using Arch.Core.Extensions;
using LunarEngine.Assets;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.InputEngine;
using LunarEngine.Physics;
using LunarEngine.Scenes;
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
    private PhysicsEngine _physicsEngine = new();
    private GraphicsEngine _graphicsEngine;


    private List<ScriptableSystem> _systems;

    
    public static GameEngine CreateGameEngine()
    {
        GameEngine engine = new GameEngine();
        engine.Initialize();
        
        return engine;
    }
    internal void StartEngine()
    {
        _graphicsEngine.Start();
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
        _graphicsEngine = new GraphicsEngine();
        _graphicsEngine.Initialize();
        _graphicsEngine.OnUpdateLoopTick += GameLoop;
        _graphicsEngine.OnApiInitialized += OnApiInitialized;
        _graphicsEngine.OnWindowInitialized += OnEngineStart;
        _graphicsEngine.OnViewportResized += OnViewportResized;
        _graphicsEngine.OnWindowClosed += OnClose;
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

        var activeSceneWorld = _sceneManager.ActiveScenes.World;
        _sceneManager.AwakeScenes();
        _sceneManager.StartScenes();
        _graphicsEngine.InjectSpriteRendererSystem(_sceneManager.ActiveScenes.SpriteRendererSystem);
        _physicsEngine.InitializePhysicsSystem(activeSceneWorld);
    }

    private void OnApiInitialized(GL gl)
    {
        AssetManager.InitializeAssetManager();
        LoadScene();
    }
    private void LoadScene()
    {
        var scene = new ECSScene();
        _sceneManager.AddScene(scene);
        
    }
    private void GameLoop(double dt)
    {
        Time.DeltaTime = (float)dt;
        _accumulatedTime += dt;
        _input.Update(dt);
        _sceneManager.ActiveScenes.Update((float)dt);
        while (_accumulatedTime >= PhysicsEngine.FIXED_TIMESTAMP)
        {
            _physicsEngine.TickPhysics(PhysicsEngine.FIXED_TIMESTAMP);
            _sceneManager.TickScenes(PhysicsEngine.FIXED_TIMESTAMP);
            _accumulatedTime -= PhysicsEngine.FIXED_TIMESTAMP;
        }
        _physicsEngine.InterpolatePhysics((float)(_accumulatedTime / PhysicsEngine.FIXED_TIMESTAMP));
    }
}

public static class Time
{
    public static float DeltaTime { get; internal set; }
}