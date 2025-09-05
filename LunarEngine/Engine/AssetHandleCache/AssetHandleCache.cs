using LunarEngine.Assets;
using LunarEngine.Engine.Graphics;
using Silk.NET.OpenGL;

namespace LunarEngine.Engine.AssetHandleCache;

public interface IAssetHandleCache
{
    public TextureHandle GetTextureHandle(TextureAsset asset);
    public TextureHandle GetTextureHandle(string textureName);
    public ShaderHandle GetShaderHandle(ShaderAsset asset);
    public ShaderHandle GetShaderHandle(string shaderName);
}
public class AssetHandleCache : IAssetHandleCache
{
    private AssetManager _assetManager;
    private GL _glApi;
    private Dictionary<string, TextureHandle> _textureHandles;
    private Dictionary<string, ShaderHandle> _shaderHandles;
    private bool _shouldClearCache;

    public AssetHandleCache(AssetManager assetManager, GL glApi)
    {
        _assetManager = assetManager;
        _glApi = glApi;
    }

    public void ClearCache()
    {

    }
    public TextureHandle GetTextureHandle(TextureAsset asset)
    {
        if (_textureHandles.TryGetValue(asset.Key, out var handle))
        {
            return handle;
        }
        handle = new TextureHandle(_glApi, asset.Pixels, asset.Width, asset.Height);
        return handle;
    }

    public ShaderHandle GetShaderHandle(ShaderAsset asset)
    {
        if (_shaderHandles.TryGetValue(asset.Key, out var handle))
        {
            return handle;
        }
        handle = new ShaderHandle(_glApi, asset.VertexPath, asset.FragPath);
        return handle;
    }
    public TextureHandle GetTextureHandle(string textureName)
    {
        var asset = _assetManager.TextureLibrary.GetAsset(textureName);
        return GetTextureHandle(asset);
    }

    public ShaderHandle GetShaderHandle(string shaderName)
    {
        var shader = _assetManager.ShaderLibrary.GetAsset(shaderName);
        return GetShaderHandle(shader);
    }
}