using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.Engine.Graphics;
using LunarEngine.Graphics;

namespace LunarEngine.GameObjects;
public struct SpriteRenderer : IComponent
{
    public Sprite Sprite;
    public Vector4 Color;
}
public struct Shader : IComponent
{
    public ShaderHandle Value;
}
public struct Texture : IComponent
{
    public TextureHandle Value;
}