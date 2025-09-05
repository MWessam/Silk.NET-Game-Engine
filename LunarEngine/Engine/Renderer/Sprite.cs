using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngine.Assets;
using LunarEngine.GameObjects;
using Silk.NET.OpenGL;

namespace LunarEngine.Engine.Graphics;

public class Sprite : IDisposable
{
    public TextureHandle Texture { get; private set; }
    public ShaderHandle Shader { get; private set; }
    public int PPU = 1000;
    private BufferObject<float> _instanceBuffer;
    private VertexArrayObject<float, uint> _vao;
    private GL _glApi;
    public Sprite(TextureHandle texture, ShaderHandle shader, GL glApi)
    {
        Texture = texture;
        Shader = shader;
        _glApi = glApi;
    }
    public void ChangeShader(ShaderHandle shaderHandle)
    {
        Shader = shaderHandle;
    }
    public void ChangeTexture(TextureHandle textureHandle)
    {
        Texture = textureHandle;
    }
    public void Bind(SpriteData spriteData)
    {
        _vao.Bind();
        _instanceBuffer.SetBufferData(spriteData);
        Texture.Bind();
        Shader.Bind();
    }
    
    public void Initialize(Quad quad)
    {
        _vao = new VertexArrayObject<float, uint>(_glApi);
        quad.BindToVAO(ref _vao);
        _vao.Bind();
        _instanceBuffer = new BufferObject<float>(_glApi, BufferTargetARB.ArrayBuffer);
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
        private TextureHandle _texture;
        internal Builder()
        {
        }
        public Builder WithShader(ShaderHandle shaderHandle)
        {
            _shader = shaderHandle;
            return this;
        }


        public Builder WithTexture(TextureHandle texture)
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