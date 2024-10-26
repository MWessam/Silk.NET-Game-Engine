using System.Numerics;
using LunarEngineCS.Assets;
using LunarEngineCS.GameObjects;
using LunarEngineCS.RenderingEngine;
using Silk.NET.OpenGL;

namespace LunarEngineCS.GameEngine;

public class GameEngine
{
    private GraphicsEngine _graphicsEngine;
    private AssetManager _assetManager;

    public static GameEngine CreateGameEngine()
    {
        GameEngine engine = new GameEngine();
        engine._assetManager = new AssetManager();
        engine.Initialize();
        return engine;
    }
    internal void StartEngine()
    {
        _graphicsEngine.Start();
    }
    private void Initialize()
    {
        CreateWindow();
    }
    private void CreateWindow()
    {
        _graphicsEngine = GraphicsEngine.Create();
        _graphicsEngine.OnUpdateLoopTick += GameLoop;
        _graphicsEngine.OnApiInitialized += OnApiInitialized;
    }
    private void OnApiInitialized(GL gl)
    {
        _assetManager.LoadGLApi(gl);
        _assetManager.InitializeAssetManager();
        LoadScene();
    }
    private SpriteRenderer _spriteRenderer;
    private void LoadScene()
    {
        var spriteRenderer = _assetManager.CreateSpriteRenderer();
        var spriteRenderer2 = _assetManager.CreateSpriteRenderer();
        var object1 = GameObject.CreateGameObject("obj1");
        var object2 = GameObject.CreateGameObject("obj2");
        object1.AddComponent<IRenderer>(spriteRenderer);
        object2.AddComponent<IRenderer>(spriteRenderer2);
        spriteRenderer.Transform.LocalPosition = new Vector3(0.5f, 0.5f, 0.0f);
        spriteRenderer2.Transform.LocalPosition = new Vector3(-0.5f, -0.5f, 0.0f);
        var scene = Scene.CreateScene();
        scene.AddShader(spriteRenderer.Sprite.Shader);
        scene.AddTexture(spriteRenderer.Sprite.Texture);
        scene.AddObject(object1);
        scene.AddObject(object2);
        _graphicsEngine.AddScene(scene);
    }

    private void GameLoop(double dt)
    {
    }
}