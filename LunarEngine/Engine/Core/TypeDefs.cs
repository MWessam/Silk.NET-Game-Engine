using System.Numerics;
using System.Runtime.InteropServices;

namespace LunarEngine.Engine.Core;

[StructLayout(LayoutKind.Sequential)]
public struct QuadVertex_V2F_T2F
{
    public Vector2 Vertices;
    public Vector2 Uv;
}

public class SpriteInstanceVertex_T32F_C4B
{
    public Matrix4x4 TransformMatrix;
    public Vector4Byte Color;
}
public struct Vector4Byte
{
    public byte R;
    public byte B;
    public byte G;
    public byte A;
}