using ImGuiNET;
using LunarEngine.ECS.Components;
using LunarEngine.UI;

namespace LunarEngine.Editor.Systems;

public class CameraInspector : IComponentInspector<CameraComponent>
{
    public void OnDrawInspector(ref CameraComponent component)
    {
        EditorUIEngine.DrawInputFloatUIElement(ref component.Camera.Near, "Near");
        
        ImGui.Text("Far");
        EditorUIEngine.DrawInputFloatUIElement(ref component.Camera.Far, "Far");
        
        ImGui.Text("Width");
        EditorUIEngine.DrawInputFloatUIElement(ref component.Camera.Width, "Width");
        
        ImGui.Text("Height");
        EditorUIEngine.DrawInputFloatUIElement(ref component.Camera.Height, "Height");
    }
}