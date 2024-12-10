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
    [All<Transform, IsInstantiating>]
    public void InitializeTransformMatrix(Entity entity, ref Transform transform)
    {
        CalculateTransform(ref transform, Quaternion.Identity, Vector3.Zero, Vector3.One, true);
    }
    [Query]
    [All<Position, Transform>, None<Rotation, Scale>]
    public void UpdateTransformMatrixNoRotNoScale(Entity entity, ref Position position, ref Transform transform)
    {
        CalculateTransform(ref transform, Quaternion.Identity, position.Value, Vector3.One, position.IsDirty);
    }
    [Query]
    [All<Rotation, Transform>, None<Position, Scale>]
    public void UpdateTransformMatrixNoPosNoScale(Entity entity, ref Rotation rotation, ref Transform transform)
    {
        CalculateTransform(ref transform, rotation.Value, Vector3.Zero, Vector3.One, rotation.IsDirty);
    }
    [Query]
    [All<Scale, Transform>, None<Rotation, Position>]
    public void UpdateTransformMatrixNoRotNoPos(Entity entity, ref Scale scale, ref Transform transform)
    {
        CalculateTransform(ref transform, Quaternion.Identity, Vector3.Zero, scale.ActualValue, scale.IsDirty);
    }
    [Query]
    [All<Position, Scale, Transform>, None<Rotation>]
    public void UpdateTransformMatrixNoRot(Entity entity, ref Position position, ref Scale scale, ref Transform transform)
    {
        CalculateTransform(ref transform, Quaternion.Identity, position.Value, scale.ActualValue, position.IsDirty || scale.IsDirty);
    }
    [Query]
    [All<Rotation, Scale, Transform>, None<Position>]
    public void UpdateTransformMatrixNoPos(Entity entity, ref Rotation rotation, ref Scale scale, ref Transform transform)
    {
        CalculateTransform(ref transform, rotation.Value, Vector3.Zero, scale.ActualValue, rotation.IsDirty || scale.IsDirty);
    }
    [Query]
    [All<Rotation, Position, Transform>, None<Scale>]
    public void UpdateTransformMatrixNoScale(Entity entity, ref Rotation rotation, ref Position position, ref Transform transform)
    {
        CalculateTransform(ref transform, rotation.Value, position.Value, Vector3.One, rotation.IsDirty || position.IsDirty);
    }
    [Query]
    [All<Position, Rotation, Scale, Transform>]
    public void UpdateTransformMatrixAll(Entity entity, ref Rotation rotation, ref Position position, ref Scale scale, ref Transform transform)
    {
        CalculateTransform(ref transform, rotation.Value, position.Value, scale.ActualValue, rotation.IsDirty || position.IsDirty || scale.IsDirty);
    }

    private static void CalculateTransform(ref Transform transform, Quaternion rotation, Vector3 position, Vector3 scale, bool isDirty)
    {
        if (!isDirty) return;
        transform.Value = Matrix4x4.CreateScale(scale) *
                          Matrix4x4.CreateFromQuaternion(rotation) *
                          Matrix4x4.CreateTranslation(position);
    }
}