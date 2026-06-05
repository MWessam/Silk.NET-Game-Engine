namespace LunarEngine.Events;

public readonly struct InspectorTargetSelectedEvent
{
    public readonly ECS.EntityReference Entity;
    public readonly ECS.IWorld World;

    public InspectorTargetSelectedEvent(ECS.EntityReference entity, ECS.IWorld world)
    {
        Entity = entity;
        World = world;
    }
}
