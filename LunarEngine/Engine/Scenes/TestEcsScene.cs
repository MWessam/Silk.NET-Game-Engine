using ComponentFactories;
using LunarEngine.Components;
using LunarEngine.ECS.Components;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.Physics;

namespace LunarEngine.Scenes;

public class TestEcsScene : ECSScene
{
    public TestEcsScene()
    {
        World.Create<Name, Camera, Transform, Position, IsInstantiating>(new()
        {
            Value = "Camera",
        });
        World.Create<TagComponent, Shader, IsInstantiating>(new TagComponent("default"));
        var birb = World.Create<Name, SpriteRenderer, Transform, Position, IsInstantiating>(new Name()
        {
            Value = "Birb"
        });
        
        var wallLeft = World.Create<Name, SpriteRenderer, Transform, Position, Scale, BoxCollider2D, IsInstantiating>(new Name()
        {
            Value = "Wall Left"
        });
        var wallRight = World.Create<Name, SpriteRenderer, Transform, Position, Scale, BoxCollider2D, IsInstantiating>(new Name()
        {
            Value = "Wall Right"
        });
        var wallUp = World.Create<Name, SpriteRenderer, Transform, Position, Scale, BoxCollider2D, IsInstantiating>(new Name()
        {
            Value = "Wall Up"
        });
        var wallDown = World.Create<Name, SpriteRenderer, Transform, Position, Scale, BoxCollider2D, IsInstantiating>(new Name()
        {
            Value = "Wall Down"
        });
    }
}