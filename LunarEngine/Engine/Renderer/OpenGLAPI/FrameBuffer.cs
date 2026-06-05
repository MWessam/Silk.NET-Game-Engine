using System.Diagnostics;
using System.Drawing;
using LunarEngine.GameEngine;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace LunarEngine.Engine.Graphics;
public unsafe struct FrameBuffer : IDisposable, IFrameBuffer
{
    private GL _api;

    private Vector2D<int> _size;
    private uint _handle;
    private uint _colorTexture;

    public uint ColorTexture => _colorTexture;
    public Vector2D<int> Size => _size;
    private uint _depthTexture;
    private bool _defaultRenderTarget;
    private ColorTextureAttachment _colorAttachment;

    public ITexture2D ColorAttachment => _colorAttachment;

    public static FrameBuffer CreateDefaultRenderFrameBuffer(GL api)
    {
        var frameBuffer = new FrameBuffer();
        frameBuffer._api = api;
        frameBuffer._handle = 0;
        frameBuffer._api.BindFramebuffer(GLEnum.Framebuffer, 0);
        frameBuffer._defaultRenderTarget = true;
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
        Debug.Assert(status == GLEnum.FramebufferComplete, $"Framebuffer is not complete! Status: {status}");
        if (status != GLEnum.FramebufferComplete) throw new Exception("Framebuffer is not complete!");
        _api.BindFramebuffer(GLEnum.Framebuffer, 0);

        _colorAttachment = new ColorTextureAttachment(_api, _colorTexture, (uint)_size.X, (uint)_size.Y);
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
        // _api.Viewport();
    }

    /// <summary>
    /// MAKE SURE FBO IS BOUND FIRST!
    /// </summary>
    /// <param name="newSize"></param>
    public void Resize(Vector2D<int> newSize)
    {
        _size = newSize;
        _api.BindTexture(GLEnum.Texture2D, _colorTexture);
        _api.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)_size.X, (uint)_size.Y, 0, GLEnum.Rgba, GLEnum.UnsignedByte, null);
        _api.BindTexture(GLEnum.Texture2D, _depthTexture);
        _api.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.DepthComponent, (uint)_size.X, (uint)_size.Y, 0, GLEnum.DepthComponent, GLEnum.Float, null);
        _colorAttachment = new ColorTextureAttachment(_api, _colorTexture, (uint)_size.X, (uint)_size.Y);
    }

    public void Dispose()
    {
        _api.DeleteFramebuffer(_handle);
        _api.DeleteTexture(_colorTexture);
        _api.DeleteTexture(_depthTexture);
    }

    private class ColorTextureAttachment : ITexture2D
    {
        private GL _api;
        private uint _handle;
        private uint _width;
        private uint _height;

        public ColorTextureAttachment(GL api, uint handle, uint width, uint height)
        {
            _api = api;
            _handle = handle;
            _width = width;
            _height = height;
        }

        public uint Width => _width;
        public uint Height => _height;
        public uint NativeHandle => _handle;

        public void Bind(int unit = 0)
        {
            _api.ActiveTexture((TextureUnit)unit);
            _api.BindTexture(GLEnum.Texture2D, _handle);
        }

        public void Dispose() { }
    }
}
