using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Components;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;

namespace LunarEngine.GameEngine;

public partial class InitializationSystem : ScriptableSystem
{
    public InitializationSystem(World world) : base(world)
    {
    }
    public override void Start()
    {
        ConfirmInitializedStateQuery(World);
    }
    [Query]
    [All<IsInstantiating>]
    public void ConfirmInitializedState(Entity entity)
    {
        World.Remove<IsInstantiating>(entity);
    }
    [Query]
    [All<IsDestroying>]
    public void ConfirmDestroyedState(Entity entity)
    {
        World.Remove<IsDestroying>(entity);
    }
}