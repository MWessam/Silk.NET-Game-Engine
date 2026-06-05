using Arch.Buffer;
using Arch.Core;
using LunarEngine.ECS.Components;

namespace LunarEngine.ECS;

public delegate void QueryCallback<T1>(EntityReference entity, ref T1 component) where T1 : struct, IComponent;

public interface IWorld
{
    EntityReference Create();
    EntityReference Create<T1>(in T1 c1) where T1 : struct, IComponent;
    EntityReference Create<T1, T2>(in T1 c1, in T2 c2) where T1 : struct, IComponent where T2 : struct, IComponent;
    EntityReference Create<T1, T2, T3>(in T1 c1, in T2 c2, in T3 c3) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent;
    EntityReference Create<T1, T2, T3, T4>(in T1 c1, in T2 c2, in T3 c3, in T4 c4) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent where T4 : struct, IComponent;
    EntityReference Create<T1, T2, T3, T4, T5>(in T1 c1, in T2 c2, in T3 c3, in T4 c4, in T5 c5) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent where T4 : struct, IComponent where T5 : struct, IComponent;
    EntityReference Create<T1, T2, T3, T4, T5, T6>(in T1 c1, in T2 c2, in T3 c3, in T4 c4, in T5 c5, in T6 c6) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent where T4 : struct, IComponent where T5 : struct, IComponent where T6 : struct, IComponent;
    EntityReference Create<T1, T2, T3, T4, T5, T6, T7>(in T1 c1, in T2 c2, in T3 c3, in T4 c4, in T5 c5, in T6 c6, in T7 c7) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent where T4 : struct, IComponent where T5 : struct, IComponent where T6 : struct, IComponent where T7 : struct, IComponent;
    EntityReference Create<T1, T2, T3, T4, T5, T6, T7, T8>(in T1 c1, in T2 c2, in T3 c3, in T4 c4, in T5 c5, in T6 c6, in T7 c7, in T8 c8) where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent where T4 : struct, IComponent where T5 : struct, IComponent where T6 : struct, IComponent where T7 : struct, IComponent where T8 : struct, IComponent;

    void Destroy(EntityReference entity);

    ref T Get<T>(EntityReference entity) where T : struct, IComponent;
    Components<T1, T2> Get<T1, T2>(EntityReference entity) where T1 : struct, IComponent where T2 : struct, IComponent;

    void Set<T>(EntityReference entity, in T component) where T : struct, IComponent;
    void Set(EntityReference entity, object component);

    bool Has<T>(EntityReference entity) where T : struct, IComponent;

    object[] GetAllComponents(EntityReference entity);

    CommandBuffer CreateCommandBuffer();
    void Playback(CommandBuffer buffer);

    bool IsAlive(EntityReference entity);

    void Query<T1>(QueryCallback<T1> callback) where T1 : struct, IComponent;

    World NativeWorld { get; }
}
