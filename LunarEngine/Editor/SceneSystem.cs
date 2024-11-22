using System.Drawing;
using System.Numerics;
using Arch.Core;
using Hexa.NET.ImGui;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.UI;
using Silk.NET.Maths;

namespace LunarEngine.ECS.Systems;

public class SceneSystem
{
    private IUiElement _uiElement;
    private FrameBuffer _sceneFrameBuffer;
    public Vector2D<int> NewViewport;
    public void Awake()
    {
        _sceneFrameBuffer = new FrameBuffer(GraphicsEngine.Api,GraphicsEngine.WindowContext.Size);
        _uiElement = new DockableUiMenu()
        {
            Label = "Scene"
        };
        GraphicsEngine.SetRenderTarget(_sceneFrameBuffer);
    }

    public void PreRenderScene(in double t)
    {

    }

    public void PostRender(in double t)
    {
        _uiElement.Draw(() =>
        {
            var contentRegionAvail = ImGui.GetContentRegionAvail();
            NewViewport = new Vector2D<int>((int)contentRegionAvail.X, (int)contentRegionAvail.Y);
            ImGui.Image(_sceneFrameBuffer._colorTexture, new Vector2(_sceneFrameBuffer._size.X, _sceneFrameBuffer._size.Y), Vector2.UnitY, Vector2.UnitX);
        });
    }
}