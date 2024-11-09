using System.Runtime.InteropServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Components;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using World = Arch.Core.World;

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
        var shader = World.Create<TagComponent, ShaderComponent, NeedsInitialization>(new TagComponent("default"));
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
        TransformSystem.Awake();
        SpriteRendererSystem.Awake();
        _shaderSystem.Awake();
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
        _shaderSystem.Update(dt);
        SpriteRendererSystem.Update(dt);
        foreach (var system in CollectionsMarshal.AsSpan(_systems))
        {
            system.Update(dt);
        }
        _initializationSystem.Update(dt);
    }

    public void Tick(float dt)
    {
        foreach (var system in CollectionsMarshal.AsSpan(_systems))
        {
            system.Tick(dt);
        }
    }
}

