using System.Collections;
using System.Numerics;
using LunarEngine.Utilities;
using Arch.Core;
namespace LunarEngine.Components;
public struct Position
{
    public Vector3 Value;
}
public struct Rotation
{
    public Quaternion Value;
}
public struct Scale
{
    public Vector3 Value;
}
public struct Transform
{
    public Matrix4x4 Value;
}
public struct Parent
{
    public Transform ParentEntity;
}
public struct DirtyTransform
{
}
