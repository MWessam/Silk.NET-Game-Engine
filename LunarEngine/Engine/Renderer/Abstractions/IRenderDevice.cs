using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace LunarEngine.Renderer;

public enum BlendFactor
{
    Zero = 0,
    One = 1,
    SrcColor = 0x0300,
    OneMinusSrcColor = 0x0301,
    SrcAlpha = 0x0302,
    OneMinusSrcAlpha = 0x0303,
    DstAlpha = 0x0304,
    OneMinusDstAlpha = 0x0305,
    DstColor = 0x0306,
    OneMinusDstColor = 0x0307,
}

public interface IRenderDevice : IDisposable
{
    IBuffer CreateBuffer<T>(Span<T> data, BufferTargetARB type, BufferUsageARB usage) where T : unmanaged;
    IVertexArray CreateVertexArray();
    IShader CreateShader(string vertexPath, string fragmentPath);
    ITexture2D CreateTexture2D(Span<byte> pixels, uint width, uint height);
    ITexture2D CreateTexture2D(ImageResult image);
    IFrameBuffer CreateFrameBuffer(Vector2D<int> size);

    void SetViewport(int x, int y, uint width, uint height);
    void SetClearColor(float r, float g, float b, float a);
    void Clear(uint mask);
    void DrawArrays(PrimitiveType type, int first, uint count);
    void DrawElements(PrimitiveType type, uint count, DrawElementsType elementType, nint offset);
    void BindDefaultFramebuffer();

    void EnableBlend();
    void BlendFunc(BlendFactor src, BlendFactor dst);
    void LineWidth(float width);
}
