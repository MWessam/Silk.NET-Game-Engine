using LunarEngine.Engine.ECS.Systems;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameEngine;
using LunarEngine.Scenes;

namespace LunarEngine.Engine.Gizmos;

public class GizmosLayer : BaseLayer
{
    private SceneManager _sceneManager;
    private GizmosSystem _gizmosSystem;
    private Renderer _renderer;
    public GizmosLayer(SceneManager sceneManager, Renderer renderer) : base("Gizmos")
    {
        _sceneManager = sceneManager;
        _renderer = renderer;
    }

    public override void OnAttach()
    {
        base.OnAttach();
        _gizmosSystem = new GizmosSystem(_sceneManager.ActiveScenes.World, _renderer);
        _gizmosSystem.Awake();
    }

    public override void OnUpdate(TimeStep timeStep)
    {
        _gizmosSystem.Update(timeStep);
    }
}