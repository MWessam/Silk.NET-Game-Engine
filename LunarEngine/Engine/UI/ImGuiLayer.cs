using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using LunarEngine.GameEngine;
using LunarEngine.Platform;
using Silk.NET.OpenGL;

namespace LunarEngine.UI;

public class ImGuiLayer : BaseLayer
{
    public uint DockspaceId;
    public ImGuiController ImGUIController { get; private set; }
    private bool _blockEvents = true;
    private bool _isInitialized;
    private IWindow _window;
    private IInputContext _inputContext;
    private GL _api;
    
    public uint ActiveWidgetId => ImGui.GetCurrentContext().ActiveId;
    public ImGuiLayer(string name, IWindow window, GL api, IInputContext inputContext) : base(name)
    {
        _window = window;
        _api = api;
        _inputContext = inputContext;
    }
    public override void OnAttach()
    {

        // io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
    }

    public override void OnInitialize()
    {
        var api = _api;
        var silkWindow = (SilkWindow)_window;
        var silkInput = (SilkInputContext)_inputContext;
        ImGUIController = new ImGuiController(api, silkWindow.NativeWindow, silkInput.NativeContext);
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
    }

    public void Begin()
    {
        // ImGui.NewFrame();
        // ImGuizmo.BeginFrame();
    }
    public override void OnDetach()
    {
        ImGui.DestroyContext();
    }

    public override void OnUpdate(TimeStep timeStep)
    {
        ImGUIController.Update(timeStep);
        DockSpace();
    }

    public void End()
    {
        var io = ImGui.GetIO();
        io.DisplaySize = (Vector2)_window.Size;
        ImGUIController.Render();
        // ImGui.EndFrame();
        // if (io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable)
        // {
        //     
        // }
    }

    public void SetDarkTheme()
    {
        var colors = ImGui.GetStyle();
    }


    public void BlockEvents(bool block)
    {
        _blockEvents = block;
    }
    
    private void DockSpace()
    {
        // ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        // ImGui.SetNextWindowPos(viewport.Pos);
        // ImGui.SetNextWindowSize(viewport.Size);
        // ImGui.SetNextWindowViewport(viewport.ID);
        // ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        // ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        // ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        // ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0)); // Fully transparent
        // ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0, 0, 0));   // Hide borders
        // ImGui.SetNextWindowBgAlpha(0.0f);
        // ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar |
        //                                ImGuiWindowFlags.NoCollapse |
        //                                ImGuiWindowFlags.NoResize |
        //                                ImGuiWindowFlags.NoMove |
        //                                ImGuiWindowFlags.NoBackground |
        //                                ImGuiWindowFlags.NoBringToFrontOnFocus |
        //                                ImGuiWindowFlags.NoNavFocus;
        DockspaceId = ImGui.DockSpaceOverViewport((ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.NoWindowMenuButton | ImGuiDockNodeFlags.PassthruCentralNode);
        if (!_isInitialized)
        {
            SetupDockLayout(DockspaceId);
        }
        
    }
    unsafe void SetupDockLayout(uint dockspaceID)
    {
        // Split the dock space into regions
        uint leftDockId = 0;
        uint rightDockId = 0;
        uint centerGapId = 0;
        ImGuiP.DockBuilderSplitNode(dockspaceID, ImGuiDir.Left, 0.2f, &leftDockId, &rightDockId);
        var tr = ImGuiP.DockBuilderSplitNode(rightDockId, ImGuiDir.Left, 0.3f, &centerGapId, &rightDockId);
        
        
        // Dock windows into specific regions
        ImGuiP.DockBuilderDockWindow("Hierarchy", leftDockId);
        ImGuiP.DockBuilderDockWindow("Scene", centerGapId);
        ImGuiP.DockBuilderDockWindow("Game", centerGapId);
        ImGuiP.DockBuilderDockWindow("Inspector", rightDockId);
        
        // Finalize the layout
        ImGuiP.DockBuilderFinish(dockspaceID);
        _isInitialized = true;
    }
}
