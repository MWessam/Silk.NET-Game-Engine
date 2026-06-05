using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngine.Assets;
using LunarEngine.GameObjects;
using Silk.NET.OpenGL;

namespace LunarEngine.Engine.Graphics;

public class Sprite : IDisposable
{
    public ITexture2D Texture { get; private set; }
    public IShader Shader { get; private set; }
    public int PPU = 1000;
    private IBuffer _instanceBuffer;
    private IVertexArray _vao;
    private IRenderDevice _device;
    public Sprite(ITexture2D texture, IShader shader, IRenderDevice device)
    {
        Texture = texture;
        Shader = shader;
        _device = device;
    }
    public void ChangeShader(IShader shaderHandle)
    {
        Shader = shaderHandle;
    }
    public void ChangeTexture(ITexture2D textureHandle)
    {
        Texture = textureHandle;
    }
    public void Bind(SpriteData spriteData)
    {
        _vao.Bind();
        _instanceBuffer.SetData(spriteData);
        Texture.Bind();
        Shader.Bind();
    }
    public void Initialize(Quad quad)
    {
        _vao = _device.CreateVertexArray();
        quad.BindToVAO(_vao);
        _vao.Bind();
        _instanceBuffer = _device.CreateBuffer<float>(Span<float>.Empty, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw);
        _instanceBuffer.Bind();
        var layout = new BufferLayout();
        layout.Push(1, ElementType.Mat4, true);    
        layout.Push(4, ElementType.Float, true);
        _vao.AddVertexBuffer(_instanceBuffer, layout);
    }
    public void Dispose()
    {
        Texture.Dispose();
        Shader.Dispose();
        _instanceBuffer.Dispose();
        _vao.Dispose();
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct SpriteData
{
    public Matrix4x4 TransformMatrix;
    public Vector4 Color;
}
