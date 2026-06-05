using Silk.NET.OpenGL;

namespace LunarEngine.Engine.Graphics;

public struct Quad
{
    public IBuffer QuadVbo;
    public IBuffer QuadEbo;
    private uint[] _indices;
    private float[] _vertices;
    private BufferLayout _layout;
    private Quad(IRenderDevice device)
    {
        QuadEbo = device.CreateBuffer<uint>(Span<uint>.Empty, BufferTargetARB.ElementArrayBuffer, BufferUsageARB.StaticDraw);
        QuadVbo = device.CreateBuffer<float>(Span<float>.Empty, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw);
        _indices =
        [
            0, 1, 3,
            1, 2, 3
        ];
        _vertices =
        [
            //X   Y   S     T
            1f,  1f,  1.0f, 0.0f,
            1f, -1f,  1.0f, 1.0f,
            -1f, -1f, 0.0f, 1.0f,
            -1f,  1f, 0.0f, 0.0f
        ];
        _layout = new BufferLayout();
        _layout.Push(2, ElementType.Float);
        _layout.Push(2, ElementType.Float);
    }

    public void BindToVAO(IVertexArray vao)
    {
        vao.Bind();
        QuadVbo.Bind();
        QuadEbo.Bind();
        vao.AddVertexBuffer(QuadVbo, _layout);
        vao.SetIndexBuffer(QuadEbo);
        vao.Unbind();
    }
    public static Quad CreateQuad(IRenderDevice device)
    {
        var definition = new Quad(device);
        definition.QuadVbo.Bind();
        definition.QuadEbo.Bind();
        definition.QuadEbo.SetData(definition._indices.AsSpan());
        definition.QuadVbo.SetData(definition._vertices.AsSpan());
        return definition;
    }
}
