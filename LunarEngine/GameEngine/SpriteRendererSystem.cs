using System.Numerics;
using Arch.Bus;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Assets;
using LunarEngine.Components;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.OpenGLAPI;
using Silk.NET.OpenGL;

namespace LunarEngine.GameEngine;

public partial class SpriteRendererSystem : ScriptableSystem
{
    Quad _quad;
    private GL _gl;
    public SpriteRendererSystem(GL gl, World world) : base(world)
    {
        _gl = gl;
        _quad = Quad.CreateQuad(_gl);
    }
    public override void Awake()
    {
        InitSpritesQuery(World);
    }
    public override void Update(in float data)
    {
        InitSpritesQuery(World);
    }
    public void Render(in float data)
    {
        RenderQuery(World, in data);
    }
    [Query]
    [All<SpriteRendererComponent, NeedsInitialization>]
    public void InitSprites(Entity entity, ref SpriteRendererComponent spriteRendererComponent)
    {
        var sprite = Sprite.GetSpriteBuilder()
            .WithTexture(AssetManager.TextureLibrary.DefaultAsset.Texture)
            .Build();
        spriteRendererComponent.Color = Vector4.One;
        sprite.Initialize(_quad);
        spriteRendererComponent.Sprite = sprite;
        EventBus.Send(new AssignShaderEvent()
        {
            ShaderName = "default",
            Sprite = sprite
        });
    }
    [Query]
    [All<SpriteRendererComponent, Transform>]
    public void Render([Data] in float dt, ref SpriteRendererComponent spriteRenderer, ref Transform transform)
    {
        spriteRenderer.Sprite.Render(new ()
        {
            Color = spriteRenderer.Color,
            TransformMatrix = transform.Value,
        });
    }
}