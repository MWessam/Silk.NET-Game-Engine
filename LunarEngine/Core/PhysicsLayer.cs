using LunarEngine.Physics;
using LunarEngine.Scenes;

namespace LunarEngine.GameEngine;

public class PhysicsLayer : BaseLayer
{
    private float _accumulatedTime;
    private readonly SceneManager _sceneManager;

    public PhysicsLayer(SceneManager sceneManager) : base("Physics")
    {
        _sceneManager = sceneManager;
    }

    public override void OnUpdate(TimeStep timeStep)
    {
        _accumulatedTime += timeStep;
        while (_accumulatedTime >= PhysicsEngine.FIXED_TIMESTAMP)
        {
            PhysicsEngine.TickPhysics(PhysicsEngine.FIXED_TIMESTAMP);
            _sceneManager.ActiveScenes.Tick(PhysicsEngine.FIXED_TIMESTAMP);
            _accumulatedTime -= PhysicsEngine.FIXED_TIMESTAMP;
        }
        PhysicsEngine.InterpolatedTime = (_accumulatedTime / PhysicsEngine.FIXED_TIMESTAMP);
    }
}
