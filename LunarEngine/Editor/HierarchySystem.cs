using Arch.Bus;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Hexa.NET.ImGui;
using LunarEngine.Components;
using LunarEngine.ECS.Components;
using LunarEngine.GameObjects;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

public partial class HierarchySystem : ScriptableSystem
{
    private IUiElement _hierarchyMenu;
    private string[] _options;
    private string[] _hierarchyOptions;
    private int _option = -1;
    private int _hierarchyOption = -1;

    public HierarchySystem(World world) : base(world)
    {
        _options =
        [
            "Delete"
        ];
        _hierarchyOptions =
        [
            "Create Entity"
        ];
    }

    public override void Awake()
    {
        _hierarchyMenu = new DockableUiMenu()
        {
            // MenuWidth = 240,
            // Alignment = EAlignment.AlignTop,
            // Justification = EJustification.JustifyLeft,
            Label = "Hierarchy",
            ImGuiDir = ImGuiDir.Left,
            // StretchY = true,
        };
    }
    public override void Update(in double d)
    {
        CommandBuffer = new();
        _hierarchyMenu.Draw(() =>
        {
            if (ImGui.BeginPopupContextItem($"ContextMenu_Hierarchy"))
            {
                // // Add a combo box for options
                if (ImGui.Combo("Actions##Hierarchy", ref _hierarchyOption, _hierarchyOptions, _hierarchyOptions.Length))
                {
                    switch (_hierarchyOption)
                    {
                        case 0:
                            
                            var entity = CommandBuffer.Create([typeof(Name), typeof(Transform)]);
                            CommandBuffer.Set(entity, new Name()
                            {
                                Value = "Entity"
                            });
                            break;
                    }
                    _hierarchyOption = -1;
                }
                ImGui.EndPopup();
            }
            if (ImGui.BeginListBox("##HierarchyList"))
            {
                UpdateHierarchyQuery(World);  // Render the hierarchy content
                ImGui.EndListBox();
            }
        });
        CommandBuffer.Playback(World);
    }

    [Query]
    [All<Name>]
    private void UpdateHierarchy(Entity entity, ref Name name)
    {
        if (ImGui.Selectable($"{name.Value}##{entity.Id}"))
        {
            EventBus.Send(new InspectorTarget()
            {
                Entity = World.Reference(entity)
            });
        }
        if (ImGui.BeginPopupContextItem($"ContextMenu_{name.Value}"))
        {
            // // Add a combo box for options
            if (ImGui.Combo("Actions##Entity", ref _option, _options, _options.Length))
            {
                switch (_options[_option])
                {
                    case "Delete":
                        CommandBuffer.Destroy(entity);
                        break;
                }
                _option = -1;
            }
            ImGui.EndPopup();
        }
    }
}

public struct InspectorTarget
{
    public EntityReference Entity;
}