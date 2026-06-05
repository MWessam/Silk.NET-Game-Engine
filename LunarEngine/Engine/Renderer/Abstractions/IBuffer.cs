namespace LunarEngine.Renderer;

public interface IBuffer : IDisposable
{
    void Bind();
    void Unbind();
    unsafe void SetData<T>(Span<T> data) where T : unmanaged;
    unsafe void SetData<T>(T data) where T : unmanaged;
}
