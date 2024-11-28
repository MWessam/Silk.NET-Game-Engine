using System.Reflection;
using LunarEngine.Engine.Graphics;
using LunarEngine.Graphics;
using Serilog;
using Silk.NET.OpenGL;

namespace LunarEngine.Assets;

public interface IAsset
{
    public string Key { get; }
}
public abstract class BaseAssetLibrary<TAsset> where TAsset : IAsset
{
    private Dictionary<string, TAsset> _assets = new();
    protected GL Gl;
    public abstract TAsset DefaultAsset { get; }

    protected BaseAssetLibrary(GL gl)
    {
        Gl = gl;
    }
    protected BaseAssetLibrary() {}

    public bool TryGetAsset(string assetName, out TAsset asset)
    {
        if (_assets.TryGetValue(assetName, out asset))
        {
            return true;
        }
        return false;
    }
    public TAsset GetAsset(string assetName)
    {
        if (_assets.TryGetValue(assetName, out var asset))
        {
            return asset;
        }
        Log.Error($"Couldn't find {nameof(TAsset)} of name {assetName}. Returning default asset.");
        return DefaultAsset;
    }
    public bool AddAsset(string assetName, TAsset asset)
    {
        if (_assets.TryAdd(assetName, asset))
        {
            return true;
        }
        Log.Error($"Couldn't add {nameof(TAsset)} of name {assetName}. It already exists!");
        return false;
    }
    public List<TAsset> GetAllAssets()
    {
        return _assets.Values.ToList();
    }

    public void UpdateAsset(string assetName, TAsset asset)
    {
        if (_assets.ContainsKey(assetName))
        {
            _assets[assetName] = asset;
            return;
        }
        AddAsset(assetName, asset);
    }

    public static Builder<TLibrary> CreateLibraryBuilder<TLibrary>() where TLibrary : BaseAssetLibrary<TAsset>
    {
        return new Builder<TLibrary>(CreateInstance<TLibrary>());
    }
    private static TLibrary CreateInstance<TLibrary>() where TLibrary : BaseAssetLibrary<TAsset>
    {
        var constructor = typeof(TLibrary).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, Array.Empty<Type>());
        if (constructor == null)
        {
            throw new InvalidOperationException($"No suitable constructor found for type {typeof(TLibrary).Name}");
        }
        return (TLibrary)constructor.Invoke(null);
    }
    public class Builder<TLibrary> where TLibrary : BaseAssetLibrary<TAsset>
    {
        private List<(string key, TAsset asset)> _assets = new();
        private TLibrary _library;

        public Builder(TLibrary library)
        {
            _library = library;
        }

        public Builder<TLibrary> WithAsset(string key, TAsset asset)
        {
            _assets.Add((key, asset));
            return this;
        }

        public TLibrary Build()
        {
            _library.Gl = Renderer.Instance.Api;
            if (_assets.Count == 0)
            {
                var defaultAsset = _library.DefaultAsset;
                _library.AddAsset(defaultAsset.Key, defaultAsset);
            }
            else
            {
                foreach (var assetPair in _assets)
                {
                    _library.AddAsset(assetPair.key, assetPair.asset);
                }
            }
            return _library;
        }
    }
}

public class AssetNotFoundException(string message) : Exception(message);
public class AssetAlreadyStoredException(string message) : Exception(message);
