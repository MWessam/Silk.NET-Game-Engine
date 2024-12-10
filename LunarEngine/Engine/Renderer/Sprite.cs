using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngine.GameObjects;
using Silk.NET.OpenGL;

namespace LunarEngine.Engine.Graphics;

public class Sprite : IDisposable
{
    public LGTexture Texture { get; private set; }
    public ShaderHandle Shader { get; private set; }
    public int PPU = 1000;
    private BufferObject<float> _instanceBuffer;
    private VertexArrayObject<float, uint> _vao;
    private GL _gl;
    private int random = 0;
    public Sprite(LGTexture texture, ShaderHandle shader, GL gl)
    {
        Texture = texture;
        Shader = shader;
        _gl = gl;
        random = new Random().Next(1, 1000);
    }
    public void ChangeShader(ShaderHandle shaderHandle)
    {
        Shader = shaderHandle;
    }
    public void ChangeTexture(LGTexture textureHandle)
    {
        Texture = textureHandle;
    }
    public unsafe void Render(SpriteData spriteData)
    {
        _vao.Bind();
        _instanceBuffer.SetBufferData(spriteData);
        Texture.Bind();
        Shader.Bind();
        _gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*) 0);
    }
    
    public void Initialize(Quad quad)
    {
        _vao = new VertexArrayObject<float, uint>(_gl);
        quad.BindToVAO(ref _vao);
        _vao.Bind();
        _instanceBuffer = new BufferObject<float>(_gl, BufferTargetARB.ArrayBuffer);
        _instanceBuffer.Bind();
        _instanceBuffer.Layout.Push(1, BufferObject<float>.BufferLayout.ElementType.Mat4, true);    
        _instanceBuffer.Layout.Push(4, BufferObject<float>.BufferLayout.ElementType.Float, true);
        _vao.AddVertexBuffer(ref _instanceBuffer);
    }
    public static Builder GetSpriteBuilder()
    {
        return new Builder();
    }
    public class Builder
    {
        private ShaderHandle _shader;
        private LGTexture _texture;
        internal Builder()
        {
        }
        public Builder WithShader(ShaderHandle shaderHandle)
        {
            _shader = shaderHandle;
            return this;
        }
        public Builder WithTexture(LGTexture texture)
        {
            _texture = texture;
            return this;
        }
        public Sprite Build()
        {
            var gl = Renderer.Instance.Api;
            ArgumentNullException.ThrowIfNull(gl, nameof(gl));
            ArgumentNullException.ThrowIfNull(_shader, nameof(_shader));
            ArgumentNullException.ThrowIfNull(_texture, nameof(_texture));
            var sprite = new Sprite(_texture, _shader, gl);
            return sprite;
        }
    }

    public void Dispose()
    {
        Texture.Dispose();
        Shader.Dispose();
        _instanceBuffer.Dispose();
        _vao.Dispose();
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct SpriteData
{
    public Matrix4x4 TransformMatrix;
    public Vector4 Color;
}