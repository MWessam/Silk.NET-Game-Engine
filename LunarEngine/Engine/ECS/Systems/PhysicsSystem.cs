using System.Numerics;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Components;
using LunarEngine.GameObjects;

namespace LunarEngine.Physics;

public partial class PhysicsSystem : ScriptableSystem
{
    public static readonly Vector3 GRAVITY = new Vector3(0.0f, -9.89665f, 0.0f);
    public PhysicsSystem(World world) : base(world)
    {
    }
    public override void Update(in double dt)
    {
        CommandBuffer = new();
        InitializePhysicsQuery(World);
        InterpolatePhysicsQuery(World);
        CommandBuffer.Playback(World);
    }

    public override void Tick(double dt)
    {
        CommandBuffer = new();
        InitializePhysicsQuery(World);
        PhysicsTickQuery(World, in dt);
        PhysicsUpdatePositionTickQuery(World);
        CommandBuffer.Playback(World);
    }
    [Query]
    [All<RigidBody2D, Position>]
    public void InitializePhysics(ref RigidBody2D rb, ref Position position)
    {
        if (rb.IsInitialized) return;
        rb.Mass = 1.0f;
        rb.GravityScale = 0.4f;
        rb.CurrentPosition = position.Value;
        rb.IsInitialized = true;
    }
    [Query]
    [All<RigidBody2D, Position, Interpolating>]
    private void InterpolatePhysics(Entity entity, ref Position pos, ref RigidBody2D rb)
    {
        pos.Value = Vector3.Lerp(rb.PreviousPosition, rb.CurrentPosition, (float)PhysicsEngine.InterpolatedTime);
    }
    [Query]
    [All<RigidBody2D, Position>]
    private void PhysicsTick([Data] in double deltaT, Entity entity, ref RigidBody2D rb)
    {
        float deltaTFloat = (float)deltaT;
        rb.PreviousPosition = rb.CurrentPosition;
        rb.NetForce += GRAVITY * rb.Mass * rb.GravityScale;
        rb.Acceleration = rb.NetForce / rb.Mass;
        rb.Velocity += rb.Acceleration * (deltaTFloat / 2);
        rb.CurrentPosition += rb.Velocity * deltaTFloat;
        rb.Velocity += rb.Acceleration * (deltaTFloat / 2);
        rb.NetForce = Vector3.Zero;
        rb.CurrentPosition = rb.CurrentPosition;
    }
    [Query]
    [All<RigidBody2D, Position>, None<Interpolating>]
    private void PhysicsUpdatePositionTick(Entity entity, ref RigidBody2D rb, ref Position pos)
    {
        pos.Value = rb.CurrentPosition;
    }
}