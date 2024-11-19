using System.Numerics;
using Hexa.NET.ImGui;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LunarEngine.UI;

public static class UIEngine
{
    public static uint DockspaceId;
    private static bool _isInitialized;
    public static ImGuiController ImGUIController { get; private set; }

    public static void Initialize(IWindow window, GL gl, IInputContext inputContext)
    {
        ImGUIController = new ImGuiController(gl, window, inputContext);
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
    }

    public static void Update(float dt)
    {
        ImGUIController.Update(dt);
        DockSpace();
    }

    public static void Render()
    {
        ImGUIController.Render();
    }

    private static void DockSpace()
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
    static unsafe void SetupDockLayout(uint dockspaceID)
    {
        // Split the dock space into regions
        uint leftDockId = 0;
        uint rightDockId = 0;
        uint centerGapId = 0;
        ImGuiP.DockBuilderSplitNode(dockspaceID, ImGuiDir.Left, 0.2f, &leftDockId, &rightDockId);
        var tr = ImGuiP.DockBuilderSplitNode(rightDockId, ImGuiDir.Left, 0.3f, &centerGapId, &rightDockId);
        
        
        // Dock windows into specific regions
        ImGuiP.DockBuilderDockWindow("Hierarchy", leftDockId);
        ImGuiP.DockBuilderDockWindow("Inspector", centerGapId);
        
        // Finalize the layout
        ImGuiP.DockBuilderFinish(dockspaceID);
        _isInitialized = true;
    }

    public static void PreRender()
    {
        DockSpace();
    }
}

public static class EditorUIEngine
{
    /// <summary>
    /// Creates drag and keyboard input vector3 field.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="label">MANDATORY! LABEL MUST BE UNIQUE. To hide label, u can prefix it with ##</param>
    /// <param name="dragSpeed"></param>
    public static void DrawInputDragFloat3UIElement(ref Vector3 value, string label, float dragSpeed = 0.1f)
    {
        Vector3 currentValue = value;
        if (ImGui.DragFloat3($"{label}##drag", ref currentValue, dragSpeed))
        {
            value = currentValue;
        }
        if (ImGui.InputFloat3($"{label}##input", ref currentValue))
        {
            value = currentValue;
        }
    }
    /// <summary>
    /// Creates drag and keyboard input vector4 field.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="label">MANDATORY! LABEL MUST BE UNIQUE. To hide label, u can prefix it with ##</param>
    /// <param name="dragSpeed"></param>
    public static void DrawInputDragFloat4UIElement(ref Vector4 value, string label, float dragSpeed = 0.1f)
    {
        Vector4 currentValue = value;
        if (ImGui.DragFloat4($"{label}##drag", ref currentValue, dragSpeed))
        {
            value = currentValue;
        }
        if (ImGui.InputFloat4($"{label}##input", ref currentValue))
        {
            value = currentValue;
        }
    }
    /// <summary>
    /// Creates drag and keyboard input float field.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="label">MANDATORY! LABEL MUST BE UNIQUE. To hide label, u can prefix it with ##</param>
    /// <param name="dragSpeed"></param>
    public static void DrawInputDragFloatUIElement(ref float value, string label, float dragSpeed = 0.1f)
    {
        float currentValue = value;
        if (ImGui.DragFloat($"{label}##drag", ref currentValue, dragSpeed))
        {
            value = currentValue;
        }
        if (ImGui.InputFloat($"{label}##input", ref currentValue))
        {
            value = currentValue;
        }
    }
    /// <summary>
    /// Creates keyboard input vector3 field.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="label">MANDATORY! LABEL MUST BE UNIQUE. To hide label, u can prefix it with ##</param>
    public static void DrawInputFloatUIElement(ref float value, string label)
    {
        float currentValue = value;
        if (ImGui.InputFloat($"{label}##input", ref currentValue))
        {
            value = currentValue;
        }
    }


}
public interface IUiElement
{
    void Draw(Action innerUiElementDrawCall);
}
public class DockableUiMenu : IUiElement
{
    public string Label;
    public ImGuiDir ImGuiDir;
    private bool _isInitialized = false;
    public void Draw(Action? innerUiElementDrawCall = null)
    {
        if (!_isInitialized)
        {
            // SetupDockLayout(UIEngine.DockspaceId);
        }
        ImGui.Begin(Label);
        innerUiElementDrawCall?.Invoke();
        ImGui.End();
    }
}
public class UiMenu : IUiElement
{
    public float MenuWidth;
    public float MenuHeight;
    public float PositionX;
    public float PositionY;
    public bool StretchY;
    public bool StretchX;
    public EAlignment Alignment = EAlignment.None;
    public EJustification Justification = EJustification.None;
    public EUiState UiState;
    public string Label;
    public void Draw(Action innerUiContentDrawCall)
    {
        var displaySize = ImGui.GetIO().DisplaySize;
        switch (UiState)
        {
            case EUiState.Open:
                PositionContent(displaySize);
                ResizeWindow(displaySize);
                ImGui.Begin(Label, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);
                if (ImGui.Button($"-##Collapse{Label}"))
                {
                    UiState = EUiState.Close;
                }
                innerUiContentDrawCall();
                break;
            
            default:
            case EUiState.Close:
                float sizeY = AdjustHeight(displaySize);
                PositionContent(displaySize, PositionX, PositionY, 20, sizeY);
                ResizeWindow(20, sizeY);
                ImGui.Begin(Label, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize );
                if (ImGui.Button($"+##{Label}"))
                {
                    UiState = EUiState.Open;
                }
                break;
        }
        ImGui.End();
    }

