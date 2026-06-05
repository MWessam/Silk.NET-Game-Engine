using System.Drawing;
using System.Numerics;
using Hexa.NET.ImGui;
using LunarEngine.Engine.Graphics;
using LunarEngine.Events;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.Graphics;
using LunarEngine.Scenes;
using LunarEngine.UI;
using Silk.NET.Maths;

namespace LunarEngine.ECS.Systems;

public class SceneSystem
{
    private IUiElement _uiElement;
    private IFrameBuffer _sceneFrameBuffer;
    public Vector2D<int> NewViewport;
    private bool _isFocused = false;
    private IRenderer _renderer;
    private readonly EventBus<SceneFocusEvent> _eventBus;

    public SceneSystem(IRenderer renderer, EventBus<SceneFocusEvent> eventBus)
    {
        _renderer = renderer;
        _eventBus = eventBus;
    }
    public void Awake()
    {
        _sceneFrameBuffer = _renderer.CreateFrameBuffer(new Vector2D<int>(800, 600));
        _uiElement = new DockableUiMenu()
        {
            Label = "Scene"
        };
    }

    public void PreRenderScene(in double t)
    {

    }

    public void Draw(ECSScene scene, EditorCamera camera, in double t)
    {
        double dt = t;
        _uiElement.Draw(() =>
        {
            var contentRegionAvail = ImGui.GetContentRegionAvail();
            if (ImGui.IsWindowFocused() && !_isFocused)
            {
                _isFocused = true;
                _eventBus.Publish(new SceneFocusEvent(_isFocused));

            }
            else if (!ImGui.IsWindowFocused() && _isFocused)
            {
                _isFocused = false;
                _eventBus.Publish(new SceneFocusEvent(_isFocused));
            }
            NewViewport = new Vector2D<int>((int)contentRegionAvail.X, (int)contentRegionAvail.Y);
            _sceneFrameBuffer.Bind();
            _sceneFrameBuffer.Resize(NewViewport);
            scene.SetSceneCameraViewport(NewViewport);
            camera.UpdateViewportCamera(NewViewport);
            scene.RenderScenes(dt, camera);
            ImGui.Image(_sceneFrameBuffer.ColorAttachment.NativeHandle, new Vector2(_sceneFrameBuffer.Size.X, _sceneFrameBuffer.Size.Y), Vector2.UnitY, Vector2.UnitX);
            _sceneFrameBuffer.Unbind();
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
