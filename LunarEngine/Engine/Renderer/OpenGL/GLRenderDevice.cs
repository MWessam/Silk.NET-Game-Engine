using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace LunarEngine.Engine.Graphics;

public class GLRenderDevice : IRenderDevice
{
    private readonly GL _gl;
    public GL GL => _gl;

    public GLRenderDevice(GL gl)
    {
        _gl = gl;
    }

    public IBuffer CreateBuffer<T>(Span<T> data, BufferTargetARB type, BufferUsageARB usage) where T : unmanaged
    {
        var buf = new BufferObject<T>(_gl, type);
        if (!data.IsEmpty)
        {
            buf.SetBufferData(data);
        }
        return buf;
    }

    public IVertexArray CreateVertexArray() => new VertexArrayObject<float, uint>(_gl);

    public IShader CreateShader(string vertexPath, string fragmentPath) => new ShaderHandle(_gl, vertexPath, fragmentPath);

    public ITexture2D CreateTexture2D(Span<byte> pixels, uint width, uint height) => new TextureHandle(_gl, pixels, width, height);

    public ITexture2D CreateTexture2D(ImageResult image) => new TextureHandle(_gl, image);

    public IFrameBuffer CreateFrameBuffer(Vector2D<int> size) => new FrameBuffer(_gl, size);

    public void SetViewport(int x, int y, uint width, uint height) => _gl.Viewport(x, y, width, height);

    public void SetClearColor(float r, float g, float b, float a) => _gl.ClearColor(r, g, b, a);

    public void Clear(uint mask) => _gl.Clear(mask);

    public void DrawArrays(PrimitiveType type, int first, uint count) => _gl.DrawArrays(type, first, count);

    public unsafe void DrawElements(PrimitiveType type, uint count, DrawElementsType elementType, nint offset)
    {
        _gl.DrawElements(type, count, elementType, (void*)offset);
    }

    public void BindDefaultFramebuffer() => _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

    public void EnableBlend() => _gl.Enable(GLEnum.Blend);

    public void BlendFunc(BlendFactor src, BlendFactor dst) => _gl.BlendFunc((GLEnum)src, (GLEnum)dst);

    public void LineWidth(float width) => _gl.LineWidth(width);

    public void Dispose() { }
}
