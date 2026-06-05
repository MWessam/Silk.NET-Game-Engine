using System.Numerics;
using System.Reflection;
using Arch.Buffer;
using Hexa.NET.ImGui;
using LunarEngine.ECS;
using LunarEngine.ECS.Components;
using LunarEngine.Events;
using LunarEngine.Application;
using LunarEngine.UI;
using Serilog;
using ImGuiDir = Hexa.NET.ImGui.ImGuiDir;

namespace LunarEngine.Editor.Systems;

public class InspectorSystem
{
    private EntityReference _entity;
    private bool _isComponentDropdownOpen = false;
    private int _selectedComponent = -1;
    private Dictionary<Type, IComponentInspector> _componentInspectors = new();
    private List<Type> _componentTypes = new();
    private IUiElement _inspectorMenu;
    private MethodInfo _genericCommandBufferAddMethod;
    private MethodInfo _genericCommandBufferRemoveMethod;
    private List<Type> _defaultComponents = new();
    private readonly IWorld _world;
    private CommandBuffer _commandBuffer;

    public void AddComponentInspector<T>(IComponentInspector<T> componentInspector) where T : struct, IComponent
    {
        AddComponentInspector(typeof(T), componentInspector);
    }

    public void AddComponentInspector(Type componentType, IComponentInspector componentInspector)
    {
        if (!_componentInspectors.TryAdd(componentType, componentInspector))
        {
            Log.Error($"Component inspector of type {componentType.Name} is already added.");
            return;
        }
    }

    public InspectorSystem(IWorld world, EventBus<InspectorTargetSelectedEvent> eventBus)
    {
        _world = world;
        _commandBuffer = world.CreateCommandBuffer();
        eventBus.Subscribe(OnInspectorTargetSelected);

        _defaultComponents =
        [
            typeof(Transform),
            typeof(Name),
        ];
        DiscoverAllComponents();
        _genericCommandBufferRemoveMethod = typeof(CommandBuffer).GetMethods().First(x => x.Name == "Remove");
        _genericCommandBufferAddMethod = typeof(CommandBuffer).GetMethods().First(x => x.Name == "Add");
    }

    public void Awake()
    {
        _inspectorMenu = new DockableUiMenu()
        {
            Label = "Inspector",
            ImGuiDir = ImGuiDir.Right,
        };
    }

    public void Update(in double t)
    {
        UpdateInspector();
        _world.Playback(_commandBuffer);
    }

    private void OnInspectorTargetSelected(InspectorTargetSelectedEvent evt)
    {
        _entity = evt.Entity;
    }

    public void UpdateInspector()
    {
        if (!_entity.IsValid) return;
        _inspectorMenu.Draw(InnerUiElementDrawCall);
    }

    private void InnerUiElementDrawCall()
    {
        DrawComponentInspectors();
        DrawAddComponent();
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
                for (int i = 0; i < _componentTypes.Count; i++)
                {
                    bool isSelected = (_selectedComponent == i);
                    if (ImGui.Selectable(_componentTypes[i].Name, isSelected))
                    {
                        _selectedComponent = i;
                        var selectedComponentType = _componentTypes[_selectedComponent];
                        if (_world.GetAllComponents(_entity).Any(x => x!.GetType() == selectedComponentType))
                        {
                            break;
                        }
                        _genericCommandBufferAddMethod.MakeGenericMethod(selectedComponentType).Invoke(_commandBuffer,
                            [_entity.NativeEntity, Activator.CreateInstance(selectedComponentType)!]);

                        _isComponentDropdownOpen = false;
                    }
                }

                ImGui.EndCombo();
            }
        }
    }

    private void DrawComponentInspectors()
    {
        var components = _world.GetAllComponents(_entity);

        List<Action> inspectorDrawCommandQueue = new();

        for (var i = 0; i < components.Length; i++)
        {
            object component = components[i];
            if (component is null)
            {
                Log.Error($"Null component found in entity{_entity.Id}");
                continue;
            }

            var componentType = component.GetType();
            Action drawAction = null;
            if (_componentInspectors.TryGetValue(componentType, out var componentInspector))
            {
                var inspectorType = componentInspector.GetType();
                var expectedInspectorType = typeof(IComponentInspector<>).MakeGenericType(componentType);

                if (!expectedInspectorType.IsAssignableFrom(inspectorType))
                {
                    Log.Error(
                        $"Component inspector type mismatch: {inspectorType.Name} does not match expected {expectedInspectorType.Name} for component {componentType.Name}");
                    continue;
                }

                var methodName = "OnDrawInspector";
                var drawMethod = expectedInspectorType.GetMethod(methodName);
                if (drawMethod is null)
                {
                    Log.Error(
                        $"For some unholy reason the method {methodName} is not found in type {expectedInspectorType.Name}...");
                    return;
                }

                drawAction = () =>
                {
                    DrawComponentName(componentType);
                    object?[] parameters = [component];
                    drawMethod.Invoke(componentInspector, parameters);
                    component = parameters[0];
                    _world.Set(_entity, component);
                    ImGui.Separator();
                };
            }
            else
            {
                drawAction = () => DrawComponentName(componentType);
            }
            if (component is Name)
            {
                inspectorDrawCommandQueue.Insert(0, drawAction);
            }
            else
            {
                inspectorDrawCommandQueue.Add(drawAction);
            }
        }

        foreach (var drawCommand in inspectorDrawCommandQueue)
        {
            drawCommand?.Invoke();
        }

        return;
    }

    private void DrawComponentName(Type componentType)
    {
        ImGui.Text(componentType.Name);
        if (_defaultComponents.All(x => x.Name != componentType.Name))
        {
            ImGui.SameLine();
            if (ImGui.Button($"Remove##{componentType.Name}"))
            {
                _genericCommandBufferRemoveMethod.MakeGenericMethod(componentType).Invoke(_commandBuffer, [_entity.NativeEntity]);
            }
        }
        ImGui.Separator();
    }

    private void DiscoverAllComponents()
    {
        var componentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Where(type => typeof(IComponent).IsAssignableFrom(type))
            .ToList();
        _componentTypes = componentTypes;
    }
}
