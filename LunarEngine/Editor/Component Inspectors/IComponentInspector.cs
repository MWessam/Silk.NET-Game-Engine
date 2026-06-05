using System.Numerics;
using LunarEngine.ECS.Components;

namespace LunarEngine.Editor.Systems;

public interface IComponentInspector
{
    Type ComponentType { get; }
}
public interface IComponentInspector<T> : IComponentInspector where T : struct, IComponent
{
    void OnDrawInspector(ref T component);
    Type IComponentInspector.ComponentType => typeof(T);
}

