using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;

namespace LunarEngine.Physics;

public static class PhysicsEngine
{
    public const float FIXED_TIMESTAMP = 0.01666667f;
    public static readonly Vector3 GRAVITY = new Vector3(0.0f, -9.89665f, 0.0f);
    private static List<Rigidbody2D> _rigidBodies = new(1024);
    private static ConcurrentQueue<Rigidbody2D> _rigidBodiesToAdd = new();
    public static float InterpolatedStep;
    private static bool _isInPhysicsLoop;

    public static void AddPhysicsObjects(IEnumerable<Rigidbody2D> rigidBodies)
    {
        foreach (var rb in rigidBodies)
        {
            _rigidBodies.Add(rb);
        }
    }
    public static void AddPhysicsObjects(params Rigidbody2D[] rigidBodies)
    {
        foreach (var rb in rigidBodies)
        {
            _rigidBodies.Add(rb);
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
            rb.FixedUpdate(FIXED_TIMESTAMP);
        }
        _isInPhysicsLoop = false;
        CheckForModifiedItems();
    }

    private static void SolveVerlet(Rigidbody2D rb)
    {
        rb.Acceleration = GRAVITY * rb.GravityScale;
        rb.NewPosition = rb.CurrentPosition * 2.0f - rb.PreviousPosition +
                         rb.Acceleration * FIXED_TIMESTAMP * FIXED_TIMESTAMP;
        rb.PreviousPosition = rb.CurrentPosition;
        rb.CurrentPosition = rb.NewPosition;
    }

    private static void CheckForModifiedItems()
    {
        // if (_rigidBodiesToAdd.Any())
        // {
        //     foreach (var rb in _rigidBodiesToAdd)
        //     {
        //         _rigidBodies.Add(rb);
        //     }
        // }
    }

    public static void Interpolate(float interpolation)
    {
        InterpolatedStep = interpolation > 1 ? 1 : interpolation;
    }
}