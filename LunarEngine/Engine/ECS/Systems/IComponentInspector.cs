using System.Numerics;
using ImGuiNET;
using LunarEngine.Components;
using LunarEngine.ECS.Components;

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
        Vector3 currentValue = component.Value;
        if (ImGui.DragFloat3("##drag", ref currentValue, 0.1f))
        {
            component.Value = currentValue;
        }
        if (ImGui.InputFloat3("##input", ref currentValue))
        {
            component.Value = currentValue;
        }
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