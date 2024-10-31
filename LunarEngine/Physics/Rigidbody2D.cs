using System.Numerics;
using LunarEngine.GameObjects;
using Serilog;

namespace LunarEngine.Physics;

public sealed class Rigidbody2D : Component
{
    private Transform _cachedTransform;
    public Vector3 Velocity;
    public Vector3 Acceleration;
    public float Mass;
    public bool IsInterpolating;
    public float GravityScale = 1.0f;
    internal Vector3 PreviousPosition;
    internal Vector3 CurrentPosition;
    internal Vector3 NewPosition;
    
    public Rigidbody2D(GameObject gameObject) : base(gameObject)
    {
    }

    public override void Awake()
    {
        _cachedTransform = Transform;
        PreviousPosition = _cachedTransform.Position;
        CurrentPosition = _cachedTransform.Position;
        NewPosition = _cachedTransform.Position;
        PhysicsEngine.AddPhysicsObjects(this);
    }

    public override void FixedUpdate(float fixedDelta)
    {
    }

    public override void Clone(IComponent component)
    {
        var rb = (Rigidbody2D)component;
        Mass = rb.Mass;
        GravityScale = rb.GravityScale;
        IsInterpolating = rb.IsInterpolating;
    }

    public override void Update(double delta)
    {
        _cachedTransform.Position = Vector3.Lerp(PreviousPosition, CurrentPosition, PhysicsEngine.InterpolatedStep);
    }

    public void OverridePosition(Vector3 position)
    {
        PreviousPosition = position;
        CurrentPosition = position;
        NewPosition = position;
    }
}