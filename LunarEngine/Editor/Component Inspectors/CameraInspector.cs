using ImGuiNET;
using LunarEngine.GameObjects;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

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