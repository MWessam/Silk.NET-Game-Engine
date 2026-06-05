using Silk.NET.Input;

namespace LunarEngine.Platform;

public class SilkInputContext : IInputContext
{
    private readonly Silk.NET.Input.IInputContext _context;

    public IReadOnlyList<IKeyboard> Keyboards => _context.Keyboards;
    public IReadOnlyList<IMouse> Mice => _context.Mice;

    public SilkInputContext(IWindow window)
    {
        if (window is not SilkWindow silkWindow)
            throw new ArgumentException("Window must be a SilkWindow.", nameof(window));

        _context = silkWindow.NativeWindow.CreateInput();
    }

    public void Dispose() => _context.Dispose();

    // Internal access for consumers that still need the underlying Silk.NET input context
    internal Silk.NET.Input.IInputContext NativeContext => _context;
}
