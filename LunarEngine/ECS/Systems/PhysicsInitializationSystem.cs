using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.GameObjects;

namespace LunarEngine.Physics;

public partial class PhysicsInitializationSystem : ScriptableSystem
{
    public PhysicsInitializationSystem(World world) : base(world)
    {
    }
    [Query]
    [All<NeedsPhysicsInitialization>]
    public void ConfirmInitializedState(Entity entity)
    {
        World.Remove<NeedsPhysicsInitialization>(entity);
    }
}