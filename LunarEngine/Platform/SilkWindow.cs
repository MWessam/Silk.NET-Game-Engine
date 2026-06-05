using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace LunarEngine.Platform;

public class SilkWindow : IWindow
{
    private readonly Silk.NET.Windowing.IWindow _window;

    public event Action? OnLoad;
    public event Action<double>? OnUpdate;
    public event Action<Vector2D<int>>? OnResize;
    public event Action? OnClosing;

    public Vector2D<int> Size => _window.Size;
    public Vector2D<int> FramebufferSize => _window.FramebufferSize;
    public string Title { get => _window.Title; set => _window.Title = value; }

    public SilkWindow(string title, int width, int height)
    {
        var options = WindowOptions.Default;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);
        _window = Silk.NET.Windowing.Window.Create(options);

        _window.Load += () => OnLoad?.Invoke();
        _window.Update += dt => OnUpdate?.Invoke(dt);
        _window.FramebufferResize += size => OnResize?.Invoke(size);
        _window.Closing += () => OnClosing?.Invoke();
    }

    public void Run() => _window.Run();
    public void Close() => _window.Close();
    public void Dispose() => _window.Dispose();

    // Internal access for consumers that still need the underlying Silk.NET window
    internal Silk.NET.Windowing.IWindow NativeWindow => _window;
}
