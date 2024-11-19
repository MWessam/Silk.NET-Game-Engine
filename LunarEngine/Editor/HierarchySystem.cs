using System.Numerics;
using Arch.Bus;
using Arch.Core;
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
        _hierarchyMenu = new UiMenu()
        {
            MenuWidth = 240,
            Alignment = EAlignment.AlignTop,
            Justification = EJustification.JustifyLeft,
            Label = "Hierarchy",
            StretchY = true,
        };
    }
    public override void Update(in double d)
    {
        _hierarchyMenu.Draw(() =>
        {
            if (ImGui.BeginListBox("##HierarchyList"))
            {
                UpdateHierarchyQuery(World);  // Render the hierarchy content
                UpdateHierarchyNoNameQuery(World);
                ImGui.EndListBox();
            }
        });

    }

    private string[] options = ["Option A", "Option B"];
    private int _option;
    
    [Query]
    [All<Name>]
    private void UpdateHierarchy(Entity entity, ref Name name)
    {
        if (ImGui.Selectable(name.Value))
        {
            EventBus.Send(new InspectorTarget()
            {
                Entity = entity
            });
        }
        if (ImGui.BeginPopupContextItem($"ContextMenu_{name.Value}"))
        {
            
            // // Add a combo box for options
            if (ImGui.Combo("Actions", ref _option, options, options.Length))
            {
                Log.Debug($"Selected {options[_option]}");
            }
            ImGui.EndPopup();
        }
    }
    [Query]
    private void UpdateHierarchyNoName(Entity entity)
    {
        if (ImGui.Selectable(entity.Id.ToString()))
        {
            EventBus.Send(new InspectorTarget()
            {
                Entity = entity
            });
        }
    }
}

public struct InspectorTarget
{
    public Entity Entity;
}