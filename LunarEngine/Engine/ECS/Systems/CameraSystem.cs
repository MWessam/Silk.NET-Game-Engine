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
    [All<Camera, Position, IsInstantiating>]
    public void InitializeCamera(Entity entity, ref Camera camera, ref Position position)
    {
        camera.Width = 5;
        camera.Height = 5;
        camera.Near = 0.1f;
        camera.Far = 1000.0f;
        
        position.Value = new Vector3(0.0f, 0.0f, -1.0f);
        World.Add<DirtyTransform>(entity);
    }
    [Query]
    [All<Camera, Position, Transform>]
    public void UpdateViewProjection(ref Camera camera, ref Position position, ref Transform transform)
    {
        var forward = new Vector3(transform.Value.M31, transform.Value.M32, transform.Value.M33);
        var up = new Vector3(transform.Value.M21, transform.Value.M22, transform.Value.M23);
        camera.View = Matrix4x4.CreateLookAt(position.Value, position.Value + forward, up);
        camera.Projection = Matrix4x4.CreateOrthographic(camera.Width, camera.Height, camera.Near, camera.Far);
    }
    [Query]
    [All<Camera>]
    public void UpdateViewProjectionUniform(ref Camera camera)
    {
        var viewProjection = camera.View * camera.Projection;
        var viewProjectionEvent = new ViewProjectionEvent()
        {
            ViewProjection = viewProjection,
        };
        EventBus.Send(viewProjectionEvent);
    }

    public void UpdateViewportCamera(Vector2D<int> viewport)
    {
        var cameraQuery = new QueryDescription().WithAll<Camera>();
        float aspectRatio = (float)viewport.X / viewport.Y;
        World.Query(cameraQuery, (ref Camera camera) =>
        {
            camera.Width = camera.Height * aspectRatio;
        });
    }
}