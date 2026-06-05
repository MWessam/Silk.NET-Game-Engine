using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Components;
using LunarEngine.Core;
using LunarEngine.ECS.Components;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.Physics;
using World = Arch.Core.World;

namespace LunarEngine.Scenes;

public class TestEcsScene : ECSScene
{
    public TestEcsScene(ServiceContainer services) : base(services)
    {
        // World.Create<Name, CameraComponent, Transform, Position, IsInstantiating>(new()
        // {
            // Value = "Camera",
        // });
        var birb = World.Create<Name, SpriteRenderer, Transform, BoxCollider2D, Position, Scale, IsInstantiating>(new Name()
        {
            Value = "Birb"
        });
        World.Set(birb, new Scale()
        {
            BaseValue = Vector3.One,
            UserValue = Vector3.One,
        });
        
        var wallLeft = World.Create<Name, SpriteRenderer, Transform, Position, Scale, BoxCollider2D, IsInstantiating>(new Name()
        {
            Value = "Collider"
        });
        World.Set(wallLeft, new Scale()
        {
            BaseValue = Vector3.One,
            UserValue = Vector3.One
        });

    }
}
