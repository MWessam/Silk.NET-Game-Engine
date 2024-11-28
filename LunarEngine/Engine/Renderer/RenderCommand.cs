using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngine.Engine.Core;

namespace LunarEngine.Engine.Graphics;

/// <summary>
/// Base class for all render commands.
/// Will be fed to the renderer and the renderer will know how to use them approperiately.
/// </summary>
public class RenderCommand
{
    public enum CommandType
    {
        Callback,
        SpriteDraw,
        Unknown
    }
    public CommandType Type = CommandType.Unknown;
}

public class SpriteDrawCommand : RenderCommand
{
    public Sprite Sprite;
    public SpriteData SpriteData;
    
    public SpriteDrawCommand()
    {
        Type = CommandType.SpriteDraw;
    }
    
    public void Init(Sprite sprite, SpriteData spriteData)
    {
        Sprite = sprite;
        SpriteData = spriteData;
    }
}