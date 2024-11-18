using ImGuiNET;
using LunarEngine.GameObjects;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

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