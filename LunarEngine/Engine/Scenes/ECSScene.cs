using Arch.Core;
using Arch.System.SourceGenerator;
using LunarEngine.Assets;
using LunarEngine.Core;
using LunarEngine.ECS;
using LunarEngine.ECS.Systems;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.Physics;
using Silk.NET.Maths;
using World = Arch.Core.World;

namespace LunarEngine.Scenes;

public class ECSScene : IScene
{
    public World World { get; }
    IWorld IScene.World => _ecsWorld;

    private readonly ECSWorld _ecsWorld;
    private readonly SystemScheduler _scheduler = new();
    private readonly SpriteRendererSystem _spriteRendererSystem;
    private readonly IRenderer _renderer;
    private readonly AssetManager _assetManager;

    public string Name { get; set; } = string.Empty;
    public SystemScheduler Scheduler => _scheduler;
    public bool IsActive { get; set; } = true;
    public int SceneId { get; set; }

    public ECSScene(ServiceContainer services)
    {
        _ecsWorld = new ECSWorld();
        World = _ecsWorld.NativeWorld;
        _renderer = services.Get<IRenderer>();
        _assetManager = services.Get<AssetManager>();

        _spriteRendererSystem = new SpriteRendererSystem(World, _assetManager, _renderer);

        _scheduler.Register(new PhysicsSystem(World));
        _scheduler.Register(_spriteRendererSystem);
        _scheduler.Register(new TransformSystem(World));
        _scheduler.Register(new CameraSystem(World));
        _scheduler.Register(new InitializationSystem(World));
    }

    public void AddSystem(ScriptableSystem system)
    {
        _scheduler.Register(system);
    }

    public void Awake()
    {
        _scheduler.RunAwake();
    }

    public void Start()
    {
        _scheduler.RunStart();
    }

    public void Update(double dt)
    {
        _scheduler.RunStage(SystemStage.Update, dt);
    }

    public void Tick(double dt)
    {
        _scheduler.RunFixedUpdate(dt);
    }

    public void FixedUpdate(double fixedDeltaTime)
    {
        Tick(fixedDeltaTime);
    }

    public void RenderScenes(double dt, Camera camera)
    {
        _renderer.BeginFrame(camera.ViewProjection);
        _spriteRendererSystem.SetViewProjection(camera.ViewProjection);
        _spriteRendererSystem.Render(dt);
        _renderer.EndFrame();
    }

    public void RenderScenes(double dt)
    {
        var cameraQuery = new QueryDescription().WithAll<CameraComponent>();
        CameraComponent? primaryCamera = default;
        World.Query(cameraQuery, (ref CameraComponent camera) =>
        {
            if (primaryCamera != null)
            {
                return;
            }
            if (camera.IsPrimary)
            {
                primaryCamera = camera;
            }
        });
        if (primaryCamera == null)
        {
            return;
        }
        RenderScenes(dt, primaryCamera.Value.Camera);
    }

    public void Render(IRenderer renderer)
    {
        // TODO: Delegate sprite rendering to passed renderer when ECSScene no longer owns renderer internally
        RenderScenes(0);
    }

    public void SetSceneCameraViewport(Vector2D<int> newViewport)
    {
        var cameraQuery = new QueryDescription().WithAll<CameraComponent>();
        World.Query(cameraQuery, (ref CameraComponent camera) =>
        {
            camera.Camera.Width = camera.Camera.Height * ((float)newViewport.X / newViewport.Y);
        });
    }

    public void Dispose()
    {
        // TODO: Implement proper disposal in Phase 12
    }
}
