using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngine.Scenes;
using Serilog;

namespace LunarEngine.Physics;

public static class PhysicsEngine
{
    public const float FIXED_TIMESTAMP = 0.01666667f;
    public static readonly Vector3 GRAVITY = new Vector3(0.0f, -9.89665f, 0.0f);
    public static float InterpolatedStep;
    private static List<Rigidbody2D> _rigidBodies = new(1024);
    private static LinkedList<Rigidbody2D> _rigidBodiesToAdd = new();
    private static bool _isInPhysicsLoop;

    public static void AddPhysicsObjects(IEnumerable<Rigidbody2D> rigidBodies)
    {
        foreach (var rb in rigidBodies)
        {
            _rigidBodiesToAdd.AddLast(rb);
        }
    }
    public static void AddPhysicsObjects(params Rigidbody2D[] rigidBodies)
    {
        foreach (var rb in rigidBodies)
        {
            _rigidBodiesToAdd.AddLast(rb);
        }
    }

    public static void TickPhysics()
    {
        _isInPhysicsLoop = true;
        var physicsSpan = CollectionsMarshal.AsSpan(_rigidBodies);
        for (int i = 0; i < _rigidBodies.Count; ++i)
        {
            var rb = physicsSpan[i];
            SolveVerlet(rb);
        }
        _isInPhysicsLoop = false;
    }

    private static void SolveVerlet(Rigidbody2D rb)
    {
        if (rb.Mass == 0.0f) return;
        rb.PreviousPosition = rb.CurrentPosition;
        rb.NetForce += GRAVITY * rb.Mass * rb.GravityScale;
        rb.Acceleration = rb.NetForce / rb.Mass;
        rb.Velocity += rb.Acceleration * (FIXED_TIMESTAMP / 2);
        rb.CurrentPosition += rb.Velocity * FIXED_TIMESTAMP;
        rb.Velocity += rb.Acceleration * (FIXED_TIMESTAMP / 2);
        rb.NetForce = Vector3.Zero; 
        // rb.NewPosition = rb.CurrentPosition * 2.0f - rb.PreviousPosition +
                         // rb.Acceleration * FIXED_TIMESTAMP * FIXED_TIMESTAMP;
    }
    public static void Interpolate(float interpolation)
    {
        InterpolatedStep = interpolation > 1 ? 1 : interpolation;
    }

    public static void UpdateBufferedObjects()
    {
        if (_rigidBodiesToAdd.Count == 0) return;
        _rigidBodies.AddRange(_rigidBodiesToAdd);
        _rigidBodiesToAdd.Clear();
    }
}