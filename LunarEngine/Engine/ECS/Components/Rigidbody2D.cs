using System.Numerics;
using Arch.Core;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.GameObjects;
using Serilog;

namespace LunarEngine.Physics;
public enum EBodyType
{
    Dynamic = 0,
    Kinematic = 1,
    Static = 2,
    None = -1,
}
public struct RigidBody2D : IComponent
{
    public float Mass;
    public float GravityScale;
    public bool IsInterpolating;
    public Vector2 PreviousPosition;
    public Vector2 CurrentPosition;
    public float CurrentRotation;
    public EBodyType BodyType;
    
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
