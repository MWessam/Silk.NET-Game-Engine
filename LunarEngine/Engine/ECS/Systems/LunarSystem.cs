using Arch.Buffer;
using Arch.Core;
using Arch.System;
using LunarEngine.Core;
using LunarEngine.ECS;

namespace LunarEngine.ECS.Systems;

public partial class ScriptableSystem : BaseSystem<World, double>, ISystem
{
    public virtual int Order => 0;
    public virtual SystemStage Stage => SystemStage.Update;

    protected CommandBuffer CommandBuffer;
    public ScriptableSystem(World world) : base(world)
    {
        CommandBuffer = new CommandBuffer();
    }

    public void SetCommandBuffer(CommandBuffer commandBuffer)
    {
        CommandBuffer = commandBuffer;
    }

    public virtual void Awake()
    {
        
    }
    public virtual void OnEnable()
    {
        
    }
    public virtual void Start()
    {
        
    }
    public virtual void Tick(double dt)
    {
        
    }
    

    public sealed override void Initialize()
    {
    }

    public virtual void Initialize(IWorld world, ServiceContainer services)
    {
    }

    public void Update(double deltaTime)
    {
        Update(in deltaTime);
    }
}

