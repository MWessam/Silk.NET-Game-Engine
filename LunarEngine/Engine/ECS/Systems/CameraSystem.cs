using System.Numerics;
using Arch.Bus;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Components;
using LunarEngine.ECS.Systems;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using Silk.NET.Maths;

namespace LunarEngine.GameEngine;

public partial class CameraSystem : ScriptableSystem
{
    public CameraSystem(World world) : base(world)
    {
    }

    public override void Awake()
    {
        InitializeCameraQuery(World);
    }

    public override void Update(in double dt)
    {
        InitializeCameraQuery(World);
        UpdateViewProjectionQuery(World);
        UpdateViewProjectionUniformQuery(World);
    }

    [Query]
    [All<CameraComponent, Position, IsInstantiating>]
    public void InitializeCamera(Entity entity, ref CameraComponent camera, ref Position position)
    {
        camera.Camera = new();
        position.Value = new Vector3(0.0f, 0.0f, -1.0f);
        World.Add<DirtyTransform>(entity);
    }
    [Query]
    [All<CameraComponent, Position, Transform>]
    public void UpdateViewProjection(ref CameraComponent camera, ref Position position, ref Transform transform)
    {
        var forward = new Vector3(transform.Value.M31, transform.Value.M32, transform.Value.M33);
        var up = new Vector3(transform.Value.M21, transform.Value.M22, transform.Value.M23);
        camera.Camera.View = Matrix4x4.CreateLookAt(position.Value, position.Value + forward, up);
        camera.Camera.Projection = Matrix4x4.CreateOrthographic(camera.Camera.Width, camera.Camera.Height, camera.Camera.Near, camera.Camera.Far);
    }
    [Query]
    [All<CameraComponent>]
    public void UpdateViewProjectionUniform(ref CameraComponent camera)
    {
        var viewProjection = camera.Camera.View * camera.Camera.Projection;
        camera.Camera.ViewProjection = viewProjection;
    }

    public void UpdateViewportCamera(Vector2D<int> viewport)
    {
        // const float PPU = 10;
        var cameraQuery = new QueryDescription().WithAll<CameraComponent>();
        var aspectRatio = (float)viewport.X / viewport.Y;
        World.Query(cameraQuery, (ref CameraComponent camera) =>
        {
            // var orthoSize = viewport.Y / (2 * PPU);
            // camera.Height = orthoSize;
            camera.Camera.Width = camera.Camera.Height * aspectRatio;
        });
    }
}