using Arch.Core;

namespace LunarEngine.ECS;

public readonly struct EntityReference
{
    public int Id => _entity.Id;
    public bool IsValid => World?.IsAlive(this) ?? false;
    public IWorld World { get; }

    internal Arch.Core.Entity NativeEntity => _entity;
    internal Arch.Core.EntityReference NativeReference => _reference;

    private readonly Arch.Core.Entity _entity;
    private readonly Arch.Core.EntityReference _reference;

    internal EntityReference(Arch.Core.Entity entity, Arch.Core.EntityReference reference, IWorld world)
    {
        _entity = entity;
        _reference = reference;
        World = world;
    }
}
