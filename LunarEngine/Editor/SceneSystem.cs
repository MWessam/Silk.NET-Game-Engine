using System.Drawing;
using System.Numerics;
using Arch.Bus;
using Arch.Core;
using Hexa.NET.ImGui;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.Scenes;
using LunarEngine.UI;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace LunarEngine.ECS.Systems;

public class SceneSystem
{
    private IUiElement _uiElement;
    private FrameBuffer _sceneFrameBuffer;
    public Vector2D<int> NewViewport;
    public void Awake()
    {
        _sceneFrameBuffer = new FrameBuffer(Renderer.Instance.Api, new Vector2D<int>(800, 600));
        _uiElement = new DockableUiMenu()
        {
            Label = "Scene"
        };
    }

    public void PreRenderScene(in double t)
    {

    }

    public void Draw(Renderer renderer, ECSScene scene, in double t)
    {
        double dt = t;
        _uiElement.Draw(() =>
        {
            var contentRegionAvail = ImGui.GetContentRegionAvail();
            NewViewport = new Vector2D<int>((int)contentRegionAvail.X, (int)contentRegionAvail.Y);
            _sceneFrameBuffer.Bind();
            _sceneFrameBuffer.Resize(NewViewport);
            scene.SetSceneCameraViewport(NewViewport);
            renderer.BeginFrame();
            renderer.Clear();
            scene.RenderScenes(dt);
            renderer.Render(dt);
            ImGui.Image(_sceneFrameBuffer._colorTexture, new Vector2(_sceneFrameBuffer._size.X, _sceneFrameBuffer._size.Y), Vector2.UnitY, Vector2.UnitX);
            _sceneFrameBuffer.Unbind();
            renderer.EndFrame();
        });
    }
}

public struct OnViewportUpdated
{
    public Vector2D<int> Viewport;
}
public class GameSystem
{
    
}