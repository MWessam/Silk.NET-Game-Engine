using LunarEngine.Engine.ECS.Systems;
using LunarEngine.GameEngine;
using LunarEngine.Scenes;

namespace LunarEngine.Engine.Gizmos;

public class GizmosLayer : BaseLayer
{
    private SceneManager _sceneManager;
    private GizmosSystem _gizmosSystem;
    public GizmosLayer(SceneManager sceneManager) : base("Gizmos")
    {
        _sceneManager = sceneManager;
    }

    public override void OnAttach()
    {
        base.OnAttach();
        _gizmosSystem = new GizmosSystem(_sceneManager.ActiveScenes.World);
        _gizmosSystem.Awake();
    }

    public override void OnUpdate(TimeStep timeStep)
    {
        _gizmosSystem.Update(timeStep);
    }
}