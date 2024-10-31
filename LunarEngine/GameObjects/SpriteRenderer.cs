using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Graphics;

namespace LunarEngine.GameObjects;

public interface IRenderer
{
    void Render();
}

public class SpriteRenderer : Component, IRenderer
{
    public Sprite Sprite;
    public Vector4 Color;
    public SpriteRenderer(GameObject gameObject, Sprite sprite, Vector4 color) : base(gameObject)
    {
        Sprite = sprite;
        Color = color;
    }
    public void Render()
    {
        Sprite.Render(new()
        {
            Color = Color,
            TransformMatrix = Transform.Model,
        });
    }


    public override void Clone(IComponent component)
    {
        var spriteRenderer = (SpriteRenderer)component;
        Sprite = spriteRenderer.Sprite;
        Color = spriteRenderer.Color;
    }
}


internal class SpriteRendererFactory : BaseComponentFactory
{
    public override IComponent? Produce(GameObject gameObject)
    {
        var sprite = Sprite
            .GetSpriteBuilder()
            .WithShader(AssetManager.ShaderLibrary.DefaultAsset.Shader)
            .WithTexture(AssetManager.TextureLibrary.DefaultAsset.Texture)
            .Build();
        return new SpriteRenderer(gameObject, sprite, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
    }
}
