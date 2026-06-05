using LunarEngine.Assets;
using LunarEngine.Engine.Graphics;
using LunarEngine.InputEngine;
using LunarEngine.Scenes;

namespace LunarEngine.GameEngine;

public class SceneLayer : BaseLayer
{
    private readonly SceneManager _sceneManager;
    private readonly Renderer _renderer;
    private readonly AssetManager _assetManager;
    private readonly Input _input;

    public SceneLayer(SceneManager sceneManager, Renderer renderer, AssetManager assetManager,
                      Input input) : base("Scene")
    {
        _sceneManager = sceneManager;
        _renderer = renderer;
        _assetManager = assetManager;
        _input = input;
    }

    public override void OnInitialize()
    {
        _sceneManager.AddScene(new TestEcsScene(_renderer, _assetManager));
    }

    public override void OnUpdate(TimeStep timeStep)
    {
        Time.DeltaTime = timeStep;
        _input.Update(timeStep);
        _sceneManager.ActiveScenes.Update(timeStep);
    }
}
