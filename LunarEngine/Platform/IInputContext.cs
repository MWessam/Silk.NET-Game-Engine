using Silk.NET.Input;

namespace LunarEngine.Platform;

public interface IInputContext : IDisposable
{
    IReadOnlyList<IKeyboard> Keyboards { get; }
    IReadOnlyList<IMouse> Mice { get; }
}
