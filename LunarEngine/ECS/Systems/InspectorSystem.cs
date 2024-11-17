using System.Numerics;
using Arch.Bus;
using Arch.Core;
using Arch.Core.Utils;
using ImGuiNET;
using LunarEngine.ECS.Components;
using LunarEngine.GameObjects;
using Serilog;

namespace LunarEngine.ECS.Systems;

public interface IComponentInspector
{
}
public interface IComponentInspector<T> : IComponentInspector where T : struct
{
    void OnDrawInspector(T component);
}
public partial class InspectorSystem : ScriptableSystem
{
    private Entity _entity;
    private bool _isOpen = false;
    private Dictionary<Type, IComponentInspector> _componentInspectors = new();

    public void AddComponentInspector<T>(IComponentInspector componentInspector) where T : struct
    {
        if (_componentInspectors.TryAdd(typeof(T), componentInspector))
        {
            Log.Error($"Component inspector of type {nameof(T)} is already added.");
            return;
        }
    }
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
        
        // Get a copy of all components of entity
        var components = World.GetAllComponents(_entity);
        
        // Store all draw actions such that I can prioritize name component.
        LinkedList<Action> inspectorDrawCommandQueue = new();
        
        foreach (var component in components)
        {
            if (component is null)
            {
                Log.Error($"Null component found in entity{_entity.Id}");
                continue;
            }

            // Get component type and its corresponding inspector.
            var componentType = component.GetType();
            if (!_componentInspectors.TryGetValue(componentType, out var componentInspector))
            {
                Log.Error($"Couldn't find a valid component inspector for component {componentType.Name} found in entity {_entity.Id}");
                continue;
            }
            
            // Check if component inspector is the valid generic one.
            var inspectorType = componentInspector.GetType();
            var expectedInspectorType = typeof(IComponentInspector<>).MakeGenericType(componentType);

            if (!expectedInspectorType.IsAssignableFrom(inspectorType))
            {
                Log.Error($"Component inspector type mismatch: {inspectorType.Name} does not match expected {expectedInspectorType.Name} for component {componentType.Name}");
                continue;
            }
            
            // Get draw inspector method.
            var methodName = "OnDrawInspector";
            var drawMethod = expectedInspectorType.GetMethod(methodName);
            if (drawMethod is null)
            {
                Log.Error($"For some unholy reason the method {methodName} is not found in type {expectedInspectorType.Name}...");
                return;
            }
            
            // Store draw action as an action
            var drawAction = () =>
            {
                ImGui.Text(componentType.Name);
                ImGui.Separator();
                drawMethod.Invoke(componentInspector, [component]);
                World.Set(_entity, component);
                ImGui.Separator();
            };
            // Prioritize name component above all else.
            if (component is Name)
            {
                inspectorDrawCommandQueue.AddFirst(drawAction);
            }
            else
            {
                // Add rest of components to the end.
                inspectorDrawCommandQueue.AddLast(drawAction);
            }
        }
        ImGui.End();
    }
}