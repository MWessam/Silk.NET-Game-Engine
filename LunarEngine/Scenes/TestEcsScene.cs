using LunarEngine.Components;
using LunarEngine.ECS.Components;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.Physics;

namespace LunarEngine.Scenes;

public class TestEcsScene : ECSScene
{
    public TestEcsScene()
    {
        World.Create<Name, Camera, Transform, Position, NeedsInitialization>(new()
        {
            Value = "Camera",
        });
        World.Create<TagComponent, Shader, NeedsInitialization>(new TagComponent("default"));
        var birb = World.Create<Name, SpriteRenderer, Transform, Position, NeedsInitialization>(new Name()
        {
            Value = "Birb"
        });
        var birb2 = World.Create<Name, SpriteRenderer, Transform, Position, NeedsInitialization>(new Name()
        {
            Value = "Birb2"
        });
        // World.Add<NeedsPhysicsInitialization, RigidBody2D>(birb);
    }
}