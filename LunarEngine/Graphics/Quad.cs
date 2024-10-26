using LunarEngine.OpenGLAPI;
using Silk.NET.OpenGL;

namespace LunarEngine.Graphics;

public class Quad
{
    public BufferObject<float> QuadVbo;
    public BufferObject<uint> QuadEbo;
    public VertexArrayObject<float, uint> Vao;
    private uint[] _indices;
    private float[] _vertices;
    private Quad(GL _gl)
    {
        QuadEbo = new BufferObject<uint>(_gl, BufferTargetARB.ElementArrayBuffer);
        QuadVbo = new BufferObject<float>(_gl, BufferTargetARB.ArrayBuffer);
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
    }

    public void BindToVAO(VertexArrayObject<float, uint> vao)
    {
        vao.Bind();
        QuadVbo.Bind();
        QuadEbo.Bind();
        vao.AddVertexBuffer(QuadVbo);
        vao.Unbind();
    }
    public static Quad CreateQuad(GL gl)
    {
        var definition = new Quad(gl);
        definition.QuadVbo.Bind();
        definition.QuadEbo.Bind();
        definition.QuadEbo.SetBufferData(definition._indices.AsSpan());
        definition.QuadVbo.SetBufferData(definition._vertices.AsSpan());
        definition.QuadVbo.Layout.Push(2, BufferObject<float>.BufferLayout.ElementType.Float);
        definition.QuadVbo.Layout.Push(2, BufferObject<float>.BufferLayout.ElementType.Float);
        return definition;
    }
}