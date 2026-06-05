using LunarEngine.Assets;
using LunarEngine.Engine.Graphics;
using Silk.NET.OpenGL;

namespace LunarEngine.Engine.AssetHandleCache;

public class AssetHandleCache
{
    private GL _glApi;
    private Dictionary<string, TextureHandle> _textureHandles = new();
    private Dictionary<string, ShaderHandle> _shaderHandles = new();

    public AssetHandleCache(GL glApi)
    {
        _glApi = glApi;
    }

    public void ClearCache()
    {
        foreach (var handle in _textureHandles.Values)
        {
            handle.Dispose();
        }
        foreach (var handle in _shaderHandles.Values)
        {
            handle.Dispose();
        }
        _textureHandles.Clear();
        _shaderHandles.Clear();
    }
    public TextureHandle GetTextureHandle(TextureAsset asset)
    {
        if (_textureHandles.TryGetValue(asset.Key, out var handle))
        {
            return handle;
        }
        handle = new TextureHandle(_glApi, asset.Pixels, asset.Width, asset.Height);
        _textureHandles[asset.Key] = handle;
        return handle;
    }

    public ShaderHandle GetShaderHandle(ShaderAsset asset)
    {
        if (_shaderHandles.TryGetValue(asset.Key, out var handle))
        {
            return handle;
        }
        handle = new ShaderHandle(_glApi, asset.VertexPath, asset.FragPath);
        _shaderHandles[asset.Key] = handle;
        return handle;
    }
}
