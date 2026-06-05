using System.Numerics;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.ECS;
using LunarEngine.ECS.Components;
using LunarEngine.ECS.Components.Physics;
using LunarEngine.Physics;
using LunarEngine.Utilities;

namespace LunarEngine.ECS.Systems;

public partial class PhysicsSystem : ScriptableSystem
{
    public override SystemStage Stage => SystemStage.FixedUpdate;
    public override int Order => 0;

    private readonly PhysicsWorld _physicsWorld;

    public PhysicsSystem(World world) : base(world)
    {
        _physicsWorld = new PhysicsWorld();
    }

    public override void Awake()
    {
        InitializePhysicsQuery(World);
        InitializeAABBQuery(World);
        InitializeRigidbodyAABBQuery(World);
    }

    public override void Tick(double dt)
    {
        InitializePhysicsQuery(World);
        InitializeAABBQuery(World);
        InitializeRigidbodyAABBQuery(World);
        _physicsWorld.Step(World, dt);
        CommandBuffer.Playback(World);
    }

    [Query]
    [All<RigidBody2D, Position>]
    public void InitializePhysics(ref RigidBody2D rb, ref Position position)
    {
        if (rb.IsInitialized) return;
        rb.Mass = 1.0f;
        rb.GravityScale = 0.0f;
        rb.CurrentPosition = position.Value.ToVector2();
        rb.IsInitialized = true;
    }

    [Query]
    [All<BoxCollider2D, IsInstantiating>]
    public void InitializeAABB(Entity entity, ref BoxCollider2D box)
    {
        if (World.TryGet(entity, out Scale scale))
        {
            scale.UserValue = Vector3.One;
            box.Width = scale.ActualValue.X;
            box.Height = scale.ActualValue.Y;
        }
    }

    [Query]
    [All<BoxCollider2D, RigidBody2D, IsInstantiating>]
    public void InitializeRigidbodyAABB(Entity entity, ref BoxCollider2D box, ref RigidBody2D rb)
    {
        if (World.TryGet(entity, out Scale scale))
        {
            scale.UserValue = Vector3.One;
            box.Width = scale.ActualValue.X;
            box.Height = scale.ActualValue.Y;
        }
        box.Position = rb.CurrentPosition;
    }
}
