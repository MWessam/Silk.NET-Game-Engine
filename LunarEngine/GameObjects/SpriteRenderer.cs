using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Graphics;
using LunarEngine.OpenGLAPI;

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

public struct ShaderComponent
{
    public ShaderHandle Value;
}

public struct TextureComponent
{
    public TextureHandle Value;
}