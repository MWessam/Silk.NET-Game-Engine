using System.Numerics;
using Arch.Core;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.GameObjects;
using Serilog;

namespace LunarEngine.Physics;
public struct RigidBody2D : IComponent
{
    public float Mass;
    public float GravityScale;
    public bool IsInterpolating;
    
    public Vector2 PreviousPosition;
    public Vector2 CurrentPosition;
    public float CurrentRotation;
    
    // Forces
    public Vector2 TransientForce;
    public Vector2 ExternalForce;
    public Vector2 Velocity;
    public Vector2 Acceleration;
    public float AngularVelocityRadSec;
    public bool IsInitialized;
    public float MomentOfInertia;
    public float NetTorque;
}
