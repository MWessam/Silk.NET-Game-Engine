using System.Numerics;
using System.Reflection;
using Arch.Buffer;
using Arch.Bus;
using Arch.Core;
using ImGuiNET;
using LunarEngine.Components;
using LunarEngine.ECS.Components;
using LunarEngine.Engine.ECS.Components;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using LunarEngine.UI;
using Serilog;

namespace LunarEngine.ECS.Systems;


public partial class InspectorSystem : ScriptableSystem
{
    private Entity _entity;
    private bool _isComponentDropdownOpen = false;
    private int _selectedComponent = -1;
    private Dictionary<Type, IComponentInspector> _componentInspectors = new();
    private List<Type> _componentTypes = new();
    private UiMenu _inspectorMenu;
    private MethodInfo _genericCommandBufferAddMethod;
    private MethodInfo _genericCommandBufferRemoveMethod;
    private List<Type> _defaultComponents = new();

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
        _defaultComponents =
        [
            typeof(Transform),
            typeof(SpriteRenderer),
            typeof(Name),
            typeof(TagComponent),
            typeof(Camera),
        ];
        DiscoverAndAddComponentInspectors();
        Hook();
    }

    public override void Awake()
    {
        _inspectorMenu = new UiMenu()
        {
            Alignment = EAlignment.AlignTop,
            Justification = EJustification.JustifyRight,
            StretchY = true,
            Label = "Inspector",
            MenuWidth = 240,
        };
    }
    public override void Update(in double t)
    {
        CommandBuffer = new();
        UpdateInspector();
        CommandBuffer.Playback(World);
    }
    [Event(order: 0)]
    public void OnInspectorTargetSelected(InspectorTarget entity)
    {
        _entity = entity.Entity;
        _inspectorMenu.UiState = EUiState.Open;
    }
    public void UpdateInspector()
    {
        _inspectorMenu.DrawMenu(() =>
        {
            DrawComponentInspectors();
            DrawAddComponent();
        });
    }

    private void DrawAddComponent()
    {
        if (ImGui.Button("+ Add Component"))
        {
            _isComponentDropdownOpen = !_isComponentDropdownOpen;
        }

        if (_isComponentDropdownOpen)
        {
            if (ImGui.BeginCombo("Select Component Type: ",
                    _selectedComponent == -1 ? "None" : _componentTypes[_selectedComponent].Name))
            {
                // Loop through component types and create an item for each one
                for (int i = 0; i < _componentTypes.Count; i++)
                {
                    bool isSelected = (_selectedComponent == i);
                    if (ImGui.Selectable(_componentTypes[i].Name, isSelected))
                    {
                        _selectedComponent = i;  // Update the selected component index
                        var selectedComponentType = _componentTypes[_selectedComponent];
                        if (World.GetAllComponents(_entity).Any(x => x!.GetType() == selectedComponentType))
                        {
                            return;
                        }
                        _genericCommandBufferAddMethod.MakeGenericMethod(selectedComponentType).Invoke(CommandBuffer,
                            [_entity, Activator.CreateInstance(selectedComponentType)]);
                        
                        _isComponentDropdownOpen = false;
                    }
                }

                ImGui.EndCombo();  // End the combo box
            }
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
                if (_defaultComponents.All(x => x.Name != componentType.Name))
                {
                    ImGui.SameLine();
                    if (ImGui.Button($"Remove##{componentType.Name}"))
                    {
                        _genericCommandBufferRemoveMethod.MakeGenericMethod(componentType).Invoke(CommandBuffer, [_entity]);
                    }
                }
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

    /// <summary>
    /// Looks through every assembly and finds every class that implement IComponentInspector.
    /// Creates an instance of them and adds them to componentInspectors map.
    /// </summary>
    private void DiscoverAndAddComponentInspectors()
    {
        // Find all types in the current AppDomain that implement IComponentInspector<>
        var inspectorTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IComponentInspector<>)))
            .ToList();
        
        // Find all Component types in the current appdomain that implement IComponent.
        var componentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Where(type => typeof(IComponent).IsAssignableFrom(type))
            .ToList();
        _componentTypes = componentTypes;

        foreach (var inspectorType in inspectorTypes)
        {
            // Find IComponentInspector generic interface.
            var interfaceType = inspectorType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IComponentInspector<>));
            
            // Store the generic inspector component type.
            var componentType = interfaceType.GetGenericArguments()[0];

            // Create an instance of the inspector and add it
            if (Activator.CreateInstance(inspectorType) is IComponentInspector inspectorInstance)
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

        // Cache add and remove commands for components.
        _genericCommandBufferRemoveMethod = typeof(CommandBuffer).GetMethods().First(x => x.Name == "Remove");
        _genericCommandBufferAddMethod = typeof(CommandBuffer).GetMethods().First(x => x.Name == "Add");
    }
}