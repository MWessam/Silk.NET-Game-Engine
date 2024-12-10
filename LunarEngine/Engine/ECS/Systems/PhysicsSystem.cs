using System.Numerics;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Components;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.GameObjects;
using LunarEngine.Utilities;

namespace LunarEngine.Physics;

public partial class PhysicsSystem : ScriptableSystem
{
    public static readonly Vector2 GRAVITY = new Vector2(0.0f, -9.89665f);
    public PhysicsSystem(World world) : base(world)
    {
    }

    public override void Awake()
    {
        InitializePhysicsQuery(World);
    }

    public override void Update(in double dt)
    {
        CommandBuffer = new();
        InitializePhysicsQuery(World);
        PhysicsInterpolationQuery(World);
        CommandBuffer.Playback(World);
    }

    public override void Tick(double dt)
    {
        CommandBuffer = new();
        InitializePhysicsQuery(World);
        InitializeAABBQuery(World);
        InitializeRigidbodyAABBQuery(World);
        PhysicsTickQuery(World, in dt);
        PhysicsUpdatePositionTickQuery(World);
        UpdateAABBPositionQuery(World);
        CheckCollisionsQuery(World);
        CommandBuffer.Playback(World);
    }
    [Query]
    [All<RigidBody2D, Position>]
    public void InitializePhysics(ref RigidBody2D rb, ref Position position)
    {
        if (rb.IsInitialized) return;
        rb.Mass = 1.0f;
        rb.GravityScale = 0.0f;
        rb.CurrentPosition = position.Value.AsVector2();
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
    [Query]
    [All<BoxCollider2D, Position>]
    public void UpdateAABBPosition(Entity entity, ref BoxCollider2D box, ref Position position)
    {
        if (World.TryGet(entity, out Scale scale))
        {
            box.Width = scale.ActualValue.X;
            box.Height = scale.ActualValue.Y;
        }
        box.Position = position.Value.AsVector2();
    }
    [Query]
    [All<RigidBody2D, Position>]
    private void PhysicsTick([Data] in double deltaT, Entity entity, ref RigidBody2D rb, ref Position position)
    {
        if (rb.BodyType == EBodyType.Static) return;
        // // Update angular motion
        // float angularAcceleration = rb.NetTorque / rb.MomentOfInertia;
        // rb.AngularVelocityRadSec += angularAcceleration * (float)deltaT;
        // rb.CurrentRotation += rb.AngularVelocityRadSec * (float)deltaT;
        //
        // Vector2 forwardDirection = new Vector2(MathF.Cos(rb.CurrentRotation), MathF.Sin(rb.CurrentRotation));
        // rb.ExternalForce += forwardDirection;
        
        float deltaTFloat = (float)deltaT;
        rb.PreviousPosition = rb.CurrentPosition;
        rb.TransientForce += GRAVITY * rb.Mass * rb.GravityScale;
        var netForce = rb.TransientForce + rb.ExternalForce;
        rb.Acceleration = netForce / rb.Mass;
        rb.Velocity += rb.Acceleration * (deltaTFloat / 2);
        rb.CurrentPosition += rb.Velocity * deltaTFloat;
        rb.Velocity += rb.Acceleration * (deltaTFloat / 2);
        rb.TransientForce = Vector2.Zero;
    }
    [Query]
    [All<RigidBody2D, Position, Transform>]
    private void PhysicsUpdatePositionTick(Entity entity, ref RigidBody2D rb, ref Position pos, ref Transform transform)
    {
        if (rb.BodyType == EBodyType.Static) return;
        if (rb.IsInterpolating) return;
        pos.Value = rb.CurrentPosition.AsVector3(pos.Value.Z);
        pos.IsDirty = true;

    }
    [Query]
    [All<RigidBody2D, Position, Transform>]
    private void PhysicsInterpolation(Entity entity, ref RigidBody2D rb, ref Position pos, ref Transform transform)
    {
        if (rb.BodyType == EBodyType.Static) return;
        if (!rb.IsInterpolating) return;
        pos.Value = Vector3.Lerp(rb.PreviousPosition.AsVector3(), rb.CurrentPosition.AsVector3(), PhysicsEngine.InterpolatedTime);
        pos.IsDirty = true;

    }

    [Query]
    [All<BoxCollider2D, RigidBody2D, Position>]
    private void CheckCollisions(Entity entity, ref BoxCollider2D box1, ref RigidBody2D rb1, ref Position position)
    {
        QueryDescription staticCollidersQuery = new QueryDescription().WithAll<BoxCollider2D, Position>();
        BoxCollider2D box1Copy = box1;
        bool hasCollided = false;
        World.Query(in staticCollidersQuery, (Entity entity2, ref BoxCollider2D box2) =>
        {
            if (entity2 == entity)
            {
                return;
            }
            if (CheckAABBCollision(box1Copy, box2))
            {
                ResolveCollision(ref box1Copy, ref box2);
                hasCollided = true;
            }
        });
        if (hasCollided)
        {
            rb1.CurrentPosition = box1Copy.Position;
            rb1.PreviousPosition = box1Copy.Position;
            position.Value = box1Copy.Position.AsVector3();
        }
    }
    
    private void ResolveCollision(ref BoxCollider2D box1, ref BoxCollider2D box2)
    {
        // Calculate the overlap in both axes
        float overlapX = MathF.Min(box1.MaxX - box2.MinX, box2.MaxX - box1.MinX);
        float overlapY = MathF.Min(box1.MaxY - box2.MinY, box2.MaxY - box1.MinY);

        // Resolve the collision by adjusting positions
        if (overlapX < overlapY)
        {
            // Push the boxes along the X axis
            if (box1.MinX < box2.MinX)
                box1.Position.X -= overlapX / 2;  // Move box1 to the left
            else
                box1.Position.X += overlapX / 2;  // Move box1 to the right

            // // Move box2 in the opposite direction
            // box2.Position.X -= overlapX / 2;
        }
        else
        {
            // Push the boxes along the Y axis
            if (box1.MinY < box2.MinY)
                box1.Position.Y -= overlapY / 2;  // Move box1 down
            else
                box1.Position.Y += overlapY / 2;  // Move box1 up

            // // Move box2 in the opposite direction
            // box2.Position.Y -= overlapY / 2;
        }

        // Optionally, you can adjust the velocities to reflect the impact (if using physics simulation)
        // This part would depend on how you model object movement and restitution (elasticity, friction, etc.)
        // For simplicity, here we don't adjust the velocities, but you could add bounce/friction:
    
        // Reflect velocities or adjust based on restitution factor (bounciness, friction, etc.)
        // Example: apply simple elastic collision response (can be extended)
        float restitution = 0.8f; // Example restitution (bounciness)
        // velocity1 = velocity1 * restitution;
        // velocity2 = velocity2 * restitution;
    }
    
    // Check if two AABBs collide
    public bool CheckAABBCollision(BoxCollider2D box1, BoxCollider2D box2)
    {
        if (box1.MaxX < box2.MinX || box1.MinX > box2.MaxX)
            return false;  // No collision: one box is completely to the left or right of the other.

        if (box1.MaxY < box2.MinY || box1.MinY > box2.MaxY)
            return false;  // No collision: one box is completely above or below the other.

        return true;  // Collision detected
    }
}