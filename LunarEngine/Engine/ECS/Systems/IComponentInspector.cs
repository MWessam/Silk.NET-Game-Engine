namespace LunarEngine.ECS.Systems;

public interface IComponentInspector
{
}
public interface IComponentInspector<T> : IComponentInspector where T : struct
{
    void OnDrawInspector(T component);
}