using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Components;
using LunarEngine.Scenes;
using Serilog;

namespace LunarEngine.Physics;

public class PhysicsEngine
{
    public const float FIXED_TIMESTAMP = 0.01666667f;
    private bool _isInPhysicsLoop;
    private PhysicsSystem _physicsSystem;

    public void InitializePhysicsSystem(World world)
    {
        _physicsSystem = new PhysicsSystem(world);
    }

    public void TickPhysics(float deltaTime)
    {
        _isInPhysicsLoop = true;
        _physicsSystem.PhysicsTickQuery(_physicsSystem.World, in deltaTime);
        _isInPhysicsLoop = false;
    }
    public void InterpolatePhysics(float interpolatedTime)
    {
        _physicsSystem.InterpolatePhysicsQuery(_physicsSystem.World, interpolatedTime);
    }
}

public partial class PhysicsSystem : BaseSystem<World, float>
{
    public static readonly Vector3 GRAVITY = new Vector3(0.0f, -9.89665f, 0.0f);
    public PhysicsSystem(World world) : base(world)
    {
    }
    [Query]
    [All<RigidBody2DComponent, Position>, None<InterpolatingComponent>]
    private void PhysicsTick([Data] in float deltaT, ref RigidBody2DComponent rb, ref Position pos)
    {
        rb.PreviousPosition = rb.CurrentPosition;
        rb.NetForce += GRAVITY * rb.Mass * rb.GravityScale;
        rb.Acceleration = rb.NetForce / rb.Mass;
        rb.Velocity += rb.Acceleration * (deltaT / 2);
        rb.CurrentPosition += rb.Velocity * deltaT;
        rb.Velocity += rb.Acceleration * (deltaT / 2);
        rb.NetForce = Vector3.Zero;
        rb.CurrentPosition = rb.CurrentPosition;
        pos.Value = rb.CurrentPosition;

    }
    [Query]
    [All<InterpolatingComponent>]
    private void InterpolatePhysics([Data] in float interpolatedDeltaT, ref Position pos, ref RigidBody2DComponent rb)
    {
        pos.Value = Vector3.Lerp(rb.PreviousPosition, rb.CurrentPosition, interpolatedDeltaT);
    }
}