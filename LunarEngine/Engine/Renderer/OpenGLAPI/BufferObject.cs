using Silk.NET.OpenGL;

namespace LunarEngine.Renderer.OpenGL;

internal interface IBindable
{
    void Bind();
    void Unbind();
}

public struct BufferObject<TDataType> : IDisposable, IBindable, IBuffer
    where TDataType : unmanaged
{
    private uint _handle;
    private BufferTargetARB _bufferType;
    private GL _gl;
    public BufferLayout Layout { get; private set; } = new BufferLayout();

    public BufferObject(GL gl, BufferTargetARB bufferType)
    {
        _gl = gl;
        _bufferType = bufferType;
        _handle = _gl.GenBuffer();
    }

    public unsafe void SetBufferData(Span<TDataType> data)
    {
        Bind();
        fixed (void* d = data)
        {
            _gl.BufferData(_bufferType, (nuint) (data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
    }
    public unsafe void SetBufferData<T>(Span<T> data) where T : unmanaged
    {
        Bind();
        fixed (void* d = data)
        {
            _gl.BufferData(_bufferType, (nuint) (data.Length * sizeof(T)), d, BufferUsageARB.StaticDraw);
        }
    }

    public unsafe void SetBufferData<T>(T data) where T : unmanaged
    {
        Bind();
        _gl.BufferData(_bufferType, (nuint) (sizeof(T)), &data, BufferUsageARB.StaticDraw);
    }

    unsafe void IBuffer.SetData<T>(Span<T> data)
    {
        SetBufferData(data);
    }

    unsafe void IBuffer.SetData<T>(T data)
    {
        SetBufferData(data);
    }

    public void Bind()
    {
        _gl.BindBuffer(_bufferType, _handle);
    }
    public void Unbind()
    {
        _gl.BindBuffer(_bufferType, 0);
    }
    public void Dispose()
    {
        Unbind();
        _gl.DeleteBuffer(_handle);
    }
}
