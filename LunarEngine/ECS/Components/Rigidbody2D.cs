using System.Numerics;
using Arch.Core;
using LunarEngine.GameObjects;
using Serilog;

namespace LunarEngine.Physics;
public struct RigidBody2D
{
    public float Mass;
    public float GravityScale;
    public bool IsInterpolating;
    public Vector3 PreviousPosition;
    public Vector3 CurrentPosition;
    public Vector3 Velocity;
    public Vector3 Acceleration;
    public Vector3 NetForce;
}
public struct Interpolating
{
}
public struct NeedsPhysicsInitialization
{
}
