using Arch.Core;
using LunarEngine.ECS.Components;
using LunarEngine.GameObjects;

namespace LunarEngine.ECS.Systems;

public class InputSystem : ScriptableSystem
{
    public InputSystem(World world) : base(world)
    {
    }

    public override void Awake()
    {
        base.Awake();
    }

    public void UpdateInput(ref Input input)
    {
        
    }
}