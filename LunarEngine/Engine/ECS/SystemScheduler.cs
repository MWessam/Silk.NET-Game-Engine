namespace LunarEngine.ECS;

public sealed class SystemScheduler
{
    private readonly Dictionary<SystemStage, List<ISystem>> _stageSystems = new();

    public void Register(ISystem system)
    {
        var stage = system.Stage;
        if (!_stageSystems.TryGetValue(stage, out var list))
        {
            list = new List<ISystem>();
            _stageSystems[stage] = list;
        }
        list.Add(system);
    }

    public void RunStage(SystemStage stage, double deltaTime)
    {
        if (!_stageSystems.TryGetValue(stage, out var list)) return;

        var ordered = list.OrderBy(s => s.Order).ToList();
        foreach (var system in ordered)
        {
            system.Update(deltaTime);
        }
    }

    public void RunAwake()
    {
        foreach (var system in GetOrderedSystems(SystemStage.Awake))
        {
            if (system is LunarEngine.GameObjects.ScriptableSystem scriptable)
                scriptable.Awake();
            else
                system.Update(0);
        }
    }

    public void RunStart()
    {
        foreach (var system in GetOrderedSystems(SystemStage.Start))
        {
            if (system is LunarEngine.GameObjects.ScriptableSystem scriptable)
                scriptable.Start();
            else
                system.Update(0);
        }
    }

    public void RunFixedUpdate(double deltaTime)
    {
        foreach (var system in GetOrderedSystems(SystemStage.FixedUpdate))
        {
            if (system is LunarEngine.GameObjects.ScriptableSystem scriptable)
                scriptable.Tick(deltaTime);
            else
                system.Update(deltaTime);
        }
    }

    public void RunRenderPrepare(double deltaTime)
    {
        foreach (var system in GetOrderedSystems(SystemStage.RenderPrepare))
        {
            if (system is IRenderSystem renderSystem)
                renderSystem.Render(deltaTime);
            else
                system.Update(deltaTime);
        }
    }

    private IEnumerable<ISystem> GetOrderedSystems(SystemStage stage)
    {
        if (!_stageSystems.TryGetValue(stage, out var list)) return Enumerable.Empty<ISystem>();
        return list.OrderBy(s => s.Order);
    }
}
