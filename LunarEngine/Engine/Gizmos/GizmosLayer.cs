using LunarEngine.Engine.ECS.Systems;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameEngine;
using LunarEngine.Scenes;

namespace LunarEngine.Engine.Gizmos;

public class GizmosLayer : BaseLayer
{
    private readonly SceneManager _sceneManager;
    private readonly IRenderer _renderer;
    private GizmosSystem _gizmosSystem;

    public GizmosLayer(SceneManager sceneManager, IRenderer renderer) : base("Gizmos")
    {
        _sceneManager = sceneManager;
        _renderer = renderer;
    }

    public override void OnInitialize()
    {
        _gizmosSystem = new GizmosSystem(_sceneManager.ActiveScenes.World, _renderer);
        _gizmosSystem.Awake();
    }

    public override void OnUpdate(TimeStep timeStep)
    {
        _gizmosSystem.Update(timeStep);
    }
}
