using ImGuiNET;
using LunarEngine.ECS.Components;

namespace LunarEngine.ECS.Systems;

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