using System.Numerics;
using Arch.Core;
using LunarEngine.Components;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.Utilities;

namespace LunarEngine.Physics;

public class PhysicsWorld
{
    public float FixedTimeStep { get; set; } = 1f / 60f;
    public Vector2 Gravity { get; set; } = new(0, -9.81f);

    private double _accumulatedTime;
    private readonly Dictionary<(int, int), List<Entity>> _spatialHash = new();
    private const float CellSize = 64f;

    public void Step(World world, double deltaTime)
    {
        _accumulatedTime += deltaTime;

        while (_accumulatedTime >= FixedTimeStep)
        {
            RunFixedStep(world, FixedTimeStep);
            _accumulatedTime -= FixedTimeStep;
        }
    }

    private void RunFixedStep(World world, double dt)
    {
        float dtFloat = (float)dt;

        // 1. Integrate rigidbodies
        var rigidBodyQuery = new QueryDescription().WithAll<RigidBody2D, Position>();
        world.Query(in rigidBodyQuery, (Entity entity, ref RigidBody2D rb, ref Position position) =>
        {
            if (rb.BodyType == EBodyType.Static) return;

            rb.PreviousPosition = rb.CurrentPosition;
            rb.TransientForce += Gravity * rb.Mass * rb.GravityScale;
            var netForce = rb.TransientForce + rb.ExternalForce;
            rb.Acceleration = netForce / rb.Mass;
            rb.Velocity += rb.Acceleration * (dtFloat / 2);
            rb.CurrentPosition += rb.Velocity * dtFloat;
            rb.Velocity += rb.Acceleration * (dtFloat / 2);
            rb.TransientForce = Vector2.Zero;
            position.Value = rb.CurrentPosition.ToVector3(position.Value.Z);
            position.IsDirty = true;
        });

        // 2. Update AABB positions
        var aabbQuery = new QueryDescription().WithAll<BoxCollider2D, Position>();
        world.Query(in aabbQuery, (Entity entity, ref BoxCollider2D box, ref Position position) =>
        {
            if (world.TryGet(entity, out Scale scale))
            {
                box.Width = scale.ActualValue.X;
                box.Height = scale.ActualValue.Y;
            }
            box.Position = position.Value.ToVector2();
        });

        // 3. Build spatial hash
        BuildSpatialHash(world);

        // 4. Check collisions using spatial hash
        CheckCollisions(world);
    }

    private void BuildSpatialHash(World world)
    {
        _spatialHash.Clear();

        var query = new QueryDescription().WithAll<BoxCollider2D, Position>();
        world.Query(in query, (Entity entity, ref BoxCollider2D box, ref Position position) =>
        {
            var minCell = GetCell(box.MinX, box.MinY);
            var maxCell = GetCell(box.MaxX, box.MaxY);

            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    var key = (x, y);
                    if (!_spatialHash.TryGetValue(key, out var list))
                    {
                        list = new List<Entity>();
                        _spatialHash[key] = list;
                    }
                    list.Add(entity);
                }
            }
        });
    }

    private static (int x, int y) GetCell(float x, float y)
    {
        return ((int)MathF.Floor(x / CellSize), (int)MathF.Floor(y / CellSize));
    }

    private void CheckCollisions(World world)
    {
        var dynamicQuery = new QueryDescription().WithAll<BoxCollider2D, RigidBody2D, Position>();

        world.Query(in dynamicQuery, (Entity entity, ref BoxCollider2D box1, ref RigidBody2D rb1, ref Position position) =>
        {
            var minCell = GetCell(box1.MinX, box1.MinY);
            var maxCell = GetCell(box1.MaxX, box1.MaxY);

            var checkedEntities = new HashSet<Entity>();
            bool hasCollided = false;
            BoxCollider2D box1Copy = box1;
            RigidBody2D rb1Copy = rb1;

            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    var key = (x, y);
                    if (!_spatialHash.TryGetValue(key, out var cellEntities)) continue;

                    foreach (var otherEntity in cellEntities)
                    {
                        if (otherEntity == entity) continue;
                        if (!checkedEntities.Add(otherEntity)) continue;

                        if (!world.TryGet(otherEntity, out BoxCollider2D box2)) continue;

                        if (CheckAABBCollision(box1Copy, box2))
                        {
                            ResolveCollision(ref rb1Copy, ref box1Copy, ref box2);
                            hasCollided = true;
                        }
                    }
                }
            }

            if (hasCollided)
            {
                rb1.CurrentPosition = box1Copy.Position;
                rb1.PreviousPosition = box1Copy.Position;
                position.Value = box1Copy.Position.ToVector3();
                rb1.Velocity = rb1Copy.Velocity;
            }
        });
    }

    private static void ResolveCollision(ref RigidBody2D rb1Copy, ref BoxCollider2D box1, ref BoxCollider2D box2)
    {
        float overlapX = MathF.Min(box1.MaxX - box2.MinX, box2.MaxX - box1.MinX);
        float overlapY = MathF.Min(box1.MaxY - box2.MinY, box2.MaxY - box1.MinY);

        if (overlapX < overlapY)
        {
            if (box1.MinX < box2.MinX)
                box1.Position.X -= overlapX / 2;
            else
                box1.Position.X += overlapX / 2;
        }
        else
        {
            if (box1.MinY < box2.MinY)
                box1.Position.Y -= overlapY / 2;
            else
                box1.Position.Y += overlapY / 2;
        }

        float restitution = 0.8f;
        rb1Copy.Velocity = rb1Copy.Velocity * restitution;
    }

    public static bool CheckAABBCollision(BoxCollider2D box1, BoxCollider2D box2)
    {
        if (box1.MaxX < box2.MinX || box1.MinX > box2.MaxX)
            return false;

        if (box1.MaxY < box2.MinY || box1.MinY > box2.MaxY)
            return false;

        return true;
    }
}
