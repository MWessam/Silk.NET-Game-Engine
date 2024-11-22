using System.Drawing;
using System.Numerics;
using LunarEngine.Graphics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

public unsafe class FrameBuffer : IDisposable
{
    private GL _api;

    public Vector2D<int> _size;
    public uint _handle;
    public uint _colorTexture;
    public uint _depthTexture;
    public bool DefaultRenderTarget;

    public static FrameBuffer CreateDefaultRenderFrameBuffer(GL api)
    {
        var frameBuffer = new FrameBuffer();
        frameBuffer._api = api;
        frameBuffer._handle = 0;
        frameBuffer._api.BindFramebuffer(GLEnum.Framebuffer, 0);
        frameBuffer.DefaultRenderTarget = true;
        return frameBuffer;
    }
    public FrameBuffer(GL api, Vector2D<int> size)
    {
        _api = api;
        _size = size;
        _handle = _api.GenFramebuffer();
        _api.BindFramebuffer(GLEnum.Framebuffer, _handle);

        _colorTexture = _api.GenTexture();
        _api.BindTexture(GLEnum.Texture2D, _colorTexture);
        _api.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)_size.X, (uint)_size.Y, 0, GLEnum.Rgba, GLEnum.UnsignedByte, null);
        _api.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
        _api.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
        _api.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, _colorTexture, 0);

        _depthTexture = _api.GenTexture();
        _api.BindTexture(GLEnum.Texture2D, _depthTexture);
        _api.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.DepthComponent, (uint)_size.X, (uint)_size.Y, 0, GLEnum.DepthComponent, GLEnum.Float, null);
        _api.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
        _api.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
        _api.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.DepthAttachment, GLEnum.Texture2D, _depthTexture, 0);

        var status = _api.CheckFramebufferStatus(GLEnum.Framebuffer);
        if (status != GLEnum.FramebufferComplete) throw new Exception("Framebuffer is not complete!");
        _api.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    private FrameBuffer()
    {
        
    }
    public void Clear()
    {
        // _api.ClearColor(color);
        _api.Clear((uint)(GLEnum.ColorBufferBit | GLEnum.DepthBufferBit));
    }

    public void Bind()
    {
        _api.BindFramebuffer(GLEnum.Framebuffer, _handle);
        _api.Viewport(new Size((int)_size.X, (int)_size.Y));
    }

    public void Unbind()
    {
        _api.BindFramebuffer(GLEnum.Framebuffer, 0);
        _api.Viewport(GraphicsEngine.WindowContext.Size);
    }

    public void Resize(Vector2D<int> newSize)
    {
        _size = newSize;
        Bind();
        _api.BindTexture(GLEnum.Texture2D, _colorTexture);
        _api.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)_size.X, (uint)_size.Y, 0, GLEnum.Rgba, GLEnum.UnsignedByte, null);
        _api.BindTexture(GLEnum.Texture2D, _depthTexture);
        _api.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.DepthComponent, (uint)_size.X, (uint)_size.Y, 0, GLEnum.DepthComponent, GLEnum.Float, null);
        Unbind();
    }

    public void Dispose()
    {
        _api.DeleteFramebuffer(_handle);
        _api.DeleteTexture(_colorTexture);
        _api.DeleteTexture(_depthTexture);
    }
}