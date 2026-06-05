using System.Numerics;
using Arch.Buffer;
using Arch.Bus;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using LunarEngine.Assets;
using LunarEngine.Components;
using LunarEngine.ECS;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameObjects;
using LunarEngine.Utilities;

namespace LunarEngine.GameEngine;

public partial class SpriteRendererSystem : ScriptableSystem, IRenderSystem
{
    public override SystemStage Stage => SystemStage.Update;
    public override int Order => 10;

    Quad _quad;
    private AssetManager _assetManager;
    private IRenderer _renderer;
    public SpriteRendererSystem(World world, AssetManager assetManager, IRenderer renderer) : base(world)
    {
        _assetManager = assetManager;
        _renderer = renderer;
        _quad = renderer.Quad;
    }
    public override void Awake()
    {
        InitSpritesQuery(World);
        AdjustScaleQuery(World);
    }
    public override void Update(in double data)
    {
        InitSpritesQuery(World);
        AdjustScaleQuery(World);
        CommandBuffer.Playback(World);
    }
    public void Render(in double data)
    {
        RenderQuery(World, in data);
        CommandBuffer.Playback(World);
    }

    public void Render(double deltaTime) => Render(in deltaTime);

    [Query]
    [All<SpriteRenderer>]
    public void InitSprites(Entity entity, ref SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer.Sprite != null) return;
        var sprite = _renderer.CreateSprite(
            _assetManager.GetTextureHandle(_assetManager.TextureLibrary.DefaultAsset),
            _assetManager.GetShaderHandle(_assetManager.ShaderLibrary.DefaultAsset)
        );
        spriteRenderer.Color = Vector4.One;
        spriteRenderer.Sprite = sprite;
    }
    

    private Random rng = new Random();
    [Query]
    [All<SpriteRenderer, Scale>]
    public void AdjustScale(Entity entity, ref SpriteRenderer spriteRenderer, ref Scale scale)
    {
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
        _renderer.SubmitRenderCommand(spriteDrawCommand);
    }


    public void SetViewProjection(Matrix4x4 viewProjection)
    {
        var shaderQueryDescription = new QueryDescription().WithAll<SpriteRenderer>();
        World.Query(shaderQueryDescription, (ref SpriteRenderer spriteRenderer) =>
        {
            spriteRenderer.Sprite?.Shader.SetUniform("vp", viewProjection);
        });
    }
}
