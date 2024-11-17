using Silk.NET.OpenGL;

namespace LunarEngine.OpenGLAPI;

internal interface IBindable
{
    void Bind();
    void Unbind();
}

public struct BufferObject<TDataType> : IDisposable, IBindable
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
    public class BufferLayout
    {
        public uint Stride { get; private set; } = 0;
        public List<BufferElement> Elements { get; private set; } = new();
        public BufferLayout()
        {
        }

        public void Push(uint count, ElementType type, bool perInstance = false)
        {
            Elements.Add(new()
            {
                Count = count,
                Type = type,
                Normalized = false,
                PerInstance = perInstance,
            });
            CalculateOffsetAndStride();
        }

        private void CalculateOffsetAndStride()
        {
            Stride = 0;
            uint offset = 0;
            for (var i = 0; i < Elements.Count; i++)
            {
                var element = Elements[i]; 
                element.Offset = offset;
                Stride += element.GetSizeOfType() * element.Count;
                offset += element.GetSizeOfType() * element.Count;
                Elements[i] = element;
            }
        }

        public struct BufferElement
        {
            public uint Count;
            public ElementType Type;
            public bool Normalized;
            public uint Offset;
            public bool PerInstance;

            public uint GetSizeOfType()
            {
                switch (Type)
                {
                    case ElementType.Float: return 4;
                    case ElementType.Vec2: return 4 * 2;
                    case ElementType.Vec3: return 4 * 3;
                    case ElementType.Vec4: return 4 * 4;
                    case ElementType.Mat3: return 4 * 3 * 3;
                    case ElementType.Mat4: return 4 * 4 * 4;
                }
                return 0;
            }
        }
        public enum ElementType
        {
            Mat4,
            Mat3,
            Vec2,
            Vec3,
            Vec4,
            Float,
        }
    }
    
}