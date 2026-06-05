using LunarEngine.Engine.AssetHandleCache;
using LunarEngine.Engine.Graphics;
using LunarEngine.GameEngine;
using LunarEngine.GameObjects;

namespace LunarEngine.Assets;
public class AssetManager : IDisposable
{
    public ShaderLibrary ShaderLibrary;
    public TextureLibrary TextureLibrary;
    private AssetHandleCache? _handleCache;

    public void Initialize(IRenderDevice device)
    {
        ShaderLibrary = ShaderLibrary
            .CreateLibraryBuilder<ShaderLibrary>()
            .Build();
        TextureLibrary = TextureLibrary
            .CreateLibraryBuilder<TextureLibrary>()
            .Build();
        _handleCache = new AssetHandleCache(device);
    }

    public ITexture2D GetTextureHandle(TextureAsset asset) => _handleCache!.GetTextureHandle(asset);
    public ITexture2D GetTextureHandle(string textureName) => _handleCache!.GetTextureHandle(TextureLibrary.GetAsset(textureName));
    public IShader GetShaderHandle(ShaderAsset asset) => _handleCache!.GetShaderHandle(asset);
    public IShader GetShaderHandle(string shaderName) => _handleCache!.GetShaderHandle(ShaderLibrary.GetAsset(shaderName));
    public void ClearCache() => _handleCache!.ClearCache();

    public void Dispose()
    {
        _handleCache?.ClearCache();
    }
}
