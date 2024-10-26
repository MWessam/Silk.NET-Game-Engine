using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngine.Assets;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.GameEngine;

public class GameEngine
{
    private GraphicsEngine _graphicsEngine;
    private InputEngine.InputEngine _inputEngine;
    private AssetManager _assetManager;
    private List<Scene> _scenes = new();

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
        _inputEngine = InputEngine.InputEngine.Create();
        CreateWindow();
    }
    private void CreateWindow()
    {
        _graphicsEngine = Graphics.GraphicsEngine.Create();
        _graphicsEngine.OnUpdateLoopTick += GameLoop;
        _graphicsEngine.OnApiInitialized += OnApiInitialized;
        _graphicsEngine.OnWindowInitialized += OnWindowStart;
        _graphicsEngine.OnGraphicsRender += OnRenderReady;
        _graphicsEngine.OnViewportResized += OnViewportResized;
    }

    private void OnViewportResized(Vector2D<int> obj)
    {
        foreach (var scene in _scenes)
        {
            scene.ResetShadersVP();
        }
    }
    private void OnRenderReady(double obj)
    {
        foreach (var scene in _scenes)
        {
            scene.UpdateDirtyShaderUniforms();
            _graphicsEngine.Render(CollectionsMarshal.AsSpan(scene.GameObjects));
        }
    }

    private void OnWindowStart(IWindow window)
    {
        IInputContext context = window.CreateInput();
        foreach (var keyboard in context.Keyboards)
        {
            keyboard.KeyDown += _inputEngine.OnKeyDown;
            keyboard.KeyUp += _inputEngine.OnKeyUp;
        }
        foreach (var scene in _scenes)
        {
            scene.AwakeScene();
        }
        foreach (var scene in _scenes)
        {
            scene.StartScene();
        }
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
        object1.AddComponent<CustomBehaviour>(new());
        object2.AddComponent<IRenderer>(spriteRenderer2);
        spriteRenderer.Transform.LocalPosition = new Vector3(0.5f, 0.5f, 0.0f);
        spriteRenderer2.Transform.LocalPosition = new Vector3(-0.5f, -0.5f, 0.0f);
        var scene = Scene.CreateScene();
        scene.AddShader(spriteRenderer.Sprite.Shader);
        scene.AddTexture(spriteRenderer.Sprite.Texture);
        scene.AddObject(object1);
        scene.AddObject(object2);
        _scenes.Add(scene);
    }

    private void GameLoop(double dt)
    {
        _inputEngine.Update(dt);
        foreach (var scene in _scenes)
        {
            scene.UpdateScene(dt);
        }
    }
}