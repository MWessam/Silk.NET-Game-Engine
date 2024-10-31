using LunarEngine.Graphics;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace LunarEngine.UI;

public class UIEngine
{
    private ImGuiContextPtr _context;
    private IView _view;
    private bool _frameBegun;
    private int _windowWidth;
    private int _windowHeight;

    public void InitializeIMGui()
    {
        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);
        ImGui.StyleColorsDark();
        
        ImGuizmo.SetImGuiContext(_context);
        ImGuiIOPtr io = ImGui.GetIO();
        
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        
        // CreateDeviceResources();
        // SetPerFrameImGuiData(1f / 60f);
        BeginFrame();
        
    }
    public void MakeCurrent()
    {
        ImGui.SetCurrentContext(_context);
        ImGuizmo.SetImGuiContext(_context);
    }

    public void Render()
    {
        if (_frameBegun)
        {
            ImGuiContextPtr currentContext = ImGui.GetCurrentContext();

            if (currentContext != _context)
            {
                ImGui.SetCurrentContext(_context);
                ImGuizmo.SetImGuiContext(_context);
            }

            _frameBegun = false;

            ImGui.Render();
            // RenderImDrawData(ImGui.GetDrawData());

            if (currentContext != _context)
            {
                ImGui.SetCurrentContext(currentContext);
                ImGuizmo.SetImGuiContext(currentContext);
            }
        }
    }
    private void BeginFrame()
    {
        ImGui.NewFrame();
        ImGuizmo.BeginFrame();

        _frameBegun = true;
        // _keyboard = _input.Keyboards[0];

        // _keyboard.KeyChar += OnKeyChar;
    }

    private void OnWindowResized(Vector2D<int> viewport)
    {
        _windowWidth = viewport.X;
        _windowHeight = viewport.Y;
    }
}