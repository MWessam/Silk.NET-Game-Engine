using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using LunarEngine.Events;
using LunarEngine.GameEngine;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace LunarEngine.Engine.Graphics;

public class Renderer : IDisposable
{
    private List<RenderCommand> _renderQueue = new();
    
    private Matrix4x4 _viewProjectionMatrix;
    private readonly Gizmos _gizmos;
    
    public GL Api { get; private set; }
    public Matrix4x4 ViewProjectionMatrix => _viewProjectionMatrix;

    #region INITIALIZATION

    public Renderer(GL api, Gizmos gizmos)
    {
        Api = api;
        _gizmos = gizmos;
    }

    public void Initialize()
    {
        Api.ClearColor(Color.Black);
        Api.Enable(GLEnum.Blend);
        Api.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        Api.LineWidth(4.0f);
    }

    #endregion

    #region INTERFACE

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Api.ClearColor(Color.Black);
        Api.Clear((uint)(GLEnum.DepthBufferBit | GLEnum.ColorBufferBit));
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
    public FrameBuffer CreateFrameBuffer(Vector2D<int> size)
    {
        return new FrameBuffer(Api, size);
    }

    public void SetRenderTarget(FrameBuffer sceneFrameBuffer)
    {
        sceneFrameBuffer.Bind();
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
        Api.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*) 0);
    }

    #endregion
    public void Dispose()
    {
    }
}
