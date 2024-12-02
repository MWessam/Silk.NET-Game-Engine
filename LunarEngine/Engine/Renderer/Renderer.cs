using System.Drawing;
using System.Runtime.CompilerServices;
using LunarEngine.ECS.Systems;
using LunarEngine.GameEngine;
using LunarEngine.UI;
using Serilog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.Engine.Graphics;
public class Renderer : Singleton<Renderer>, ISingletonObject, IDisposable
{
    public GL Api { get; internal set; }
    
    private List<RenderCommand> _renderQueue = new();
    
    public void Render(double deltaTime = 0)
    {
        foreach (var renderCommand in _renderQueue)
        {
            // Resolve command type. Better performance than reflection
            switch (renderCommand.Type)
            {
                case RenderCommand.CommandType.SpriteDraw:
                    var spriteDrawCommand = (SpriteDrawCommand)renderCommand;
                    spriteDrawCommand.Sprite.Render(spriteDrawCommand.SpriteData);
                    break;
            }
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Api.ClearColor(Color.Black);
        Api.Clear((uint)(GLEnum.DepthBufferBit | GLEnum.ColorBufferBit));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SubmitRenderCommand(SpriteDrawCommand spriteDrawCommand)
    {
        _renderQueue.Add(spriteDrawCommand);
    }
    
    #region INTERNAL
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginFrame()
    {

    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndFrame()
    {
        Clean();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Clean()
    {
        _renderQueue.Clear();
    }
    #endregion

    public void SetRenderTarget(FrameBuffer sceneFrameBuffer)
    {
        sceneFrameBuffer.Bind();
    }

    public void InitSingleton()
    {
        
    }

    public void Dispose()
    {
        
    }
}