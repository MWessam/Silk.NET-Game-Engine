using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngine.Engine.Core;
using LunarEngine.Utilities;

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
        Unknown,
        Line,
        Quad
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

public class LineDrawCommand : RenderCommand
{
    public Vector2 StartPosition;
    public Vector2 EndPosition;
    public WireframeGizmosData_4FC LineInstanceData;

    public LineDrawCommand()
    {
        Type = CommandType.Line;
    }
    public void Init(WireframeGizmosData_4FC instance)
    {
        LineInstanceData = instance;
    }
}

public class QuadDrawCommand : RenderCommand
{
    public WireframeGizmosData_4FC QuadInstanceData;
    public Vector2 Position;
    public Vector2 Scale;
    private float[] _vertices;
    public float[] Vertices => _vertices;
    public QuadDrawCommand(Vector2 position, Vector2 scale, Vector4 color)
    {
        Type = CommandType.Quad;
        QuadInstanceData = new WireframeGizmosData_4FC { Color = color };
        Position = position;
        Scale = scale;
        Vector2 leftBottomCornerVertex = new Vector2(position.X - scale.X, position.Y - scale.Y);
        Vector2 leftTopCornerVertex = new Vector2(position.X - scale.X, position.Y + scale.Y);
        Vector2 rightBottomCornerVertex = new Vector2(position.X + scale.X, position.Y - scale.Y);
        Vector2 rightTopCornerVertex = new Vector2(position.X + scale.X, position.Y + scale.Y);
        _vertices = VectorExtensions.ConvertVectorsToFloats([
            leftBottomCornerVertex, leftTopCornerVertex, rightTopCornerVertex, rightBottomCornerVertex
        ]);
    }

    public void Init()
    {
        
    }
}


[StructLayout(LayoutKind.Sequential)]
public struct WireframeGizmosData_4FC
{
    public Vector4 Color;
}