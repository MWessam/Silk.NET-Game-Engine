using Arch.Buffer;
using Arch.Core;
using LunarEngine.Engine.ECS.Components;

namespace LunarEngine.ECS;

internal sealed class ECSWorld : IWorld
{
    public World NativeWorld { get; }

    public ECSWorld()
    {
        NativeWorld = World.Create();
    }

    public EntityReference Create()
    {
        var entity = NativeWorld.Create();
        return new EntityReference(entity, NativeWorld.Reference(entity), this);
    }

    public EntityReference Create<T1>(in T1 c1) where T1 : struct, IComponent
    {
        var entity = NativeWorld.Create(c1);
        return new EntityReference(entity, NativeWorld.Reference(entity), this);
    }

    public EntityReference Create<T1, T2>(in T1 c1, in T2 c2) where T1 : struct, IComponent where T2 : struct, IComponent
    {
        var entity = NativeWorld.Create(c1, c2);
        return new EntityReference(entity, NativeWorld.Reference(entity), this);
    }

    public EntityReference Create<T1, T2, T3>(in T1 c1, in T2 c2, in T3 c3) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent
    {
        var entity = NativeWorld.Create(c1, c2, c3);
        return new EntityReference(entity, NativeWorld.Reference(entity), this);
    }

    public EntityReference Create<T1, T2, T3, T4>(in T1 c1, in T2 c2, in T3 c3, in T4 c4) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent where T4 : struct, IComponent
    {
        var entity = NativeWorld.Create(c1, c2, c3, c4);
        return new EntityReference(entity, NativeWorld.Reference(entity), this);
    }

    public EntityReference Create<T1, T2, T3, T4, T5>(in T1 c1, in T2 c2, in T3 c3, in T4 c4, in T5 c5) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent where T4 : struct, IComponent where T5 : struct, IComponent
    {
        var entity = NativeWorld.Create(c1, c2, c3, c4, c5);
        return new EntityReference(entity, NativeWorld.Reference(entity), this);
    }

    public EntityReference Create<T1, T2, T3, T4, T5, T6>(in T1 c1, in T2 c2, in T3 c3, in T4 c4, in T5 c5, in T6 c6) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent where T4 : struct, IComponent where T5 : struct, IComponent where T6 : struct, IComponent
    {
        var entity = NativeWorld.Create(c1, c2, c3, c4, c5, c6);
        return new EntityReference(entity, NativeWorld.Reference(entity), this);
    }

    public EntityReference Create<T1, T2, T3, T4, T5, T6, T7>(in T1 c1, in T2 c2, in T3 c3, in T4 c4, in T5 c5, in T6 c6, in T7 c7) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent where T4 : struct, IComponent where T5 : struct, IComponent where T6 : struct, IComponent where T7 : struct, IComponent
    {
        var entity = NativeWorld.Create(c1, c2, c3, c4, c5, c6, c7);
        return new EntityReference(entity, NativeWorld.Reference(entity), this);
    }

    public EntityReference Create<T1, T2, T3, T4, T5, T6, T7, T8>(in T1 c1, in T2 c2, in T3 c3, in T4 c4, in T5 c5, in T6 c6, in T7 c7, in T8 c8) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent where T4 : struct, IComponent where T5 : struct, IComponent where T6 : struct, IComponent where T7 : struct, IComponent where T8 : struct, IComponent
    {
        var entity = NativeWorld.Create(c1, c2, c3, c4, c5, c6, c7, c8);
        return new EntityReference(entity, NativeWorld.Reference(entity), this);
    }

    public void Destroy(EntityReference entity)
    {
        NativeWorld.Destroy(entity.NativeReference);
    }

    public ref T Get<T>(EntityReference entity) where T : struct, IComponent
    {
        return ref NativeWorld.Get<T>(entity.NativeEntity);
    }

    public Components<T1, T2> Get<T1, T2>(EntityReference entity) where T1 : struct, IComponent where T2 : struct, IComponent
    {
        return NativeWorld.Get<T1, T2>(entity.NativeEntity);
    }

    public void Set<T>(EntityReference entity, in T component) where T : struct, IComponent
    {
        NativeWorld.Set(entity.NativeEntity, component);
    }

    public bool Has<T>(EntityReference entity) where T : struct, IComponent
    {
        return NativeWorld.Has<T>(entity.NativeEntity);
    }

    public object[] GetAllComponents(EntityReference entity)
    {
        return NativeWorld.GetAllComponents(entity.NativeReference);
    }

    public CommandBuffer CreateCommandBuffer()
    {
        return new CommandBuffer();
    }

    public void Playback(CommandBuffer buffer)
    {
        buffer.Playback(NativeWorld);
    }

    public bool IsAlive(EntityReference entity)
    {
        return NativeWorld.IsAlive(entity.NativeReference);
    }
}
