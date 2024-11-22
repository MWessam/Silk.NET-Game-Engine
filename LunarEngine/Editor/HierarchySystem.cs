using System.Numerics;
using Arch.Bus;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Hexa.NET.ImGui;
using LunarEngine.ECS.Components;
using LunarEngine.GameObjects;
using LunarEngine.UI;
using Serilog;

namespace LunarEngine.ECS.Systems;

public partial class HierarchySystem : ScriptableSystem
{
    private IUiElement _hierarchyMenu;
    public HierarchySystem(World world) : base(world)
    {
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
            if (ImGui.BeginListBox("##HierarchyList"))
            {
                UpdateHierarchyQuery(World);  // Render the hierarchy content
                UpdateHierarchyNoNameQuery(World);
                ImGui.EndListBox();
            }
        });
        CommandBuffer.Playback(World);
    }

    private string[] _options = ["Delete", "Option 2"];
    private int _option = -1;
    
    [Query]
    [All<Name>]
    private void UpdateHierarchy(Entity entity, ref Name name)
    {
        if (ImGui.Selectable(name.Value))
        {
            EventBus.Send(new InspectorTarget()
            {
                Entity = World.Reference(entity)
            });
        }
        if (ImGui.BeginPopupContextItem($"ContextMenu_{name.Value}"))
        {
            
            // // Add a combo box for options
            if (ImGui.Combo("Actions", ref _option, _options, _options.Length))
            {
                switch (_options[_option])
                {
                    case "Delete":
                        CommandBuffer.Destroy(entity);
                        break;
                }
            }
            ImGui.EndPopup();
        }
    }
    [Query]
    [None<Name>]
    private void UpdateHierarchyNoName(Entity entity)
    {
        if (ImGui.Selectable(entity.Id.ToString()))
        {
            EventBus.Send(new InspectorTarget()
            {
                Entity = World.Reference(entity)
            });
        }
    }
}

public struct InspectorTarget
{
    public EntityReference Entity;
}