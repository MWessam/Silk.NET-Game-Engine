using System.Numerics;
using Arch.Bus;
using Arch.Core;
using Arch.Core.Utils;
using ImGuiNET;
using LunarEngine.GameObjects;
using Serilog;

namespace LunarEngine.ECS.Systems;

public partial class InspectorSystem : ScriptableSystem
{
    private Entity _entity;
    private bool _isOpen = false;
    public InspectorSystem(World world) : base(world)
    {
        Hook();
    }
    public override void Update(in double t)
    {
        UpdateInspector();
    }
    [Event(order: 0)]
    public void OnInspectorTargetSelected(InspectorTarget entity)
    {
        _entity = entity.Entity;
        _isOpen = true;
    }
    public void UpdateInspector()
    {
        if (!_isOpen) return;
        ImGui.Begin("Inspector");
        var components = World.GetAllComponents(_entity);
        foreach (var component in components)
        {
            if (component is null)
            {
                Log.Error($"Null component found in entity{_entity.Id}");
                continue;
            }
            ImGui.Text(component.GetType().Name);
            ImGui.Separator();

            // Get component fields from the real type for serialization.
            var type = component.GetType();
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                // Get field name and value.
                var fieldName = field.Name;
                var fieldValue = field.GetValue(component);
                
                if (field.FieldType == typeof(int))
                {
                    int value = (int)fieldValue;
                    if (ImGui.InputInt(fieldName, ref value))
                    {
                        field.SetValue(component, value);
                    }
                }
                else if (field.FieldType == typeof(float))
                {
                    float value = (float)fieldValue;
                    if (ImGui.InputFloat(fieldName, ref value))
                    {
                        field.SetValue(component, value);
                    }
                }
                else if (field.FieldType == typeof(string))
                {
                    string value = fieldValue as string ?? string.Empty;
                    if (ImGui.InputText(fieldName, ref value, 100))
                    {
                        field.SetValue(component, value);
                    }
                }
                else if (field.FieldType == typeof(Vector3))
                {
                    Vector3 value = fieldValue is Vector3 ? (Vector3)fieldValue : default;
                    if (ImGui.InputFloat3(fieldName, ref value))
                    {
                        field.SetValue(component, value);
                    }
                }
                else if (field.FieldType == typeof(bool))
                {
                    bool value = (bool)fieldValue;
                    if (ImGui.Checkbox(fieldName, ref value))
                    {
                        field.SetValue(component, value);
                    }
                }
                else
                {
                    ImGui.Text($"{fieldName}: (Unsupported type)");
                }
            }
            World.Set(_entity, component);
            ImGui.Separator();
            
        }
        ImGui.End();
    }
}