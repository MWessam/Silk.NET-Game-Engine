using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Renderer;
using LunarEngine.Renderer.OpenGL;
using Serilog;
using StbImageSharp;

namespace LunarEngine.ECS.Components;
public struct SpriteRenderer : IComponent
{
    public Sprite? Sprite;
    public Vector4 Color;
}
public struct LGShader : IComponent
{
    public ShaderHandle Value;
}

