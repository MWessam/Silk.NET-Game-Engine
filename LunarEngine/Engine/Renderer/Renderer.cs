using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using LunarEngine.Events;
using LunarEngine.Application;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace LunarEngine.Renderer;

public class Renderer : IDisposable, IRenderer
{
    private List<RenderCommand> _renderQueue = new();
    
    private Matrix4x4 _viewProjectionMatrix;
    private readonly Gizmos _gizmos;
    private readonly IRenderDevice _device;
    private readonly Quad _quad;
    
    public IRenderDevice Device => _device;
    public Matrix4x4 ViewProjectionMatrix => _viewProjectionMatrix;
    public Quad Quad => _quad;

    #region INITIALIZATION

    public Renderer(IRenderDevice device, Gizmos gizmos)
    {
        _device = device;
        _gizmos = gizmos;
        _quad = Quad.CreateQuad(device);
    }

    public void Initialize()
    {
        _device.SetClearColor(0, 0, 0, 1);
        _device.EnableBlend();
        _device.BlendFunc(BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha);
        _device.LineWidth(4.0f);
    }

    #endregion

    #region INTERFACE

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _device.SetClearColor(0, 0, 0, 1);
        _device.Clear((uint)(GLEnum.DepthBufferBit | GLEnum.ColorBufferBit));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SubmitRenderCommand(RenderCommand renderCommand)
    {
        _renderQueue.Add(renderCommand);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginFrame(Matrix4x4 cameraViewProjection)
    {
        _viewProjectionMatrix = cameraViewProjection;
        Clean();
        Clear();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndFrame()
    {
        Render();
    }
    public IFrameBuffer CreateFrameBuffer(Vector2D<int> size)
    {
        return _device.CreateFrameBuffer(size);
    }

    public void SetRenderTarget(IFrameBuffer? target)
    {
        if (target != null)
        {
            target.Bind();
        }
        else
        {
            _device.BindDefaultFramebuffer();
        }
    }

    public Sprite CreateSprite(ITexture2D texture, IShader shader)
    {
        var sprite = new Sprite(texture, shader, _device);
        sprite.Initialize(_quad);
        return sprite;
    }

    #endregion
    
    #region INTERNAL

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Clean()
    {
        _renderQueue.Clear();
    }
    
    private void Render()
    {
        foreach (var renderCommand in _renderQueue)
        {
            // Resolve command type. Better performance than reflection
            switch (renderCommand.Type)
            {
                case RenderCommand.CommandType.SpriteDraw:
                    var spriteDrawCommand = (SpriteDrawCommand)renderCommand;
                    RenderSprite(spriteDrawCommand);
                    break;
                case RenderCommand.CommandType.Line:
                    _gizmos.DrawLine((LineDrawCommand) renderCommand, _viewProjectionMatrix);
                    break;
                case RenderCommand.CommandType.Quad:
                    _gizmos.DrawQuad((QuadDrawCommand) renderCommand, _viewProjectionMatrix);
                    break;
            }
        }
    }

    private unsafe void RenderSprite(SpriteDrawCommand spriteDrawCommand)
    {
        spriteDrawCommand.Sprite.Bind(spriteDrawCommand.SpriteData);
        _device.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
    }

    #endregion
    public void Dispose()
    {
    }
}
