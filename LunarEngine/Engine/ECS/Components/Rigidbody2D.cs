using System.Numerics;
using Arch.Core;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.GameObjects;
using Serilog;

namespace LunarEngine.Physics;
public struct RigidBody2D : IComponent
{
    public float Mass;
    public float GravityScale;
    public bool IsInitialized;
    public Vector3 PreviousPosition;
    public Vector3 CurrentPosition;
    public Vector3 Velocity;
    public Vector3 Acceleration;
    public Vector3 NetForce;
}
public struct Interpolating : IComponent
{
}
public struct NeedsPhysicsInitialization : IComponent
{
}
