using ImGuiNET;
using LunarEngine.Physics;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

public class RigidBody2DInspector : IComponentInspector<RigidBody2D>
{
    public void OnDrawInspector(ref RigidBody2D component)
    {
        ImGui.Text("YAHH BITCH! SCIENCE!");
        EditorUIEngine.DrawInputFloatUIElement(ref component.Mass, "Mass");
        EditorUIEngine.DrawInputFloatUIElement(ref component.GravityScale, "GravityScale");
    }
}