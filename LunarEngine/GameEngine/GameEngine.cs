using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Assets;
using LunarEngine.Components;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.InputEngine;
using LunarEngine.OpenGLAPI;
using LunarEngine.Physics;
using LunarEngine.Scenes;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.GameEngine;

public class GameEngine
{
    private Input _input;
    private SceneManager _sceneManager;
    private bool _isRunning;
    private object _physicsLock = 0;
    private Thread _physicsLoop;
    private double _accumulatedTime;
    private PhysicsEngine _physicsEngine = new();
    private GraphicsEngine _graphicsEngine;


    private List<ScriptableSystem> _systems;

    
    public static GameEngine CreateGameEngine()
    {
        GameEngine engine = new GameEngine();
        engine.Initialize();
        
        return engine;
    }
    internal void StartEngine()
    {
        _graphicsEngine.Start();
    }
    private void Initialize()
    {
        _input = Input.Create();
        _sceneManager = new SceneManager();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/log.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        CreateWindow();
    }
    private void CreateWindow()
    {
        _graphicsEngine = new GraphicsEngine();
        _graphicsEngine.Initialize();
        _graphicsEngine.OnUpdateLoopTick += GameLoop;
        _graphicsEngine.OnApiInitialized += OnApiInitialized;
        _graphicsEngine.OnWindowInitialized += OnEngineStart;
        _graphicsEngine.OnViewportResized += OnViewportResized;
        _graphicsEngine.OnWindowClosed += OnClose;
    }

    private void OnClose()
    {
        _isRunning = false;
    }

    private void OnViewportResized(Vector2D<int> obj)
    {
        // _sceneManager.OnViewportResized();
    }
    private void OnEngineStart(IWindow window)
    {
        _isRunning = true;
        IInputContext context = window.CreateInput();
        foreach (var keyboard in context.Keyboards)
        {
            keyboard.KeyDown += _input.OnKeyDown;
            keyboard.KeyUp += _input.OnKeyUp;
        }

        foreach (var mouse in context.Mice)
        {
            mouse.MouseDown += _input.OnMouseDown;
            mouse.MouseUp += _input.OnMouseUp;
            mouse.MouseMove += _input.OnMouseMove;
            mouse.Scroll += _input.OnMouseScroll;
        }

        var activeSceneWorld = _sceneManager.ActiveScenes.World;
        _sceneManager.AwakeScenes();
        _sceneManager.StartScenes();
        _graphicsEngine.InjectSpriteRendererSystem(_sceneManager.ActiveScenes.SpriteRendererSystem);
        _physicsEngine.InitializePhysicsSystem(activeSceneWorld);
    }

    private void OnApiInitialized(GL gl)
    {
        AssetManager.InitializeAssetManager();
        LoadScene();
    }
    private void LoadScene()
    {
        var scene = new ECSScene();
        _sceneManager.AddScene(scene);
        
    }
    private void GameLoop(double dt)
    {
        Time.DeltaTime = (float)dt;
        _accumulatedTime += dt;
        _input.Update(dt);
        _sceneManager.UpdateScenes(dt);
        while (_accumulatedTime >= PhysicsEngine.FIXED_TIMESTAMP)
        {
            _physicsEngine.TickPhysics(PhysicsEngine.FIXED_TIMESTAMP);
            _sceneManager.TickScenes(PhysicsEngine.FIXED_TIMESTAMP);
            _accumulatedTime -= PhysicsEngine.FIXED_TIMESTAMP;
        }
        _physicsEngine.InterpolatePhysics((float)(_accumulatedTime / PhysicsEngine.FIXED_TIMESTAMP));
    }
}
public partial class TransformSystem : ScriptableSystem
{
    public TransformSystem(World world) : base(world)
    {
    }

    public override void Awake()
    {
        InitializeTransformMatrixQuery(World);
    }
    [Query]
    [All<Transform, NeedsInitialization>]
    public void InitializeTransformMatrix(Entity entity, ref Transform transform, ref NeedsInitialization _)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) *
                          Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
        entity.Remove<NeedsInitialization>();
    }
