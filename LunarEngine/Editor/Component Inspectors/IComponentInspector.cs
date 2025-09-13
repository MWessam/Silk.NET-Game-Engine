using System.Numerics;
using LunarEngine.Components;
using LunarEngine.Engine.ECS.Components;

namespace LunarEngine.ECS.Systems;

public interface IComponentInspector
{
    Type ComponentType { get; }
}
public interface IComponentInspector<T> : IComponentInspector where T : struct, IComponent
{
    void OnDrawInspector(ref T component);
    Type IComponentInspector.ComponentType => typeof(T);
}

