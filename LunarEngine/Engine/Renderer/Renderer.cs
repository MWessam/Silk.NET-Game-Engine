using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using LunarEngine.Assets;
using LunarEngine.ECS.Systems;
using LunarEngine.GameEngine;
using LunarEngine.UI;
using LunarEngine.Utilities;
using Serilog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.Engine.Graphics;

public class Gizmos : Singleton<Gizmos>, ISingletonObject, IDisposable
{
    private BufferObject<float> _wireframeGizmoVbo;
    private BufferObject<float> _wireframeGizmoInstanceVbo;
    private VertexArrayObject<float, uint> _wireframeVao;
    private ShaderHandle _gizmosShader;
    private GL _api;

    public void InitializeGizmos(GL api)
    {
        _wireframeGizmoVbo = new BufferObject<float>(api, BufferTargetARB.ArrayBuffer);
        _wireframeGizmoVbo.Layout.Push(2, BufferObject<float>.BufferLayout.ElementType.Float);
        _wireframeGizmoInstanceVbo = new BufferObject<float>(api, BufferTargetARB.ArrayBuffer);
        _wireframeGizmoInstanceVbo.Layout.Push(4, BufferObject<float>.BufferLayout.ElementType.Float, true);
        _wireframeVao = new VertexArrayObject<float, uint>(api);
        _wireframeVao.Bind();
        _wireframeGizmoVbo.Bind();
        _wireframeVao.AddVertexBuffer(ref _wireframeGizmoVbo);
        _wireframeGizmoInstanceVbo.Bind();
        _wireframeVao.AddVertexBuffer(ref _wireframeGizmoInstanceVbo);
        _wireframeVao.Unbind();

        _api = api;
    }
    public void InitSingleton()
    {
    }
    public void Dispose()
    {
    }

    public void DrawLine(LineDrawCommand lineDrawCommand)
    {
        _gizmosShader = AssetManager.Instance.ShaderLibrary.GetAsset("wireframe_gizmo").Shader;
        _wireframeVao.Bind();
        _wireframeGizmoVbo.Bind();
        _wireframeGizmoVbo.SetBufferData([lineDrawCommand.StartPosition.X, lineDrawCommand.StartPosition.Y, lineDrawCommand.EndPosition.X, lineDrawCommand.EndPosition.Y]);
        _wireframeGizmoInstanceVbo.Bind();
        _wireframeGizmoInstanceVbo.SetBufferData(lineDrawCommand.LineInstanceData);
        _gizmosShader.Bind();
        _gizmosShader.SetUniform("vp", CameraSystem.SceneCamera.ViewProjection);
        _gizmosShader.UpdateDirtyUniforms();
        _api.DrawArrays(PrimitiveType.Lines, 0, 2);
    }

    public void DrawQuad(QuadDrawCommand quadDrawCommand)
    {
        _gizmosShader = AssetManager.Instance.ShaderLibrary.GetAsset("wireframe_gizmo").Shader;
        _wireframeVao.Bind();
        _wireframeGizmoVbo.Bind();
        
        _wireframeGizmoVbo.SetBufferData(quadDrawCommand.Vertices);
        _wireframeGizmoInstanceVbo.Bind();
        _wireframeGizmoInstanceVbo.SetBufferData(quadDrawCommand.QuadInstanceData);
        _gizmosShader.Bind();
        _gizmosShader.SetUniform("vp", CameraSystem.SceneCamera.ViewProjection);
        _gizmosShader.UpdateDirtyUniforms();
        _api.DrawArrays(PrimitiveType.LineLoop, 0, 4);
        
    }
}
public class Renderer : Singleton<Renderer>, ISingletonObject, IDisposable
{
    public GL Api { get; private set; }
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
                case RenderCommand.CommandType.Line:
                    Gizmos.Instance.DrawLine((LineDrawCommand) renderCommand);
                    break;
                case RenderCommand.CommandType.Quad:
                    Gizmos.Instance.DrawQuad((QuadDrawCommand) renderCommand);
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
    public void SubmitRenderCommand(RenderCommand renderCommand)
    {
        _renderQueue.Add(renderCommand);
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

    public void InitializeRenderer(GL api)
    {
        Api = api;
        Gizmos.Instance.InitializeGizmos(api);
    }

    public void Dispose()
    {
        
    }
}