[Query]
    [All<Position, Transform, DirtyTransform>, None<Rotation, Scale>]
    public void UpdateTransformMatrixNoRotNoScale(Entity entity, ref Position position, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) * Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                 Matrix4x4.CreateTranslation(position.Value);
        World.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Rotation, Transform, DirtyTransform>, None<Position, Scale>]
    public void UpdateTransformMatrixNoPosNoScale(Entity entity, ref Rotation rotation, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) * Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
        World.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Scale, Transform, DirtyTransform>, None<Rotation, Position>]
    public void UpdateTransformMatrixNoRotNoPos(Entity entity, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) * Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
        World.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Position, Scale, Transform, DirtyTransform>, None<Rotation>]
    public void UpdateTransformMatrixNoRot(Entity entity, ref Position position, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) * Matrix4x4.CreateFromQuaternion(Quaternion.Identity) *
                          Matrix4x4.CreateTranslation(position.Value);
        World.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Rotation, Scale, Transform, DirtyTransform>, None<Position>]
    public void UpdateTransformMatrixNoPos(Entity entity, ref Rotation rotation, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) * Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(Vector3.Zero);
        World.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Rotation, Position, Transform, DirtyTransform>, None<Scale>]
    public void UpdateTransformMatrixNoScale(Entity entity, ref Rotation rotation, ref Position position, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(Vector3.One) * Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(position.Value);
        World.Remove<DirtyTransform>(entity);
    }
    [Query]
    [All<Position, Rotation, Scale, Transform, DirtyTransform>]
    public void UpdateTransformMatrixAll(Entity entity, ref Rotation rotation, ref Position position, ref Scale scale, ref Transform transform)
    {
        transform.Value = Matrix4x4.CreateScale(scale.Value) * Matrix4x4.CreateFromQuaternion(rotation.Value) *
                          Matrix4x4.CreateTranslation(position.Value);
        World.Remove<DirtyTransform>(entity);
    }
}



public partial class SpriteRendererSystem : ScriptableSystem
{
    Quad _quad;
    private GL _gl;
    public SpriteRendererSystem(GL gl, World world) : base(world)
    {
        _gl = gl;
        _quad = Quad.CreateQuad(_gl);
    }
    // public override void Awake()
    // {
    //     InitSpirtesQuery(World);
    // }
    [Query]
    [All<SpriteRendererComponent, NeedsInitialization>]
    public void InitSpirtes(Entity entity, ref SpriteRendererComponent spriteRendererComponent,
        ref NeedsInitialization _)
    {
        var sprite = Sprite.GetSpriteBuilder()
            .WithShader(AssetManager.ShaderLibrary.DefaultAsset.Shader)
            .WithTexture(AssetManager.TextureLibrary.DefaultAsset.Texture)
            .Build();
        sprite.Initialize(_quad);
        spriteRendererComponent.Sprite = sprite;
    }
    [Query]
    [All<SpriteRendererComponent, Transform>, None<NeedsInitialization>]
    public void Render([Data] in float dt, ref SpriteRendererComponent spriteRenderer, ref Transform transform)
    {
        spriteRenderer.Sprite.Render(new ()
        {
            Color = spriteRenderer.Color,
            TransformMatrix = transform.Value,
        });
    }
}
public partial class CameraSystem : ScriptableSystem
{
    public CameraSystem(World world) : base(world)
    {
    }

    public override void Awake()
    {
        InitializeCameraQuery(World);
    }

    [Query]
    [All<Camera, Position, NeedsInitialization>]
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
        var up = new Vector3(transform.Value.M11, transform.Value.M12, transform.Value.M13);
        camera.View = Matrix4x4.CreateLookAt(position.Value, position.Value + forward, up);
        camera.Projection = Matrix4x4.CreateOrthographic(camera.Width, camera.Height, camera.Near, camera.Far);
    }
    [Query]
    [All<Camera, ShaderComponent>]
    public void UpdateShaderUniform(ref Camera camera, ref ShaderComponent shaderComponent)
    {
        var viewProjection = camera.View * camera.Projection;
        shaderComponent.ShaderHandle.SetUniform("vp", viewProjection);
        shaderComponent.ShaderHandle.UpdateDirtyUniforms();
    }
}

public static class Time
{
    public static float DeltaTime { get; internal set; }
}