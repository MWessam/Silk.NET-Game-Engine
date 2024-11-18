using System.Numerics;
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

public static class EditorUIEngine
{
    public static void DrawInputDragFloat3UIElement(ref Vector3 value, string label, float dragSpeed = 0.1f)
    {
        Vector3 currentValue = value;
        if (ImGui.DragFloat3($"{label}##drag", ref currentValue, dragSpeed))
        {
            value = currentValue;
        }
        if (ImGui.InputFloat3($"{label}##input", ref currentValue))
        {
            value = currentValue;
        }
    }
    public static void DrawInputDragFloatUIElement(ref float value, string label, float dragSpeed = 0.1f)
    {
        float currentValue = value;
        if (ImGui.DragFloat($"{label}##drag", ref currentValue, dragSpeed))
        {
            value = currentValue;
        }
        if (ImGui.InputFloat($"{label}##input", ref currentValue))
        {
            value = currentValue;
        }
    }
    public static void DrawInputFloatUIElement(ref float value, string label)
    {
        float currentValue = value;
        if (ImGui.InputFloat($"{label}##input", ref currentValue))
        {
            value = currentValue;
        }
    }
}