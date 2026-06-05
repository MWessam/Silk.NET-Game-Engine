using LunarEngine.Application;
using LunarEngine.Core;
using LunarEngine.ECS.Systems;
using LunarEngine.Renderer;
using LunarEngine.Scenes;

namespace LunarEngine.Editor;

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
        _gizmosSystem = new GizmosSystem(((ECSScene)_sceneManager.ActiveScene!).World, _renderer);
        _gizmosSystem.Awake();
    }

    public override void OnUpdate(TimeStep timeStep)
    {
        _gizmosSystem.Update(timeStep);
    }
}
