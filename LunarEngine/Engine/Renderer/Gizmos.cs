using System.Numerics;
using LunarEngine.Assets;
using Silk.NET.OpenGL;

namespace LunarEngine.Engine.Graphics;

public class Gizmos : IDisposable
{
    private IBuffer _wireframeGizmoVbo;
    private IBuffer _wireframeGizmoInstanceVbo;
    private IVertexArray _wireframeVao;
    private IShader _gizmosShader;
    private IRenderDevice _device;
    private readonly AssetManager _assetManager;

    public Gizmos(IRenderDevice device, AssetManager assetManager)
    {
        _device = device;
        _assetManager = assetManager;
        InitializeGizmos();
    }

    public void InitializeGizmos()
    {
        _wireframeGizmoVbo = _device.CreateBuffer<float>(Span<float>.Empty, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw);
        var vboLayout = new BufferLayout();
        vboLayout.Push(2, ElementType.Float);
        _wireframeGizmoInstanceVbo = _device.CreateBuffer<float>(Span<float>.Empty, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw);
        var instanceLayout = new BufferLayout();
        instanceLayout.Push(4, ElementType.Float, true);
        _wireframeVao = _device.CreateVertexArray();
        _wireframeVao.Bind();
        _wireframeGizmoVbo.Bind();
        _wireframeVao.AddVertexBuffer(_wireframeGizmoVbo, vboLayout);
        _wireframeGizmoInstanceVbo.Bind();
        _wireframeVao.AddVertexBuffer(_wireframeGizmoInstanceVbo, instanceLayout);
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
        _wireframeGizmoVbo.SetData(lineDrawCommand.Vertices.AsSpan());
        _wireframeGizmoInstanceVbo.Bind();
        _wireframeGizmoInstanceVbo.SetData(lineDrawCommand.LineInstanceData);
        _gizmosShader.Bind();
        _gizmosShader.SetUniform("vp", viewProjectionMatrix);
        _gizmosShader.UpdateDirtyUniforms();
        _device.DrawArrays(PrimitiveType.LineStrip, 0, (uint)lineDrawCommand.Points.Length);
    }

    public void DrawQuad(QuadDrawCommand quadDrawCommand, Matrix4x4 viewProjectionMatrix)
    {
        _gizmosShader = _assetManager.GetShaderHandle("wireframe_gizmo");
        _wireframeVao.Bind();
        _wireframeGizmoVbo.Bind();
        
        _wireframeGizmoVbo.SetData(quadDrawCommand.Vertices.AsSpan());
        _wireframeGizmoInstanceVbo.Bind();
        _wireframeGizmoInstanceVbo.SetData(quadDrawCommand.QuadInstanceData);
        _gizmosShader.Bind();
        _gizmosShader.SetUniform("vp", viewProjectionMatrix);
        _gizmosShader.UpdateDirtyUniforms();
        _device.DrawArrays(PrimitiveType.LineLoop, 0, 4);
    }
}
