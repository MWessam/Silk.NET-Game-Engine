using ImGuiNET;
using LunarEngine.Graphics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace LunarEngine.UI;

public static class UIEngine
{
    public static ImGuiController ImGUIController { get; private set; }

    public static void Initialize(IWindow window, GL gl, IInputContext inputContext)
    {
        ImGUIController = new ImGuiController(gl, window, inputContext);

    }

    public static void Update(float dt)
    {
        ImGUIController.Update(dt);
    }

    public static void Render()
    {
        ImGUIController.Render();
    }
}