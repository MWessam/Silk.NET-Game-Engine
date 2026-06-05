using Silk.NET.Maths;

namespace LunarEngine.Renderer;

public interface IFrameBuffer : IDisposable
{
    void Bind();
    void Unbind();
    void Resize(Vector2D<int> newSize);
    ITexture2D ColorAttachment { get; }
    Vector2D<int> Size { get; }
}
