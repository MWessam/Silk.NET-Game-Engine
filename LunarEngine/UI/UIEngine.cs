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
    private static ImGuiController _imGuiController;
    public static void Initialize(IWindow window, GL gl, IInputContext inputContext)
    {
        _imGuiController = new ImGuiController(gl, window, inputContext);

    }

    public static void Update(float dt)
    {
        _imGuiController.Update(dt);
    }

    public static void Render()
    {
        _imGuiController.Render();
    }
}