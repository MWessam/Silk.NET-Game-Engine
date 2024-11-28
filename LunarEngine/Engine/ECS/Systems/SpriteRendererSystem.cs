using System.Numerics;
using Arch.Buffer;
using Arch.Bus;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Assets;
using LunarEngine.Components;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
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
        CommandBuffer = new CommandBuffer();
        InitSpritesQuery(World);
        CommandBuffer.Playback(World);
    }
    public void Render(in double data)
    {
        CommandBuffer = new CommandBuffer();
        RenderQuery(World, in data);
        CommandBuffer.Playback(World);
    }
    [Query]
    [All<SpriteRenderer>]
    public void InitSprites(Entity entity, ref SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer.Sprite != null) return;
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
        var spriteDrawCommand = new SpriteDrawCommand();
        spriteDrawCommand.Init(spriteRenderer.Sprite, new SpriteData()
        {
            Color = spriteRenderer.Color,
            TransformMatrix = transform.Value
        });
        Renderer.Instance.SubmitRenderCommand(spriteDrawCommand);
    }
}