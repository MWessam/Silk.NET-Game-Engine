using LunarEngine.Engine.Graphics;
using LunarEngine.GameObjects;
using StbImageSharp;

namespace LunarEngine.Assets;

public class TextureAsset : IAsset, IDisposable
{
    private readonly byte[] _pixels;
    private readonly uint _width;
    private readonly uint _height;
    private readonly string _textureName;
    
    private TextureHandle? _handle;

    public byte[] Pixels => _pixels;
    public uint Width => _width;
    public uint Height => _height;

    public TextureAsset(string path, string textureName)
    {
        if (File.Exists(path))
        {
            ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
            _width = (uint)result.Width;
            _height = (uint)result.Height;
            _pixels = result.Data;
        }
        else
        {
            _width = 1;
            _height = 1;
            _pixels = [255, 255, 255, 255];
        }
        
        _textureName = textureName;
    }
    public string Key => _textureName;
    
    public void Dispose()
    {
        _handle?.Dispose();
    }
}