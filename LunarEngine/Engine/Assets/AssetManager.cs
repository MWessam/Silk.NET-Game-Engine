using LunarEngine.Engine.AssetHandleCache;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;
using Silk.NET.OpenGL;

namespace LunarEngine.Assets;
public class AssetManager : IDisposable
{
    public ShaderLibrary ShaderLibrary;
    public TextureLibrary TextureLibrary;
    private AssetHandleCache? _handleCache;

    public void Initialize(GL gl)
    {
        ShaderLibrary = ShaderLibrary
            .CreateLibraryBuilder<ShaderLibrary>()
            .Build();
        TextureLibrary = TextureLibrary
            .CreateLibraryBuilder<TextureLibrary>()
            .Build();
        _handleCache = new AssetHandleCache(gl);
    }

    public TextureHandle GetTextureHandle(TextureAsset asset) => _handleCache!.GetTextureHandle(asset);
    public TextureHandle GetTextureHandle(string textureName) => _handleCache!.GetTextureHandle(TextureLibrary.GetAsset(textureName));
    public ShaderHandle GetShaderHandle(ShaderAsset asset) => _handleCache!.GetShaderHandle(asset);
    public ShaderHandle GetShaderHandle(string shaderName) => _handleCache!.GetShaderHandle(ShaderLibrary.GetAsset(shaderName));
    public void ClearCache() => _handleCache!.ClearCache();

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
