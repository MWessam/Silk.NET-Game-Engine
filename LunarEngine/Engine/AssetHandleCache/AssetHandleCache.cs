using LunarEngine.Assets;
using LunarEngine.Renderer;

namespace LunarEngine.Assets;

public class AssetHandleCache
{
    private IRenderDevice _device;
    private Dictionary<string, ITexture2D> _textureHandles = new();
    private Dictionary<string, IShader> _shaderHandles = new();

    public AssetHandleCache(IRenderDevice device)
    {
        _device = device;
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
    public ITexture2D GetTextureHandle(TextureAsset asset)
    {
        if (_textureHandles.TryGetValue(asset.Key, out var handle))
        {
            return handle;
        }
        handle = _device.CreateTexture2D(asset.Pixels, asset.Width, asset.Height);
        _textureHandles[asset.Key] = handle;
        return handle;
    }

    public IShader GetShaderHandle(ShaderAsset asset)
    {
        if (_shaderHandles.TryGetValue(asset.Key, out var handle))
        {
            return handle;
        }
        handle = _device.CreateShader(asset.VertexPath, asset.FragPath);
        _shaderHandles[asset.Key] = handle;
        return handle;
    }
}
