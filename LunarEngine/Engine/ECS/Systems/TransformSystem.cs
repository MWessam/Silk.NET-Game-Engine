using System.Numerics;
using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Components;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;

namespace LunarEngine.GameEngine;

public partial class TransformSystem : ScriptableSystem
{
    private static Random _rng = new Random();

    public TransformSystem(World world) : base(world)
    {
    }

    public override void Awake()
    {
        InitializeTransformMatrixQuery(World);
    }

    public override void Update(in double dt)
    {
        CommandBuffer = new();
        InitializeTransformMatrixQuery(World);
        UpdateTransformMatrixNoRotNoScaleQuery(World);
        UpdateTransformMatrixNoPosNoScaleQuery(World);
        UpdateTransformMatrixNoRotNoPosQuery(World);
        UpdateTransformMatrixNoRotQuery(World);
        UpdateTransformMatrixNoPosQuery(World);
        UpdateTransformMatrixNoScaleQuery(World);
        UpdateTransformMatrixAllQuery(World);
        CommandBuffer.Playback(World);
    }
    [Query]
    [All<Transform, NeedsInitialization>]
    public void InitializeTransformMatrix(Entity entity, ref Transform transform, ref NeedsInitialization _)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) *
                          Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
    }
    [Query]
    [All<Position, Transform>, None<Rotation, Scale>]
    public void UpdateTransformMatrixNoRotNoScale(Entity entity, ref Position position, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) *
                          Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                          Matrix4x4.CreateTranslation(position.Value);
    }
    [Query]
    [All<Rotation, Transform>, None<Position, Scale>]
    public void UpdateTransformMatrixNoPosNoScale(Entity entity, ref Rotation rotation, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) *
                          Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
    }
    [Query]
    [All<Scale, Transform>, None<Rotation, Position>]
    public void UpdateTransformMatrixNoRotNoPos(Entity entity, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) *
                          Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
    }
    [Query]
    [All<Position, Scale, Transform>, None<Rotation>]
    public void UpdateTransformMatrixNoRot(Entity entity, ref Position position, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) *
                          Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                          Matrix4x4.CreateTranslation(position.Value);
    }
    [Query]
    [All<Rotation, Scale, Transform>, None<Position>]
    public void UpdateTransformMatrixNoPos(Entity entity, ref Rotation rotation, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) *
                          Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
    }
    [Query]
    [All<Rotation, Position, Transform>, None<Scale>]
    public void UpdateTransformMatrixNoScale(Entity entity, ref Rotation rotation, ref Position position, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) *
                          Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(position.Value);
    }
    [Query]
    [All<Position, Rotation, Scale, Transform>]
    public void UpdateTransformMatrixAll(Entity entity, ref Rotation rotation, ref Position position, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) *
                          Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(position.Value);
    }
}