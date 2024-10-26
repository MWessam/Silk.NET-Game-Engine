using System.Numerics;
using LunarEngine.Graphics;

namespace LunarEngine.GameObjects;

public interface IRenderer : IComponent
{
    void Render();
}
public class SpriteRenderer : Component, IRenderer
{
    public readonly Sprite Sprite;
    private readonly Vector4 _color;
    public SpriteRenderer(Sprite sprite, Vector4 color)
    {
        Sprite = sprite;
        _color = color;
        
    }
    public void Render()
    {
        Sprite.Render(new()
        {
            Color = _color,
            TransformMatrix = Transform.Model,
        });
    }
}