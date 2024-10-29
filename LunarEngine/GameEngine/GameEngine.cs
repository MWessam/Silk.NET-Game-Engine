using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngine.Assets;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.Scenes;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.GameEngine;

public class GameEngine
{
    private InputEngine.InputEngine _inputEngine;
    private SceneManager _sceneManager;

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
        _inputEngine = InputEngine.InputEngine.Create();
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
        GraphicsEngine.OnWindowInitialized += OnWindowStart;
        GraphicsEngine.OnGraphicsRender += OnRenderReady;
        GraphicsEngine.OnViewportResized += OnViewportResized;
    }
    private void OnViewportResized(Vector2D<int> obj)
    {
        _sceneManager.OnViewportResized();
    }
    private void OnRenderReady(double obj)
    {
        _sceneManager.RenderScenes();
    }
    private void OnWindowStart(IWindow window)
    {
        IInputContext context = window.CreateInput();
        foreach (var keyboard in context.Keyboards)
        {
            keyboard.KeyDown += _inputEngine.OnKeyDown;
            keyboard.KeyUp += _inputEngine.OnKeyUp;
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
        var object1 = GameObject.CreateGameObject("obj1", 
            typeof(SpriteRenderer), 
            typeof(CustomBehaviour));
        var object2 = GameObject.CreateGameObject("obj2",
            typeof(SpriteRenderer));
        object1.Transform.LocalPosition = new Vector3(0.5f, 0.5f, 0.0f);
        object2.Transform.LocalPosition = new Vector3(-0.5f, -0.5f, 0.0f);
        var scene = Scene.CreateScene();
        scene.AddObject(object1);
        scene.AddObject(object2);
        scene.IsActive = true;
        _sceneManager.AddScene(scene);
        
    }
    private void GameLoop(double dt)
    {
        Time.DeltaTime = (float)dt;
        _inputEngine.Update(dt);
        _sceneManager.UpdateScenes(dt);
    }
}

public static class Time
{
    public static float DeltaTime { get; internal set; } 
}