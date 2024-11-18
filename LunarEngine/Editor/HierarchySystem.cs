using System.Numerics;
using Arch.Bus;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using ImGuiNET;
using LunarEngine.ECS.Components;
using LunarEngine.GameObjects;
using LunarEngine.UI;

namespace LunarEngine.ECS.Systems;

public partial class HierarchySystem : ScriptableSystem
{
    private UiMenu _hierarchyMenu;
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
        _hierarchyMenu.DrawMenu(() =>
        {
            if (ImGui.BeginListBox(""))
            {
                UpdateHierarchyQuery(World);  // Render the hierarchy content
                UpdateHierarchyNoNameQuery(World);
                ImGui.EndListBox();
            }
        });

    }
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