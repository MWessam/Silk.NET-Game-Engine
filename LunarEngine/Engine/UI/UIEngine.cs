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
    /// <summary>
    /// Creates drag and keyboard input vector3 field.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="label">MANDATORY! LABEL MUST BE UNIQUE. To hide label, u can prefix it with ##</param>
    /// <param name="dragSpeed"></param>
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
    /// <summary>
    /// Creates drag and keyboard input vector4 field.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="label">MANDATORY! LABEL MUST BE UNIQUE. To hide label, u can prefix it with ##</param>
    /// <param name="dragSpeed"></param>
    public static void DrawInputDragFloat4UIElement(ref Vector4 value, string label, float dragSpeed = 0.1f)
    {
        Vector4 currentValue = value;
        if (ImGui.DragFloat4($"{label}##drag", ref currentValue, dragSpeed))
        {
            value = currentValue;
        }
        if (ImGui.InputFloat4($"{label}##input", ref currentValue))
        {
            value = currentValue;
        }
    }
    /// <summary>
    /// Creates drag and keyboard input float field.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="label">MANDATORY! LABEL MUST BE UNIQUE. To hide label, u can prefix it with ##</param>
    /// <param name="dragSpeed"></param>
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
    /// <summary>
    /// Creates keyboard input vector3 field.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="label">MANDATORY! LABEL MUST BE UNIQUE. To hide label, u can prefix it with ##</param>
    public static void DrawInputFloatUIElement(ref float value, string label)
    {
        float currentValue = value;
        if (ImGui.InputFloat($"{label}##input", ref currentValue))
        {
            value = currentValue;
        }
    }
}