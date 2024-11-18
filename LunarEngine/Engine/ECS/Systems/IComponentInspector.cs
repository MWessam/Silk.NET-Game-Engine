using System.Numerics;
using ImGuiNET;
using LunarEngine.Components;
using LunarEngine.ECS.Components;
using LunarEngine.GameObjects;
using LunarEngine.Physics;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

public interface IComponentInspector
{
}
public interface IComponentInspector<T> : IComponentInspector where T : struct
{
    void OnDrawInspector(ref T component);
}

public class PositionInspector : IComponentInspector<Position>
{
    public void OnDrawInspector(ref Position component)
    {
        EditorUIEngine.DrawInputDragFloat3UIElement(ref component.Value);
    }
}
public class NameInspector : IComponentInspector<Name>
{
    public void OnDrawInspector(ref Name component)
    {
        var currentValue = component.Value;
        if (ImGui.InputText("Name", ref currentValue, 100))
        {
            component.Value = currentValue;
        }
    }
}

public class CameraInspector : IComponentInspector<Camera>
{
    public void OnDrawInspector(ref Camera component)
    {
        ImGui.Text("Near");
        EditorUIEngine.DrawInputDragFloatUIElement(ref component.Near);
        
        ImGui.Text("Far");
        EditorUIEngine.DrawInputDragFloatUIElement(ref component.Far);
        
        ImGui.Text("Width");
        EditorUIEngine.DrawInputDragFloatUIElement(ref component.Width);
        
        ImGui.Text("Height");
        EditorUIEngine.DrawInputDragFloatUIElement(ref component.Height);
    }
}

public class RigidBody2DInspector : IComponentInspector<RigidBody2D>
{
    public void OnDrawInspector(ref RigidBody2D component)
    {
        
    }
}