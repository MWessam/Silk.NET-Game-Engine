using System.Numerics;
using LunarEngine.Assets;
using Silk.NET.OpenGL;

namespace LunarEngine.Engine.Graphics;

public class Gizmos : IDisposable
{
    private BufferObject<float> _wireframeGizmoVbo;
    private BufferObject<float> _wireframeGizmoInstanceVbo;
    private VertexArrayObject<float, uint> _wireframeVao;
    private ShaderHandle _gizmosShader;
    private GL _api;
    private readonly AssetManager _assetManager;

    public Gizmos(GL api, AssetManager assetManager)
    {
        _api = api;
        _assetManager = assetManager;
        InitializeGizmos(api);
    }

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
    }

    public void Dispose()
    {
    }

    public void DrawLine(LineDrawCommand lineDrawCommand, Matrix4x4 viewProjectionMatrix)
    {
        _gizmosShader = _assetManager.GetShaderHandle("wireframe_gizmo");
        _wireframeVao.Bind();
        _wireframeGizmoVbo.Bind();
        _wireframeGizmoVbo.SetBufferData(lineDrawCommand.Vertices);
        _wireframeGizmoInstanceVbo.Bind();
        _wireframeGizmoInstanceVbo.SetBufferData(lineDrawCommand.LineInstanceData);
        _gizmosShader.Bind();
        _gizmosShader.SetUniform("vp", viewProjectionMatrix);
        _gizmosShader.UpdateDirtyUniforms();
        _api.DrawArrays(PrimitiveType.LineStrip, 0, (uint)lineDrawCommand.Points.Length);
    }

    public void DrawQuad(QuadDrawCommand quadDrawCommand, Matrix4x4 viewProjectionMatrix)
    {
        _gizmosShader = _assetManager.GetShaderHandle("wireframe_gizmo");
        _wireframeVao.Bind();
        _wireframeGizmoVbo.Bind();
        
        _wireframeGizmoVbo.SetBufferData(quadDrawCommand.Vertices);
        _wireframeGizmoInstanceVbo.Bind();
        _wireframeGizmoInstanceVbo.SetBufferData(quadDrawCommand.QuadInstanceData);
        _gizmosShader.Bind();
        _gizmosShader.SetUniform("vp", viewProjectionMatrix);
        _gizmosShader.UpdateDirtyUniforms();
        _api.DrawArrays(PrimitiveType.LineLoop, 0, 4);
    }
}
