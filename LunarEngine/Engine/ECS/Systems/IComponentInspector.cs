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
        EditorUIEngine.DrawInputDragFloat3UIElement(ref component.Value, "##position");
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
        EditorUIEngine.DrawInputFloatUIElement(ref component.Near, "Near");
        
        ImGui.Text("Far");
        EditorUIEngine.DrawInputFloatUIElement(ref component.Far, "Far");
        
        ImGui.Text("Width");
        EditorUIEngine.DrawInputFloatUIElement(ref component.Width, "Width");
        
        ImGui.Text("Height");
        EditorUIEngine.DrawInputFloatUIElement(ref component.Height, "Height");
    }
}

public class RigidBody2DInspector : IComponentInspector<RigidBody2D>
{
    public void OnDrawInspector(ref RigidBody2D component)
    {
        
    }
}

public class SpriteRendererInspector : IComponentInspector<SpriteRenderer>
{
    public void OnDrawInspector(ref SpriteRenderer component)
    {
        EditorUIEngine.DrawInputDragFloat4UIElement(ref component.Color, "Color");
    }
}