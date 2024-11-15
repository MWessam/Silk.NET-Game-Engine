using System.Runtime.InteropServices;
using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Components;
using LunarEngine.ECS.Systems;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.Physics;
using World = Arch.Core.World;

namespace LunarEngine.Scenes;

public class ECSScene
{
    #region SYSTEMS
    public readonly World World;
    private readonly List<ScriptableSystem> _systems = new();
    private readonly TransformSystem _transformSystem;
    private readonly SpriteRendererSystem _spriteRendererSystem;
    private readonly CameraSystem _cameraSystem;
    private readonly InitializationSystem _initializationSystem;
    private readonly ShaderSystem _shaderSystem;
    private readonly PhysicsSystem _physicsSystem;
    private readonly PhysicsInitializationSystem _physicsInitializationSystem;
    private readonly InputSystem _inputSystem;
    #endregion
    public CommandBuffer CommandBuffer = new CommandBuffer();
    public ECSScene()
    {
        World = World.Create();
        _transformSystem = new TransformSystem(World);
        _spriteRendererSystem = new SpriteRendererSystem(GraphicsEngine.Api, World);
        _cameraSystem = new CameraSystem(World);
        _initializationSystem = new InitializationSystem(World);
        _shaderSystem = new ShaderSystem(World);
        _physicsSystem = new PhysicsSystem(World);
        _physicsInitializationSystem = new PhysicsInitializationSystem(World);
        _inputSystem = new InputSystem(World);
    }
    public bool IsActive = true;

    public int SceneId { get; set; }
    public void Awake()
    {
        _shaderSystem.Awake();
        _transformSystem.Awake();
        _spriteRendererSystem.Awake();
        _cameraSystem.Awake();
        _physicsSystem.Awake();
        _physicsInitializationSystem.Awake();
        _inputSystem.Awake();
        foreach (var system in CollectionsMarshal.AsSpan(_systems))
        {
            system.Awake();
        }
    }

    public void Start()
    {
        _transformSystem.Start();
        _spriteRendererSystem.Start();
        _cameraSystem.Start();
        _physicsSystem.Start();
        _physicsInitializationSystem.Start();
        _inputSystem.Start();
        foreach (var system in CollectionsMarshal.AsSpan(_systems))
        {
            system.Start();
        }
        _initializationSystem.Start();
    }

    public void Update(double dt)
    {
        _physicsSystem.Update(dt);
        _transformSystem.Update(dt);
        _cameraSystem.Update(dt);
        _shaderSystem.Update(dt);
        _spriteRendererSystem.Update(dt);
        _inputSystem.Update(dt);
        foreach (var system in CollectionsMarshal.AsSpan(_systems))
        {
            system.Update(dt);
        }
        _initializationSystem.Update(dt);
    }
    public void Tick(double dt)
    {
        _physicsSystem.Tick(dt);
        foreach (var system in CollectionsMarshal.AsSpan(_systems))
        {
            system.Tick(dt);
        }
    }

    public void AfterUpdate()
    {
        CommandBuffer.Playback(World);
    }

    public void RenderScenes(double dt)
    {
        _spriteRendererSystem.Render(dt);
    }
}

public class TestEcsScene : ECSScene
{
    public TestEcsScene()
    {
        World.Create<Camera, Transform, Position, NeedsInitialization>();
        World.Create<TagComponent, Shader, NeedsInitialization>(new TagComponent("default"));
        var birb = World.Create<SpriteRenderer, Transform, Position, NeedsInitialization>();
        World.Add<NeedsPhysicsInitialization, RigidBody2D>(birb);
    }
}
