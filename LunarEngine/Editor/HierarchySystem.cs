using Arch.Buffer;
using Hexa.NET.ImGui;
using LunarEngine.ECS;
using LunarEngine.ECS.Components;
using LunarEngine.Events;
using LunarEngine.UI;

namespace LunarEngine.Editor.Systems;

public class HierarchySystem
{
    private IUiElement _hierarchyMenu;
    private string[] _options;
    private string[] _hierarchyOptions;
    private int _option = -1;
    private int _hierarchyOption = -1;
    private Action _uiElementDrawCall;
    private readonly IWorld _world;
    private readonly EventBus<InspectorTargetSelectedEvent> _eventBus;
    private CommandBuffer _commandBuffer;

    public HierarchySystem(IWorld world, EventBus<InspectorTargetSelectedEvent> eventBus)
    {
        _world = world;
        _eventBus = eventBus;
        _commandBuffer = world.CreateCommandBuffer();
        _options =
        [
            "Delete"
        ];
        _hierarchyOptions =
        [
            "Create Entity"
        ];
        _uiElementDrawCall = InnerUiElementDrawCall;
    }

    public void Awake()
    {
        _hierarchyMenu = new DockableUiMenu()
        {
            Label = "Hierarchy",
            ImGuiDir = ImGuiDir.Left,
        };
    }

    public void Update(in double d)
    {
        _hierarchyMenu.Draw(_uiElementDrawCall);
        _world.Playback(_commandBuffer);
    }

    private void InnerUiElementDrawCall()
    {
        if (ImGui.BeginPopupContextItem($"ContextMenu_Hierarchy"))
        {
            if (ImGui.Combo("Actions##Hierarchy", ref _hierarchyOption, _hierarchyOptions, _hierarchyOptions.Length))
            {
                switch (_hierarchyOption)
                {
                    case 0:
                    {
                        var entity = _commandBuffer.Create([typeof(Name), typeof(Transform), typeof(IsInstantiating)]);
                        _commandBuffer.Set(in entity, new Name { Value = "Entity" });
                        break;
                    }
                }

                _hierarchyOption = -1;
            }

            ImGui.EndPopup();
        }

        if (ImGui.BeginListBox("##HierarchyList"))
        {
            _world.Query<Name>((EntityReference entity, ref Name name) =>
            {
                if (ImGui.Selectable($"{name.Value}##{entity.Id}"))
                {
                    _eventBus.Publish(new InspectorTargetSelectedEvent(entity, _world));
                }
                if (ImGui.BeginPopupContextItem($"ContextMenu_{name.Value}"))
                {
                    if (ImGui.Combo("Actions##Entity", ref _option, _options, _options.Length))
                    {
                        switch (_options[_option])
                        {
                            case "Delete":
                                _commandBuffer.Destroy(entity.NativeEntity);
                                break;
                        }
                        _option = -1;
                    }
                    ImGui.EndPopup();
                }
            });
            ImGui.EndListBox();
        }
    }
}
