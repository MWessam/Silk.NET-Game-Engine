namespace LunarEngine.Engine.Graphics;

public interface ITexture2D : IDisposable
{
    void Bind(int unit = 0);
    uint Width { get; }
    uint Height { get; }
    uint NativeHandle { get; }
}
