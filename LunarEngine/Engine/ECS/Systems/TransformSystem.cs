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
    [All<Position, Transform>, None<Rotation, Scale, Camera>]
    public void UpdateTransformMatrixNoRotNoScale(Entity entity, ref Position position, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) *
                          Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                          Matrix4x4.CreateTranslation(position.Value);
        // CommandBuffer.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Rotation, Transform, DirtyTransform>, None<Position, Scale>]
    public void UpdateTransformMatrixNoPosNoScale(Entity entity, ref Rotation rotation, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) *
                          Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
        CommandBuffer.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Scale, Transform, DirtyTransform>, None<Rotation, Position>]
    public void UpdateTransformMatrixNoRotNoPos(Entity entity, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) *
                          Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
        CommandBuffer.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Position, Scale, Transform, DirtyTransform>, None<Rotation>]
    public void UpdateTransformMatrixNoRot(Entity entity, ref Position position, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) *
                          Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                          Matrix4x4.CreateTranslation(position.Value);
        CommandBuffer.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Rotation, Scale, Transform, DirtyTransform>, None<Position>]
    public void UpdateTransformMatrixNoPos(Entity entity, ref Rotation rotation, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) *
                          Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
        CommandBuffer.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Rotation, Position, Transform, DirtyTransform>, None<Scale>]
    public void UpdateTransformMatrixNoScale(Entity entity, ref Rotation rotation, ref Position position, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) *
                          Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(position.Value);
        CommandBuffer.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Position, Rotation, Scale, Transform, DirtyTransform>]
    public void UpdateTransformMatrixAll(Entity entity, ref Rotation rotation, ref Position position, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) *
                          Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(position.Value);
        CommandBuffer.Remove<DirtyTransform>(entity);
    }
}