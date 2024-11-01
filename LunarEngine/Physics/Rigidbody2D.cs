using System.Numerics;
using LunarEngine.GameObjects;
using Serilog;

namespace LunarEngine.Physics;

public sealed class Rigidbody2D : Component
{
    private Transform _cachedTransform;
    public Vector3 Velocity;
    public Vector3 Acceleration;
    public float Mass = 1.0f;
    public bool IsInterpolating = true;
    public float GravityScale = 1.0f;
    internal Vector3 PreviousPosition;
    internal Vector3 CurrentPosition;
    internal Vector3 NetForce;
    
    public Rigidbody2D(GameObject gameObject) : base(gameObject)
    {
    }

    public override void Awake()
    {
        _cachedTransform = Transform;
        PreviousPosition = _cachedTransform.Position;
        CurrentPosition = _cachedTransform.Position;
        PhysicsEngine.AddPhysicsObjects(this);
    }

    public override void Tick(float fixedDelta)
    {
        if (!IsInterpolating)
        {
            _cachedTransform.Position = CurrentPosition;
        }
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
        if (IsInterpolating)
        {
            _cachedTransform.Position = Vector3.Lerp(PreviousPosition, CurrentPosition, PhysicsEngine.InterpolatedStep);
        }
    }

    public void OverridePosition(Vector3 position)
    {
        PreviousPosition = position;
        CurrentPosition = position;
        _cachedTransform.Position = position;
    }
}