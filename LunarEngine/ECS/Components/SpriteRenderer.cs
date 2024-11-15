using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Graphics;
using LunarEngine.OpenGLAPI;

namespace LunarEngine.GameObjects;
public struct SpriteRenderer
{
    public Sprite Sprite;
    public Vector4 Color;
}
public struct Shader
{
    public ShaderHandle Value;
}
public struct Texture
{
    public TextureHandle Value;
}