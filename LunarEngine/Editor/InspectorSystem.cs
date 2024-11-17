using System.Numerics;
using Arch.Bus;
using Arch.Core;
using Arch.Core.Utils;
using ImGuiNET;
using LunarEngine.Components;
using LunarEngine.ECS.Components;
using LunarEngine.GameObjects;
using Serilog;

namespace LunarEngine.ECS.Systems;


public partial class InspectorSystem : ScriptableSystem
{
    private Entity _entity;
    private bool _isOpen = false;
    private Dictionary<Type, IComponentInspector> _componentInspectors = new();
    private float _hierarchyWidth = 320f;     // Initial width of the hierarchy panel

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
        DiscoverAndAddComponentInspectors();
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
        var screenHeight = ImGui.GetIO().DisplaySize.Y;
        var screenWidth = ImGui.GetIO().DisplaySize.X;
        
        if (_isOpen)
        {
            // Calculate the position to anchor to the right
            var positionX = screenWidth - _hierarchyWidth;
            // Set the position and size of the hierarchy window (panel)
            ImGui.SetNextWindowPos(new Vector2(positionX, 0), ImGuiCond.Always);  // Anchor to the top-left corner
            ImGui.SetNextWindowSize(new Vector2(_hierarchyWidth, screenHeight), ImGuiCond.Always); // Fixed height, variable width
            ImGui.Begin("Inspector", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize );
            if (ImGui.Button("<##Collapse"))
            {
                _isOpen = false;
            }
            DrawComponentInspectors();
            ImGui.End();
        }
        else
        {
            // Calculate the position to anchor to the right
            var positionX = screenWidth - 48;
            ImGui.SetNextWindowPos(new Vector2(positionX, 0), ImGuiCond.Always);  // Anchor to the top-left corner
            ImGui.SetNextWindowSize(new Vector2(48, screenHeight), ImGuiCond.Always); // Fixed height, variable width
            ImGui.Begin("", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize );
            if (ImGui.Button(">##Inspector"))
            {
                _isOpen = true;
            }
            ImGui.End();
        }
    }

    private void DrawComponentInspectors()
    {
        // Get a copy of all components of entity
        var components = World.GetAllComponents(_entity);
        
        // Store all draw actions such that I can prioritize name component.
        LinkedList<Action> inspectorDrawCommandQueue = new();

        for (var i = 0; i < components.Length; i++)
        {
            object component = components[i];
            if (component is null)
            {
                Log.Error($"Null component found in entity{_entity.Id}");
                continue;
            }

            // Get component type and its corresponding inspector.
            var componentType = component.GetType();
            if (!_componentInspectors.TryGetValue(componentType, out var componentInspector))
            {
                // Log.Error(
                // $"Couldn't find a valid component inspector for component {componentType.Name} found in entity {_entity.Id}");
                continue;
            }

            // Check if component inspector is the valid generic one.
            var inspectorType = componentInspector.GetType();
            var expectedInspectorType = typeof(IComponentInspector<>).MakeGenericType(componentType);

            if (!expectedInspectorType.IsAssignableFrom(inspectorType))
            {
                Log.Error(
                    $"Component inspector type mismatch: {inspectorType.Name} does not match expected {expectedInspectorType.Name} for component {componentType.Name}");
                continue;
            }

            // Get draw inspector method.
            var methodName = "OnDrawInspector";
            var drawMethod = expectedInspectorType.GetMethod(methodName);
            if (drawMethod is null)
            {
                Log.Error(
                    $"For some unholy reason the method {methodName} is not found in type {expectedInspectorType.Name}...");
                return;
            }

            // Store draw action as an action
            var drawAction = () =>
            {
                ImGui.Text(componentType.Name);
                ImGui.Separator();
                object?[] parameters = [component];
                drawMethod.Invoke(componentInspector, parameters);
                component = parameters[0];
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

        foreach (var drawCommand in inspectorDrawCommandQueue)
        {
            drawCommand?.Invoke();
        }

        return;
    }

    private void DiscoverAndAddComponentInspectors()
    {
        // Find all types in the current AppDomain that implement IComponentInspector<>
        var inspectorTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IComponentInspector<>)))
            .ToList();

        foreach (var inspectorType in inspectorTypes)
        {
            // Get the component type handled by the inspector
            var interfaceType = inspectorType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IComponentInspector<>));
            var componentType = interfaceType.GetGenericArguments()[0];

            // Create an instance of the inspector and add it
            var inspectorInstance = Activator.CreateInstance(inspectorType) as IComponentInspector;

            if (inspectorInstance != null)
            {
                if (_componentInspectors.TryAdd(componentType, inspectorInstance))
                {
                    Log.Debug($"Added inspector for component type {componentType.Name}.");
                }
                else
                {
                    Log.Error($"Inspector for component type {componentType.Name} is already registered.");
                }
            }
            else
            {
                Log.Error($"Failed to create an instance of {inspectorType.Name}.");
            }
        }
    }
}