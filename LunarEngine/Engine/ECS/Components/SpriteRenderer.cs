using System.Numerics;
using LunarEngine.Assets;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.Engine.Graphics;
using LunarEngine.Graphics;
using Serilog;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace LunarEngine.GameObjects;
public struct SpriteRenderer : IComponent
{
    public Sprite? Sprite;
    public Vector4 Color;
}
public struct LGShader : IComponent
{
    public ShaderHandle Value;
}

