using Silk.NET.Maths;

namespace LunarEngine.Platform;

public interface IWindow : IDisposable
{
    Vector2D<int> Size { get; }
    Vector2D<int> FramebufferSize { get; }
    string Title { get; set; }

    event Action? OnLoad;
    event Action<double>? OnUpdate;
    event Action<Vector2D<int>>? OnResize;
    event Action? OnClosing;

    void Run();
    void Close();
}
