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
using Serilog;
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
    public override void Update(in double data)
    {
        InitSpritesQuery(World);
    }
    public void Render(in double data)
    {
        RenderQuery(World, in data);
    }
    [Query]
    [All<SpriteRenderer, NeedsInitialization>]
    public void InitSprites(Entity entity, ref SpriteRenderer spriteRenderer)
    {
        var sprite = Sprite.GetSpriteBuilder()
            .WithTexture(AssetManager.TextureLibrary.DefaultAsset.Texture)
            .Build();
        spriteRenderer.Color = Vector4.One;
        sprite.Initialize(_quad);
        spriteRenderer.Sprite = sprite;
        EventBus.Send(new AssignShaderEvent()
        {
            ShaderName = "default",
            Sprite = sprite
        });
    }
    [Query]
    [All<SpriteRenderer, Transform>]
    public void Render([Data] in double dt, Entity entity, ref SpriteRenderer spriteRenderer, ref Transform transform)
    {
        Log.Debug(transform.Value.Translation.ToString());
        spriteRenderer.Sprite.Render(new ()
        {
            Color = spriteRenderer.Color,
            TransformMatrix = transform.Value,
        });
    }
}