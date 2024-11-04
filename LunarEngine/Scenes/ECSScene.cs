using System.Runtime.InteropServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Components;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;

namespace LunarEngine.Scenes;

public class ECSScene
{
    public readonly World World;
    public TransformSystem TransformSystem;
    public SpriteRendererSystem SpriteRendererSystem;
    public CameraSystem CameraSystem;
    private InitializationSystem _initializationSystem;
    private ShaderSystem _shaderSystem;
    private List<ScriptableSystem> _systems;
    private Entity _birb;
    private Entity _camera;
    public ECSScene()
    {
        World = World.Create();
        _camera = World.Create<Camera, Transform, Position, NeedsInitialization>();
        World.Create<ShaderComponent, TextureComponent, NeedsInitialization>();
        _birb = World.Create<SpriteRendererComponent, Transform, Position, NeedsInitialization>();
        TransformSystem = new TransformSystem(World);
        SpriteRendererSystem = new SpriteRendererSystem(GraphicsEngine.Api, World);
        CameraSystem = new CameraSystem(World);
        _initializationSystem = new InitializationSystem(World);
        _shaderSystem = new ShaderSystem(World);
    }
    public bool IsActive = true;
    public int SceneId { get; set; }
    public void Awake()
    {
        _shaderSystem.Awake();
        TransformSystem.Awake();
        SpriteRendererSystem.Awake();
        CameraSystem.Awake();
        foreach (var system in CollectionsMarshal.AsSpan(_systems))
        {
            system.Awake();
        }
    }

    public void Start()
    {
        TransformSystem.Start();
        SpriteRendererSystem.Start();
        CameraSystem.Start();
        foreach (var system in CollectionsMarshal.AsSpan(_systems))
        {
            system.Start();
        }
    }

    public void Update(float dt)
    {
        TransformSystem.Update(dt);
        CameraSystem.Update(dt);
        foreach (var system in CollectionsMarshal.AsSpan(_systems))
        {
            system.Update(dt);
        }
        _initializationSystem.Update(dt);
        _shaderSystem.Update(dt);
    }

    public void Tick(float dt)
    {
        foreach (var system in CollectionsMarshal.AsSpan(_systems))
        {
            system.Tick(dt);
        }
    }
}

