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
using LunarEngine.Utilities;
using Silk.NET.OpenGL;

namespace LunarEngine.GameEngine;

public partial class SpriteRendererSystem : ScriptableSystem
{
    Quad _quad;
    private GL _gl;
    public SpriteRendererSystem(GL gl, World world) : base(world)
    {
        Hook();
        _gl = gl;
        _quad = Quad.CreateQuad(_gl);
    }
    public override void Awake()
    {
        InitSpritesQuery(World);
        AdjustScaleQuery(World);
    }
    public override void Update(in double data)
    {
        CommandBuffer = new CommandBuffer();
        InitSpritesQuery(World);
        AdjustScaleQuery(World);
        CommandBuffer.Playback(World);
    }
    public void Render(in double data)
    {
        CommandBuffer = new CommandBuffer();
        RenderQuery(World, in data);
        RenderOutlineQuery(World);
        CommandBuffer.Playback(World);
    }
    [Query]
    [All<SpriteRenderer>]
    public void InitSprites(Entity entity, ref SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer.Sprite != null) return;
        var sprite = Sprite.GetSpriteBuilder()
            .WithTexture(AssetManager.Instance.TextureLibrary.DefaultAsset.Texture)
            .WithShader(AssetManager.Instance.ShaderLibrary.DefaultAsset.Shader)
            .Build();
        spriteRenderer.Color = Vector4.One;
        sprite.Initialize(_quad);
        spriteRenderer.Sprite = sprite;
    }
    

    private Random rng = new Random();
    [Query]
    [All<SpriteRenderer, Scale>]
    public void AdjustScale(Entity entity, ref SpriteRenderer spriteRenderer, ref Scale scale)
    {
        spriteRenderer.Sprite.Shader.SetUniform("rng", rng.NextSingle());
        float worldWidth = ((float)spriteRenderer.Sprite.Texture.Width / spriteRenderer.Sprite.PPU);
        float worldHeight = ((float)spriteRenderer.Sprite.Texture.Height / spriteRenderer.Sprite.PPU);
        scale.BaseValue = new Vector3(worldWidth, worldHeight, 0.0f);
    }
    [Query]
    [All<SpriteRenderer, Transform>]
    public void Render([Data] in double dt, Entity entity, ref SpriteRenderer spriteRenderer, ref Transform transform)
    {
        spriteRenderer.Sprite.Shader.UpdateDirtyUniforms();
        var spriteDrawCommand = new SpriteDrawCommand();
        spriteDrawCommand.Init(spriteRenderer.Sprite, new SpriteData()
        {
            Color = spriteRenderer.Color,
            TransformMatrix = transform.Value
        });
        Renderer.Instance.SubmitRenderCommand(spriteDrawCommand);
        Renderer.Instance.SubmitRenderCommand(new LineDrawCommand(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), 
            new Vector2(-1.5f, 0.5f),
            new Vector2(1.0f, -0.9f),
            new Vector2(1.9f, 1.0f),
            new Vector2(0.0f, -0.3f)));
    }

    [Query]
    [All<SpriteRenderer, Scale, Position>]
    public void RenderOutline(ref SpriteRenderer spriteRenderer, ref Scale scale, ref Position position)
    {
        Renderer.Instance.SubmitRenderCommand(new QuadDrawCommand(position.Value.AsVector2(), scale.ActualValue.AsVector2() * 1.1f, Vector4.One));
    }
    [Event(order:0)]
    public void OnViewProjectionUpdated(ViewProjectionEvent evt)
    {
        var shaderQueryDescription = new QueryDescription().WithAll<SpriteRenderer>();
        ViewProjectionEvent @event = evt;
        World.Query(shaderQueryDescription, (ref SpriteRenderer spriteRenderer) =>
        {
            spriteRenderer.Sprite?.Shader.SetUniform("vp", @event.ViewProjection);
        });
    }
}