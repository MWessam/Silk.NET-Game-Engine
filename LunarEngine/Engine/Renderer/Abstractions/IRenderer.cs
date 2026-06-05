using System.Numerics;
using Silk.NET.Maths;

namespace LunarEngine.Renderer;

public interface IRenderer : IDisposable
{
    IRenderDevice Device { get; }
    Quad Quad { get; }
    void Initialize();
    void BeginFrame(Matrix4x4 cameraViewProjection);
    void SubmitRenderCommand(RenderCommand renderCommand);
    void EndFrame();
    IFrameBuffer CreateFrameBuffer(Vector2D<int> size);
    void SetRenderTarget(IFrameBuffer? target);
    void Clear();
    Sprite CreateSprite(ITexture2D texture, IShader shader);
}
