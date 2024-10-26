using System.Numerics;
using System.Runtime.InteropServices;
using LunarEngineCS.OpenGLAPI;
using Silk.NET.OpenGL;

namespace LunarEngineCS.RenderingEngine;

public class Sprite : IDisposable
{
    public TextureHandle Texture { get; private set; }
    public ShaderHandle Shader { get; private set; }
    private Quad _quad;
    private BufferObject<float> _instanceBuffer;
    private VertexArrayObject<float, uint> _vao;
    private GL _gl;
    public Sprite(Quad quad, TextureHandle texture, ShaderHandle shader, GL gl)
    {
        _quad = quad;
        Texture = texture;
        Shader = shader;
        _gl = gl;
    }
    public void ChangeShader(ShaderHandle shaderHandle)
    {
        Shader = shaderHandle;
    }
    public void ChangeTexture(TextureHandle textureHandle)
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
    
    private void Initialize()
    {
        _vao = new VertexArrayObject<float, uint>(_gl);
        
        _quad.BindToVAO(_vao);
        _vao.Bind();
        _instanceBuffer = new BufferObject<float>(_gl, BufferTargetARB.ArrayBuffer);
        _instanceBuffer.Bind();
        _instanceBuffer.Layout.Push(1, BufferObject<float>.BufferLayout.ElementType.Mat4, true);    
        _instanceBuffer.Layout.Push(4, BufferObject<float>.BufferLayout.ElementType.Float, true);
        _vao.AddVertexBuffer(_instanceBuffer);
    }
    public static Builder GetSpriteBuilder()
    {
        return new Builder();
    }
    public class Builder
    {
        private GL _gl;
        private ShaderHandle _shader;
        private TextureHandle _texture;
        internal Builder()
        {
        }
        public Builder WithApi(GL gl)
        {
            _gl = gl;
            return this;
        }

        public Builder WithShader(ShaderHandle shaderHandle)
        {
            _shader = shaderHandle;
            return this;
        }
        public Builder WithTexture(TextureHandle textureHandle)
        {
            _texture = textureHandle;
            return this;
        }
        public Sprite Build()
        {
            ArgumentNullException.ThrowIfNull(_gl, nameof(_gl));
            ArgumentNullException.ThrowIfNull(_shader, nameof(_shader));
            ArgumentNullException.ThrowIfNull(_texture, nameof(_texture));
            var spriteIndices = Quad.CreateQuad(_gl);
            var sprite = new Sprite(spriteIndices, _texture, _shader, _gl);
            sprite.Initialize();
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