    private void ResizeWindow(float sizeX, float sizeY)
    {
        ImGui.SetNextWindowSize(new Vector2(sizeX, sizeY), ImGuiCond.Always);
    }
    private void ResizeWindow(Vector2 displaySize)
    {
        var adjustedWidth = AdjustWidth(displaySize);
        var adjustedHeight = AdjustHeight(displaySize);
        ImGui.SetNextWindowSize(new Vector2(adjustedWidth, adjustedHeight), ImGuiCond.Always); // Fixed height, variable width
    }
    private void PositionContent(Vector2 displaySize)
    {
        var justifiedPositionX = JusitfyPosition(displaySize, PositionX, MenuWidth);
        var justifiedPositionY = AlignPosition(displaySize, PositionY, MenuHeight);
        ImGui.SetNextWindowPos(new Vector2(justifiedPositionX, justifiedPositionY), ImGuiCond.Always); // Anchor to the top-left corner
    }
    private void PositionContent(Vector2 displaySize, float positionX, float positionY, float width, float height)
    {
        var justifiedPositionX = JusitfyPosition(displaySize, positionX, width);
        var justifiedPositionY = AlignPosition(displaySize, positionY, height);
        ImGui.SetNextWindowPos(new Vector2(justifiedPositionX, justifiedPositionY), ImGuiCond.Always); // Anchor to the top-left corner
    }
    private float AdjustHeight(Vector2 displaySize)
    {
        float modifiedHeight;
        if (StretchY)
        {
            modifiedHeight = displaySize.Y - MenuHeight;
        }
        else
        {
            modifiedHeight = MenuHeight;
        }
        return modifiedHeight;
    }
    private float AdjustWidth(Vector2 displaySize)
    {
        float modifiedWidth;
        if (StretchX)
        {
            modifiedWidth = displaySize.X - MenuWidth;
        }
        else
        {
            modifiedWidth = MenuWidth;
        }
        return modifiedWidth;
    }
    private float JusitfyPosition(Vector2 displaySize, float positionX, float width)
    {
        float justifiedPosX;
        switch (Justification)
        {
            case EJustification.JustifyCenter:
                justifiedPosX = (displaySize.X / 2) + positionX;
                break;
            case EJustification.JustifyLeft:
                justifiedPosX = positionX;
                break;
            case EJustification.JustifyRight:
                justifiedPosX = displaySize.X - positionX - width;
                break;
            default:
                justifiedPosX = positionX;
                break;
        }
        return justifiedPosX;
    }
    private float AlignPosition(Vector2 displaySize, float positionY, float height)
    {
        float alignedPosY;
        switch (Alignment)
        {
            case EAlignment.AlignCenter:
                alignedPosY = (displaySize.Y / 2) + positionY;
                break;
            case EAlignment.AlignTop:
                alignedPosY = 0 + positionY;
                break;
            case EAlignment.AlignBottom:
                alignedPosY = displaySize.Y - positionY;
                break;
            default:
                alignedPosY = PositionY;
                break;
        }

        return alignedPosY;
    }
}

public enum EUiState
{
    Close,
    Open,
}

public enum EAlignment
{
    None,
    AlignTop,
    AlignBottom,
    AlignCenter,
}

public enum EJustification
{
    None,
    JustifyLeft,
    JustifyRight,
    JustifyCenter,
}