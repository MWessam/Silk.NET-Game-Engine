using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Graphics;

namespace LunarEngine.GameObjects;

public interface IRenderer
{
    void Render();
}

public struct SpriteRendererComponent
{
    public Sprite Sprite;
    public Vector4 Color;
}
