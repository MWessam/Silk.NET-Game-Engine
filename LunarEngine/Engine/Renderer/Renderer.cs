using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using LunarEngine.Events;
using LunarEngine.GameEngine;
using Silk.NET.OpenGL;

namespace LunarEngine.Engine.Graphics;

public class Renderer : IDisposable
{
    private List<RenderCommand> _renderQueue = new();
    
    private Matrix4x4 _viewProjectionMatrix;
    
    public GL Api { get; private set; }
    public Matrix4x4 ViewProjectionMatrix => _viewProjectionMatrix;



    #region INITIALIZATION

    private Renderer()
    {
        EventBus<WindowInitializedEvent>.Register(OnApiLoaded);
    }

    private void OnApiLoaded(WindowInitializedEvent windowInitializedEvent)
    {
        var gl = windowInitializedEvent.Api;
        
        gl.ClearColor(Color.Black);
        gl.Enable(GLEnum.Blend);
        gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        gl.LineWidth(4.0f);
        
        Gizmos.Instance.InitializeGizmos(gl);

        Api = gl;
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
                    Gizmos.Instance.DrawLine((LineDrawCommand) renderCommand, _viewProjectionMatrix);
                    break;
                case RenderCommand.CommandType.Quad:
                    Gizmos.Instance.DrawQuad((QuadDrawCommand) renderCommand, _viewProjectionMatrix);
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