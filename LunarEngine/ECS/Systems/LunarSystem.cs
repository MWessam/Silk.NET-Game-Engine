using Arch.Buffer;
using Arch.Core;
using Arch.System;

namespace LunarEngine.GameObjects;

public partial class ScriptableSystem : BaseSystem<World, double>
{
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
    
}

