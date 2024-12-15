using Silk.NET.OpenGL;

namespace LunarEngine.Engine.Graphics;

public struct VertexArrayObject<TVertexType, TIndexType> : IDisposable, IBindable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    private uint _handle;
    private GL _gl;
    private uint _vbBufferIndex = 0;
    
    public VertexArrayObject(GL gl)
    {
        //Saving the GL instance.
        _gl = gl;
        _handle = _gl.GenVertexArray();
    }

    public unsafe void AddVertexBuffer(ref BufferObject<TVertexType> bufferObject)
    {
        var elements = bufferObject.Layout.Elements;
        uint offset = 0;
        for (int i = 0; i < elements.Count; ++i)
        {
            var element = elements[i];
            switch (element.Type)
            {
                case BufferObject<TVertexType>.BufferLayout.ElementType.Vec2:
                case BufferObject<TVertexType>.BufferLayout.ElementType.Vec3:
                case BufferObject<TVertexType>.BufferLayout.ElementType.Vec4:
                case BufferObject<TVertexType>.BufferLayout.ElementType.Float: 
                    _gl.EnableVertexAttribArray(_vbBufferIndex);
                    _gl.VertexAttribPointer(_vbBufferIndex, (int)element.Count, VertexAttribPointerType.Float, element.Normalized, bufferObject.Layout.Stride, (void*) element.Offset);
                    if (element.PerInstance)
                    {
                        _gl.VertexAttribDivisor(_vbBufferIndex, 1);
                    }
                    ++_vbBufferIndex;
                    break;
                case BufferObject<TVertexType>.BufferLayout.ElementType.Mat3:
                case BufferObject<TVertexType>.BufferLayout.ElementType.Mat4:
                    int rowCount = (int)MathF.Round(MathF.Sqrt(element.GetSizeOfType() / 4));
                    for (uint j = 0; j < rowCount; ++j)
                    {
                        _gl.EnableVertexAttribArray(_vbBufferIndex);
                        _gl.VertexAttribPointer(_vbBufferIndex, rowCount, VertexAttribPointerType.Float, element.Normalized, bufferObject.Layout.Stride, (void*) (element.Offset + 4 * rowCount * j));
                        _gl.VertexAttribDivisor(_vbBufferIndex, 1);
                        ++_vbBufferIndex;
                    }
                    break;
            }
        }
    }

    public void Bind()
    {
        _gl.BindVertexArray(_handle);
    }

    public void Unbind()
    {
        _gl.BindVertexArray(0);
    }

    public void Dispose()
    {
        Unbind();
        _gl.DeleteVertexArray(_handle);
    }
}