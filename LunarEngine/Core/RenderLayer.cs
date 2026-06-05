using LunarEngine.Engine.Graphics;
using LunarEngine.Scenes;

namespace LunarEngine.GameEngine;

public class RenderLayer : BaseLayer
{
    private readonly SceneManager _sceneManager;
    private FrameBuffer? _sceneFrameBuffer;

    public RenderLayer(SceneManager sceneManager, FrameBuffer? sceneFrameBuffer = null) : base("Render")
    {
        _sceneManager = sceneManager;
        _sceneFrameBuffer = sceneFrameBuffer;
    }

    public override void OnUpdate(TimeStep timeStep)
    {
        var scene = _sceneManager.ActiveScenes;
        if (_sceneFrameBuffer != null)
        {
            var sceneFrameBufferValue = _sceneFrameBuffer.Value;
            sceneFrameBufferValue.Bind();
        }
        scene.RenderScenes(timeStep);
    }
}
