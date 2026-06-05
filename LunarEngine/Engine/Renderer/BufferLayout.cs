namespace LunarEngine.Renderer;

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
