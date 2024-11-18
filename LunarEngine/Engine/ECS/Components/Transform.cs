using System.Collections;
using System.Numerics;
using LunarEngine.Utilities;
using Arch.Core;
using LunarEngine.Engine.ECS.Components;

namespace LunarEngine.Components;
public struct Position : IComponent
{
    public Vector3 Value;
}
public struct Rotation : IComponent
{
    public Quaternion Value;
}
public struct Scale : IComponent
{
    public Vector3 Value;
}
public struct Transform : IComponent
{
    public int Id;
    public Matrix4x4 Value;
}
public struct Parent : IComponent
{
    public Transform ParentEntity;
}
public struct DirtyTransform : IComponent
{
}
