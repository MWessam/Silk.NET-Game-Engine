using Arch.Core;
using Arch.System.SourceGenerator;
using LunarEngine.Assets;
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

public class ECSScene
{
    public readonly World World;
    public IWorld ECSWorld => _ecsWorld;

    private readonly ECSWorld _ecsWorld;
    private readonly SystemScheduler _scheduler = new();
    private readonly SpriteRendererSystem _spriteRendererSystem;
    private readonly IRenderer _renderer;
    private readonly AssetManager _assetManager;

    public ECSScene(IRenderer renderer, AssetManager assetManager)
    {
        _ecsWorld = new ECSWorld();
        World = _ecsWorld.NativeWorld;
        _renderer = renderer;
        _assetManager = assetManager;

        _spriteRendererSystem = new SpriteRendererSystem(World, _assetManager, _renderer);

        _scheduler.Register(new PhysicsSystem(World));
        _scheduler.Register(_spriteRendererSystem);
        _scheduler.Register(new TransformSystem(World));
        _scheduler.Register(new CameraSystem(World));
        _scheduler.Register(new InitializationSystem(World));
    }

    public bool IsActive = true;
    public int SceneId { get; set; }

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

    public void SetSceneCameraViewport(Vector2D<int> newViewport)
    {
        var cameraQuery = new QueryDescription().WithAll<CameraComponent>();
        World.Query(cameraQuery, (ref CameraComponent camera) =>
        {
            camera.Camera.Width = camera.Camera.Height * ((float)newViewport.X / newViewport.Y);
        });
    }
}
