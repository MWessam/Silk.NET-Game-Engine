namespace LunarEngine.Engine.Graphics;

public interface IVertexArray : IDisposable
{
    void AddVertexBuffer(IBuffer buffer, BufferLayout layout);
    void SetIndexBuffer(IBuffer buffer);
    void Bind();
    void Unbind();
}
