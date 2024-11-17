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
    private bool _isOpen = true;        // To track if the hierarchy is collapsed
    private float _hierarchyWidth = 320f;     // Initial width of the hierarchy panel
    public HierarchySystem(World world) : base(world)
    {
    }

    public override void Update(in double d)
    {
        var screenHeight = ImGui.GetIO().DisplaySize.Y;
        if (_isOpen)
        {
            // Set the position and size of the hierarchy window (panel)
            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always);  // Anchor to the top-left corner
            ImGui.SetNextWindowSize(new Vector2(_hierarchyWidth, screenHeight), ImGuiCond.Always); // Fixed height, variable width
            ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize );
            if (ImGui.Button("<##Collapse"))
            {
                _isOpen = false;
            }
            if (ImGui.BeginListBox(""))
            {
                UpdateHierarchyQuery(World);  // Render the hierarchy content
                UpdateHierarchyNoNameQuery(World);
                ImGui.EndListBox();
            }
            ImGui.End();
        }
        else
        {
            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always);  // Anchor to the top-left corner
            ImGui.SetNextWindowSize(new Vector2(20, screenHeight), ImGuiCond.Always); // Fixed height, variable width
            ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize );
            if (ImGui.Button(">##Hierarchy"))
            {
                _isOpen = true;
            }
            ImGui.End();
        }
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