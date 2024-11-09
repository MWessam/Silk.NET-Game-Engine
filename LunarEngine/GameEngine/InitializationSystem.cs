using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;

namespace LunarEngine.GameEngine;

public partial class InitializationSystem : ScriptableSystem
{
    public InitializationSystem(World world) : base(world)
    {
    }

    [Query]
    [All<NeedsInitialization>]
    public void ConfirmInitializedState(Entity entity)
    {
        World.Remove<NeedsInitialization>(entity);
    }
}